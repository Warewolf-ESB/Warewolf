using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Common
{
    public static class Dev2EnumConverter
    {
        public static IList<TTEnum> GetEnumsToList<TTEnum>() where TTEnum : struct
        {
            Type type = typeof(TTEnum);
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");
            return Enum.GetValues(type).Cast<TTEnum>().ToList();
        }

        public static IList<string> ConvertEnumsToStringList<tEnum>() where tEnum : struct
        {
            Type type = typeof(tEnum);
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");


            object[] allAttribs = type.GetCustomAttributes(false);
            EnumDisplayStringAttribute displayAttribute = null;

            if (allAttribs != null && allAttribs.Length > 0)
            {
                foreach (object current in allAttribs)
                {
                    if (current is EnumDisplayStringAttribute)
                    {
                        displayAttribute = current as EnumDisplayStringAttribute;
                        break;
                    }
                }
            }

            IList<string> result;

            if (displayAttribute != null)
            {
                result = new List<string>(displayAttribute.Names);
            }
            else
            {
                result = new List<string>(Enum.GetNames(type));
            }

            return result;
        }

        public static string ConvertEnumValueToString(Enum value)
        {
            Type type = value.GetType();
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");


            object[] allAttribs = type.GetCustomAttributes(false);
            EnumDisplayStringAttribute displayAttribute = null;

            if (allAttribs != null && allAttribs.Length > 0)
            {
                foreach (object current in allAttribs)
                {
                    if (current is EnumDisplayStringAttribute)
                    {
                        displayAttribute = current as EnumDisplayStringAttribute;
                        break;
                    }
                }
            }

            string result;

            if (displayAttribute != null)
            {
                int i = Convert.ToInt32(value);
                result = displayAttribute.Names[i];
                //result = new List<string>(displayAttribute.Names);
            }
            else
            {
                result = value.ToString();
            }

            return result;
        }

        public static TEnum GetEnumFromStringValue<TEnum>(string name) where TEnum : struct
        {
            Type type = typeof(TEnum);
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");

            object[] allAttribs = type.GetCustomAttributes(false);
            EnumDisplayStringAttribute displayAttribute = null;

            if (allAttribs != null && allAttribs.Length > 0)
            {
                foreach (object current in allAttribs)
                {
                    if (current is EnumDisplayStringAttribute)
                    {
                        displayAttribute = current as EnumDisplayStringAttribute;
                        break;
                    }
                }
            }

            if (displayAttribute != null)
            {
                for (int i = 0; i < displayAttribute.Names.Length; i++)
                {
                    if (String.Equals(displayAttribute.Names[i], name, StringComparison.Ordinal))
                    {
                        return (TEnum)Enum.ToObject(type, i);
                    }
                }
            }

            TEnum result;

            if (Enum.TryParse<TEnum>(name, out result))
            {
                return result;
            }

            throw new ArgumentException("name is not a valid enum member name.");
        }

        public static object GetEnumFromStringValue(string name, Type type)
        {
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");

            object[] allAttribs = type.GetCustomAttributes(false);
            EnumDisplayStringAttribute displayAttribute = null;

            if (allAttribs != null && allAttribs.Length > 0)
            {
                foreach (object current in allAttribs)
                {
                    if (current is EnumDisplayStringAttribute)
                    {
                        displayAttribute = current as EnumDisplayStringAttribute;
                        break;
                    }
                }
            }

            if (displayAttribute != null)
            {
                for (int i = 0; i < displayAttribute.Names.Length; i++)
                {
                    if (String.Equals(displayAttribute.Names[i], name, StringComparison.Ordinal))
                    {
                        return Enum.ToObject(type, i);
                    }
                }
            }

            foreach (var value in Enum.GetValues(type))
            {
                if (value.ToString() == name)
                {
                    return value;
                }
            }

            throw new ArgumentException("name is not a valid enum member name.");
        }
    }

    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class EnumDisplayStringAttribute : Attribute
    {
        private string[] _names;

        public string[] Names { get { return _names; } }

        public EnumDisplayStringAttribute(params string[] names)
        {
            _names = names;
        }
    }

}
