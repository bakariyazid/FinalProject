using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs.Instructor;
using QRCodeAttendance.Models.DTOs.Reports;
using QRCodeAttendance.Models.DTOs.Session;
using QRCodeAttendance.Models.Enums;
using QRCodeAttendance.Models.Extensions;

namespace QRCodeAttendance.Controllers
{
    public class InstructorController : Controller
    {
        private readonly ILogger<InstructorController> _logger;
        private readonly IInstructorService _instructorService;
        private readonly ISessionService _sessionService;
        private readonly IStudentService _studentService;
        private readonly IReportService _reportService;
        

        public InstructorController(
            ILogger<InstructorController> logger,
            IInstructorService instructorService,
            ISessionService sessionService,
            IStudentService studentService,
            IReportService reportService) 
        {
            _logger = logger;
            _instructorService = instructorService;
            _sessionService = sessionService;
            _studentService = studentService;
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error");
        }

        // REGISTER INSTRUCTOR

        [HttpGet]
        public IActionResult RegisterInstructor()
        {
            TempData.Remove("Alert");
            TempData.Remove("AlertType");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterInstructor(CreateInstructorRequestModel model)
        {
            var response = await _instructorService.RegisterInstructor(model);

            if (response.Status)
            {
                TempData["Alert"] = "Instructor registered successfully!";
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


        // DASHBOARD

        [HttpGet]
            public async Task<IActionResult> InstructorDashboard()
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var instructorIdClaim = User.FindFirst("InstructorId")?.Value;
                Console.WriteLine($"USERID: {userId}");
                Console.WriteLine($"INSTRUCTORID: {instructorIdClaim}");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("InstructorDashboard: UserId claim is missing");
                    return RedirectToAction("Login", "User");
                }

                if (!Guid.TryParse(userId, out Guid instructorId))
                {
                    _logger.LogError("InstructorDashboard: Invalid UserId format: {UserId}", userId);
                    return RedirectToAction("Login", "User");
                }

                _logger.LogInformation("Instructor {UserId} requested dashboard", instructorId);

                var response = await _instructorService.GetDashboard(instructorId);

                if (!response.Status)
                {
                    _logger.LogWarning("Dashboard failed for {UserId}: {Message}", instructorId, response.Message);
                    ViewBag.ErrorMessage = response.Message;
                }

                return View(response.Data);
            }
       
            [HttpGet]
            public async Task<IActionResult> ViewMyStudents(StudentLevel level)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return RedirectToAction("Login", "User");
                }

                var instructor = await _instructorService.GetInstructorProfile(userGuid);
                
                var response = await _instructorService.GetStudentsByDeptAndLevel(instructor.Data.Department, level);

                 if (!response.Status)
                {
                    _logger.LogWarning("Failed to fetch students for {UserId}: {Message}", userId, response.Message);
                    ViewBag.ErrorMessage = response.Message;
                }

                ViewBag.Level = level.GetDescription();
                ViewBag.Department = instructor.Data.Department.ToString();

                return View(response.Data);
            }
      
    [HttpGet]
        public async Task<IActionResult> Report(Guid? sessionId, string? courseCode)
        {
            string instructorName = User.Identity?.Name ?? "Instructor";

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized();
            }

            var instructor = await _instructorService.GetInstructorProfile(userId);
            if (!instructor.Status || instructor.Data == null)
            {
                return Unauthorized();
            }

            var instructorId = instructor.Data.InstructorId;
            var sessionsResponse = await _sessionService.GetSessionsByInstructor(instructorId);
            ViewBag.CompletedSessions = sessionsResponse.Data?
                .Where(s => DateTime.UtcNow >= s.SessionEndTime.AddMinutes(-10))
                .OrderByDescending(s => s.SessionStartTime)
                .ToList() ?? new List<SessionDto>();

            CourseReportDto report;
            if (sessionId.HasValue)
            {
                report = await _reportService.GenerateSessionReportAsync(sessionId.Value, instructorId);
            }
            else if (!string.IsNullOrWhiteSpace(courseCode))
            {
                report = await _reportService.GenerateCourseReportAsync(courseCode, instructorId);
            }
            else
            {
                report = new CourseReportDto
                {
                    InstructorId = instructorId,
                    InstructorName = instructorName,
                    IsReportAvailable = false,
                    Message = "Select a completed class session to view its attendance report."
                };
            }

            report.InstructorId = instructorId;
            report.InstructorName ??= instructorName; 
            return View("Report", report);
        }

            [HttpGet("Instructor/InsProfile")]
            public async Task<IActionResult> InsProfile()
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var instructorUserId))
                {
                    return RedirectToAction("Login", "User");
                }

                _logger.LogInformation("Instructor {UserId} requested profile", userId);

                var response = await _instructorService.GetInstructorProfile(instructorUserId);

                if (response == null || response.Data == null)
                {
                    _logger.LogWarning("Profile data for user {UserId} was not found.", userId);
                    return NotFound("Instructor profile not found.");
                }

                return View(response.Data);
            }

        [HttpGet("Instructor/EditInsProfile")]
        public async Task<IActionResult> EditInsProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var instructorUserId))
            {
                return RedirectToAction("Login", "User");
            }

            var response = await _instructorService.GetInstructorProfile(instructorUserId);
            
            if (response == null || !response.Status || response.Data == null)
            {
                return NotFound("Instructor profile not found.");
            }

            var editModel = new UpdateInstructorRequestModel
            {
                FirstName = response.Data.FirstName,
                LastName = response.Data.LastName,
                Email = response.Data.Email,
                PhoneNumber = response.Data.PhoneNumber,
                Address = response.Data.Address,
                Gender = response.Data.Gender,
                Department = response.Data.Department,
                DateOfBirth = response.Data.DateOfBirth
            };
            return View(editModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditInsProfile(UpdateInstructorRequestModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var instructorUserId))
            {
                return RedirectToAction("Login", "User");
            }

            _logger.LogInformation("Instructor {UserId} updating profile", userId);

            var response = await _instructorService.UpdateInsProfile(instructorUserId, model);

            if (!response.Status)
            {
                _logger.LogWarning("Profile update failed for {UserId}: {Message}", userId, response.Message);
                ViewBag.ErrorMessage = response.Message;
                return View("EditInsProfile", model);
            }

            _logger.LogInformation(" Instructor Profile updated successfully for {UserId}", userId);
            return RedirectToAction("InsProfile");
        }

       
    }
}
