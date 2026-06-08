using System.ComponentModel.DataAnnotations;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Admin
{
    public class CreateInstructorInvitationRequestModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Instructor email")]
        public string InstructorEmail { get; set; } = string.Empty;
    }

    public class InstructorInvitationDto
    {
        public Guid Id { get; set; }
        public string InstructorEmail { get; set; } = string.Empty;
        public string InvitationCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public Departments? Department { get; set; }
        public InstructorInvitationStatus Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }

    public class AdminInvitationDashboardDto
    {
        public CreateInstructorInvitationRequestModel Form { get; set; } = new();
        public List<InstructorInvitationDto> PendingRequests { get; set; } = new();
        public List<InstructorInvitationDto> RecentInvitations { get; set; } = new();
    }

    public class CreateInstructorAccessRequestModel
    {
        [Required]
        [StringLength(120, MinimumLength = 4)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Instructor email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "WhatsApp number")]
        public string WhatsAppNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department")]
        public Departments Department { get; set; }
    }
}
