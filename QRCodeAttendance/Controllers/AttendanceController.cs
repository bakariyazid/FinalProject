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
using QRCodeAttendance.Models.DTOs.Attendance;

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


        [HttpGet("attendance/scan/{sessionId:guid}")]
        public IActionResult ScanQRCode(Guid? sessionId)
        {
            ViewBag.SessionId = sessionId;
            return View();
        }

       [HttpPost("attendance/scan")]
        public async Task<IActionResult> ScanQRCode(Guid sessionId, string qrCode)
        {
            var response = await _attendanceService.MarkAttendance(sessionId, qrCode);

            if (!response.Status)
            {
                ViewBag.ErrorMessage = response.Message;
                ViewBag.SessionId = sessionId;
                return View(); 
            }

            TempData["SuccessMessage"] = "QR Code scanned successfully. Attendance marked.";
            return RedirectToAction("StudentDashboard", "Student");
        }

        [HttpPost("attendance/sync-offline")]
        public async Task<IActionResult> SyncOfflineAttendance([FromBody] OfflineAttendanceScanRequestModel request)
        {
            var response = await _attendanceService.SyncOfflineAttendance(request);

            return Ok(new OfflineAttendanceScanResponseModel
            {
                ClientScanId = request.ClientScanId,
                Synced = response.Status,
                Message = response.Message
            });
        }



        // GET: /Session/Attendance/{id}
        [HttpGet]
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
            return Redirect($"/attendance/scan/{sessionId}");
        }
    }
}
