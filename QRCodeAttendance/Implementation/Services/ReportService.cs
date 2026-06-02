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

        public async Task<CourseReportDto> GenerateSessionReportAsync(Guid sessionId, Guid instructorId)
        {
            var session = await _sessionRepository.GetSessionWithAttendances(sessionId);

            if (session == null || session.InstructorId != instructorId)
            {
                return new CourseReportDto
                {
                    SessionId = sessionId,
                    IsReportAvailable = false,
                    Message = "Report not found for this instructor."
                };
            }

            var reportAvailableFrom = session.SessionEndTime.AddMinutes(-10);
            var now = DateTime.UtcNow;
            var orderedAttendances = session.Attendances
                .OrderBy(a => a.ScanTime)
                .ToList();

            var report = new CourseReportDto
            {
                SessionId = session.Id,
                CourseName = session.CourseName,
                CourseCode = session.CourseCode,
                InstructorId = session.InstructorId,
                InstructorName = session.Instructor?.FullName(),
                SessionStartTime = session.SessionStartTime,
                SessionEndTime = session.SessionEndTime,
                ReportAvailableFrom = reportAvailableFrom,
                IsReportAvailable = now >= reportAvailableFrom,
                TotalSessions = 1,
                AttendanceRecords = orderedAttendances.Select(a => new AttendanceRecordDto
                {
                    StudentName = a.Student?.FullName() ?? a.StudentName,
                    StudentEmail = a.Student?.Email ?? string.Empty,
                    SessionDate = session.SessionStartTime,
                    ScanTime = a.ScanTime,
                    Status = a.Status.ToString()
                }).ToList()
            };

            report.TotalPresent = orderedAttendances.Count(a => a.Status == AttendanceStatus.Present);
            report.TotalLate = orderedAttendances.Count(a => a.Status == AttendanceStatus.Incomplete);
            report.TotalAbsent = orderedAttendances.Count(a => a.Status == AttendanceStatus.Absent);

            var totalScans = orderedAttendances.Count;
            report.AverageAttendancePercentage = totalScans == 0
                ? 0
                : Math.Round((double)report.TotalPresent / totalScans * 100, 2);

            report.DailyStats.Add(new DailyStatDto
            {
                Date = session.SessionStartTime,
                Count = report.TotalPresent
            });

            if (!report.IsReportAvailable)
            {
                report.Message = $"Report will be available when the QR Code expires at {reportAvailableFrom.ToLocalTime():hh:mm tt}.";
            }

            return report;
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
                    StudentName = a.Student?.FullName() ?? a.StudentName, 
                    StudentEmail = a.Student?.Email ?? string.Empty,
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
                        a.Status == AttendanceStatus.Present);
                    
                    double studentRate = Math.Round(((double)attendedCount / report.TotalSessions) * 100, 1);

                    if (studentRate < 75)
                    {
                        report.AtRiskStudents.Add(new StudentRiskDto
                        {
                            Name = student.Data?.FullName() ?? "Unknown Student",
                            RegNumber = student.Data?.MatricNumber, 
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
                    Count = s.Attendances.Count(a => a.Status == AttendanceStatus.Present)
                }).ToList();

            return report;
        }
    }
}
