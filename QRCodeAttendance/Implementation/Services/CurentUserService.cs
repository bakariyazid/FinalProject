using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using QRCodeAttendance.Interface.Services;

namespace QRCodeAttendance.Implementation.Services
{
   
        public class CurrentUserService : ICurrentUserService
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public CurrentUserService(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            public Guid UserId
            {
                get
                {
                    var userId = _httpContextAccessor.HttpContext?
                        .User?
                        .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
                }
            }

            public string Email =>
                _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

            public string Role =>
                _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
            
}
