using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Interface.Repositories
{
    public interface IStudentRepository : IBaseRepository
    {
        Task<IReadOnlyList<Student>> GetStudentsByCriteria(Departments department, StudentLevel level);
        

    }
}