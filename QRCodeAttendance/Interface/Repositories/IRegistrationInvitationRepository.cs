using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IRegistrationInvitationRepository : IBaseRepository
    {
        Task<bool> HasPendingRequestForWhatsApp(string whatsAppNumber);
        Task<bool> HasActiveInvitationForWhatsApp(string whatsAppNumber);
        Task<bool> InvitationCodeExists(string invitationCode);
        Task<RegistrationInvitation?> GetById(Guid id);
        Task<IReadOnlyList<RegistrationInvitation>> GetPendingRequests();
        Task<IReadOnlyList<RegistrationInvitation>> GetRecentInvitations(int count);
    }
}
