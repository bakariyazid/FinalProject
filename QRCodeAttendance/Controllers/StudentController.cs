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

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "mitc-logo.jpg");
            var pdfBytes = BuildAttendancePdf(response.Data, logoPath);
            var safeCourseCode = string.Concat(response.Data.CourseCode.Where(char.IsLetterOrDigit));
            var fileName = $"Attendance-{safeCourseCode}-{response.Data.ScanTime:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        private static byte[] BuildAttendancePdf(StudentAttendanceReportItemDto report, string logoPath)
        {
            static string Escape(string value) => value
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("©", "\\251");

            static void AppendRect(StringBuilder content, double x, double y, double width, double height, string fillColor)
            {
                content.AppendLine(fillColor);
                content.AppendLine($"{x:0.##} {y:0.##} {width:0.##} {height:0.##} re f");
            }

            static void AppendText(StringBuilder content, double x, double y, string font, int size, string color, string text)
            {
                content.AppendLine("BT");
                content.AppendLine(color);
                content.AppendLine($"/{font} {size} Tf");
                content.AppendLine($"{x:0.##} {y:0.##} Td");
                content.AppendLine($"({Escape(text)}) Tj");
                content.AppendLine("ET");
            }

            static void AppendLabelValue(StringBuilder content, double y, string label, string value)
            {
                AppendText(content, 74, y, "F2", 10, "0.36 0.43 0.54 rg", label.ToUpperInvariant());
                AppendText(content, 220, y, "F1", 12, "0.08 0.12 0.20 rg", value);
            }

            static byte[] Ascii(string value) => Encoding.ASCII.GetBytes(value);

            var content = new StringBuilder();
            var logoWidth = 0;
            var logoHeight = 0;
            var logoBytes = System.IO.File.Exists(logoPath) ? System.IO.File.ReadAllBytes(logoPath) : null;
            var hasLogo = logoBytes != null && TryReadJpegSize(logoBytes, out logoWidth, out logoHeight);

            AppendRect(content, 0, 704, 612, 88, "0.16 0.29 0.62 rg");
            AppendRect(content, 0, 0, 612, 704, "0.98 0.99 1 rg");

            if (hasLogo)
            {
                var logoDrawWidth = 152d;
                var logoDrawHeight = logoDrawWidth * logoHeight / logoWidth;
                content.AppendLine("q");
                content.AppendLine($"{logoDrawWidth:0.##} 0 0 {logoDrawHeight:0.##} 48 {748 - logoDrawHeight / 2:0.##} cm");
                content.AppendLine("/Logo Do");
                content.AppendLine("Q");
            }

            AppendText(content, 244, 759, "F2", 18, "1 1 1 rg", "MITC QRCode Attendance");
            AppendText(content, 244, 737, "F1", 13, "0.88 0.93 1 rg", "Student Class Report");
            AppendText(content, 48, 662, "F2", 22, "0.08 0.12 0.20 rg", report.CourseName);
            AppendText(content, 48, 640, "F1", 12, "0.36 0.43 0.54 rg", $"Generated on {DateTime.Now:MMM dd, yyyy hh:mm tt}");

            AppendRect(content, 48, 462, 516, 146, "1 1 1 rg");
            content.AppendLine("0.86 0.90 0.96 RG");
            content.AppendLine("48 462 516 146 re S");
            AppendText(content, 74, 582, "F2", 13, "0.16 0.29 0.62 rg", "Student Information");
            AppendLabelValue(content, 554, "Student", report.StudentName);
            AppendLabelValue(content, 528, "Matric Number", report.MatricNumber);
            AppendLabelValue(content, 502, "Department", report.Department.ToString());
            AppendLabelValue(content, 476, "Level", report.Level.GetDescription());

            AppendRect(content, 48, 276, 516, 158, "1 1 1 rg");
            content.AppendLine("0.86 0.90 0.96 RG");
            content.AppendLine("48 276 516 158 re S");
            AppendText(content, 74, 408, "F2", 13, "0.16 0.29 0.62 rg", "Class Details");
            AppendLabelValue(content, 378, "Course Code", report.CourseCode);
            AppendLabelValue(content, 352, "Instructor", $"ENG {report.InstructorName}");
            AppendLabelValue(content, 326, "Class Start", report.SessionStartTime.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt"));
            AppendLabelValue(content, 300, "Class End", report.SessionEndTime.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt"));

            AppendRect(content, 48, 118, 516, 120, "0.93 0.97 1 rg");
            content.AppendLine("0.72 0.82 0.96 RG");
            content.AppendLine("48 118 516 120 re S");
            AppendText(content, 74, 212, "F2", 13, "0.16 0.29 0.62 rg", "Attendance Summary");
            AppendLabelValue(content, 182, "Status", report.Status.ToString());
            AppendLabelValue(content, 156, "First Scan", FormatScanTime(report.FirstScanTime));
            AppendLabelValue(content, 130, "Second Scan", FormatScanTime(report.SecondScanTime));

            content.AppendLine("0.16 0.29 0.62 RG");
            content.AppendLine("48 84 m 564 84 l S");
            AppendText(content, 48, 58, "F1", 9, "0.36 0.43 0.54 rg", $"© {DateTime.Now.Year} MITC QR Code Attendance. All rights reserved.");
            AppendText(content, 48, 42, "F1", 9, "0.36 0.43 0.54 rg", "Built with heart by BAKARI Yazid Akanni.");

            var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
            var contentObjectNumber = hasLogo ? 7 : 6;
            var resources = hasLogo
                ? "<< /Font << /F1 4 0 R /F2 5 0 R >> /XObject << /Logo 6 0 R >> >>"
                : "<< /Font << /F1 4 0 R /F2 5 0 R >> >>";

            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources {resources} /Contents {contentObjectNumber} 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>"
            };

            var objectBytes = objects.Select(Ascii).ToList();

            if (hasLogo && logoBytes != null)
            {
                var imageHeader = Ascii($"<< /Type /XObject /Subtype /Image /Width {logoWidth} /Height {logoHeight} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {logoBytes.Length} >>\nstream\n");
                var imageFooter = Ascii("\nendstream");
                objectBytes.Add(CombineBytes(imageHeader, logoBytes, imageFooter));
            }

            objectBytes.Add(Ascii($"<< /Length {contentBytes.Length} >>\nstream\n{content}\nendstream"));

            using var output = new MemoryStream();
            void Write(string value)
            {
                var bytes = Encoding.ASCII.GetBytes(value);
                output.Write(bytes, 0, bytes.Length);
            }

            Write("%PDF-1.4\n");
            var offsets = new List<long> { 0 };
            for (var i = 0; i < objectBytes.Count; i++)
            {
                offsets.Add(output.Position);
                Write($"{i + 1} 0 obj\n");
                output.Write(objectBytes[i], 0, objectBytes[i].Length);
                Write("\nendobj\n");
            }

            var xrefPosition = output.Position;
            Write($"xref\n0 {objectBytes.Count + 1}\n");
            Write("0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1))
            {
                Write($"{offset:0000000000} 00000 n \n");
            }

            Write($"trailer\n<< /Size {objectBytes.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");
            return output.ToArray();
        }

        private static byte[] CombineBytes(params byte[][] parts)
        {
            var length = parts.Sum(p => p.Length);
            var result = new byte[length];
            var offset = 0;

            foreach (var part in parts)
            {
                Buffer.BlockCopy(part, 0, result, offset, part.Length);
                offset += part.Length;
            }

            return result;
        }

        private static bool TryReadJpegSize(byte[] bytes, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (bytes.Length < 4 || bytes[0] != 0xFF || bytes[1] != 0xD8)
            {
                return false;
            }

            var index = 2;
            while (index + 9 < bytes.Length)
            {
                if (bytes[index] != 0xFF)
                {
                    index++;
                    continue;
                }

                var marker = bytes[index + 1];
                index += 2;

                if (marker == 0xD9 || marker == 0xDA)
                {
                    break;
                }

                if (index + 1 >= bytes.Length)
                {
                    break;
                }

                var segmentLength = (bytes[index] << 8) + bytes[index + 1];
                if (segmentLength < 2 || index + segmentLength > bytes.Length)
                {
                    break;
                }

                if ((marker >= 0xC0 && marker <= 0xC3) || (marker >= 0xC5 && marker <= 0xC7) ||
                    (marker >= 0xC9 && marker <= 0xCB) || (marker >= 0xCD && marker <= 0xCF))
                {
                    height = (bytes[index + 3] << 8) + bytes[index + 4];
                    width = (bytes[index + 5] << 8) + bytes[index + 6];
                    return width > 0 && height > 0;
                }

                index += segmentLength;
            }

            return false;
        }

        private static string FormatScanTime(DateTime? scanTime)
        {
            return scanTime.HasValue
                ? scanTime.Value.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt")
                : "Not completed";
        }
    }
}








        
