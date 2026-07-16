using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs.Instructor;
using QRCodeAttendance.Models.DTOs.Student;
using QRCodeAttendance.Models.DTOs.User;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
         

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
            
        }

        public IActionResult Index()
        {
            return View();
        }
      


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var loginResponse = await _userService.LoginAsync(model, CancellationToken.None);

            if (!loginResponse.Status)
            {
                ViewBag.ErrorMessage = loginResponse.Message == "Invalid credentials"
                    ? "Invalid credential kindly check your input"
                    : loginResponse.Message;
                return View(model);
            }

            var user = loginResponse.Data;
            Console.WriteLine($"USER: {user.UserId}, ROLE: {user.Role}");
            var role = user.Role; 

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? user.Email),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("InstructorId", user.InstructorId?.ToString() ?? string.Empty)
            };


            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
             var authenticationProperties = new AuthenticationProperties();
            var principal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal,
                authenticationProperties
            );
            return role switch
            {
                "Student" => RedirectToAction("StudentDashboard", "Student"),
                "Instructor" => RedirectToAction("InstructorDashboard", "Instructor"),
                "Admin" => RedirectToAction("AdminDashboard", "Admin"),
                _ => RedirectToAction("Login", "User")
            };
        }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToAction("Login", "User");
    }
        
    }
}
