using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Student
{
    public class StudentAttendanceReportDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string MatricNumber { get; set; } = string.Empty;
        public Departments Department { get; set; }
        public StudentLevel Level { get; set; }
        public int TotalAttended { get; set; }
        public List<StudentAttendanceReportItemDto> Records { get; set; } = new();
    }

    public class   StudentAttendanceReportItemDto
    {
        public Guid AttendanceId { get; set; }
        public Guid SessionId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string MatricNumber { get; set; } = string.Empty;
        public Departments Department { get; set; }
        public StudentLevel Level { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime SessionStartTime { get; set; }
        public DateTime SessionEndTime { get; set; }
        public DateTime ScanTime { get; set; }
        public DateTime? FirstScanTime { get; set; }
        public DateTime? SecondScanTime { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
