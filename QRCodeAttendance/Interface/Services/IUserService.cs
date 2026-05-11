using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Instructor;
using QRCodeAttendance.Models.DTOs.InstructorDto;
using QRCodeAttendance.Models.DTOs.Student;
using QRCodeAttendance.Models.DTOs.StudentDto;
using QRCodeAttendance.Models.DTOs.User;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Services
{
    public interface IUserService
    {        
            Task<BaseResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken);
            Task<BaseResponse<User>> GetByEmail(string email);
            Task <BaseResponse<IReadOnlyList<InstructorDto>>> GetAllInstructors(CancellationToken cancellationToken);
            
    }
}