using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.Entities
{
    public class Instructor : BaseUser
    {
        public Guid UserId {get; set;}
        public Departments Department {get; set;}
        public required string PasswordHash {get; set;}
        public virtual User? User {get; set;}
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

       
    }
}