using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Session
{
    public class CreateSessionRequestModel
    {   
        [Required(ErrorMessage = "Course name is required")]
        public string CourseName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Course code is required")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a student level")]
        public StudentLevel Level { get; set; }
        public Departments Department { get; set; }
        public DateTime QRCodeExpiry { get; set; }
        public DateTime SessionStartTime { get; set; }
        public DateTime SessionEndTime { get; set; }
        
    }

    public class UpdateSessionRequestModel
    {
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public StudentLevel Level { get; set; }
        public Departments Department { get; set; }
        public DateTime SessionStartTime { get; set; }
        public DateTime SessionEndTime { get; set; }
    }
}
