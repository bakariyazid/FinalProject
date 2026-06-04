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

        public async Task<bool> HasPendingRequestForWhatsApp(string whatsAppNumber)
        {
            var normalizedNumber = NormalizeWhatsAppNumber(whatsAppNumber);
            var invitations = await _qrCodeDbContext.RegistrationInvitations
                .Where(i => i.Status == InstructorInvitationStatus.Pending)
                .ToListAsync();

            return invitations.Any(i => NormalizeWhatsAppNumber(i.WhatsAppNumber) == normalizedNumber);
        }

        public async Task<bool> HasActiveInvitationForWhatsApp(string whatsAppNumber)
        {
            var normalizedNumber = NormalizeWhatsAppNumber(whatsAppNumber);
            var invitations = await _qrCodeDbContext.RegistrationInvitations
                .Where(i =>
                    i.Status == InstructorInvitationStatus.Approved &&
                    !i.IsUsed &&
                    i.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            return invitations.Any(i => NormalizeWhatsAppNumber(i.WhatsAppNumber) == normalizedNumber);
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

        private static string NormalizeWhatsAppNumber(string value)
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

            if (digits.StartsWith("0") && digits.Length == 11)
            {
                return $"234{digits[1..]}";
            }

            if (digits.Length == 10)
            {
                return $"234{digits}";
            }

            return digits;
        }
    }
}
