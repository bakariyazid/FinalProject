using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using QRCodeAttendance.Contract.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
  public interface IBaseRepository
    {
        Task<T> Add<T> (T entity) where T : BaseEntity;
        void Update<T> (T entity) where T : BaseEntity;
        Task Delete<T> (T entity) where T : BaseEntity;
        Task<T> Get<T> (Expression<Func<T, bool>> expression) where T : BaseEntity;
        Task<IReadOnlyList<T>> GetAll<T>() where T : BaseEntity;
        IQueryable<T> QueryWhere<T>(Expression<Func<T, bool>> expression) where T : BaseEntity;
        Task<int> Count<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;
    }
}