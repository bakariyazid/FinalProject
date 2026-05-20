using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace QRCodeAttendance.Models.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            if (field == null)
            {
                return value.ToString();
            }

            var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
            if (!string.IsNullOrWhiteSpace(displayAttribute?.Name))
            {
                return displayAttribute.Name;
            }

            var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute?.Description ?? value.ToString();
        }
    }
}
