using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Admin;

namespace QRCodeAttendance.Interface.Services
{
    public interface IAdminInvitationService
    {
        Task<BaseResponse<AdminInvitationDashboardDto>> GetInvitationDashboard();
        Task<BaseResponse<bool>> SubmitInstructorAccessRequest(CreateInstructorAccessRequestModel request);
        Task<BaseResponse<InstructorInvitationDto>> GenerateInstructorInvitation(CreateInstructorInvitationRequestModel request);
        Task<BaseResponse<InstructorInvitationDto>> ApproveInstructorRequest(Guid requestId);
        Task<BaseResponse<bool>> RejectInstructorRequest(Guid requestId, string? reason);
    }
}
