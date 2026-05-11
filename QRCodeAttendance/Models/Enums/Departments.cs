using System.Text.Json.Serialization;

namespace QRCodeAttendance.Models.Enums
{
     [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Departments
    {
        ElectricalDepartment = 1,
        HardWareDepartment,
        WebDesignDepartment,
        SoftwareDepartment,
        PlumbingDepartment,
        BuildingDepartment
           
    }
}