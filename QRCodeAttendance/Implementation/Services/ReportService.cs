using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs.Reports;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Implementation.Services
{
    public class ReportService : IReportService
    {
        private readonly ISessionRepository _sessionRepository;

        public ReportService(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }


   public async Task<CourseReportDto> GenerateCourseReportAsync(string courseCode, Guid instructorId)
        {
            // 1. Fetch sessions from repository
            var sessions = await _sessionRepository.GetSessionsByCourseAndInstructorAsync(courseCode, instructorId);

            if (sessions == null || !sessions.Any())
            {
                return new CourseReportDto { CourseCode = courseCode }; 
            }

            var headerInfo = sessions.First();

            // 2. Map data to the DTO
            var report = new CourseReportDto
            {
                CourseName = headerInfo.CourseName,
                CourseCode = headerInfo.CourseCode,
                TotalSessions = sessions.Count,
                
                AttendanceRecords = sessions.SelectMany(s => s.Attendances.Select(a => new AttendanceRecordDto
                {
                    StudentName = a.Student.FullName(), 
                    StudentEmail = a.Student.Email,
                    SessionDate = s.SessionStartTime,
                    ScanTime = a.ScanTime,
                    Status = a.Status.ToString()
                })).ToList()
            };

            var allAttendances = sessions.SelectMany(s => s.Attendances).ToList();
            report.TotalPresent = allAttendances.Count(a => a.Status == AttendanceStatus.Present);
            report.TotalLate = allAttendances.Count(a => a.Status == AttendanceStatus.Late);
            report.TotalAbsent = allAttendances.Count(a => a.Status == AttendanceStatus.Absent);

            var uniqueStudents = allAttendances
                .GroupBy(a => a.StudentId)
                .Select(g => new { 
                    StudentId = g.Key, 
                    Data = g.First().Student, 
                    Attendances = g.ToList() 
                }).ToList();

            if (uniqueStudents.Any() && report.TotalSessions > 0)
            {
                double totalPossible = report.TotalSessions * uniqueStudents.Count;
                report.AverageAttendancePercentage = Math.Round((double)report.TotalPresent / totalPossible * 100, 2);

                foreach (var student in uniqueStudents)
                {
                    int attendedCount = student.Attendances.Count(a => 
                        a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
                    
                    double studentRate = Math.Round(((double)attendedCount / report.TotalSessions) * 100, 1);

                    if (studentRate < 75)
                    {
                        report.AtRiskStudents.Add(new StudentRiskDto
                        {
                            Name = student.Data.FullName(),
                            RegNumber = student.Data.MatricNumber, 
                            AttendanceRate = studentRate
                        });
                    }
                }
            }

            report.DailyStats = sessions
                .OrderBy(s => s.SessionStartTime)
                .Select(s => new DailyStatDto
                {
                    Date = s.SessionStartTime,
                    Count = s.Attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
                }).ToList();

            return report;
        }
    }
}