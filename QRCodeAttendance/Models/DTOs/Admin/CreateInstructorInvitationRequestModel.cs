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
        public InstructorInvitationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }

    public class AdminInvitationDashboardDto
    {
        public CreateInstructorInvitationRequestModel Form { get; set; } = new();
        public List<InstructorInvitationDto> RecentInvitations { get; set; } = new();
    }
}
