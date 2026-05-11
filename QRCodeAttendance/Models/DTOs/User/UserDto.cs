using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.DTOs.Role;

namespace QRCodeAttendance.Models.DTOs.User
{
    public class UserDto 
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public  Guid RoleId {get; set;}
        public  Guid Id { get; set; }        
        public  DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
       
    }
}