using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Models.DTOs.Reports
{
    public class CourseReportDto
        {
            public string? CourseName { get; set; }
            public string? CourseCode { get; set; }
            public Guid? SessionId { get; set; }
            public Guid InstructorId { get; set; }
            public string? InstructorName { get; set; }
            public DateTime? SessionStartTime { get; set; }
            public DateTime? SessionEndTime { get; set; }
            public DateTime? ReportAvailableFrom { get; set; }
            public bool IsReportAvailable { get; set; } = true;
            public string? Message { get; set; }
            public int TotalSessions { get; set; }
            public int TotalPresent { get; set; }
            public int TotalLate { get; set; }
            public int TotalAbsent { get; set; }
            public double AverageAttendancePercentage { get; set; }
            public List<DailyStatDto> DailyStats { get; set; } = new();
            public List<StudentRiskDto> AtRiskStudents { get; set; } = new();
            public List<AttendanceRecordDto> AttendanceRecords { get; set; } = new();
        }
    
}
