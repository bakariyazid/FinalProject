using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs.Attendance;
using QRCodeAttendance.Models.DTOs.Session;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs
{
    public class StudentDashboardDto
    {
        public required string UserName { get; set; }
        public required string MatricNumber { get; set; }
        public required Departments Department { get; set; }
        public required StudentLevel Level { get; set; }
        public int TotalSessionsAttended { get; set; }
        public int TotalSessionsAvailable { get; set; }
        public double AttendancePercentage { get; set; }
        public List<AttendanceDto>? RecentAttendances { get; set; }
        public List<ActiveSessionDto> ActiveSessions { get; set; } = new();
        public List<MissedSessionDto> MissedSessions { get; set; } = new();
    }

    public class ActiveSessionDto
    {
        public Guid Id { get; set; } 
        public required string CourseName { get; set; }
        public required string CourseCode { get; set; }
        public required StudentLevel Level {get; set;}
        public required Departments Department { get; set; }
        public DateTime SessionStartTime { get; set; } 
        public bool IsActive { get; set; }
        public required DateTime SessionEndTime { get; set; }
        public required string InstructorName { get; set; }
        public AttendanceStatus? AttendanceStatus { get; set; }
        public DateTime? FirstScanTime { get; set; }
        public DateTime? SecondScanTime { get; set; }
    }
        public class MissedSessionDto {
            public required string CourseName { get; set; }
            public required string CourseCode { get; set; }
            public required StudentLevel Level {get; set;}
            public required Departments Department { get; set; }
            public required DateTime StartTime { get; set; }
            public required DateTime EndTime { get; set; }
            public required string InstructorName { get; set; }
        }
}
