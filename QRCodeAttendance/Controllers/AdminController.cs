using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs.Admin;

namespace QRCodeAttendance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminInvitationService _adminInvitationService;

        public AdminController(IAdminInvitationService adminInvitationService)
        {
            _adminInvitationService = adminInvitationService;
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            var response = await _adminInvitationService.GetInvitationDashboard();
            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateInstructorCode([Bind(Prefix = "Form")] CreateInstructorInvitationRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                var dashboardResponse = await _adminInvitationService.GetInvitationDashboard();
                var dashboard = dashboardResponse.Data;
                dashboard.Form = model;
                return View("AdminDashboard", dashboard);
            }

            var response = await _adminInvitationService.GenerateInstructorInvitation(model);

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

    }
}
