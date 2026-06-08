using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IInvitationRepository : IBaseRepository
    {
        Task<bool> HasPendingRequestForWhatsApp(string whatsAppNumber);
        Task<bool> HasActiveInvitationForWhatsApp(string whatsAppNumber);
        Task<bool> InvitationCodeExists(string invitationCode);
        Task<Invitation?> GetApprovedByCode(string invitationCode);
        Task<Invitation?> GetById(Guid id);
        Task<IReadOnlyList<Invitation>> GetPendingRequests();
        Task<IReadOnlyList<Invitation>> GetRecentInvitations(int count);
    }
}
