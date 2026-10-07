using System;
using System.ComponentModel.DataAnnotations;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.Entities
{
    public class Invitation : BaseEntity
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string InstructorEmail { get; set; } = null!;

        // Set only after the instructor proves control of this inbox.
        public string? InvitationCodeHash { get; set; }

        // The OTP itself is never persisted; only its SHA-256 hash is stored.
        public string? VerificationOtpHash { get; set; }

        public DateTime? OtpExpiresAt { get; set; }

        public int OtpFailedAttempts { get; set; }

        public bool IsEmailVerified { get; set; }

        public string? RejectionReason { get; set; }

        // public string? RejectionReason { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        [Required]
        public InstructorInvitationStatus Status { get; set; } = InstructorInvitationStatus.Approved;
    }
}
