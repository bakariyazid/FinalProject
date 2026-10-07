using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Admin;

namespace QRCodeAttendance.Interface.Services
{
    public interface IAdminInvitationService
    {
        Task<BaseResponse<AdminInvitationDashboardDto>> GetInvitationDashboard();
        Task<BaseResponse<InstructorInvitationDto>> RequestInstructorEmailVerification(CreateInstructorInvitationRequestModel request);
        Task<BaseResponse<InstructorInvitationDto>> VerifyInstructorEmail(VerifyInstructorEmailRequestModel request);
        Task<BaseResponse> DeleteInstructorInvitation(Guid invitationId);
    }
}
