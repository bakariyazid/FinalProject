using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs.Admin;

namespace QRCodeAttendance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminInvitationService _adminInvitationService;
        private readonly IAdminProfileService _adminProfileService;

        public AdminController(IAdminInvitationService adminInvitationService, IAdminProfileService adminProfileService)
        {
            _adminInvitationService = adminInvitationService;
            _adminProfileService = adminProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            var response = await _adminInvitationService.GetInvitationDashboard();
            return View(response.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return RedirectToAction("Login", "User");

            var response = await _adminProfileService.GetProfile(userId);
            return response.Status && response.Data != null ? View(response.Data) : NotFound(response.Message);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return RedirectToAction("Login", "User");

            var response = await _adminProfileService.GetProfile(userId);
            if (!response.Status || response.Data == null) return NotFound(response.Message);
            return View(new UpdateAdminProfileRequestModel
            {
                FirstName = response.Data.FirstName, LastName = response.Data.LastName, Email = response.Data.Email,
                PhoneNumber = response.Data.PhoneNumber, Address = response.Data.Address,
                Gender = response.Data.Gender, DateOfBirth = response.Data.DateOfBirth
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UpdateAdminProfileRequestModel model)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return RedirectToAction("Login", "User");
            if (!ModelState.IsValid) return View(model);

            var response = await _adminProfileService.UpdateProfile(userId, model);
            if (!response.Status)
            {
                ModelState.AddModelError(string.Empty, response.Message);
                return View(model);
            }

            TempData["Alert"] = response.Message;
            TempData["AlertType"] = "success";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestInstructorEmailVerification([Bind(Prefix = "Form")] CreateInstructorInvitationRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                var dashboardResponse = await _adminInvitationService.GetInvitationDashboard();
                var dashboard = dashboardResponse.Data;
                dashboard.Form = model;
                return View("AdminDashboard", dashboard);
            }

            var response = await _adminInvitationService.RequestInstructorEmailVerification(model);

            if (!response.Status)
            {
                TempData["Alert"] = response.Message;
                TempData["AlertType"] = "warning";
                return RedirectToAction(nameof(AdminDashboard));
            }

            TempData["Alert"] = response.Message;
            TempData["AlertType"] = "success";

            return RedirectToAction(nameof(AdminDashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyInstructorEmail([Bind(Prefix = "VerificationForm")] VerifyInstructorEmailRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                var dashboardResponse = await _adminInvitationService.GetInvitationDashboard();
                var dashboard = dashboardResponse.Data;
                dashboard.VerificationForm = model;
                return View("AdminDashboard", dashboard);
            }

            var response = await _adminInvitationService.VerifyInstructorEmail(model);
            TempData["Alert"] = response.Message;
            TempData["AlertType"] = response.Status ? "success" : "warning";
            return RedirectToAction(nameof(AdminDashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructorInvitation(Guid invitationId)
        {
            var response = await _adminInvitationService.DeleteInstructorInvitation(invitationId);
            TempData["Alert"] = response.Message;
            TempData["AlertType"] = response.Status ? "success" : "warning";

            return RedirectToAction(nameof(AdminDashboard));
        }

    }
}
