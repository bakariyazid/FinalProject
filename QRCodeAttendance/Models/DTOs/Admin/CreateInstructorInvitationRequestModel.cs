using System.ComponentModel.DataAnnotations;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Admin
{
    public class CreateInstructorInvitationRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Instructor email")]
        public string InstructorEmail { get; set; } = string.Empty;
    }

    public class VerifyInstructorEmailRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string InstructorEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the six-digit verification code.")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "Enter the six-digit verification code.")]
        public string VerificationCode { get; set; } = string.Empty;
    }

    public class InstructorInvitationDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string InstructorEmail { get; set; } = string.Empty;
        public InstructorInvitationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
    }

    public class AdminInvitationDashboardDto
    {
        public CreateInstructorInvitationRequestModel Form { get; set; } = new();
        public VerifyInstructorEmailRequestModel VerificationForm { get; set; } = new();
        public List<InstructorInvitationDto> RecentInvitations { get; set; } = new();
    }
}
