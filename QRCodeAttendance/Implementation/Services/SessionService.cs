using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.DTOs.Session;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Implementation.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInstructorRepository _instructorRepository;
        private readonly ILogger<SessionService> _logger;
        public SessionService(ISessionRepository sessionRepository, IUnitOfWork unitOfWork ,
         IAttendanceRepository attendanceRepository, ICurrentUserService currentUserService,
          IInstructorRepository instructorRepository, ILogger<SessionService> logger)
        {
            _sessionRepository = sessionRepository;
            _unitOfWork = unitOfWork;
            _attendanceRepository = attendanceRepository;
            _currentUserService = currentUserService;
            _instructorRepository = instructorRepository;
            _logger = logger;
        }
       

    public async Task<BaseResponse<SessionDto>> CreateSessionAsync(Guid instructorId, CreateSessionRequestModel request)
        {
            _logger.LogInformation("Instructor {instructorId} is creating a session for course {CourseName}", instructorId, request.CourseName);


            var instructor = await _instructorRepository.Get<Instructor>(i => i.Id == instructorId);
            if (instructor == null)
            {
                _logger.LogWarning("Instructor {InstructorId} not found", instructorId);
                return new BaseResponse<SessionDto> { Status = false, Message = "Instructor not found" };
            }

            if (string.IsNullOrWhiteSpace(request.CourseName) || string.IsNullOrWhiteSpace(request.CourseCode))
            {
                return new BaseResponse<SessionDto> { Status = false, Message = "Course name and code are required" };
            }

            var session = new Session
            {
                Id = Guid.NewGuid(),
                CourseName = request.CourseName,
                CourseCode = request.CourseCode,
                Level = request.Level,
                Department = request.Department,
                SessionStartTime = request.SessionStartTime.ToUniversalTime(),
                SessionEndTime = request.SessionEndTime.ToUniversalTime(),
                IsActive = true,
                InstructorId = instructor.Id,
                QRCodeToken = string.Empty,
                QRCodeExpiry = request.QRCodeExpiry.ToUniversalTime(),
                CreatedDate = DateTime.UtcNow.ToUniversalTime(),
                UpdatedDate = DateTime.UtcNow.ToUniversalTime()
            };

            await _sessionRepository.Add(session);

            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (saveResult > 0)
            {
                _logger.LogInformation("Session {SessionId} created successfully", session.Id);

                var sessionDto = new SessionDto
                {
                    Id = session.Id,
                    InstructorId = session.InstructorId,
                    CourseName = session.CourseName,
                    CourseCode = session.CourseCode,
                    Level = session.Level,
                    Department = session.Department,
                    SessionStartTime = session.SessionStartTime,
                    SessionEndTime = session.SessionEndTime,
                    IsActive = session.IsActive,
                    QRCodeToken = string.Empty,
                    QRCodeExpiry = session.QRCodeExpiry,
                    CreatedDate = session.CreatedDate,
                    UpdatedDate = session.UpdatedDate 
                };

                return new BaseResponse<SessionDto>
                {
                    Status = true,
                    Message = "Session created successfully",
                    Data = sessionDto
                };
            }

            return new BaseResponse<SessionDto>
            {
                Status = false,
                Message = "Session creation unsuccessful - Database save failed"
            };
        }

        public async Task<BaseResponse<SessionDto>> GenerateSessionQrCode(Guid sessionId)
            {
                var session = await _sessionRepository.Get<Session>(s => s.Id == sessionId);

                if (session == null)
                {
                    return new BaseResponse<SessionDto>
                    {
                        Status = false,
                        Message = "Session not found"
                    };
                }

                var now = DateTime.UtcNow;
                var hardCutoff = session.SessionEndTime.AddMinutes(-10);

                if (now < session.SessionStartTime)
                {
                    return new BaseResponse<SessionDto>
                    {
                        Status = false,
                        Message = "Too early to generate QR code. Wait until the class start time."
                    };
                }

                if (now >= hardCutoff)
                {
                    session.IsActive = false;
                    session.QRCodeToken = null;
                    session.QRCodeExpiry = now;
                    session.UpdatedDate = now;

                    _sessionRepository.Update(session);
                    await _unitOfWork.SaveChangesAsync();

                    return new BaseResponse<SessionDto>
                    {
                        Status = false,
                        Message = "Attendance has closed. QR Code has expired.",
                        Data = MapSessionToDto(session)
                    };
                }

                if (!string.IsNullOrWhiteSpace(session.QRCodeToken) && now < session.QRCodeExpiry)
                {
                    return new BaseResponse<SessionDto>
                    {
                        Status = true,
                        Message = "Current QR Code is still valid",
                        Data = MapSessionToDto(session)
                    };
                }

                var standardExpiry = now.AddMinutes(5);
                session.QRCodeExpiry = standardExpiry > hardCutoff ? hardCutoff : standardExpiry;
                session.QRCodeToken = Guid.NewGuid().ToString("N");
                session.IsActive = true;
                session.UpdatedDate = now;

                _sessionRepository.Update(session);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<SessionDto>
                {
                    Status = true,
                    Message = "QR Code rotated successfully",
                    Data = MapSessionToDto(session)
                };
            }

        public async Task<int> RotateDueQrCodesAsync()
        {
            var now = DateTime.UtcNow;
            var sessions = await _sessionRepository.GetAll<Session>();
            var changedCount = 0;

            foreach (var session in sessions)
            {
                var hardCutoff = session.SessionEndTime.AddMinutes(-10);

                if (now < session.SessionStartTime)
                {
                    continue;
                }

                if (now >= hardCutoff)
                {
                    if (session.IsActive || !string.IsNullOrWhiteSpace(session.QRCodeToken))
                    {
                        session.IsActive = false;
                        session.QRCodeToken = null;
                        session.QRCodeExpiry = now;
                        session.UpdatedDate = now;

                        _sessionRepository.Update(session);
                        changedCount++;
                    }

                    continue;
                }

                var qrIsMissing = string.IsNullOrWhiteSpace(session.QRCodeToken);
                var qrIsExpired = now >= session.QRCodeExpiry;

                if (!qrIsMissing && !qrIsExpired)
                {
                    continue;
                }

                var standardExpiry = now.AddMinutes(5);
                session.QRCodeToken = Guid.NewGuid().ToString("N");
                session.QRCodeExpiry = standardExpiry > hardCutoff ? hardCutoff : standardExpiry;
                session.IsActive = true;
                session.UpdatedDate = now;

                _sessionRepository.Update(session);
                changedCount++;
            }

            if (changedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return changedCount;
        }

        private static SessionDto MapSessionToDto(Session session)
        {
            return new SessionDto
            {
                Id = session.Id,
                InstructorId = session.InstructorId,
                CourseName = session.CourseName,
                CourseCode = session.CourseCode,
                Level = session.Level,
                Department = session.Department,
                SessionStartTime = session.SessionStartTime,
                SessionEndTime = session.SessionEndTime,
                IsActive = session.IsActive,
                QRCodeToken = session.QRCodeToken,
                QRCodeExpiry = session.QRCodeExpiry,
                CreatedDate = session.CreatedDate,
                UpdatedDate = session.UpdatedDate
            };
        }

        public async Task<BaseResponse<SessionDto>> GetSessionById(Guid sessionId)
        {
            var response = new BaseResponse<SessionDto>();

            try
            {
                var session = await _sessionRepository.Get<Session>(s => s.Id == sessionId);

                if (session == null)
                {
                    response.Status = false;
                    response.Message = "Session not found";
                    return response;
                }

                response.Data = new SessionDto
                {
                    Id = session.Id,
                    InstructorId = session.InstructorId,
                    CourseName = session.CourseName,
                    CourseCode = session.CourseCode,
                    Level = session.Level,
                    Department = session.Department,
                    SessionStartTime = session.SessionStartTime,
                    SessionEndTime = session.SessionEndTime,
                    IsActive = session.IsActive,
                    QRCodeToken = session.QRCodeToken,
                    QRCodeExpiry = session.QRCodeExpiry,
                    CreatedDate = session.CreatedDate,
                    UpdatedDate = session.UpdatedDate
                };

                response.Status = true;
                response.Message = "Session retrieved successfully";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<BaseResponse<IReadOnlyList<SessionDto>>> GetAllSessions()
        {
            var response = new BaseResponse<IReadOnlyList<SessionDto>>();

            try
            {
                var sessions = await _sessionRepository.GetAll<Session>();

                var sessionDtos = sessions.Select(session => new SessionDto
                {
                    Id = session.Id,
                    CourseName = session.CourseName,
                    CourseCode = session.CourseCode,
                    Level = session.Level,
                    Department = session.Department,
                    SessionStartTime = session.SessionStartTime,
                    SessionEndTime = session.SessionEndTime,
                    IsActive = session.IsActive,
                    QRCodeToken = session.QRCodeToken,
                    QRCodeExpiry = session.QRCodeExpiry,
                    CreatedDate = session.CreatedDate,
                    UpdatedDate = session.UpdatedDate
                }).ToList();

                response.Data = sessionDtos;
                response.Status = true;
                response.Message = "Sessions retrieved successfully";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<BaseResponse<SessionDto>> UpdateSession(Guid sessionId, UpdateSessionRequestModel request)
            {
                 
                var session = await _sessionRepository.Get<Session>(s => s.Id == sessionId);

                if (session == null)
                {
                    return new BaseResponse<SessionDto>
                    {
                        Status = false,
                        Message = "Session not found"
                    };
                }
                
                session.CourseName = request.CourseName;
                session.CourseCode = request.CourseCode;
                session.Level = request.Level;
                session.Department = request.Department;
                session.SessionStartTime = request.SessionStartTime.ToUniversalTime();
                session.SessionEndTime = request.SessionEndTime.ToUniversalTime();
                session.UpdatedDate = DateTime.UtcNow.ToUniversalTime();

                _sessionRepository.Update(session);
                await _unitOfWork.SaveChangesAsync();

                var sessionDto = new SessionDto
                {
                    Id = session.Id,
                    CourseName = session.CourseName,
                    CourseCode = session.CourseCode,
                    Level = session.Level,
                    Department = session.Department,
                    SessionStartTime = session.SessionStartTime,
                    SessionEndTime = session.SessionEndTime,
                    IsActive = session.IsActive,
                    CreatedDate = session.CreatedDate,
                    UpdatedDate = session.UpdatedDate
                };

                return new BaseResponse<SessionDto>
                {
                    Status = true,
                    Message = "Session updated successfully",
                    Data = sessionDto
                };
            }

        public async Task<BaseResponse<bool>> DeleteSession(Guid sessionId)
        {
            var session = await _sessionRepository.Get<Session>(s => s.Id == sessionId);

            if (session == null)
            {
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "Session not found"
                };
            }

            await _sessionRepository.Delete(session);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Status = true,
                Message = "Session deleted successfully",
                Data = true
            };
        }

            public async Task<BaseResponse<IReadOnlyList<SessionDto>>> GetSessionsByDate(DateTime date)
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);
 
                var sessions = await _sessionRepository.GetSessionsByDate(date);

                if (sessions == null || !sessions.Any())
                {
                    return new BaseResponse<IReadOnlyList<SessionDto>>
                    {
                        Status = false,
                        Message = "No sessions found for the selected date.",
                        Data = new List<SessionDto>()
                    };
                }

                var sessionDtos = sessions.Select(s => new SessionDto
                {
                    Id = s.Id,
                    CourseName = s.CourseName,
                    CourseCode = s.CourseCode,
                    Level = s.Level,
                    Department = s.Department,
                    SessionStartTime = s.SessionStartTime,
                    SessionEndTime = s.SessionEndTime,
                    IsActive = s.IsActive,
                    QRCodeToken = s.QRCodeToken,
                    QRCodeExpiry = s.QRCodeExpiry
              
                }).ToList();

                return new BaseResponse<IReadOnlyList<SessionDto>>
                {
                    Status = true,
                    Message = "Sessions retrieved successfully.",
                    Data = sessionDtos
                };
            }


      public async Task<BaseResponse<IReadOnlyList<SessionDto>>> GetSessionsByInstructor(Guid instructorId)
        {
            var response = new BaseResponse<IReadOnlyList<SessionDto>>();

            var sessions = await _sessionRepository.GetAll(s => s.InstructorId == instructorId);

            if (sessions == null || !sessions.Any())
            {
                response.Status = true; 
                response.Message = "No sessions found.";
                response.Data = new List<SessionDto>().AsReadOnly();
                return response;
            }

            var sessionDtos = sessions.Select(s => new SessionDto
            {
                Id = s.Id,
                CourseName = s.CourseName,
                CourseCode = s.CourseCode,
                Level = s.Level,    
                Department = s.Department,
                SessionStartTime = s.SessionStartTime,
                SessionEndTime = s.SessionEndTime,
                IsActive = s.IsActive,
                InstructorId = s.InstructorId,
                QRCodeToken = s.QRCodeToken,
                QRCodeExpiry = s.QRCodeExpiry,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate
            })
            .OrderByDescending(s => s.SessionStartTime) 
            .ToList();

            response.Status = true;
            response.Message = "Sessions retrieved successfully";
            response.Data = sessionDtos.AsReadOnly();

            return response;
        }
       public async Task<BaseResponse<IReadOnlyList<AttendanceDto>>> GetSessionAttendance(Guid sessionId)
            {
                var response = new BaseResponse<IReadOnlyList<AttendanceDto>>();

                try
                {
                    var attendances = await _attendanceRepository.GetBySession(sessionId);

                    if (attendances == null || !attendances.Any())
                    {
                        response.Status = false;
                        response.Message = "No attendance records found.";
                        return response;
                    }

                    var attendanceDtos = attendances.Select(a => new AttendanceDto
                    {
                        Id = a.Id,
                        StudentId = a.StudentId,
                        SessionId = a.SessionId,
                        StudentName = a.Student?.FullName() ?? a.StudentName, 
                        CourseName = a.ClassSession?.CourseName ?? a.CourseName,
                        CourseCode = a.ClassSession?.CourseCode ?? a.CourseCode,
                        Status = a.Status, 
                        ScanTime = a.ScanTime,
                        CreatedDate = a.CreatedDate
                    }).ToList();

                    response.Status = true;
                    response.Data = attendanceDtos;
                    return response;
                }
                catch (Exception ex)
                {
                    response.Status = false;
                    response.Message = $"Error: {ex.Message}";
                    return response;
                }
            }

       
    }
}
