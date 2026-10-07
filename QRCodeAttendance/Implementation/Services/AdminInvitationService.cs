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
        private const int OtpExpiryMinutes = 10;
        private const int MaximumOtpAttempts = 5;
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

        public async Task<BaseResponse<InstructorInvitationDto>> RequestInstructorEmailVerification(CreateInstructorInvitationRequestModel request)
        {
            var email = NormalizeEmail(request.InstructorEmail);

            // First gate: reject empty / clearly invalid addresses before any DB write or SMTP call
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
            {
                return new BaseResponse<InstructorInvitationDto>
                {
                    Status = false,
                    Message = "Please enter a valid instructor email address."
                };
            }

            try
            {
                // Strict format check (same family of rules as System.Net.Mail)
                _ = new System.Net.Mail.MailAddress(email);
            }
            catch
            {
                return new BaseResponse<InstructorInvitationDto>
                {
                    Status = false,
                    Message = "Please enter a valid instructor email address."
                };
            }

            if (await _invitationRepository.HasActiveInvitationForEmail(email))
            {
                return new BaseResponse<InstructorInvitationDto>
                {
                    Status = false,
                    Message = "This email already has a pending verification or active registration code."
                };
            }

            var otp = GenerateOtp();
            var otpExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
            var invitation = new Invitation
            {
                InstructorEmail = email,
                VerificationOtpHash = HashOtp(email, otp),
                OtpExpiresAt = otpExpiresAt,
                Status = InstructorInvitationStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                // Keep ExpiryDate aligned with OTP window until email is verified
                ExpiryDate = otpExpiresAt,
                IsUsed = false
            };

            await _invitationRepository.Add(invitation);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _emailService.SendInstructorEmailVerificationCodeAsync(email, otp, invitation.OtpExpiresAt.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver instructor verification code to {InstructorEmail}", email);
                invitation.Status = InstructorInvitationStatus.Rejected;
                invitation.RejectionReason = ex.Message;
                invitation.UpdatedDate = DateTime.UtcNow;
                _invitationRepository.Update(invitation);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<InstructorInvitationDto>
                {
                    Status = false,
                    Message = "The verification code could not be delivered. Please check the email address and try again.",
                    Data = ToInvitationDto(invitation)
                };
            }

            return new BaseResponse<InstructorInvitationDto>
            {
                Status = true,
                Message = $"A six-digit verification code was sent to {email}. Ask the instructor for the code before issuing their registration code.",
                Data = ToInvitationDto(invitation)
            };
        }

        public async Task<BaseResponse<InstructorInvitationDto>> VerifyInstructorEmail(VerifyInstructorEmailRequestModel request)
        {
            var email = NormalizeEmail(request.InstructorEmail);
            var invitation = await _invitationRepository.GetPendingVerificationByEmail(email);

            if (invitation == null || invitation.OtpExpiresAt <= DateTime.UtcNow)
            {
                return new BaseResponse<InstructorInvitationDto> { Status = false, Message = "The verification code is invalid or has expired. Request a new code." };
            }

            if (invitation.OtpFailedAttempts >= MaximumOtpAttempts)
            {
                return new BaseResponse<InstructorInvitationDto> { Status = false, Message = "Too many incorrect attempts. Request a new verification code." };
            }

            if (invitation.VerificationOtpHash == null || !CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(invitation.VerificationOtpHash),
                    Convert.FromHexString(HashOtp(email, request.VerificationCode))))
            {
                invitation.OtpFailedAttempts++;
                invitation.UpdatedDate = DateTime.UtcNow;
                _invitationRepository.Update(invitation);
                await _unitOfWork.SaveChangesAsync();
                return new BaseResponse<InstructorInvitationDto> { Status = false, Message = "The verification code is incorrect." };
            }

            var code = await GenerateUniqueCode(email);
            invitation.IsEmailVerified = true;
            invitation.VerificationOtpHash = null;
            invitation.OtpExpiresAt = null;
            invitation.InvitationCodeHash = HashInvitationCode(email, code);
            invitation.ExpiryDate = DateTime.UtcNow.AddHours(CodeExpiryHours);
            invitation.Status = InstructorInvitationStatus.Approved;
            invitation.UpdatedDate = DateTime.UtcNow;
            _invitationRepository.Update(invitation);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _emailService.SendInstructorInvitationCodeAsync(email, code, invitation.ExpiryDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver verified instructor invitation to {InstructorEmail}", email);
                invitation.Status = InstructorInvitationStatus.Rejected;
                invitation.UpdatedDate = DateTime.UtcNow;
                _invitationRepository.Update(invitation);
                await _unitOfWork.SaveChangesAsync();
                return new BaseResponse<InstructorInvitationDto> { Status = false, Message = "Email verification succeeded, but the registration code could not be delivered. Request a new verification code.", Data = ToInvitationDto(invitation) };
            }

            return new BaseResponse<InstructorInvitationDto> { Status = true, Message = $"Email verified. The registration code was sent to {email} and expires after 24 hours.", Data = ToInvitationDto(invitation) };
        }

        public async Task<BaseResponse> DeleteInstructorInvitation(Guid invitationId)
        {
            var invitation = await _invitationRepository.GetById(invitationId);
            if (invitation == null)
            {
                return new BaseResponse
                {
                    Status = false,
                    Message = "The invitation could not be found."
                };
            }

            await _invitationRepository.Delete(invitation);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Status = true,
                Message = "Instructor invitation deleted successfully."
            };
        }

        public static string HashInvitationCode(string email, string code)
        {
            var normalized = NormalizeCode(code);
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToHexString(hashBytes);
        }

        private static string HashOtp(string email, string otp)
        {
            var normalized = NormalizeEmail(email) + ":" + otp.Trim();
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
        }

        private static string GenerateOtp() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

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
                UsedAt = invitation.UsedAt,
                IsEmailVerified = invitation.IsEmailVerified,
                OtpExpiresAt = invitation.OtpExpiresAt
            };
        }
    }
}
