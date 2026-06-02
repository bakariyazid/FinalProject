using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Instructor;

public class CreateInstructorRequestModel
{       
        public Guid RoleId {get; set;}

        [Required]
        public required string  FirstName { get; set; }
        
        [Required]
        public required string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; } 

        [Required]
        public required string Address { get; set; }

        [Required]
        public required string PhoneNumber { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public Departments Department {get; set;}

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string InvitationCode { get; set; }

        [Required]
        public required string EmailConfirmed { get; set; }
       
        [Required]
        public required string PasswordHash { get; set; }  

        [Required]  
        public required string ConfirmPassword { get; set; }

}


public class UpdateInstructorRequestModel
{
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateOfBirth { get; set; } 
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; } 
        public Gender Gender { get; set; }
        public Departments Department {get; set;}
        public string? Email { get; set; }   
}
