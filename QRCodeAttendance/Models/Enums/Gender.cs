using System.Text.Json.Serialization;

namespace QRCodeAttendance.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum Gender
    {
        Male = 1,
        Female,
        Other
    }
}