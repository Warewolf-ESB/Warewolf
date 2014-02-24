using System;
using System.ComponentModel;
using Dev2.Studio.UI.Tests.Enums;

namespace Dev2.Common.ExtMethods
{
    public static class BaseEnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static string GetToolboxName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(ToolboxNameAttribute))
                        as ToolboxNameAttribute;

            return attribute == null ? value.ToString() : attribute.ToolboxName;
        }
    }
}
