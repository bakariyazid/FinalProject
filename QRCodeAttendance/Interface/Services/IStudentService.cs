using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.DTOs.Session;
using QRCodeAttendance.Models.DTOs.Student;
using QRCodeAttendance.Models.DTOs.StudentDto;

namespace QRCodeAttendance.Interface.Services
{
    public interface IStudentService
    {
        Task<BaseResponse<bool>> RegisterStudent(CreateStudentRequestModel request);
        Task<BaseResponse<StudentDto>> GetStudentProfile(Guid userId);
        Task<BaseResponse<bool>> UpdateStudentProfile(Guid userId, UpdateStudentRequestModel request);
        // Task<BaseResponse<IReadOnlyList<AttendanceDto>>> GetMyAttendance(Guid studentId);
        Task<BaseResponse<double>> GetMyAttendancePercentage(Guid studentId);
        // Task<BaseResponse<IReadOnlyList<SessionDto>>> GetAvailableSessions(Guid studentId);
        // Task<BaseResponse<bool>> MarkAttendanceWithQr(Guid studentId, string qrCode);
        Task<BaseResponse<StudentDashboardDto>> GetDashboard(Guid studentId);
        Task<BaseResponse<StudentAttendanceReportDto>> GetAttendanceReport(Guid userId);
        Task<BaseResponse<StudentAttendanceReportDto>> GetAttendanceReportByStudentId(Guid studentId);
        Task<BaseResponse<StudentAttendanceReportItemDto>> GetAttendanceReportItem(Guid userId, Guid sessionId);
    }
}
