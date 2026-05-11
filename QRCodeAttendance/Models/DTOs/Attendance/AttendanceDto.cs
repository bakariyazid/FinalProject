using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Attendance
{
    public class AttendanceDto
    {
        public Guid Id {get; set;}
        public Guid StudentId {get; set;}
        public Guid SessionId {get; set;}
        public string CourseName {get; set;}
        public string CourseCode {get; set;}
        public DateTime ScanTime {get; set;}
        public AttendanceStatus Status {get; set;}
        public string StudentName {get; set;}
        public DateTime CreatedDate {get; set;}
        public DateTime UpdatedDate {get; set;}
    
    }
}