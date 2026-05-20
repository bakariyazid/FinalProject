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

            var qrHardExpiry = session.SessionEndTime.AddMinutes(-10);

            if (now >= qrHardExpiry)
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

            var lateThreshold = session.SessionStartTime.AddMinutes(45);
            AttendanceStatus autoStatus = now <= lateThreshold
                ? AttendanceStatus.Present
                : AttendanceStatus.Late;

            
            var alreadyMarked = await _attendanceRepository.HasStudentMarkedAttendance(studentId, sessionId);
            if (alreadyMarked)
                return new BaseResponse<bool> { Status = false, Message = "You have already marked attendance" };

            
            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                SessionId = sessionId,
                StudentName = student.FullName(), 
                CourseName = session.CourseName,
                CourseCode = session.CourseCode,
                ScanTime = DateTime.UtcNow.ToUniversalTime(),
                Status = autoStatus,
                CreatedDate = DateTime.UtcNow.ToUniversalTime()
            };

            await _attendanceRepository.Add(attendance);
            await _unitOfWork.SaveChangesAsync(); 

            return new BaseResponse<bool> 
            { 
                Status = true, 
                Message = $"Successfully marked as {autoStatus}",
                Data = true 
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
                Status = a.Status
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
                Status = a.Status
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
                Status = attendance.Status
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
                    Status = a.Status
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
                Status = a.Status
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
