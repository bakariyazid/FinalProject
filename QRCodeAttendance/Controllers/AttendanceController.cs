using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QRCodeAttendance.Implementation.Services;
using QRCodeAttendance.Interface.Services;

namespace QRCodeAttendance.Controllers
{
   
    public class AttendanceController : Controller
    {
        private readonly ILogger<AttendanceController> _logger;
        private readonly ISessionService _sessionService;
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(ILogger<AttendanceController> logger, ISessionService sessionService,
        IAttendanceService attendanceService)
        {
            _logger = logger;
            _sessionService = sessionService;
            _attendanceService = attendanceService;
        }


        [HttpGet]
        public IActionResult ScanQRCode(Guid? sessionId)
        {
            ViewBag.SessionId = sessionId;
            return View();
        }

       [HttpPost]
        public async Task<IActionResult> ScanQRCode(string qrCode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _attendanceService.MarkAttendance(Guid.Parse(userId), qrCode);

            if (!response.Status)
            {
                ViewBag.ErrorMessage = response.Message;
                return View(); 
            }

            TempData["SuccessMessage"] = "Attendance marked successfully!";
            return RedirectToAction("StudentDashboard");
        }



        // GET: /Session/Attendance/{id}
        public async Task<IActionResult> Attendance(Guid id)
        {
            var response = await _sessionService.GetSessionAttendance(id);

            if (!response.Status)
            {
                _logger.LogError(response.Message);
                return View("Error", response.Message);
            }

            return View(response.Data);
        }




        // VIEW ATTENDANCE

        [HttpGet("attendance/{sessionId}")]
        public async Task<IActionResult> ViewAttendance(Guid sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("Instructor {UserId} viewing attendance for session {SessionId}", userId, sessionId);

            var response = await _sessionService.GetSessionAttendance(sessionId);

            if (!response.Status)
            {
                _logger.LogWarning("Failed to fetch attendance for session {SessionId}: {Message}", sessionId, response.Message);
                ViewBag.ErrorMessage = response.Message;
            }

            return View(response.Data);
        }
    }
}