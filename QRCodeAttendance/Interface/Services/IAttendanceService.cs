using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Interface.Services
{
    public interface IAttendanceService
    {
        // Task <BaseResponse<bool>> MarkAttendance(Guid studentId, Guid sessionId);
        Task<BaseResponse<bool>> MarkAttendance(Guid sessionId, string qrCode);
        Task<BaseResponse<bool>> SyncOfflineAttendance(OfflineAttendanceScanRequestModel request);
        Task<bool> HasStudentMarkedAttendance(Guid studentId, Guid sessionId);
        Task<IReadOnlyList<AttendanceDto>> GetAttendanceBySession(Guid sessionId);
        Task<IReadOnlyList<AttendanceDto>> GetAttendanceByStudent(Guid studentId);
        public  Task<BaseResponse<bool>> UpdateAttendanceStatus(Guid id, AttendanceStatus status);
        public Task<BaseResponse<bool>> DeleteAttendance(Guid id);
        Task<AttendanceDto?> GetAttendanceById(Guid id);
        Task<IReadOnlyList<AttendanceDto>> GetAttendanceByInstructor(Guid instructorId);
        Task<double> GetAttendancePercentage(Guid studentId, string courseName);
        Task<IReadOnlyList<AttendanceDto>> GetAttendanceByCourseName(string courseName);
        Task<bool> MarkAttendanceWithStatus(Guid studentId, Guid sessionId, AttendanceStatus status);
         Task<int> GetTotalAttendanceForStudent(Guid studentId);
    }
}
