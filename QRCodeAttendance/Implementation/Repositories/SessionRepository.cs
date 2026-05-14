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
    public class SessionRepository(QRCodeDbContext qrCodeDbContext) : BaseRepository(qrCodeDbContext), ISessionRepository
    {

    
        public async Task<Session?> GetActiveSession()
        {
                return await _qrCodeDbContext.Sessions
                .FirstOrDefaultAsync(s => s.IsActive);
        }


        public async Task<List<Session>> GetSessionsByCourseAndInstructorAsync(string courseCode, Guid instructorId)
        {
            return await _qrCodeDbContext.Sessions
                .Include(s => s.Attendances)
                    .ThenInclude(a => a.Student)
                .Where(s => s.CourseCode == courseCode && s.InstructorId == instructorId)
                .OrderByDescending(s => s.SessionStartTime)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Session>> GetSessionsByCourseName(string courseName)
        {
            return await _qrCodeDbContext.Sessions
                .Where(s => s.CourseName == courseName)
                .ToListAsync();
        }

        public async Task<Session?> GetSessionWithAttendances(Guid sessionId)
        {
            return await _qrCodeDbContext.Sessions
                .Include(s => s.Attendances)
                    .ThenInclude(a => a.Student)
                .Include(s => s.Instructor)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<IReadOnlyList<Session>> GetSessionsByInstructorId(Guid instructorId)
        {
            return await _qrCodeDbContext.Sessions
                .Where(s => s.InstructorId == instructorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Session>> GetSessionsByDate(DateTime date)
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                return await _qrCodeDbContext.Sessions
                    .Where(s => s.SessionStartTime >= startDate && s.SessionStartTime < endDate)
                    .ToListAsync();
            }

        public async Task<IReadOnlyList<Session>> GetAll(Expression<Func<Session, bool>> expression)
        {
                return await _qrCodeDbContext.Sessions
                .AsNoTracking()
                .Include(s => s.Instructor)
                .Where(expression)
                .ToListAsync();
        }


    }
}
