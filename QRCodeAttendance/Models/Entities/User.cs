using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using QRCodeAttendance.Contract.Entities;


namespace QRCodeAttendance.Models.Entities
{
    public class User : BaseEntity
    {
        public  string UserName { get; set; }
        public  string Email { get; set; }
        public  string PasswordHash { get; set; }
        public Guid RoleId {get; set;}
        public Role? Role { get; set; }
        public Student? Student { get; set; }
        public Instructor? Instructor { get; set; }

    }
}