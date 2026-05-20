using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Attendance
{
    public class CreateAttendanceRequestModel
    {
        public string QrCodeData { get; set; } = string.Empty;
    }
}
