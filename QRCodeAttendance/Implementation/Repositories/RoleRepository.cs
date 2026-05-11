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
    public class RoleRepository : BaseRepository, IRoleRepository
    {
         public RoleRepository(QRCodeDbContext qrCodeDbContext) : base(qrCodeDbContext)
            {

            }

        public async Task<Role?> GetByName(string RoleName)
        {
            return await _qrCodeDbContext.Set<Role>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == RoleName);
        }

        public async Task<IEnumerable<Role>> GetRoles()
                {
                    return await _qrCodeDbContext.Set<Role>()
                        .AsNoTracking()
                        .ToListAsync();
                }

        public async Task<IEnumerable<Role>> GetRolesByIdsAsync(Expression<Func<Role, bool>> expression)
            {
                return await _qrCodeDbContext.Set<Role>()
                    .Where(expression)
                    .AsNoTracking()
                    .ToListAsync();
            }
    }
}