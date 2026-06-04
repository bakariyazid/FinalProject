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

    public class OfflineAttendanceScanRequestModel
    {
        public Guid SessionId { get; set; }
        public string QrCode { get; set; } = string.Empty;
        public DateTime ScannedAt { get; set; }
        public string ClientScanId { get; set; } = string.Empty;
    }

    public class OfflineAttendanceScanResponseModel
    {
        public string ClientScanId { get; set; } = string.Empty;
        public bool Synced { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
