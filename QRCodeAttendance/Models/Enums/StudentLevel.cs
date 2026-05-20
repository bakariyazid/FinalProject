using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace QRCodeAttendance.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StudentLevel
    {
        [Description("100 Level")]
        [Display(Name = "100 Level")]
        HundredLevel = 1,
        
        [Description("200 Level")]
        [Display(Name = "200 Level")]
        TwoHundredLevel = 2,
      
    }
}
