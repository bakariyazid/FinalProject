using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IAttendanceRepository : IBaseRepository
    {
        Task<List<Attendance>> GetDetailedAttendanceByCourse(string courseCode, Guid instructorId);
        public Task<IReadOnlyList<Attendance>> GetByUser(Guid userId);
        Task <bool> HasStudentMarkedAttendance(Guid studentId, Guid sessionId);
        Task<Attendance?> GetByStudentAndSession(Guid studentId, Guid sessionId);
        Task<IReadOnlyList<Attendance>> GetByStudentId(Guid studentId);
        Task<IReadOnlyList<Attendance>> GetBySession(Guid sessionId);
        Task<IReadOnlyList<Attendance>> GetAttendanceByInstructor(Guid instructorId);
        Task <IReadOnlyList<Attendance>> GetAll (Guid studentId);
        Task<IReadOnlyList<Attendance>> GetAll(Expression<Func<Attendance, bool>> expression);
        
    }
}