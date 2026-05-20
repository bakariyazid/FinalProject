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

    public class AttendanceRepository : BaseRepository, IAttendanceRepository
    {
          public AttendanceRepository(QRCodeDbContext qrCodeDbContext) : base(qrCodeDbContext)
            {

            }

        public async Task<List<Attendance>> GetDetailedAttendanceByCourse(string courseCode, Guid instructorId)
            {
                return await _qrCodeDbContext.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.ClassSession)
                    .Where(a => a.ClassSession != null
                            && a.ClassSession.CourseCode == courseCode 
                            && a.ClassSession.InstructorId == instructorId)
                    .ToListAsync();
            }
        public async Task<IReadOnlyList<Attendance>> GetByStudentId(Guid studentId)
            {
                return await _qrCodeDbContext.Attendances
                    .AsNoTracking()
                    .Include(a => a.ClassSession)
                        .ThenInclude(s => s!.Instructor)
                    .Where(a => a.StudentId == studentId)
                    .OrderByDescending(a => a.ScanTime)
                    .ToListAsync();
            }

        public async Task<Attendance?> GetByStudentAndSession(Guid studentId, Guid sessionId)
            {
                return await _qrCodeDbContext.Attendances
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => 
                        a.StudentId == studentId && 
                        a.SessionId == sessionId);
            }

        public async Task<IReadOnlyList<Attendance>> GetBySession(Guid sessionId)
        {
                    return await _qrCodeDbContext.Attendances
                .Include(a => a.Student)
                .Include(a => a.ClassSession)
                .Where(a => a.SessionId == sessionId)
                .AsNoTracking()
                .OrderByDescending(a => a.ScanTime)
                .ToListAsync();
        }

      

        public async Task<IReadOnlyList<Attendance>> GetByUser(Guid userId)
        {
                return await _qrCodeDbContext.Attendances
                .Where(a => a.StudentId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> HasStudentMarkedAttendance(Guid studentId, Guid sessionId)
        {
            return await _qrCodeDbContext.Attendances.AnyAsync(a => a.StudentId == studentId && a.SessionId == sessionId);
        }
        public async Task<IReadOnlyList<Attendance>> GetAttendanceByInstructor(Guid instructorId)
        {
            return await _qrCodeDbContext.Attendances
                .Where(a => a.ClassSession != null && a.ClassSession.InstructorId == instructorId)
                .Include(a => a.ClassSession)
                .AsNoTracking()
                .ToListAsync();
        }
        
        public async Task<IReadOnlyList<Attendance>> GetAll(Guid studentId)
        {
            return await _qrCodeDbContext.Attendances
            .Where(a => a.StudentId == studentId)
            .ToListAsync();
        }

     public async Task<IReadOnlyList<Attendance>> GetAll(Expression<Func<Attendance, bool>> expression)
        {
            return await _qrCodeDbContext.Set<Attendance>()
                .Where(expression)
                .ToListAsync();
        }
    }
}
