using System;
using System.ComponentModel;
using System.Reflection;

namespace StreamingServices.Helpers
{
    public static class EnumDescription
    {
        public static string GetDescription(this Enum @enum)
        {
            var defName = @enum.ToString();
            var name = @enum
                .GetType()
                .GetField(defName)
                ?.GetCustomAttribute<DescriptionAttribute>()
                .Description;

            return name ?? defName;
        }
    }
}
