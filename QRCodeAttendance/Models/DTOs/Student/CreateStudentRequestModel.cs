using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Student;


public class CreateStudentRequestModel
{
        [Required]
        [MaxLength(55)]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(120)]
        public required string LastName { get; set; }

        [Required]
        [MaxLength(20)]
        public required string MatricNumber {get; set;}

        [Required]
        public DateTime DateOfBirth { get; set; } 

        [Required]
        [MaxLength(50)]
        public required string Address { get; set; }

        [Required]
        [MaxLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public StudentLevel StudentLevel {get; set;}

        [Required]
        public Departments Department {get; set;}

        [Required]
        [MaxLength(30)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MaxLength(30)]
        public required string EmailConfirmed { get; set; }
       
        [Required]
        [MaxLength(12)]
        public required string PasswordHash { get; set; }  

        [Required]  
        [MaxLength(12)]
        public required string ConfirmPassword { get; set; }
    
}

public class UpdateStudentRequestModel
{

        [MaxLength(55)]
        public string? FirstName { get; set; }

        [MaxLength(120)]
        public string? LastName { get; set; }

        [MaxLength(20)]
        public string? MatricNumber {get; set;}

        public DateTime DateOfBirth { get; set; } 

        [MaxLength(50)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public Gender Gender   { get; set; }   

        public StudentLevel StudentLevel {get; set;}

        public Departments Department {get; set;}

        [MaxLength(30)]
        public string? Email { get; set; }

}