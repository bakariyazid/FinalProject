using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IRoleRepository : IBaseRepository
    {
        Task<Role?> GetByName(string RoleName);
        // // Task Add(Role role);
        // Task<Role?> GetById(Guid id);
        // Task<IReadOnlyList<Role>> GetAll();
        Task<IEnumerable<Role>> GetRolesByIdsAsync(Expression<Func<Role, bool>> expression);
        Task<IEnumerable<Role>> GetRoles();

    }
}