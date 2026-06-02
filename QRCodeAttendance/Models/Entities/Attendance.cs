using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.Entities
{
    public class Attendance : BaseEntity
    {
        public Guid StudentId {get; set;}
        public Guid SessionId {get; set;}
        public DateTime ScanTime {get; set;}
        public DateTime? FirstScanTime {get; set;}
        public DateTime? SecondScanTime {get; set;}
        public required string StudentName {get; set;}
        public required string CourseName {get; set;}
        public required string CourseCode {get; set;}
        public AttendanceStatus Status {get; set;}
        public Session? ClassSession {get; set;}
        public Student? Student {get; set;}
        // public virtual Instructor Instructor {get; set;}
    }
}
