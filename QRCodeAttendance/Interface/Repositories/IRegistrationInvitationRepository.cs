using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IRegistrationInvitationRepository : IBaseRepository
    {
        Task<bool> HasActiveInvitation(string instructorEmail);
        Task<bool> HasPendingRequest(string instructorEmail);
        Task<bool> InvitationCodeExists(string invitationCode);
        Task<RegistrationInvitation?> GetById(Guid id);
        Task<IReadOnlyList<RegistrationInvitation>> GetPendingRequests();
        Task<IReadOnlyList<RegistrationInvitation>> GetRecentInvitations(int count);
    }
}
