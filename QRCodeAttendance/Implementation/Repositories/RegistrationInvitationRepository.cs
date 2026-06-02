using Microsoft.EntityFrameworkCore;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;
using QRCodeAttendance.Persistence.QRCodeAttendanceDb;

namespace QRCodeAttendance.Implementation.Repositories
{
    public class RegistrationInvitationRepository : BaseRepository, IRegistrationInvitationRepository
    {
        public RegistrationInvitationRepository(QRCodeDbContext qrCodeDbContext) : base(qrCodeDbContext)
        {
        }

        public async Task<bool> HasActiveInvitation(string instructorEmail)
        {
            var email = instructorEmail.Trim().ToLowerInvariant();

            return await _qrCodeDbContext.RegistrationInvitations
                .AnyAsync(i => i.InstructorEmail.ToLower() == email && !i.IsUsed && i.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<bool> HasPendingRequest(string instructorEmail)
        {
            var email = instructorEmail.Trim().ToLowerInvariant();
            return await _qrCodeDbContext.RegistrationInvitations
                .AnyAsync(i => i.InstructorEmail.ToLower() == email && i.Status == InstructorInvitationStatus.Pending);
        }

        public async Task<bool> InvitationCodeExists(string invitationCode)
        {
            return await _qrCodeDbContext.RegistrationInvitations
                .AnyAsync(i => i.InvitationCode == invitationCode);
        }

        public async Task<RegistrationInvitation?> GetById(Guid id)
        {
            return await _qrCodeDbContext.RegistrationInvitations.FindAsync(id);
        }

        public async Task<IReadOnlyList<RegistrationInvitation>> GetPendingRequests()
        {
            return await _qrCodeDbContext.RegistrationInvitations
                .Where(i => i.Status == InstructorInvitationStatus.Pending)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<RegistrationInvitation>> GetRecentInvitations(int count)
        {
            return await _qrCodeDbContext.RegistrationInvitations
                .OrderByDescending(i => i.CreatedDate)
                .Take(count)
                .ToListAsync();
        }
    }
}
