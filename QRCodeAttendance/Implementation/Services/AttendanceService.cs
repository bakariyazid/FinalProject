using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Implementation.Repositories;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Implementation.Services
{
    public class AttendanceService : IAttendanceService
    {
          private readonly IAttendanceRepository _attendanceRepository;
          private readonly IUnitOfWork _unitOfWork; 
          private readonly ISessionRepository _sessionRepository;
          private readonly ICurrentUserService _currentUserService;
          private readonly IStudentRepository _studentRepository;

        public AttendanceService(IAttendanceRepository attendanceRepository, IUnitOfWork unitOfWork, 
        ISessionRepository sessionRepository, ICurrentUserService currentUserService, IStudentRepository studentRepository)
        {
            _attendanceRepository = attendanceRepository;
            _unitOfWork = unitOfWork;
            _sessionRepository = sessionRepository;
            _currentUserService = currentUserService;
            _studentRepository = studentRepository;
        }

        public async Task<BaseResponse<bool>> MarkAttendance(Guid sessionId, string qrCode)
        {
            var student = await _studentRepository.Get<Student>(s => s.UserId == _currentUserService.UserId);
            if (student == null)
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "Student record not found"
                };

            var studentId = student.Id;

            var session = await _sessionRepository.Get<Session>(s => s.Id == sessionId);
            if (session == null) 
                return new BaseResponse<bool> 
                {
                     Status = false, 
                     Message = "Session not found" 
                };

            var now = DateTime.UtcNow.ToUniversalTime();
            var firstScanStart = session.SessionStartTime;
            var firstScanEnd = session.SessionStartTime.AddMinutes(25);
            var secondScanStart = session.SessionEndTime.AddMinutes(-20);
            var secondScanEnd = session.SessionEndTime.AddMinutes(-10);
            var isFirstScanWindow = now >= firstScanStart && now <= firstScanEnd;
            var isSecondScanWindow = now >= secondScanStart && now < secondScanEnd;

            if (now >= secondScanEnd)
            {
                session.IsActive = false;
                session.QRCodeToken = null;
                session.QRCodeExpiry = now;

                _sessionRepository.Update(session);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "Attendance has closed"
                };
            }

            if (!session.IsActive || now < session.SessionStartTime)
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "Attendance is not open yet. Please wait until the class start time."
                };

            if (!isFirstScanWindow && !isSecondScanWindow)
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = $"No scan is active now. First scan is open for the first 25 minutes. Second scan opens from {secondScanStart.ToLocalTime():hh:mm tt} to {secondScanEnd.ToLocalTime():hh:mm tt}."
                };

            if (string.IsNullOrWhiteSpace(session.QRCodeToken) || now >= session.QRCodeExpiry)
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "This QR Code has expired. Please scan the current live QR Code"
                };

            if (qrCode != session.QRCodeToken)
                return new BaseResponse<bool> 
                { 
                    Status = false, 
                    Message = "Invalid or expired QR Code" 
                };

            var attendance = await _attendanceRepository.Get<Attendance>(a => a.StudentId == studentId && a.SessionId == sessionId);

            if (isFirstScanWindow)
            {
                if (attendance != null)
                {
                    if (attendance.FirstScanTime.HasValue)
                        return new BaseResponse<bool>
                        {
                            Status = false,
                            Message = attendance.SecondScanTime.HasValue
                                ? "You have already completed both scans for this class."
                                : "You have already completed the first scan. Wait for the second scan window."
                        };

                    attendance.FirstScanTime = now;
                    attendance.ScanTime = now;
                    attendance.Status = AttendanceStatus.Incomplete;
                    attendance.UpdatedDate = now;
                    _attendanceRepository.Update(attendance);
                }
                else
                {
                    attendance = CreateAttendance(student, session, now, AttendanceStatus.Incomplete);
                    attendance.FirstScanTime = now;
                    await _attendanceRepository.Add(attendance);
                }

                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool>
                {
                    Status = true,
                    Message = "First scan recorded. You must complete the second scan when it opens to be fully present.",
                    Data = true
                };
            }

            if (attendance == null || !attendance.FirstScanTime.HasValue || attendance.Status == AttendanceStatus.Absent)
            {
                if (attendance == null)
                {
                    attendance = CreateAttendance(student, session, now, AttendanceStatus.Absent);
                    await _attendanceRepository.Add(attendance);
                }
                else
                {
                    attendance.Status = AttendanceStatus.Absent;
                    attendance.ScanTime = now;
                    attendance.UpdatedDate = now;
                    _attendanceRepository.Update(attendance);
                }

                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "Second scan rejected because you did not complete the first scan. You have been marked absent for this class."
                };
            }

            if (attendance.SecondScanTime.HasValue)
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "You have already completed both scans for this class."
                };

            attendance.SecondScanTime = now;
            attendance.ScanTime = now;
            attendance.Status = AttendanceStatus.Present;
            attendance.UpdatedDate = now;
            _attendanceRepository.Update(attendance);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Status = true,
                Message = "Second scan recorded. You are fully present for this class.",
                Data = true
            };
        }

        private static Attendance CreateAttendance(Student student, Session session, DateTime scanTime, AttendanceStatus status)
        {
            return new Attendance
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                SessionId = session.Id,
                StudentName = student.FullName(),
                CourseName = session.CourseName,
                CourseCode = session.CourseCode,
                ScanTime = scanTime,
                Status = status,
                CreatedDate = scanTime,
                UpdatedDate = scanTime
            };
        }

        public async Task<IReadOnlyList<AttendanceDto>> GetAttendanceBySession(Guid sessionId)
        {
            var attendances = await _attendanceRepository.GetBySession(sessionId);

            return attendances.Select(a => new AttendanceDto
            {
                Id = a.Id,
                StudentId = a.StudentId,
                SessionId = a.SessionId,
                StudentName = a.StudentName,
                ScanTime = a.ScanTime,
                CourseName = a.CourseName,
                CourseCode = a.CourseCode,
                Status = a.Status
                ,
                FirstScanTime = a.FirstScanTime,
                SecondScanTime = a.SecondScanTime
            }).ToList();
        }

        public async Task<IReadOnlyList<AttendanceDto>> GetAttendanceByStudent(Guid studentId)
        {
            var attendances = await _attendanceRepository.GetByStudentId(studentId);

            return attendances.Select(a => new AttendanceDto
            {
                Id = a.Id,
                StudentId = a.StudentId,
                SessionId = a.SessionId,
                StudentName = a.StudentName,
                CourseName = a.CourseName,
                CourseCode = a.CourseCode,
                ScanTime = a.ScanTime,
                Status = a.Status,
                FirstScanTime = a.FirstScanTime,
                SecondScanTime = a.SecondScanTime
            }).ToList();
        }

        public async Task<IReadOnlyList<AttendanceDto>> GetAttendanceForInstructor(Guid instructorId)
        {
             var attendances = await _attendanceRepository.GetAttendanceByInstructor(instructorId);

            return attendances.Select(a => new AttendanceDto
            {
                Id = a.Id,
                StudentId = a.StudentId,
                SessionId = a.SessionId,
                StudentName = a.StudentName,
                CourseName = a.CourseName,
                CourseCode = a.CourseCode,
                ScanTime = a.ScanTime,
                Status = a.Status,
                FirstScanTime = a.FirstScanTime,
                SecondScanTime = a.SecondScanTime
            }).ToList();
        }

        public async Task<AttendanceDto?> GetAttendanceById(Guid id)
        {
            var attendance = await _attendanceRepository.Get<Attendance>(a => a.Id == id);

            if (attendance == null)
                return null;

            return new AttendanceDto
            {
                Id = attendance.Id,
                StudentId = attendance.StudentId,
                SessionId = attendance.SessionId,
                StudentName = attendance.StudentName,
                CourseName = attendance.CourseName,
                CourseCode = attendance.CourseCode,
                ScanTime = attendance.ScanTime,
                Status = attendance.Status,
                FirstScanTime = attendance.FirstScanTime,
                SecondScanTime = attendance.SecondScanTime
            };
        }
    

        public async Task<bool> HasStudentMarkedAttendance(Guid studentId, Guid sessionId)
        {
            var attendance = await _attendanceRepository.Get<Attendance>(a => a.StudentId == studentId && a.SessionId == sessionId);
            return attendance != null;
        }

        public async Task<IReadOnlyList<AttendanceDto>> GetAttendanceByInstructor(Guid instructorId)
            {
                var sessions = await _sessionRepository.GetAll(s => s.InstructorId == instructorId);
                var sessionIds = sessions.Select(s => s.Id).ToList();

                if (!sessionIds.Any()) return new List<AttendanceDto>();

                var attendances = await _attendanceRepository.GetAll(a => sessionIds.Contains(a.SessionId));

                return attendances.Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student?.FullName() ?? "Unknown", 
                    CourseName = a.ClassSession?.CourseName ?? "Unknown",
                    CourseCode = a.ClassSession?.CourseCode ?? "Unknown",
                    SessionId = a.SessionId,
                    ScanTime = a.ScanTime,
                    Status = a.Status,
                    FirstScanTime = a.FirstScanTime,
                    SecondScanTime = a.SecondScanTime
                }).ToList();
            }
            
        public async Task<double> GetAttendancePercentage(Guid studentId, string courseName)
        {
            var sessions = await _sessionRepository.GetSessionsByCourseName(courseName);

            var sessionIds = sessions.Select(s => s.Id).ToList();

            if (!sessionIds.Any())
                return 0;

            var attendances = await _attendanceRepository.GetAll<Attendance>();

            var studentAttendances = attendances
                .Where(a => a.StudentId == studentId && sessionIds.Contains(a.SessionId))
                .ToList();

            double percentage = (double)studentAttendances.Count / sessionIds.Count * 100;

            return percentage;
        }

    public async Task<IReadOnlyList<AttendanceDto>> GetAttendanceByCourseName(string courseName)
        {
            var sessions = await _sessionRepository.GetSessionsByCourseName(courseName);

            var sessionIds = sessions.Select(s => s.Id).ToList();

            var attendances = await _attendanceRepository.GetAll<Attendance>();

            var filtered = attendances
                .Where(a => sessionIds.Contains(a.SessionId))
                .ToList();

            var result = filtered.Select(a => new AttendanceDto
            {
                Id = a.Id,
                StudentId = a.StudentId,
                StudentName = a.StudentName,
                CourseName = a.CourseName,
                CourseCode = a.CourseCode,
                SessionId = a.SessionId,
                ScanTime = a.ScanTime,
                Status = a.Status,
                FirstScanTime = a.FirstScanTime,
                SecondScanTime = a.SecondScanTime
            }).ToList();

            return result;
        }



     
       public async Task<BaseResponse<bool>> DeleteAttendance(Guid id)
        {
            var attendance = await _attendanceRepository.Get<Attendance>(a => a.Id == id);

            if (attendance == null)
            {
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "Attendance record not found",
                    Data = false
                };
            }

            await _attendanceRepository.Delete(attendance);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Status = true,
                Message = "Attendance deleted successfully",
                Data = true
            };
        }      

        public async Task<BaseResponse<bool>> UpdateAttendanceStatus(Guid id, AttendanceStatus status)
        {
            var attendance = await _attendanceRepository.Get<Attendance>(a => a.Id == id);

            if (attendance == null)
            {
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "Attendance not found",
                    Data = false
                };
            }

            attendance.Status = status;

            _attendanceRepository.Update(attendance);

            await _unitOfWork.SaveChangesAsync(); 

            return new BaseResponse<bool>
            {
                Status = true,
                Message = "Attendance updated successfully",
                Data = true
            };
        }
        public async Task<int> GetTotalAttendanceForStudent(Guid studentId)
        {
               return (await _attendanceRepository.GetAll(studentId)).Count;
        }

        public Task<bool> MarkAttendanceWithStatus(Guid studentId, Guid sessionId, AttendanceStatus status)
        {
            throw new NotImplementedException();
        }


    }
}
