using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Models.DTOs.User
{
    public class LoginRequestModel
    {
        [EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        public required string Password { get; set; }
    }
    public class LoginResponseModel : BaseResponse
    {
        public Guid UserId { get; set; }
        public Guid? InstructorId { get; set; }
        public required string Email { get; set; }   
        public required string Role { get; set; }
        public required string FirstName { get; set; }
        public required string FullName { get; set; }        

    }
}