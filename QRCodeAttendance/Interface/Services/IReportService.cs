using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.DTOs.Reports;

namespace QRCodeAttendance.Interface.Services
{
    public interface IReportService
    {
        Task<CourseReportDto> GenerateCourseReportAsync(string courseCode, Guid instructorId);
        Task<CourseReportDto> GenerateSessionReportAsync(Guid sessionId, Guid instructorId);
    }
}
