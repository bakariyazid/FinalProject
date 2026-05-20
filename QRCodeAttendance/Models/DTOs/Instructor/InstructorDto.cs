using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.InstructorDto;

public class InstructorDto 
{
        public Guid InstructorId {get; set;}
        public Guid UserId {get; set;}
        public Guid RoleId {get; set;}
        public string FirstName {get; set;} = string.Empty;
        public string LastName {get; set;} = string.Empty;
        public string PhoneNumber {get; set;} = string.Empty;
        public string Address {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        public Gender Gender {get; set;}
        public DateTime DateOfBirth {get; set;}
        public string PasswordHash {get; set;} = string.Empty;
        public string ConfirmPassword {get; set;} = string.Empty;
        public string FullName {get; set;} = string.Empty;
        public Departments Department {get; set;}
        public DateTime CreatedDate {get; set;}
        public DateTime UpdatedDate {get; set;}

}
