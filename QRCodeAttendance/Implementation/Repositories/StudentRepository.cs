using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Models.Enums;
using QRCodeAttendance.Persistence.QRCodeAttendanceDb;

namespace QRCodeAttendance.Implementation.Repositories
{
    public class StudentRepository : BaseRepository, IStudentRepository
        {
            public StudentRepository(QRCodeDbContext qrCodeDbContext) : base(qrCodeDbContext)
            {
                
            }

       

        public async Task<IReadOnlyList<Student>> GetStudentsByCriteria(Departments department, StudentLevel level)
            {
                var students = await _qrCodeDbContext.Students
                    .AsNoTracking()
                    .Where(s => s.Department == department && s.StudentLevel == level)
                    .ToListAsync();

                return students;
            }

    }
    
}