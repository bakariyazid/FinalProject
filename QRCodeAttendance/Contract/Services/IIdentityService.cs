using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Contract.Services
{
    public interface IIdentityService
    {
        string GenerateSalt();
        string? GetUserIdentity();
        string GetClaimValue(string type);
        string GenerateToken(User user, IEnumerable<string> roles);
        public IEnumerable<Claim>? ValidateToken(string jwtToken);
        JwtSecurityToken? GetClaims(string token);
        public string GetPasswordHash(string password, string? salt = null);
        Task<User> FindByNameAsync(string userName);
        Task<User?> FindUserAsync(string UserName);
        // bool CheckPasswordAsync(User user, string password);
        public Task<User> GetLoggedInUser();


    }
}
