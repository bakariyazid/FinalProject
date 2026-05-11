using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Persistence.QRCodeAttendanceDb;

namespace QRCodeAttendance.Implementation.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly QRCodeDbContext _qrCodeDbContext;

        public UnitOfWork(QRCodeDbContext qrCodeDbContext)
        {
            _qrCodeDbContext = qrCodeDbContext ?? throw new ArgumentNullException (nameof(QRCodeDbContext));
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _qrCodeDbContext.Database.BeginTransactionAsync();
        }

          public IExecutionStrategy CreateExecutionStrategy()
        {
            return _qrCodeDbContext.Database.CreateExecutionStrategy();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _qrCodeDbContext.SaveChangesAsync(cancellationToken);
        }

        
    }
}