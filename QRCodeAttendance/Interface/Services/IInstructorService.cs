using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.DTOs.Instructor;
using QRCodeAttendance.Models.DTOs.InstructorDto;
using QRCodeAttendance.Models.DTOs.Session;
using QRCodeAttendance.Models.DTOs.StudentDto;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Interface.Services
{
    public interface IInstructorService
    {
        Task<BaseResponse<bool>> RegisterInstructor(CreateInstructorRequestModel request);
        Task<BaseResponse<IReadOnlyList<StudentDto>>> GetStudentsByDeptAndLevel(Departments department, StudentLevel level);
        Task<BaseResponse<InstructorDto>> GetInstructorByIdAsync(Guid instructorId, CancellationToken cancellationToken);
        Task<BaseResponse<InstructorDashboardDto>> GetDashboard(Guid userId);
        Task<BaseResponse<InstructorDto>> GetInstructorProfile(Guid userId);
        
        Task<BaseResponse<bool>> UpdateInsProfile(Guid userId, UpdateInstructorRequestModel request);
        Task<BaseResponse<IReadOnlyList<SessionDto>>> GetInstructorSessions(Guid userId);


    }
}