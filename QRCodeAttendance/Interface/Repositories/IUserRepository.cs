using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IUserRepository : IBaseRepository
    {
            Task<User?> GetByEmail(string email);
            Task<IReadOnlyList<User>> GetUsersByRole(Role role);
            Task<User?> GetUserWithRole(Expression<Func<User, bool>> predicate);
            Task<bool> Any(Expression<Func<User, bool>> expression);
        
    }
}