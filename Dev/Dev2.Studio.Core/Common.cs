using System;
using System.Xml;
using Dev2.Data.Interfaces.Enums;

namespace Dev2.Studio.Core
{
    internal static class Common
    {
        public const string RootTag = "DataList";
        public const string Description = "Description";
        public const string IsEditable = "IsEditable";
        public static bool ParseIsEditable(XmlAttribute attr)
        {
            return ParseBoolAttribute(attr);
        }
        public static string ParseDescription(XmlAttribute attr)
        {
            var result = string.Empty;
            if (attr != null)
            {
                result = attr.Value;
            }
            return result;
        }

        public static bool ParseBoolAttribute(XmlAttribute attr)
        {
            var result = true;
            if (attr != null)
            {
                bool.TryParse(attr.Value, out result);
            }
            return result;
        }
        
        public static enDev2ColumnArgumentDirection ParseColumnIODirection(XmlAttribute attr)
    
        {
            enDev2ColumnArgumentDirection result = enDev2ColumnArgumentDirection.None;

            if (attr == null)
            {
                return result;
            }
            if (!Enum.TryParse(attr.Value, true, out result))
            {
                result = enDev2ColumnArgumentDirection.None;
            }
            return result;
        }
    }
}