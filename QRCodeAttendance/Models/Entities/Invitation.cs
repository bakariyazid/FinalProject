using System;
using System.ComponentModel.DataAnnotations;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.Entities
{
    public class Invitation : BaseEntity
    {
        [Required]
        [EmailAddress]
        public string InstructorEmail { get; set; } = null!;

        [Required]
        public string InvitationCodeHash { get; set; } = null!;

        public string? RejectionReason { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        [Required]
        public InstructorInvitationStatus Status { get; set; } = InstructorInvitationStatus.Approved;
    }
}
