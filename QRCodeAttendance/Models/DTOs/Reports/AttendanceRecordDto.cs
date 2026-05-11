using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Reports
{
    public class AttendanceRecordDto
        {
            public required string StudentName { get; set; }
            public required string StudentEmail { get; set; }
            public DateTime SessionDate { get; set; }
            public DateTime? ScanTime { get; set; }
            public string? Status { get; set; } 
        }
}