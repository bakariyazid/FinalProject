using System.Security.Cryptography;
using System.Text;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Admin;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Implementation.Services
{
    public class AdminInvitationService : IAdminInvitationService
    {
        private const int RecentInvitationLimit = 25;
        private const int CodeExpiryHours = 24;
        private const string CodeLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string CodeDigits = "23456789";
        private const string CodeCharacters = CodeLetters + CodeDigits;

        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminInvitationService> _logger;

        public AdminInvitationService(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<AdminInvitationService> logger)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<BaseResponse<AdminInvitationDashboardDto>> GetInvitationDashboard()
        {
            var invitations = await _invitationRepository.GetRecentInvitations(RecentInvitationLimit);

            return new BaseResponse<AdminInvitationDashboardDto>
            {
                Status = true,
                Message = "Invitation dashboard loaded successfully.",
                Data = new AdminInvitationDashboardDto
                {
                    RecentInvitations = invitations.Select(ToInvitationDto).ToList()
                }
            };
        }

        public async Task<BaseResponse<InstructorInvitationDto>> GenerateInstructorInvitation(CreateInstructorInvitationRequestModel request)
        {
            var email = NormalizeEmail(request.InstructorEmail);

            if (await _invitationRepository.HasActiveInvitationForEmail(email))
            {
                return new BaseResponse<InstructorInvitationDto>
                {
                    Status = false,
                    Message = "This instructor already has an active registration code. Wait until it expires or the instructor uses it."
                };
            }

            var code = await GenerateUniqueCode(email);
            var invitation = new Invitation
            {
                InstructorEmail = email,
                InvitationCodeHash = HashInvitationCode(email, code),
                Status = InstructorInvitationStatus.Approved,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddHours(CodeExpiryHours),
                IsUsed = false
            };

            await _invitationRepository.Add(invitation);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _emailService.SendInstructorInvitationCodeAsync(email, code, invitation.ExpiryDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver instructor invitation code to {InstructorEmail}", email);
                invitation.Status = InstructorInvitationStatus.Rejected;
                invitation.RejectionReason = ex.Message;
                invitation.UpdatedDate = DateTime.UtcNow;
                _invitationRepository.Update(invitation);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<InstructorInvitationDto>
                {
                    Status = false,
                    Message = $"The code was generated but the email could not be delivered: {ex.Message}",
                    Data = ToInvitationDto(invitation)
                };
            }

            return new BaseResponse<InstructorInvitationDto>
            {
                Status = true,
                Message = $"Instructor code sent to {email}. The code is hidden from admins and expires after 24 hours.",
                Data = ToInvitationDto(invitation)
            };
        }

        public static string HashInvitationCode(string email, string code)
        {
            var normalized = NormalizeCode(code);
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToHexString(hashBytes);
        }

        private async Task<string> GenerateUniqueCode(string email)
        {
            string code;
            string codeHash;

            do
            {
                code = $"INS-{GenerateCodeSegment(8)}";
                codeHash = HashInvitationCode(email, code);
            }
            while (await _invitationRepository.InvitationCodeHashExists(codeHash));

            return code;
        }

        private static string GenerateCodeSegment(int length)
        {
            if (length < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "A code must contain at least two characters.");
            }

            var code = new char[length];
            code[0] = CodeLetters[RandomNumberGenerator.GetInt32(CodeLetters.Length)];
            code[1] = CodeDigits[RandomNumberGenerator.GetInt32(CodeDigits.Length)];

            for (var i = 2; i < code.Length; i++)
            {
                code[i] = CodeCharacters[RandomNumberGenerator.GetInt32(CodeCharacters.Length)];
            }

            for (var i = code.Length - 1; i > 0; i--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(i + 1);
                (code[i], code[swapIndex]) = (code[swapIndex], code[i]);
            }

            return new string(code);
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpperInvariant();
        }

        private static InstructorInvitationDto ToInvitationDto(Invitation invitation)
        {
            return new InstructorInvitationDto
            {
                Id = invitation.Id,
                InstructorEmail = invitation.InstructorEmail,
                Status = invitation.Status,
                RejectionReason = invitation.RejectionReason,
                CreatedDate = invitation.CreatedDate,
                ExpiryDate = invitation.ExpiryDate,
                IsUsed = invitation.IsUsed,
                UsedAt = invitation.UsedAt
            };
        }
    }
}
