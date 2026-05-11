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
    public class InstructorRepository : BaseRepository, IInstructorRepository
    {
        public InstructorRepository(QRCodeDbContext qrCodeDbContext) : base(qrCodeDbContext)
        {

        }

        public async Task<Instructor?> GetByUserId(Guid userId)
        {
            return await _qrCodeDbContext.Instructors
                .FirstOrDefaultAsync(i => i.UserId == userId);
        }

        public async Task<Instructor?> GetInstructorById(Guid instructorId)
            {
                return await _qrCodeDbContext.Instructors
                    .FirstOrDefaultAsync(i => i.Id == instructorId);      
            }

        public async Task<IReadOnlyList<Instructor>> GetAllInstructors()
        {
            return await _qrCodeDbContext.Instructors.ToListAsync();
        }

    }
}