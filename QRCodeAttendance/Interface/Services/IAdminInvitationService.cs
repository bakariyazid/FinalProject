using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Admin;

namespace QRCodeAttendance.Interface.Services
{
    public interface IAdminInvitationService
    {
        Task<BaseResponse<AdminInvitationDashboardDto>> GetInvitationDashboard();
        Task<BaseResponse<InstructorInvitationDto>> GenerateInstructorInvitation(CreateInstructorInvitationRequestModel request);
    }
}
