using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Models.DTOs.Reports
{
    public class StudentRiskDto
    {
        public string? Name { get; set; }
        public string? RegNumber { get; set; }
        public double AttendanceRate { get; set; }
    }
}