using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.Entities
{
    public class Student : BaseUser
    {
        public Guid UserId {get; set;}
        public required string MatricNumber {get; set;}
        public StudentLevel StudentLevel {get; set;}
        public Departments Department {get; set;}
        public required string PasswordHash {get; set;}
        public virtual User? User {get; set;}
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();


    }
}