using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs.Session;

namespace QRCodeAttendance.Models.DTOs
{
    public class InstructorDashboardDto
    {
        public int TotalSessions { get; set; }
        public int TotalAttendances { get; set; }
        public List<SessionDto>? RecentSessions { get; set; } = new List<SessionDto>();
        public List<int>? AttendanceStats { get; set; } = new();
        
    }
}