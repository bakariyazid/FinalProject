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
using QRCodeAttendance.Models.DTOs.Instructor;
using QRCodeAttendance.Models.DTOs.InstructorDto;
using QRCodeAttendance.Models.DTOs.Student;
using QRCodeAttendance.Models.DTOs.StudentDto;
using QRCodeAttendance.Models.DTOs.User;
using QRCodeAttendance.Models.Entities;
namespace QRCodeAttendance.Implementation.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IRoleRepository _roleRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
   

        public UserService(IUserRepository userRepository,
            UserManager<User> userManager, 
            ILogger<UserService> logger, IRoleRepository roleRepository, 
            IStudentRepository studentRepository, IInstructorRepository instructorRepository, 
            IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _instructorRepository = instructorRepository ?? throw new ArgumentNullException(nameof(instructorRepository));
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        }
       

        public async Task<BaseResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetUserWithRole(u => u.Email == request.Email);

                if (user == null)
                {
                    return new BaseResponse<LoginResponseModel>
                    {
                        Message = "Invalid credentials",
                        Status = false
                    };
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

                if (!isPasswordValid)
                {
                    return new BaseResponse<LoginResponseModel>
                    {
                        Message = "Invalid credentials",
                        Status = false
                    };
                }

                var role = user.Role?.Name;

                if (string.IsNullOrEmpty(role))
                {
                    return new BaseResponse<LoginResponseModel>
                    {
                        Message = "User role not assigned",
                        Status = false
                    };
                }

                var response = new LoginResponseModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = string.Empty, 
                    FullName = string.Empty,  
                    Role = role
                };

              if (role == "Student" && user.Student != null)
                {
                    response.FirstName = user.Student.FirstName;
                    response.FullName = user.Student.FullName();
                }

                else if (role == "Instructor" && user.Instructor != null)
                {
                    response.FirstName = user.Instructor.FirstName;
                    response.FullName = user.Instructor.FullName();
                    response.InstructorId = user.Instructor.Id;
                }
                else if (role == "Admin")
                {
                    response.FirstName = "Admin";
                    response.FullName = "System Admin";
                }
                else
                {
                    return new BaseResponse<LoginResponseModel>
                    {
                        Message = "User profile not found",
                        Status = false
                    };
                }

                return new BaseResponse<LoginResponseModel>
                {
                    Message = "Login successful",
                    Status = true,
                    Data = response
                };
            }
                            
    public async Task<BaseResponse<IReadOnlyList<InstructorDto>>> GetAllInstructors(CancellationToken cancellationToken)
        {
             var instructors = await _roleRepository.GetAll<Instructor>();
            if (!instructors.Any())
            {
                _logger.LogError("No data found");
                return new BaseResponse<IReadOnlyList<InstructorDto>>
                {
                    Message = "No data found",
                    Status = false
                };
            }
            return new BaseResponse<IReadOnlyList<InstructorDto>>
            {
                Message = "Data fetched successfully",
                Data = instructors.Select(p => new InstructorDto
                {
                    InstructorId = p.Id,
                    // FirstName = p.FirstName,
                    // LastName = p.LastName,
                    FullName = p.FullName(),
                    Gender = p.Gender,
                    Department = p.Department,
                    UserId = p.UserId,
                }).ToList()
            };

        }


        public async Task<BaseResponse<User>> GetByEmail(string email)
            {
                var user = await _userRepository.GetByEmail(email);

                if (user == null)
                {
                    return new BaseResponse<User>
                    {
                        Status = false,
                        Message = "User not found"
                    };
                }

                return new BaseResponse<User>
                {
                    Status = true,
                    Message = "User retrieved successfully",
                    Data = user
                };
            }
    }
}


/////Instructor Registration Code before refactor

 // var userExists = await _userRepository.Any(u => u.Email == request.Email);
                // if (userExists)
                // {
                //     _logger.LogError("User with email already exists");
                //     return new BaseResponse<bool>
                //     {
                //         Message = "User with email already exists",
                //         Status = false
                //     };
                // }

                // if (request.Password != request.ConfirmPassword)
                // {
                //     return new BaseResponse<bool>
                //     {
                //         Message = "Password does not match!",
                //         Status = false
                //     };
                // }

                // (var passwordResult, var message) = ValidatePassword(request.Password);
                // if (!passwordResult)
                // {
                //     return new BaseResponse<bool>
                //     {
                //         Message = message,
                //         Status = false
                //     };
                // }

                // var role = await _roleRepository.GetByName("Instructor");
                // if (role == null)
                // {
                //     _logger.LogError("Instructor role not found");
                //     return new BaseResponse<bool>
                //     {
                //         Message = "System error: Role not initialized",
                //         Status = false
                //     };
                // }

                // var passwordHasher = new PasswordHasher<User>();

                // var user = new User
                // {
                //     Email = request.Email,
                //     UserName = request.Email,
                //     RoleId = role.Id,
                //     CreatedDate = DateTime.UtcNow,
                //     UpdatedDate = DateTime.UtcNow
                // };

                // user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

                // var strategy = _unitOfWork.CreateExecutionStrategy();

                // var response = await strategy.ExecuteAsync(async () =>
                // {
                //     using var transaction = await _unitOfWork.BeginTransactionAsync();

                //     try
                //     {
                       
                //         await _userRepository.Add(user);

                //         var instructor = new Instructor
                //         {
                //             FirstName = request.FirstName,
                //             LastName = request.LastName,
                //             UserId = user.Id,
                //             CreatedDate = DateTime.UtcNow,
                //             UpdatedDate = DateTime.UtcNow
                //         };

                //         await _instructorRepository.Add(instructor);

                //         await _unitOfWork.SaveChangesAsync();

                //         await transaction.CommitAsync();

                //         return new BaseResponse<bool>
                //         {
                //             Message = "Instructor registered successfully",
                //             Status = true
                //         };
                //     }
                //     catch (Exception ex)
                //     {
                //         await transaction.RollbackAsync();
                //         _logger.LogError(ex, "Error during instructor registration");

                //         return new BaseResponse<bool>
                //         {
                //             Message = "An error occurred while creating instructor: " + ex.Message,
                //             Status = false
                //         };
                //     }
                // });

                // return response;





//////Student Registration Code before refactor
                // var userExists = await _userRepository.Any(u => u.Email == request.Email);
                // if (userExists)
                // {
                //     _logger.LogError("User with email already exists");
                //     return new BaseResponse<bool>
                //     {
                //         Message = "User with email already exists",
                //         Status = false
                //     };
                // }

                // if (request.PasswordHash != request.ConfirmPassword)
                // {
                //     return new BaseResponse<bool>
                //     {
                //         Message = "Password does not match!",
                //         Status = false
                //     };
                // }

                // (var passwordResult, var message) = ValidatePassword(request.PasswordHash);
                // if (!passwordResult)
                // {
                //     return new BaseResponse<bool>
                //     {
                //         Message = message,
                //         Status = false
                //     };
                // }

                // var role = await _roleRepository.GetByName("Student");
                // if (role == null)
                // {
                //     _logger.LogError("Student role not found");
                //     return new BaseResponse<bool>
                //     {
                //         Message = "System error: Role not initialized",
                //         Status = false
                //     };
                // }

                // var passwordHasher = new PasswordHasher<User>();

                // var user = new User
                // {
                //     Email = request.Email,
                //     UserName = request.Email,
                //     RoleId = role.Id,
                //     CreatedDate = DateTime.UtcNow,
                //     UpdatedDate = DateTime.UtcNow
                // };


                // user.PasswordHash = passwordHasher.HashPassword(user, request.PasswordHash);

                // var strategy = _unitOfWork.CreateExecutionStrategy();

                // var response = await strategy.ExecuteAsync(async () =>
                // {
                //     using var transaction = await _unitOfWork.BeginTransactionAsync();

                //     try
                //     {
                //         await _userRepository.Add(user);

                //         var student = new Student
                //         {
                //             FirstName = request.FirstName,
                //             LastName = request.LastName,
                //             UserId = user.Id,
                //             CreatedDate = DateTime.UtcNow,
                //             UpdatedDate = DateTime.UtcNow
                //         };

                //         await _studentRepository.Add(student);

                //         await _unitOfWork.SaveChangesAsync();

                //         await transaction.CommitAsync();

                //         return new BaseResponse<bool>
                //         {
                //             Message = "Student registered successfully",
                //             Status = true
                //         };
                //     }
                //     catch (Exception ex)
                //     {
                //         await transaction.RollbackAsync();
                //         _logger.LogError(ex, "Error during student registration");

                //         return new BaseResponse<bool>
                //         {
                //             Message = "An error occurred while creating student: " + ex.Message,
                //             Status = false
                //         };
                //     }
                // });

                // return response;






                                    // if (newUser == null)
                    // {
                    //     _logger.LogError("User Creation unsuccessful");
                    //     return new BaseResponse<bool>
                    //     {
                    //         Message = "User Creation unsuccessful",
                    //         Status = false
                    //     };

                    // }
