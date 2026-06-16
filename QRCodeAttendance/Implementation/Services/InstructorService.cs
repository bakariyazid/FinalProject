using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCodeAttendance.Contract.Services;
using QRCodeAttendance.Implementation.Repositories;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.DTOs.Instructor;
using QRCodeAttendance.Models.DTOs.InstructorDto;
using QRCodeAttendance.Models.DTOs.Session;
using QRCodeAttendance.Models.DTOs.StudentDto;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Implementation.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly IIdentityService _identityService;
        private readonly IRoleRepository _roleRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<InstructorService> _logger;
        private readonly IStudentRepository _studentRepository;
        private readonly IInvitationRepository _invitationRepository;

        public InstructorService(IInstructorRepository instructorRepository, ISessionRepository sessionRepository,
        IAttendanceRepository attendanceRepository, IUnitOfWork unitOfWork, ISessionService sessionService, 
        IUserRepository userRepository, IIdentityService identityService, IRoleRepository roleRepository, 
        UserManager<User> userManager, ILogger<InstructorService> logger, IStudentRepository studentRepository,
        IInvitationRepository invitationRepository)
        {
            _instructorRepository = instructorRepository ?? throw new ArgumentNullException(nameof(instructorRepository));
            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            _attendanceRepository = attendanceRepository ?? throw new ArgumentNullException(nameof(attendanceRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
        }

 public async Task<BaseResponse<bool>> RegisterInstructor(CreateInstructorRequestModel request)
            {
                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var invitationCode = request.InvitationCode.Trim().ToUpperInvariant();
                var invitationCodeHash = AdminInvitationService.HashInvitationCode(normalizedEmail, invitationCode);

                var invitation = await _invitationRepository.GetApprovedByEmailAndCodeHash(normalizedEmail, invitationCodeHash);

                if (invitation == null)
                {
                    return new BaseResponse<bool>
                    {
                        Message = "Invalid instructor invitation code for this email address.",
                        Status = false
                    };
                }

                if (invitation.IsUsed)
                {
                    return new BaseResponse<bool>
                    {
                        Message = "This instructor invitation code has already been used.",
                        Status = false
                    };
                }

                if (invitation.ExpiryDate <= DateTime.UtcNow.ToUniversalTime())
                {
                    return new BaseResponse<bool>
                    {
                        Message = "This instructor invitation code has expired. Please contact the admin for a new code.",
                        Status = false
                    };
                }

                var instructorExist = await _userRepository.Any(u => u.Email.ToLower() == normalizedEmail);
                if (instructorExist)
                {
                    return new BaseResponse<bool>
                    {
                        Message = "Instructor with email already exist",
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
                            Email = normalizedEmail,
                            PasswordHash = _identityService.GetPasswordHash(request.PasswordHash),
                            UserName = normalizedEmail,
                            RoleId = (await _roleRepository.GetByName("Instructor"))?.Id ?? throw new Exception("Instructor role not found"),
                        };

                        var createResult = await _userManager.CreateAsync(user);

                        if (!createResult.Succeeded)
                        {
                            throw new Exception(string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        }
                        
                var hashedPassword = _identityService.GetPasswordHash(request.PasswordHash);
                        user.PasswordHash = hashedPassword;
                        var instructor = new Instructor
                        {
                            UserId = user.Id,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            Email = normalizedEmail,
                            Department = request.Department,
                            Address = request.Address,
                            Gender = request.Gender,
                            PhoneNumber = request.PhoneNumber,
                            DateOfBirth = request.DateOfBirth,
                            PasswordHash = hashedPassword,
                            CreatedDate = DateTime.UtcNow
                        };

                        await _instructorRepository.Add(instructor);
                        invitation.IsUsed = true;
                        invitation.UsedAt = DateTime.UtcNow;
                        invitation.UpdatedDate = DateTime.UtcNow;
                        await _unitOfWork.SaveChangesAsync();

                        await transaction.CommitAsync();

                        return new BaseResponse<bool>
                        {
                            Message = "Instructor created successfully",
                            Status = true
                        };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error creating instructor");

                        return new BaseResponse<bool>
                        {
                            Message = "An error occurred while creating instructor: " + ex.Message,
                            Status = false
                        };
                    }
                });
            }
 public async Task<BaseResponse<IReadOnlyList<StudentDto>>> GetStudentsByDeptAndLevel(Departments department, StudentLevel level)
            {
                var students = await _studentRepository.GetStudentsByCriteria(department, level);

                if (students == null || !students.Any())
                {
                    return new BaseResponse<IReadOnlyList<StudentDto>>
                    {
                        Message = $"No {level} students found in the {department} department.",
                        Status = false
                    };
                }

                return new BaseResponse<IReadOnlyList<StudentDto>>
                {
                    Message = "Students fetched successfully",
                    Status = true,
                    Data = students.Select(p => new StudentDto
                    {
                        StudentId = p.Id,
                        FullName = $"{p.FirstName} {p.LastName}",
                        Gender = p.Gender,
                        DateOfBirth = p.DateOfBirth,
                        Email = p.Email,
                        PhoneNumber = p.PhoneNumber,
                        MatricNumber = p.MatricNumber,
                        Department = p.Department,
                        StudentLevel = p.StudentLevel,
                        UserId = p.UserId,
                    }).ToList()
                };
            }
        
        public async Task<BaseResponse<InstructorDto>> GetInstructorByIdAsync(Guid instructorId, CancellationToken cancellationToken)
                {
                    var instructor = await _instructorRepository.GetInstructorById(instructorId);
                    if (instructor == null)
                    {
                        _logger.LogError("Instructor doesn't exist");
                        return new BaseResponse<InstructorDto>
                        {
                            Message = "Instructor doesn't exist",
                            Status = false
                        };
                    }

                    _logger.LogInformation("Instructor fetched successfully");
                    return new BaseResponse<InstructorDto>
                    {
                        Message = "Instructor fetched successfully",
                        Status = true,
                        Data = new InstructorDto
                        {
                            InstructorId = instructor.Id,
                            FirstName = instructor.FirstName,
                            LastName = instructor.LastName,
                            FullName = instructor.FullName(),
                            Address = instructor.Address,
                            DateOfBirth = instructor.DateOfBirth,
                            Email = instructor.User?.Email ?? instructor.Email,
                            PhoneNumber = instructor.PhoneNumber,
                            Department = instructor.Department,
                            CreatedDate = instructor.CreatedDate,
                            UpdatedDate = instructor.UpdatedDate

                        }
                    };
                }

    public async Task<BaseResponse<InstructorDashboardDto>> GetDashboard(Guid userId)
        {
            var response = new BaseResponse<InstructorDashboardDto>();

            try
            {
                
                var instructor = await _instructorRepository.Get<Instructor>(i => i.UserId == userId);
                if (instructor == null )
                {
                    response.Status = false;
                    response.Message = "Instructor not found.";
                    return response;
                }

                var sessions = await _sessionRepository
                    .GetSessionsByInstructorId(instructor.Id);

                var totalSessions = sessions.Count();

                var sessionIds = sessions.Select(s => s.Id).ToList();

                var attendances = await _attendanceRepository
                    .GetAll(a => sessionIds.Contains(a.SessionId));

                var totalAttendances = attendances.Count();

                var recentSessions = sessions
                    .OrderByDescending(s => s.SessionStartTime)
                    .Take(5)
                    .Select(s => new SessionDto
                    {
                        Id = s.Id,
                        CourseName = s.CourseName,
                        CourseCode = s.CourseCode,
                        SessionStartTime = s.SessionStartTime,
                        Level = s.Level,
                        Department = s.Department,
                        SessionEndTime = s.SessionEndTime,
                        IsActive = s.IsActive
                    })
                    .ToList();

                var dashboard = new InstructorDashboardDto
                {
                    TotalSessions = totalSessions,
                    TotalAttendances = totalAttendances,
                    RecentSessions = recentSessions
                };

                response.Status = true;
                response.Message = "Dashboard retrieved successfully.";
                response.Data = dashboard;

                return response;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
                return response;
            }
        }

      
        public async Task<BaseResponse<IReadOnlyList<SessionDto>>> GetInstructorSessions(Guid userId)
        {
             var instructor = await _instructorRepository.Get<Instructor>(i => i.UserId == userId);

            if (instructor == null)
            {
                return new BaseResponse<IReadOnlyList<SessionDto>>
                {
                    Status = false,
                    Message = "Instructor not found"
                };
            }

            return await _sessionService.GetSessionsByInstructor(instructor.Id);
        }


        public async Task<BaseResponse<InstructorDto>> GetInstructorProfile(Guid userId)
            {
                var instructor = await _instructorRepository.Get<Instructor>(i => i.UserId == userId);

                if (instructor == null)
                {
                    return new BaseResponse<InstructorDto>
                    {
                        Status = false,
                        Message = "Instructor not found",
                        Data = default!
                    };
                }

                var instructorDto = new InstructorDto
                {
                    InstructorId = instructor.Id,
                    UserId = instructor.UserId,
                    FirstName = instructor.FirstName, 
                    LastName = instructor.LastName,   
                    FullName = instructor.FullName(),
                    Email = instructor.Email,
                    PhoneNumber = instructor.PhoneNumber,
                    Address = instructor.Address,
                    Gender = instructor.Gender,
                    Department = instructor.Department,
                    DateOfBirth = instructor.DateOfBirth,
                    CreatedDate = instructor.CreatedDate,
                    UpdatedDate = instructor.UpdatedDate
                };

                return new BaseResponse<InstructorDto>
                {
                    Status = true,
                    Message = "Instructor retrieved successfully",
                    Data = instructorDto
                };
            }

      
        public async Task<BaseResponse<bool>> UpdateInsProfile(Guid userId, UpdateInstructorRequestModel request)
            {
                var response = new BaseResponse<bool>();

                var instructor = await _instructorRepository.Get<Instructor>(i => i.UserId == userId);

                if (instructor == null)
                {
                    response.Status = false;
                    response.Message = "Instructor not found";
                    response.Data = false;
                    return response;
                }

                instructor.FirstName = request.FirstName ?? instructor.FirstName;
                instructor.LastName = request.LastName ?? instructor.LastName;
                instructor.PhoneNumber = request.PhoneNumber ?? instructor.PhoneNumber;
                instructor.Gender = request.Gender != default ? request.Gender : instructor.Gender;
                instructor.Department = request.Department != default ? request.Department : instructor.Department;
                instructor.DateOfBirth = request.DateOfBirth != default ? request.DateOfBirth : instructor.DateOfBirth;
                instructor.Address = request.Address ?? instructor.Address;
                instructor.UpdatedDate = DateTime.UtcNow.ToUniversalTime();


                if (instructor.User != null)
                {
                    instructor.User.Email = request.Email ?? instructor.User.Email;
                }

                _instructorRepository.Update(instructor);
                await _unitOfWork.SaveChangesAsync();

                response.Status = true;
                response.Message = "Profile updated successfully";
                response.Data = true;

                return response;
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
