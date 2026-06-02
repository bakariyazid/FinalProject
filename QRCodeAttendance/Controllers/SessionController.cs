using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs.Session;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Controllers
{
    
    public class SessionController : Controller
    {
        private readonly ILogger<SessionController> _logger;
        private readonly ISessionService _sessionService;
        private readonly IInstructorService _instructorService;

        public SessionController(ILogger<SessionController> logger, 
        ISessionService sessionService, IInstructorService instructorService)
        {
            _logger = logger;
            _sessionService = sessionService;
            _instructorService = instructorService;
        }
 

        [HttpGet]
        public IActionResult CreateSession()
        {
            return View();
        }

        [HttpPost]
        
    public async Task<IActionResult> CreateSession(CreateSessionRequestModel model)
        {
            var instructorIdClaim = User.FindFirst("InstructorId")?.Value;
            Console.WriteLine($"INSTRUCTOR: {instructorIdClaim}");

            if (string.IsNullOrEmpty(instructorIdClaim))
            {
                _logger.LogWarning("Instructor claim not found.");
                return Forbid();
            }

            var instructorId = Guid.Parse(instructorIdClaim);

            var response = await _sessionService.CreateSessionAsync(instructorId, model);

            if (!response.Status)
            {
                 _logger.LogWarning("Session creation failed for {instructorIdClaim}: {Message}", instructorId, response.Message);
                ViewBag.ErrorMessage = response.Message;
                return View(model);
            }
                _logger.LogInformation("Session created successfully by {instructorIdClaim}", instructorId);
          
            return RedirectToAction("Details", "Session");
        }
    


        //Session Details
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
            {
                var sessionsResponse = await _sessionService.GetAllSessions(); 
                var sessions = sessionsResponse.Data;

                ViewBag.TargetId = id; 

                return View(sessions);
            }

       // THE TRIGGER (Point your button here)
        [HttpGet]
        public async Task<IActionResult> GenerateQR(Guid id)
        {
            var response = await _sessionService.GenerateSessionQrCode(id);
            
            if (!response.Status)
            {
                TempData["ErrorMessage"] = response.Message;
                return RedirectToAction("Details"); 
            }

            return RedirectToAction("GenerateQRCode", new { id = id });
        }


    [HttpGet]
    public async Task<IActionResult> GenerateQRCode(Guid id)
    {
        var response = await _sessionService.GetSessionById(id);
        if (response == null || !response.Status) return NotFound();

        if (string.IsNullOrEmpty(response.Data.QRCodeToken) || DateTime.UtcNow >= response.Data.QRCodeExpiry)
        {
            var genResult = await _sessionService.GenerateSessionQrCode(id);
            
            if (!genResult.Status)
            {
                TempData["ErrorMessage"] = genResult.Message;
                return RedirectToAction("Details"); 
            }
 
            return View(genResult.Data);
        }

        return View(response.Data); 
    }
            [HttpPost]
            public async Task<IActionResult> GenerateQrCode(Guid sessionId)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var response = await _sessionService.GenerateSessionQrCode(sessionId);

                if (!response.Status)
                {
                    return Json(new { success = false, message = response.Message });
                }

                return Json(new { 
                    success = true, 
                    token = response.Data.QRCodeToken, 
                    expiry = response.Data.QRCodeExpiry.ToString("O") 
                });
            }

        [HttpGet("Session/Edit/{sessionId}")]
        public async Task<IActionResult> EditSession(Guid sessionId)
        {
            try
            {
                var response = await _sessionService.GetSessionById(sessionId);

                if (response == null || !response.Status)
                {
                    _logger.LogWarning("Edit Session Failed: Session with ID {SessionId} not found or status false.", sessionId);
                    return NotFound();
                }

                var model = new UpdateSessionRequestModel
                {
                    CourseName = response.Data.CourseName,
                    CourseCode = response.Data.CourseCode,
                    Level = response.Data.Level,
                    Department = response.Data.Department,
                    SessionStartTime = response.Data.SessionStartTime.ToLocalTime(),
                    SessionEndTime = response.Data.SessionEndTime.ToLocalTime()
                };

                ViewBag.SessionId = sessionId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching session {SessionId} for editing.", sessionId);
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpPost("Session/Edit/{sessionId}")]
        public async Task<IActionResult> EditSession(Guid sessionId, UpdateSessionRequestModel model)
        {
            try
            {
                ViewBag.SessionId = sessionId;

                if (model.SessionStartTime >= model.SessionEndTime)
                {
                    _logger.LogWarning("Validation Failed: Start time must be before end time for Session {SessionId}.", sessionId);
                    ViewBag.Error = "The session cannot end before it starts.";
                    return View(model);
                }

                var response = await _sessionService.UpdateSession(sessionId, model);

                if (!response.Status)
                {
                    _logger.LogWarning("Update failed: {Message}", response.Message);
                    ViewBag.Error = response.Message;
                    return View(model);
                }

                return RedirectToAction(nameof(Details), new { id = sessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error updating session {SessionId}", sessionId);
                return View("Error");
            }
        }

        // GET: /Session/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _sessionService.GetSessionById(id);

            if (response == null || !response.Status)
            {
                return NotFound();
            }

            return View("DeleteSession", response.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var response = await _sessionService.DeleteSession(id);

            if (!response.Status)
            {
                _logger.LogError(response.Message);
                return View("Error", response.Message);
            }

            return RedirectToAction(nameof(Details));
            
        }


        [HttpGet]
            public async Task<IActionResult> LiveAttendanceStatus(Guid sessionId)
            {
                var response = await _sessionService.GetSessionAttendance(sessionId);
                var attendances = response.Status && response.Data != null
                    ? response.Data.OrderByDescending(a => a.ScanTime).ToList()
                    : new List<QRCodeAttendance.Models.DTOs.Attendance.AttendanceDto>();

                var latest = attendances.FirstOrDefault();

                return Json(new
                {
                    success = true,
                    count = attendances.Count,
                    latest = latest == null ? null : new
                    {
                        id = latest.Id,
                        studentName = latest.StudentName,
                        courseName = latest.CourseName,
                        courseCode = latest.CourseCode,
                        scanTime = latest.ScanTime.ToLocalTime().ToString("hh:mm tt")
                    }
                });
            }

       

        

         [HttpGet]
        public async Task<IActionResult> Sessions()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "User");

                if (!Guid.TryParse(userIdString, out var userId))
                {
                    return RedirectToAction("Login", "User");
                }

                var result = await _instructorService.GetInstructorSessions(userId);

                if (!result.Status)
                {
                    ViewBag.Error = result.Message;
                    return View(new List<SessionDto>());
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading instructor sessions");
                return View("Error");
            }
        }


       

        
    }
}
