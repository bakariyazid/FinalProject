using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QRCodeAttendance.Models.Enums
{
     [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Departments
    {
        [Description("Electrical Department")]
        [Display(Name = "Electrical Department")]
        ElectricalDepartment = 1,

        [Description("Hardware Department")]
        [Display(Name = "Hardware Department")]
        HardWareDepartment,

        [Description("Web Design Department ")]
        [Display(Name = "Web Design Department ")]
        WebDesignDepartment,

        [Description("Software Department")]
        [Display(Name = "Software Department")]
        SoftwareDepartment,

        [Description("Plumbing Department")]
        [Display(Name = "Plumbing Department")]
        PlumbingDepartment,

        [Description("Building Department")]
        [Display(Name = "Building Department")]
        BuildingDepartment
           
    }
}