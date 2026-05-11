using System.Text.Json.Serialization;

namespace QRCodeAttendance.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StudentLevel
    {
        HundredLevel = 1,
        TwoHundredLevel

    }
}