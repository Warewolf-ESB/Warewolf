using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Dev2.Studio.Core.AppResources.Attributes;

namespace Dev2.Studio.Core.AppResources.ExtensionMethods
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static string GetTreeDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(TreeCategory))
                        as TreeCategory;

            return attribute == null ? value.ToString() : attribute.Value;
        }

        public static string GetIconLocation(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(IconLocation))
                        as IconLocation;

            return attribute == null ? string.Empty : attribute.Value;
        }

        public static int GetDisplayOrder(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(DisplayAttribute))
                        as DisplayAttribute;

            return attribute == null ? 0 : attribute.Order;
        }
    }
}
