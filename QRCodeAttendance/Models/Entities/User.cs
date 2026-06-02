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
        public bool EmailConfirmed { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Guid RoleId {get; set;}
        public Role? Role { get; set; }
        public Student? Student { get; set; }
        public Instructor? Instructor { get; set; }

    }
}
