using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs.Attendance;

namespace QRCodeAttendance.Models.Entities
{
    public class StudentDashboard
    {
        public required string UserName { get; set; }
        public required string MatricNumber { get; set; }
        public int TotalSessionsAttended { get; set; }
        public int TotalSessionsAvailable { get; set; }
        public List<AttendanceDto>? RecentAttendances { get; set; }
    }
}