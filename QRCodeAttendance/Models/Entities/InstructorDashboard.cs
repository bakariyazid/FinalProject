using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs.Session;

namespace QRCodeAttendance.Models.Entities
{
    public class InstructorDashboard
    {
        public int TotalSessions { get; set; }
        public int TotalAttendances { get; set; }
        public List<SessionDto>? RecentSessions { get; set; }
    }
}