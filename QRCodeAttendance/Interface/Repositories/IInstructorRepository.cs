using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IInstructorRepository : IBaseRepository
        {
            // Task<Instructor?> GetByUserId(Guid userId);
            Task<Instructor?> GetInstructorById(Guid instructorId);
            // Task<IReadOnlyList<Instructor>> GetAllInstructors();
        }
}