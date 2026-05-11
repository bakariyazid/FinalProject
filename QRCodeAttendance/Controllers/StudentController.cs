using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs.Student;

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
            _logger.LogInformation("Student {UserId} requested dashboard", userId);

            var response = await _studentService.GetDashboard(Guid.Parse(userId));

            if (!response.Status)
            {
                _logger.LogWarning("Failed to load dashboard for {UserId}: {Message}", userId, response.Message);
                ViewBag.ErrorMessage = response.Message;
            }

            return View(response.Data);
        }



        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Student {UserId} requested profile", userId);

            var response = await _studentService.GetStudentProfile(Guid.Parse(userId));

            return View(response.Data);
        }

        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateStudentRequestModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Student {UserId} updating profile", userId);

            var response = await _studentService.UpdateStudentProfile(Guid.Parse(userId), model);

            if (!response.Status)
            {
                _logger.LogWarning("Profile update failed for {UserId}: {Message}", userId, response.Message);
                ViewBag.ErrorMessage = response.Message;
                return View("Profile", model);
            }

            _logger.LogInformation("Profile updated successfully for {UserId}", userId);
            return RedirectToAction("Profile");
        }



         [HttpGet("attendance-percentage")] 
        public async Task<IActionResult> AttendancePercentage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Student {UserId} requested attendance percentage", userId);

            var response = await _studentService.GetMyAttendancePercentage(Guid.Parse(userId));

            if (!response.Status)
            {
                _logger.LogWarning("Failed to calculate attendance percentage for {UserId}: {Message}", userId, response.Message);
                ViewBag.ErrorMessage = response.Message;
                return View();
            }

            ViewBag.AttendancePercentage = response.Data;
            return View();
        }
    }
}








        