using Microsoft.EntityFrameworkCore;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;
using QRCodeAttendance.Persistence.QRCodeAttendanceDb;

namespace QRCodeAttendance.Implementation.Repositories
{
    public class InvitationRepository : BaseRepository, IInvitationRepository
    {
        public InvitationRepository(QRCodeDbContext qrCodeDbContext) : base(qrCodeDbContext)
        {
        }

        public async Task<bool> HasActiveInvitationForEmail(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _qrCodeDbContext.Invitations
                .AnyAsync(i =>
                    i.InstructorEmail == normalizedEmail &&
                    i.Status == InstructorInvitationStatus.Approved &&
                    !i.IsUsed &&
                    i.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<bool> InvitationCodeHashExists(string invitationCodeHash)
        {
            return await _qrCodeDbContext.Invitations
                .AnyAsync(i => i.InvitationCodeHash == invitationCodeHash);
        }

        public async Task<Invitation?> GetApprovedByEmailAndCodeHash(string email, string invitationCodeHash)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _qrCodeDbContext.Invitations
                .FirstOrDefaultAsync(i =>
                    i.InstructorEmail == normalizedEmail &&
                    i.InvitationCodeHash == invitationCodeHash &&
                    i.Status == InstructorInvitationStatus.Approved);
        }

        public async Task<Invitation?> GetById(Guid id)
        {
            return await _qrCodeDbContext.Invitations.FindAsync(id);
        }

        public async Task<IReadOnlyList<Invitation>> GetRecentInvitations(int count)
        {
            return await _qrCodeDbContext.Invitations
                .OrderByDescending(i => i.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

    }
}
