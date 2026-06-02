using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.DTOs.Reports;
using QRCodeAttendance.Models.DTOs.Session;

namespace QRCodeAttendance.Interface.Services
{
    public interface ISessionService
    {
        Task<BaseResponse<SessionDto>> CreateSessionAsync( Guid instructorId, CreateSessionRequestModel request);
        Task<BaseResponse<SessionDto>> GetSessionById(Guid sessionId);
        Task<BaseResponse<IReadOnlyList<SessionDto>>> GetAllSessions();
        Task<BaseResponse<SessionDto>> UpdateSession(Guid sessionId, UpdateSessionRequestModel request);
        Task<BaseResponse<bool>> DeleteSession(Guid sessionId);
        Task<BaseResponse<IReadOnlyList<AttendanceDto>>> GetSessionAttendance(Guid sessionId);
        Task<BaseResponse<IReadOnlyList<SessionDto>>> GetSessionsByInstructor(Guid instructorId);
        Task<BaseResponse<SessionDto>> GenerateSessionQrCode(Guid sessionId);
        Task<int> RotateDueQrCodesAsync();
        // Task<BaseResponse<bool>> ValidateSessionQrCode(Guid sessionId, string qrCode);

   }
}
