using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCodeAttendance.Contract.Services;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.DTOs.Student;
using QRCodeAttendance.Models.DTOs.StudentDto;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;


namespace QRCodeAttendance.Implementation.Services
{
    public class StudentService : IStudentService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<StudentService> _logger;
        private readonly IRoleRepository _roleRepository;

        public StudentService(IAttendanceRepository attendanceRepository, ISessionRepository sessionRepository, IStudentRepository studentRepository,
        IUserRepository userRepository, IUnitOfWork unitOfWork, IIdentityService identityService, UserManager<User> userManager, ILogger<StudentService> logger, IRoleRepository roleRepository)
        {
            _attendanceRepository = attendanceRepository;
            _sessionRepository = sessionRepository;
            _studentRepository = studentRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _userManager = userManager;
            _logger = logger;
            _roleRepository = roleRepository;
        }

        public async Task<BaseResponse<bool>> RegisterStudent(CreateStudentRequestModel request)
                    {
                        var studentExist = await _userRepository.Any(u => u.Email == request.Email);
                        if (studentExist)
                        {
                            return new BaseResponse<bool>
                            {
                                Message = "Student with email already exist",
                                Status = false
                            };
                        }

                        if (request.PasswordHash != request.ConfirmPassword)
                        {
                            return new BaseResponse<bool>
                            {
                                Message = "Password doesn't match!",
                                Status = false,
                            };
                        }

                        (var valid, var message) = ValidatePassword(request.PasswordHash);
                        if (!valid)
                            return new BaseResponse<bool> { Message = message ?? string.Empty, Status = false };

                        var strategy = _unitOfWork.CreateExecutionStrategy();

                        return await strategy.ExecuteAsync(async () =>
                        {
                            using var transaction = await _unitOfWork.BeginTransactionAsync();

                            try
                            {
                                var user = new User
                                {
                                    Email = request.Email,
                                    PasswordHash = _identityService.GetPasswordHash(request.PasswordHash),
                                    UserName = request.Email,
                                    RoleId = (await _roleRepository.GetByName("Student"))?.Id ?? throw new Exception("Student role not found"),
                                };

                                var createResult = await _userManager.CreateAsync(user);

                                if (!createResult.Succeeded)
                                {
                                    throw new Exception(string.Join(", ", createResult.Errors.Select(e => e.Description)));
                                }
                                
                        var hashedPassword = _identityService.GetPasswordHash(request.PasswordHash);
                                user.PasswordHash = hashedPassword;

                                var student = new Student
                                {
                                    FirstName = request.FirstName,
                                    LastName = request.LastName,
                                    Email = request.Email,
                                    Address = request.Address,
                                    Gender = request.Gender,
                                    DateOfBirth = request.DateOfBirth,
                                    PhoneNumber = request.PhoneNumber,
                                    MatricNumber = request.MatricNumber,
                                    StudentLevel = request.StudentLevel,
                                    Department = request.Department,
                                    PasswordHash = hashedPassword,
                                    UserId = user.Id,
                                    CreatedDate = DateTime.UtcNow
                                };
                                
                                await _studentRepository.Add(student);
                                await _unitOfWork.SaveChangesAsync();

                                await transaction.CommitAsync();

                                return new BaseResponse<bool>
                                {
                                    Message = "Student created successfully",
                                    Status = true
                                };
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError(ex, "Error creating student, rolling back.....");

                                return new BaseResponse<bool>
                                {
                                    Message = "An error occurred while creating student: " + ex.Message,
                                    Status = false
                                };
                            }
                        });
                    }

        public async Task<BaseResponse<StudentDashboardDto>> GetDashboard(Guid studentId)
            {
                var response = new BaseResponse<StudentDashboardDto>();

                var now = DateTime.UtcNow; 

                try
                {
                    var student = await _studentRepository.Get<Student>(s => s.UserId == studentId);
                    if (student == null)
                    {
                        _logger.LogWarning("Dashboard access failed: Student with UserId {StudentId} not found.", studentId);
                        response.Status = false;
                        response.Message = "Student record not found.";
                        return response;
                    }

                    var attendances = await _attendanceRepository.GetAll(a => a.StudentId == student.Id);
                    var presentSessionIds = attendances
                        .Where(a => a.Status == AttendanceStatus.Present)
                        .Select(a => a.SessionId)
                        .ToHashSet();

                    var sessions = await _sessionRepository.GetAll(s => 
                        s.Level == student.StudentLevel && 
                        s.Department == student.Department);

                    var dashboard = new StudentDashboardDto
                    {
                        UserName = $"{student.FirstName} {student.LastName}",
                        MatricNumber = student.MatricNumber,
                        Level = student.StudentLevel,
                        Department = student.Department,
                        TotalSessionsAttended = presentSessionIds.Count,
                        
                        TotalSessionsAvailable = sessions.Count(s => s.SessionEndTime <= now),

                        ActiveSessions = sessions
                            .Where(s => s.IsActive == true && now >= s.SessionStartTime && now <= s.SessionEndTime)
                            .Select(s =>
                            {
                                var attendance = attendances.FirstOrDefault(a => a.SessionId == s.Id);
                                return new ActiveSessionDto
                                {
                                    Id = s.Id,
                                    CourseName = s.CourseName,
                                    CourseCode = s.CourseCode,
                                    Level = s.Level,
                                    SessionStartTime = s.SessionStartTime,
                                    IsActive = s.IsActive,
                                    Department = s.Department,
                                    SessionEndTime = s.SessionEndTime,
                                    InstructorName = s.Instructor != null ? $"{s.Instructor.FirstName} {s.Instructor.LastName}" : "Department Staff",
                                    AttendanceStatus = attendance?.Status,
                                    FirstScanTime = attendance?.FirstScanTime,
                                    SecondScanTime = attendance?.SecondScanTime
                                };
                            }).ToList(),

                        MissedSessions = sessions
                            .Where(s => s.SessionEndTime < now && !presentSessionIds.Contains(s.Id))
                            .Select(s => new MissedSessionDto
                            {
                                CourseName = s.CourseName,
                                CourseCode = s.CourseCode,
                                Level = s.Level,
                                Department = s.Department,
                                StartTime = s.SessionStartTime,
                                EndTime = s.SessionEndTime,
                                InstructorName = s.Instructor != null ? $"{s.Instructor.FirstName} {s.Instructor.LastName}" : "Instructor"
                            }).ToList(),

                        RecentAttendances = attendances
                            .OrderByDescending(a => a.ScanTime)
                            .Take(5)
                            .Select(a => new AttendanceDto
                            {
                                Id = a.Id,
                                SessionId = a.SessionId,
                                CourseName = a.CourseName ?? a.ClassSession?.CourseName ?? "Unknown Course",
                                CourseCode = a.CourseCode ?? a.ClassSession?.CourseCode ?? "N/A",
                                ScanTime = a.ScanTime,
                                FirstScanTime = a.FirstScanTime,
                                SecondScanTime = a.SecondScanTime,
                                Status = a.Status
                            }).ToList()
                    };

                    response.Status = true;
                    response.Message = "Dashboard retrieved successfully";
                    response.Data = dashboard;
                    
                    _logger.LogInformation("Dashboard successfully generated for Student: {MatricNumber}", student.MatricNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the dashboard for StudentId: {StudentId}", studentId);
                    response.Status = false;
                    response.Message = "An internal error occurred while loading your dashboard.";
                }

                    return response;
            }

        public async Task<BaseResponse<StudentAttendanceReportDto>> GetAttendanceReport(Guid userId)
        {
            var student = await _studentRepository.Get<Student>(s => s.UserId == userId);
            if (student == null)
            {
                return new BaseResponse<StudentAttendanceReportDto>
                {
                    Status = false,
                    Message = "Student record not found."
                };
            }

            var attendances = await _attendanceRepository.GetByStudentId(student.Id);
            var records = attendances
                .OrderByDescending(a => a.ScanTime)
                .Select(a => ToStudentAttendanceReportItem(student, a))
                .ToList();

            return new BaseResponse<StudentAttendanceReportDto>
            {
                Status = true,
                Message = "Attendance report retrieved successfully.",
                Data = new StudentAttendanceReportDto
                {
                    StudentName = student.FullName(),
                    MatricNumber = student.MatricNumber,
                    Department = student.Department,
                    Level = student.StudentLevel,
                    TotalAttended = records.Count(r => r.Status == AttendanceStatus.Present),
                    Records = records
                }
            };
        }

        public async Task<BaseResponse<StudentAttendanceReportDto>> GetAttendanceReportByStudentId(Guid studentId)
        {
            var student = await _studentRepository.Get<Student>(s => s.Id == studentId);
            if (student == null)
            {
                return new BaseResponse<StudentAttendanceReportDto>
                {
                    Status = false,
                    Message = "Student record not found."
                };
            }

            var attendances = await _attendanceRepository.GetByStudentId(student.Id);
            var records = attendances
                .OrderByDescending(a => a.ScanTime)
                .Select(a => ToStudentAttendanceReportItem(student, a))
                .ToList();

            return new BaseResponse<StudentAttendanceReportDto>
            {
                Status = true,
                Message = "Attendance report retrieved successfully.",
                Data = new StudentAttendanceReportDto
                {
                    StudentName = student.FullName(),
                    MatricNumber = student.MatricNumber,
                    Department = student.Department,
                    Level = student.StudentLevel,
                    TotalAttended = records.Count(r => r.Status == AttendanceStatus.Present),
                    Records = records
                }
            };
        }

        public async Task<BaseResponse<StudentAttendanceReportItemDto>> GetAttendanceReportItem(Guid userId, Guid sessionId)
        {
            var student = await _studentRepository.Get<Student>(s => s.UserId == userId);
            if (student == null)
            {
                return new BaseResponse<StudentAttendanceReportItemDto>
                {
                    Status = false,
                    Message = "Student record not found."
                };
            }

            var attendance = (await _attendanceRepository.GetByStudentId(student.Id))
                .FirstOrDefault(a => a.SessionId == sessionId);

            if (attendance == null)
            {
                return new BaseResponse<StudentAttendanceReportItemDto>
                {
                    Status = false,
                    Message = "Attendance report was not found for this class."
                };
            }

            return new BaseResponse<StudentAttendanceReportItemDto>
            {
                Status = true,
                Message = "Attendance report retrieved successfully.",
                Data = ToStudentAttendanceReportItem(student, attendance)
            };
        }

        private static StudentAttendanceReportItemDto ToStudentAttendanceReportItem(Student student, Attendance attendance)
        {
            var session = attendance.ClassSession;

            return new StudentAttendanceReportItemDto
            {
                AttendanceId = attendance.Id,
                SessionId = attendance.SessionId,
                StudentName = student.FullName(),
                MatricNumber = student.MatricNumber,
                Department = student.Department,
                Level = student.StudentLevel,
                CourseName = attendance.CourseName ?? session?.CourseName ?? "Unknown Course",
                CourseCode = attendance.CourseCode ?? session?.CourseCode ?? "N/A",
                InstructorName = session?.Instructor != null
                    ? $"{session.Instructor.FirstName} {session.Instructor.LastName}"
                    : "Instructor",
                SessionStartTime = session?.SessionStartTime ?? attendance.ScanTime,
                SessionEndTime = session?.SessionEndTime ?? attendance.ScanTime,
                ScanTime = attendance.ScanTime,
                FirstScanTime = attendance.FirstScanTime,
                SecondScanTime = attendance.SecondScanTime,
                Status = attendance.Status
            };
        }

        public async Task<BaseResponse<double>> GetMyAttendancePercentage(Guid studentId)
            {
                var student = await _studentRepository.Get<Student>(s => s.UserId == studentId);
                if (student == null) 
                return new BaseResponse<double> 
                { 
                    Status = false, 
                    Message = "Student not found"
                };

                var now = DateTime.UtcNow;

                int totalSessions = await _sessionRepository.Count<Session>(s => 
                    s.SessionEndTime <= now && 
                    s.Level == student.StudentLevel && 
                    s.Department == student.Department);

                if (totalSessions == 0)
                {
                    return new BaseResponse<double> { Status = true, Message = "No sessions yet", Data = 0 };
                }

                int attendedCount = await _attendanceRepository.Count<Attendance>(a => 
                    a.StudentId == student.Id &&
                    (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late));

                double percentage = ((double)attendedCount / totalSessions) * 100;

                return new BaseResponse<double>
                {
                    Status = true,
                    Message = "Attendance percentage calculated",
                    Data = Math.Round(percentage, 2)
                };
            }

            public async Task<BaseResponse<StudentDto>> GetStudentProfile(Guid userId)
                {
                    var student = await _studentRepository.Get<Student>(x => x.UserId == userId);

                    if (student == null)
                    {
                        return new BaseResponse<StudentDto>
                        {
                            Message = "Student not found",
                            Status = false,
                            Data = default!
                        };
                    }

                    var studentDto = new StudentDto
                    {
                        StudentId = student.Id,
                        UserId = student.UserId, 
                        FullName = student.FullName(),
                        Email = student.Email,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        PhoneNumber = student.PhoneNumber,
                        Address = student.Address,
                        Gender = student.Gender,
                        DateOfBirth = student.DateOfBirth,
                        MatricNumber = student.MatricNumber,
                        StudentLevel = student.StudentLevel,
                        Department = student.Department,
                        CreatedDate = student.CreatedDate, 
                        UpdatedDate = student.UpdatedDate  
                    };

                    return new BaseResponse<StudentDto>
                    {
                        Data = studentDto,
                        Message = "Student profile retrieved successfully",
                        Status = true
                    };
                }

    

    public async Task<BaseResponse<bool>> UpdateStudentProfile(Guid userId, UpdateStudentRequestModel request)
        {
            try
            {
                var student = await _studentRepository.Get<Student>(s => s.UserId == userId);
                
                if (student == null)
                {
                    _logger.LogWarning("Update failed: Student with UserId {UserId} not found.", userId);
                    return new BaseResponse<bool> { Status = false, Message = "Student record not found." };
                }

                student.FirstName = request.FirstName ?? student.FirstName;
                student.LastName = request.LastName ?? student.LastName;
                student.PhoneNumber = request.PhoneNumber ?? student.PhoneNumber;
                student.Address = request.Address ?? student.Address;
                student.Email = request.Email ?? student.Email;
                student.Gender = request.Gender != default ? request.Gender : student.Gender;
                student.StudentLevel = request.StudentLevel != default ? request.StudentLevel : student.StudentLevel;
                student.Department = request.Department != default ? request.Department : student.Department;
                student.DateOfBirth = request.DateOfBirth != default ? request.DateOfBirth : student.DateOfBirth;
                student.UpdatedDate = DateTime.UtcNow.ToUniversalTime();

                _studentRepository.Update(student);
                
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated profile for Student {UserId}.", userId);

                return new BaseResponse<bool> 
                { 
                    Status = true, 
                    Message = "Profile updated successfully",
                    Data = true 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating profile for Student {UserId}. Error: {Message}", userId, ex.Message);

                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "An unexpected error occurred while saving your changes. Please try again later.",
                    Data = false
                };
            }
        }

        

         private static (bool, string?) ValidatePassword(string password)
                {
                    // Minimum length of password
                    int minLength = 8;

                    // Maximum length of password
                    int maxLength = 50;

                    // Check for null or empty password
                    if (string.IsNullOrEmpty(password))
                    {
                        return (false, "Password cannot be null or empty.");
                    }

                    // Check length of password
                    if (password.Length < minLength || password.Length > maxLength)
                    {
                        return (false, $"Password must be between {minLength} and {maxLength} characters long.");
                    }

                    // Check for at least one uppercase letter, one lowercase letter, and one digit
                    bool hasUppercase = false;
                    bool hasLowercase = false;
                    bool hasDigit = false;

                    foreach (char c in password)
                    {
                        if (char.IsUpper(c))
                        {
                            hasUppercase = true;
                        }
                        else if (char.IsLower(c))
                        {
                            hasLowercase = true;
                        }
                        else if (char.IsDigit(c))
                        {
                            hasDigit = true;
                        }
                    }

                    if (!hasUppercase || !hasLowercase || !hasDigit)
                    {
                        return (false, "Password must contain at least one uppercase letter, one lowercase letter, and one digit.");
                    }

                    // Check for any characters
                    string invalidCharacters = @" !""#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
                    if (password.IndexOfAny(invalidCharacters.ToCharArray()) == -1)
                    {
                        return (false, "Password must contain one or more characters.");
                    }

                    // Password is valid
                    return (true, null);
                }



    
          
    }
}
