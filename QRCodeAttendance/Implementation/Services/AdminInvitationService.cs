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
        private const int RequestExpiryDays = 7;
        private const int ApprovedCodeExpiryDays = 1;
        private const string DefaultRejectionReason = "The applicant could not be verified as an instructor.";

        private readonly IRegistrationInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminInvitationService(
            IRegistrationInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<AdminInvitationDashboardDto>> GetInvitationDashboard()
        {
            var invitations = await _invitationRepository.GetRecentInvitations(RecentInvitationLimit);
            var pendingRequests = await _invitationRepository.GetPendingRequests();

            return new BaseResponse<AdminInvitationDashboardDto>
            {
                Status = true,
                Message = "Invitation dashboard loaded successfully.",
                Data = new AdminInvitationDashboardDto
                {
                    PendingRequests = pendingRequests.Select(ToInvitationDto).ToList(),
                    RecentInvitations = invitations
                        .Where(i => i.Status != InstructorInvitationStatus.Pending)
                        .Select(ToInvitationDto)
                        .ToList()
                }
            };
        }

        public async Task<BaseResponse<bool>> SubmitInstructorAccessRequest(CreateInstructorAccessRequestModel request)
        {
            var email = NormalizeEmail(request.Email);

            if (await _invitationRepository.HasPendingRequest(email))
            {
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "You already have an instructor access request waiting for admin review."
                };
            }

            if (await _invitationRepository.HasActiveInvitation(email))
            {
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "You already have an active instructor invitation code. Check your email or contact the admin."
                };
            }

            var requestEntity = new RegistrationInvitation
            {
                InstructorEmail = email,
                FullName = request.FullName.Trim(),
                WhatsAppNumber = request.WhatsAppNumber.Trim(),
                Department = request.Department,
                Status = InstructorInvitationStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(RequestExpiryDays),
                IsUsed = false
            };

            await _invitationRepository.Add(requestEntity);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Status = true,
                Message = "Your instructor access request has been submitted. The admin will review it and share the next step through WhatsApp."
            };
        }

        public async Task<BaseResponse<InstructorInvitationDto>> GenerateInstructorInvitation(CreateInstructorInvitationRequestModel request)
        {
            var email = NormalizeEmail(request.InstructorEmail);

            if (await _invitationRepository.HasActiveInvitation(email))
            {
                return new BaseResponse<InstructorInvitationDto>
                {
                    Status = false,
                    Message = "This instructor already has an active invitation code."
                };
            }

            var invitation = new RegistrationInvitation
            {
                InstructorEmail = email,
                InvitationCode = await GenerateUniqueCode(),
                Status = InstructorInvitationStatus.Approved,
                ReviewedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(ApprovedCodeExpiryDays),
                IsUsed = false
            };

            await _invitationRepository.Add(invitation);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<InstructorInvitationDto>
            {
                Status = true,
                Message = $"Invitation code generated for {email}: {invitation.InvitationCode}",
                Data = ToInvitationDto(invitation)
            };
        }

        public async Task<BaseResponse<InstructorInvitationDto>> ApproveInstructorRequest(Guid requestId)
        {
            var request = await _invitationRepository.GetById(requestId);
            if (request == null || request.Status != InstructorInvitationStatus.Pending)
            {
                return new BaseResponse<InstructorInvitationDto>
                {
                    Status = false,
                    Message = "Instructor request was not found or has already been reviewed."
                };
            }

            request.InvitationCode = await GenerateUniqueCode();
            request.Status = InstructorInvitationStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.ExpiryDate = DateTime.UtcNow.AddDays(ApprovedCodeExpiryDays);
            request.UpdatedDate = DateTime.UtcNow;

            _invitationRepository.Update(request);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<InstructorInvitationDto>
            {
                Status = true,
                Message = $"Request approved. Click WhatsApp to send the invitation code to {request.FullName}.",
                Data = ToInvitationDto(request)
            };
        }

        public async Task<BaseResponse<bool>> RejectInstructorRequest(Guid requestId, string? reason)
        {
            var request = await _invitationRepository.GetById(requestId);
            if (request == null || request.Status != InstructorInvitationStatus.Pending)
            {
                return new BaseResponse<bool>
                {
                    Status = false,
                    Message = "Instructor request was not found or has already been reviewed."
                };
            }

            request.Status = InstructorInvitationStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.RejectionReason = string.IsNullOrWhiteSpace(reason)
                ? DefaultRejectionReason
                : reason.Trim();
            request.UpdatedDate = DateTime.UtcNow;

            _invitationRepository.Update(request);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Status = true,
                Message = $"Request rejected for {request.InstructorEmail}."
            };
        }

        private async Task<string> GenerateUniqueCode()
        {
            string code;

            do
            {
                code = $"INS-{Random.Shared.Next(100000, 999999)}";
            }
            while (await _invitationRepository.InvitationCodeExists(code));

            return code;
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static string DisplayName(RegistrationInvitation invitation)
        {
            return string.IsNullOrWhiteSpace(invitation.FullName) ? invitation.InstructorEmail : invitation.FullName;
        }

        private static InstructorInvitationDto ToInvitationDto(RegistrationInvitation invitation)
        {
            return new InstructorInvitationDto
            {
                Id = invitation.Id,
                InstructorEmail = invitation.InstructorEmail,
                InvitationCode = invitation.InvitationCode ?? string.Empty,
                FullName = invitation.FullName,
                WhatsAppNumber = invitation.WhatsAppNumber,
                Department = invitation.Department,
                Status = invitation.Status,
                ReviewedAt = invitation.ReviewedAt,
                RejectionReason = invitation.RejectionReason,
                CreatedDate = invitation.CreatedDate,
                ExpiryDate = invitation.ExpiryDate,
                IsUsed = invitation.IsUsed,
                UsedAt = invitation.UsedAt
            };
        }
    }
}
