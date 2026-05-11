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

        if (string.IsNullOrEmpty(response.Data.QRCodeToken))
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



        // GET: /Session/Edit/{id}
        public IActionResult Edit(Guid id)
        {
            return View();
        }

        // POST: /Session/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid sessionId, UpdateSessionRequestModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _sessionService.UpdateSession(sessionId, model);

            if (!response.Status)
            {
                ModelState.AddModelError("", response.Message);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Session/Delete/{id}
        public IActionResult Delete(Guid id)
        {
            return View(id);
        }

        // POST: /Session/DeleteConfirmed
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

            return RedirectToAction(nameof(Index));
        }

        
        // public async Task<IActionResult> GenerateQr(Guid id)
        // {
        //     var response = await _sessionService.GenerateSessionQrCode(id);

        //     if (!response.Status)
        //     {
        //         _logger.LogError(response.Message);
        //         return View("Error", response.Message);
        //     }

        //     return Content(response.Data); 
        // }

        // // POST: /Session/ValidateQr
        // [HttpPost]
        // public async Task<IActionResult> ValidateQr(Guid sessionId, string qrCode)
        // {
        //     var response = await _sessionService.ValidateSessionQrCode(sessionId, qrCode);

        //     return Json(new
        //     {
        //         success = response.Status,
        //         message = response.Message
        //     });
        // }

        // GET: /Session
        public async Task<IActionResult> ByDate(DateTime date)
        {
            var response = await _sessionService.GetSessionsByDate(date);

            if (!response.Status)
            {
                _logger.LogError(response.Message);
                return View("Error", response.Message);
            }

            return View("Index", response.Data);
        }


        [HttpGet]
        public async Task<IActionResult> Sessions()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "User");

                var userId = Guid.Parse(userIdString);

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


       

        

        // public async Task<IActionResult> Details(Guid id)
        // {
        //     var response = await _sessionService.GetSessionById(id);

        //     if (!response.Status)
        //     {
        //         _logger.LogError(response.Message);
        //         return NotFound(response.Message);
        //     }

        //     return View(response.Data);
        // }

        // [HttpPost]
        // public async Task<IActionResult> StartSession(Guid id)
        // {
        //     var response = await _sessionService.StartSession(id);

        //     if (!response.Status)
        //         return BadRequest(response.Message);

        //     return RedirectToAction(nameof(Details), new { id });
        // }

        // [HttpPost]
        // public async Task<IActionResult> EndSession(Guid id)
        // {
        //     var response = await _sessionService.EndSession(id);

        //     if (!response.Status)
        //         return BadRequest(response.Message);

        //     return RedirectToAction(nameof(Details), new { id });
        // }
    }
}