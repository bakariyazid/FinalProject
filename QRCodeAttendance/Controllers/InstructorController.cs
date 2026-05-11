using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs.Instructor;
using QRCodeAttendance.Models.DTOs.Session;
using QRCodeAttendance.Models.Enums;

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
                var instructor = await _instructorService.GetInstructorProfile(Guid.Parse(userId));
                
                var response = await _instructorService.GetStudentsByDeptAndLevel(instructor.Data.Department, level);

                 if (!response.Status)
                {
                    _logger.LogWarning("Failed to fetch students for {UserId}: {Message}", userId, response.Message);
                    ViewBag.ErrorMessage = response.Message;
                }

                ViewBag.Level = level.ToString();
                ViewBag.Department = instructor.Data.Department.ToString();

                return View(response.Data);
            }
      
    [HttpGet]
        public async Task<IActionResult> Report(string courseCode)
        {
            string instructorName = User.Identity?.Name ?? "Instructor";

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid instructorId))
            {
                return Unauthorized();
            }

            var report = await _reportService.GenerateCourseReportAsync(courseCode, instructorId);

            report.InstructorId = instructorId;
            report.InstructorName = instructorName; 
            return View("Report", report);
        }


                [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Student {UserId} requested profile", userId);

            var response = await _instructorService.GetInstructorProfile(Guid.Parse(userId));

            return View(response.Data);
        }

        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateInstructorRequestModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Student {UserId} updating profile", userId);

            var response = await _instructorService.UpdateInstructorProfile(Guid.Parse(userId), model);

            if (!response.Status)
            {
                _logger.LogWarning("Profile update failed for {UserId}: {Message}", userId, response.Message);
                ViewBag.ErrorMessage = response.Message;
                return View("Profile", model);
            }

            _logger.LogInformation("Profile updated successfully for {UserId}", userId);
            return RedirectToAction("Profile");
        }

       
    }
}