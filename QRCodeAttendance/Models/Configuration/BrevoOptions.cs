namespace QRCodeAttendance.Models.Configuration
{
    public class BrevoOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SenderName { get; set; } = "MITC QRCode Attendance";
        public string SenderEmail { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
    }
}
