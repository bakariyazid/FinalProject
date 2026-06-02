using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Persistence.QRCodeAttendanceDb;

namespace QRCodeAttendance.Identity
{
    public class UserStore :IUserStore<User>, IUserPasswordStore<User>, IUserRoleStore<User>, IQueryableUserStore<User>, IUserEmailStore<User>
    {
        private readonly QRCodeDbContext _qrCodeDbContext;

        public UserStore(QRCodeDbContext qrCodeDbContext)
        {
            _qrCodeDbContext = qrCodeDbContext;
        }

        public IQueryable<User> Users => _qrCodeDbContext.Set<User>().AsQueryable();

       public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var normalizedRoleName = roleName.Trim().ToLower();
            var role = await _qrCodeDbContext.Set<Role>()
                .SingleAsync(r => r.Name.ToLower() == normalizedRoleName, cancellationToken);

            user.RoleId = role.Id;

            _qrCodeDbContext.Update(user);
            await _qrCodeDbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            await _qrCodeDbContext.AddAsync(user, cancellationToken);
            await _qrCodeDbContext.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            _qrCodeDbContext.Entry(user).State = EntityState.Deleted;
            await _qrCodeDbContext.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            _qrCodeDbContext.Dispose();
        }

        public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            normalizedEmail = normalizedEmail.ToLower();
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(normalizedEmail))
            {
                throw new ArgumentNullException(nameof(normalizedEmail));
            }
            return await _qrCodeDbContext.Set<User>().SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
        }

        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }
            return await _qrCodeDbContext.Set<User>().FindAsync(new object[] { Guid.Parse(userId) }, cancellationToken);
        }

        public async Task<User?> FindByNameAsync(string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }
            return await _qrCodeDbContext.Set<User>().FirstOrDefaultAsync(u => u.Email == userName, cancellationToken);
        }

        public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string?>(user.Email.ToLower());
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(true);
        }

        public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string?>(user.Email.ToLower());
        }

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string?>(user.Email.ToLower());
        }

        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string?>(user.PasswordHash);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(true);
        }
        public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                    {
                        throw new ArgumentNullException(nameof(user));
                    }
            var role = await _qrCodeDbContext.Set<Role>()
                    .Where(r => r.Id == user.RoleId)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync(cancellationToken);

            return role == null ? new List<string>() : new List<string> { role };
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string?>(user.Email.ToLower());
        }

       public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await _qrCodeDbContext.Set<User>()
                    .Include(u => u.Role)
                    .Where(u => u.Role != null && u.Role.Name == roleName)
                    .ToListAsync(cancellationToken);
            }
       
     public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

    public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
                    if (user == null)
                    {
                        throw new ArgumentNullException(nameof(user));
                    }
            var isInRole = await _qrCodeDbContext.Set<Role>()
                .Where(r => r.Id == user.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync(cancellationToken);

            return isInRole?.ToLower() == roleName.ToLower();
        }

 public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            throw new NotSupportedException("Users must always have a role. Use AddToRoleAsync to change the role instead.");
        }

        public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.Email = (email ?? string.Empty).ToLower();
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.Email = (normalizedEmail ?? string.Empty).ToLower();
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.Email = (normalizedName ?? string.Empty).ToLower();
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                user.PasswordHash = passwordHash ?? string.Empty;
                return Task.CompletedTask;
            }


        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.Email = (userName ?? string.Empty).ToLower();
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            _qrCodeDbContext.Set<User>().Entry(user).State = EntityState.Modified;
            await _qrCodeDbContext.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
    }

}
