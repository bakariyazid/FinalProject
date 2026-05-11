using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Persistence.QRCodeAttendanceDb;

namespace QRCodeAttendance.Implementation.Repositories
{
public class UserRepository : BaseRepository, IUserRepository
    {
      public UserRepository(QRCodeDbContext qrCodeDbContext) : base(qrCodeDbContext)
            {

            }

        public async Task<User?> GetByEmail(string email)
        {
            return await _qrCodeDbContext.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IReadOnlyList<User>> GetUsersByRole(Role role)
        {
            return await _qrCodeDbContext.Users
                .Include(u => u.Role)
                .Where(u => u.Role == role)
                .ToListAsync();
        }
        public async Task<User?> GetUserWithRole(Expression<Func<User, bool>> predicate)
            {
                return await _qrCodeDbContext.Users
                    .Include(u => u.Role)
                    .Include(u => u.Student)
                    .Include(u => u.Instructor)
                    .FirstOrDefaultAsync(predicate);
            }

        public async Task<bool> Any(Expression<Func<User, bool>> expression)
            {
                return await _qrCodeDbContext.Set<User>()
                    .AnyAsync(expression);

            }
    }       

}