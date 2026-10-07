using System.ComponentModel.DataAnnotations;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Admin
{
    public class AdminProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class UpdateAdminProfileRequestModel : AdminProfileDto
    {
        [Required, EmailAddress]
        public new string Email { get; set; } = string.Empty;

        [Required]
        public new string FirstName { get; set; } = string.Empty;

        [Required]
        public new string LastName { get; set; } = string.Empty;
    }
}
