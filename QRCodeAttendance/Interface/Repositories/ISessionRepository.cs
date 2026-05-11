using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface ISessionRepository : IBaseRepository
        {
            Task<Session?> GetActiveSession();
            Task<IReadOnlyList<Session>> GetAll(Expression<Func<Session, bool>> expression);
            
            Task<Session?> GetSessionWithAttendances(Guid sessionId);
            Task<List<Session>> GetSessionsByCourseAndInstructorAsync(string courseCode, Guid instructorId);
            Task<IReadOnlyList<Session>> GetSessionsByCourseName(string courseName);
            Task<IReadOnlyList<Session>> GetSessionsByInstructorId(Guid instructorId);
            Task<IEnumerable<Session>> GetSessionsByDate(DateTime date);
        }
    
}