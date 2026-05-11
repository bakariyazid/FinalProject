using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Models.DTOs.Reports
{
    public class CreateReportRequestModel
    {
        public string? CourseCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
}
}