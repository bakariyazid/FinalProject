using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Student;
using QRCodeAttendance.Models.Enums;
using QRCodeAttendance.Models.Extensions;

namespace QRCodeAttendance.Controllers
{   
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;
        private readonly IAttendanceService _attendanceService;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger, IAttendanceService attendanceService)
        {
            _studentService = studentService;
            _logger = logger;
            _attendanceService = attendanceService;
        
        }

        // Register Student
          [HttpGet]
        public IActionResult RegisterStudent()
        {
            TempData.Remove("Alert");
            TempData.Remove("AlertType");
            return View();
        }

        
 public async Task<IActionResult> RegisterStudent(CreateStudentRequestModel model)
        {
            var response = await _studentService.RegisterStudent(model);

            if (response.Status)
            {
                TempData["Alert"] = "Student registered successfully!";
                TempData["AlertType"] = "success";

                return RedirectToAction("Login", "User");
            }
            else
            {
                TempData["Alert"] = response.Message;
                TempData["AlertType"] = "danger";

                return View(model); 
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> StudentDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var studentUserId))
            {
                return RedirectToAction("Login", "User");
            }

            _logger.LogInformation("Student {UserId} requested dashboard", userId);

            var response = await _studentService.GetDashboard(studentUserId);

            if (!response.Status)
            {
                _logger.LogWarning("Failed to load dashboard for {UserId}: {Message}", userId, response.Message);
                ViewBag.ErrorMessage = response.Message;
            }

            return View(response.Data ?? new StudentDashboardDto
            {
                UserName = User.Identity?.Name ?? "Student",
                MatricNumber = "N/A",
                Department = Departments.SoftwareDepartment,
                Level = StudentLevel.HundredLevel
            });
        }

            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // _logger.LogInformation("Student {UserId} requested profile", userId);

            // var response = await _studentService.GetStudentProfile(Guid.Parse(userId));

            // return View(response.Data);

        [HttpGet("Student/StdProfile")]
        public async Task<IActionResult> StdProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var studentUserId))
                {
                    return RedirectToAction("Login", "User");
                }

                _logger.LogInformation("Student {UserId} requested profile", userId);

                var response = await _studentService.GetStudentProfile(studentUserId);

                if (response == null || response.Data == null)
                {
                    _logger.LogWarning("Profile data for user {UserId} was not found.", userId);
                    return NotFound("Student profile not found.");
                }

                return View(response.Data);
        }

        [HttpGet("Student/EditStdProfile")]
        public async Task<IActionResult> EditStdProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var studentUserId))
            {
                return RedirectToAction("Login", "User");
            }

            var response = await _studentService.GetStudentProfile(studentUserId);
            
            if (response == null || !response.Status || response.Data == null)
            {
                return NotFound("Student profile not found.");
            }

            var model = new UpdateStudentRequestModel
            {
                FirstName = response.Data.FirstName,
                LastName = response.Data.LastName,
                Email = response.Data.Email,
                PhoneNumber = response.Data.PhoneNumber,
                Address = response.Data.Address,
                Gender = response.Data.Gender,
                Department = response.Data.Department,
                DateOfBirth = response.Data.DateOfBirth,
                StudentLevel = response.Data.StudentLevel
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditStdProfile(UpdateStudentRequestModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var studentUserId))
            {
                return RedirectToAction("Login", "User");
            }

            _logger.LogInformation("Student {UserId} updating profile", userId);

            var response = await _studentService.UpdateStudentProfile(studentUserId, model);

            if (!response.Status)
            {
                _logger.LogWarning("Profile update failed for {UserId}: {Message}", userId, response.Message);
                ViewBag.ErrorMessage = response.Message;
                return View("Profile", model);
            }

            _logger.LogInformation("Profile updated successfully for {UserId}", userId);
            return RedirectToAction("StdProfile");
        }



         [HttpGet("attendance-percentage")] 
        public async Task<IActionResult> AttendancePercentage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var studentUserId))
            {
                return RedirectToAction("Login", "User");
            }

            _logger.LogInformation("Student {UserId} requested attendance percentage", userId);

            var response = await _studentService.GetMyAttendancePercentage(studentUserId);

            if (!response.Status)
            {
                _logger.LogWarning("Failed to calculate attendance percentage for {UserId}: {Message}", userId, response.Message);
                ViewBag.ErrorMessage = response.Message;
                return View();
            }

            ViewBag.AttendancePercentage = response.Data;
            return View();
        }

        [HttpGet("Student/AttendanceReport")]
        public async Task<IActionResult> AttendanceReport(Guid? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var studentUserId))
            {
                return RedirectToAction("Login", "User");
            }

            var response = id.HasValue
                ? await _studentService.GetAttendanceReportByStudentId(id.Value)
                : await _studentService.GetAttendanceReport(studentUserId);

            if (!response.Status)
            {
                ViewBag.ErrorMessage = response.Message;
            }

            return View(response.Data ?? new StudentAttendanceReportDto());
        }

        [HttpGet("Student/AttendanceReportPdf/{sessionId}")]
        public async Task<IActionResult> AttendanceReportPdf(Guid sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var studentUserId))
            {
                return RedirectToAction("Login", "User");
            }

            var response = await _studentService.GetAttendanceReportItem(studentUserId, sessionId);
            if (!response.Status)
            {
                return NotFound(response.Message);
            }

            if (response.Data.Status != AttendanceStatus.Present || !response.Data.FirstScanTime.HasValue || !response.Data.SecondScanTime.HasValue)
            {
                return BadRequest("PDF download is only available after both scans are completed.");
            }

            var pdfBytes = BuildAttendancePdf(response.Data);
            var safeCourseCode = string.Concat(response.Data.CourseCode.Where(char.IsLetterOrDigit));
            var fileName = $"Attendance-{safeCourseCode}-{response.Data.ScanTime:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        private static byte[] BuildAttendancePdf(StudentAttendanceReportItemDto report)
        {
            static string Escape(string value) => value
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");

            var lines = new[]
            {
                "QRCode Attendance - Student Class Report",
                "",
                $"Student: {report.StudentName}",
                $"Matric Number: {report.MatricNumber}",
                $"Department: {report.Department}",
                $"Level: {report.Level.GetDescription()}",
                "",
                $"Course: {report.CourseName}",
                $"Course Code: {report.CourseCode}",
                $"Instructor: {report.InstructorName}",
                $"Class Start: {report.SessionStartTime.ToLocalTime():MMM dd, yyyy hh:mm tt}",
                $"Class End: {report.SessionEndTime.ToLocalTime():MMM dd, yyyy hh:mm tt}",
                "",
                $"Attendance Status: {report.Status}",
                $"First Scan: {FormatScanTime(report.FirstScanTime)}",
                $"Second Scan: {FormatScanTime(report.SecondScanTime)}",
                "",
                $"Generated: {DateTime.Now:MMM dd, yyyy hh:mm tt}"
            };

            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F1 22 Tf");
            content.AppendLine("72 760 Td");
            content.AppendLine($"({Escape(lines[0])}) Tj");
            content.AppendLine("0 -34 Td");
            content.AppendLine("/F1 12 Tf");

            foreach (var line in lines.Skip(1))
            {
                content.AppendLine("0 -22 Td");
                content.AppendLine($"({Escape(line)}) Tj");
            }

            content.AppendLine("ET");

            var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                $"<< /Length {contentBytes.Length} >>\nstream\n{content}\nendstream"
            };

            using var output = new MemoryStream();
            void Write(string value)
            {
                var bytes = Encoding.ASCII.GetBytes(value);
                output.Write(bytes, 0, bytes.Length);
            }

            Write("%PDF-1.4\n");
            var offsets = new List<long> { 0 };
            for (var i = 0; i < objects.Count; i++)
            {
                offsets.Add(output.Position);
                Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }

            var xrefPosition = output.Position;
            Write($"xref\n0 {objects.Count + 1}\n");
            Write("0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1))
            {
                Write($"{offset:0000000000} 00000 n \n");
            }

            Write($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");
            return output.ToArray();
        }

        private static string FormatScanTime(DateTime? scanTime)
        {
            return scanTime.HasValue
                ? scanTime.Value.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt")
                : "Not completed";
        }
    }
}








        
