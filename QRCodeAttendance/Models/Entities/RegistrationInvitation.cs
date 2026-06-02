using System;
using System.ComponentModel.DataAnnotations;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.Entities
{
    public class RegistrationInvitation : BaseEntity
    {
        [Required]
        [EmailAddress]
        public string InstructorEmail { get; set; } = null!;

        public string? InvitationCode { get; set; } 

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string WhatsAppNumber { get; set; } = string.Empty;

        [Required]
        public string InstructorIdentifier { get; set; } = string.Empty;

        public Departments? Department { get; set; }

        [Required]
        public InstructorInvitationStatus Status { get; set; } = InstructorInvitationStatus.Manual;

        public DateTime? ReviewedAt { get; set; }

        public string? RejectionReason { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }
    }
}
