using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Session
{
    public class SessionDto
    {
        public Guid Id {get; set;}
        public Guid InstructorId {get; set;}
        public required string CourseName {get; set;}
        public required string CourseCode {get; set;}
        public Departments Department {get; set;}
        public StudentLevel Level { get; set; }
        public DateTime SessionStartTime {get; set;}
        public DateTime SessionEndTime {get; set;}
        public bool IsActive {get; set;}
        public string? QRCodeToken { get; set; } 
        public DateTime QRCodeExpiry { get; set; }
        public DateTime CreatedDate {get; set;}
        public DateTime UpdatedDate {get; set;}
        
    }
}
