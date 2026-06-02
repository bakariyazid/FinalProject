using System.Text.Json.Serialization;

namespace QRCodeAttendance.Models.Enums
{
        [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Incomplete,
        Excused

    }
}
