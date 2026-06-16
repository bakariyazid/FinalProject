using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IInvitationRepository : IBaseRepository
    {
        Task<bool> HasActiveInvitationForEmail(string email);
        Task<bool> InvitationCodeHashExists(string invitationCodeHash);
        Task<Invitation?> GetApprovedByEmailAndCodeHash(string email, string invitationCodeHash);
        Task<Invitation?> GetById(Guid id);
        Task<IReadOnlyList<Invitation>> GetRecentInvitations(int count);
    }
}
