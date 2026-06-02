using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Persistence.QRCodeAttendanceDb;

namespace QRCodeAttendance.Implementation.Repositories
{
public class BaseRepository : IBaseRepository
{
    protected readonly QRCodeDbContext _qrCodeDbContext;

    public BaseRepository(QRCodeDbContext qrCodeDbContext)
    {
        _qrCodeDbContext = qrCodeDbContext ?? throw new ArgumentNullException(nameof(qrCodeDbContext));
    }

    public virtual async Task<T> Add<T>(T entity) where T : BaseEntity
    {
        var entry = await _qrCodeDbContext.Set<T>().AddAsync(entity);
        return entry.Entity;
    }

    public virtual async Task<int> Count<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
    {
        return await _qrCodeDbContext.Set<T>().CountAsync(predicate);
    }

    public virtual void Update<T>(T entity) where T : BaseEntity
    {
        _qrCodeDbContext.Set<T>().Update(entity);
    }

    public virtual async Task Delete<T>(T entity) where T : BaseEntity
    {
        _qrCodeDbContext.Set<T>().Remove(entity);
    }

    public virtual async Task<T> Get<T>(Expression<Func<T, bool>> expression) where T : BaseEntity
    {  
         #pragma warning disable CS8603
        return await _qrCodeDbContext.Set<T>().FirstOrDefaultAsync(expression);
        #pragma warning restore CS8603
    }

    public virtual async Task<IReadOnlyList<T>> GetAll<T>() where T : BaseEntity
    {
        return await _qrCodeDbContext.Set<T>().ToListAsync();
    }

    public virtual IQueryable<T> QueryWhere<T>(Expression<Func<T, bool>> expression) where T : BaseEntity
    {
        return _qrCodeDbContext.Set<T>().Where(expression);
    }
}
}