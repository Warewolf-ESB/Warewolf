using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Dev2.Settings.Logging
{
    public static class EnumHelper<T>
    {
        public static string GetEnumDescription(string value)
        {
            Type type = typeof(T);
            var name = Enum.GetNames(type).Where(f => f.Equals(value, StringComparison.CurrentCultureIgnoreCase)).Select(d => d).FirstOrDefault();

            if (name == null)
            {
                return string.Empty;
            }
            var field = type.GetField(name);
            var customAttribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttribute.Length > 0 ? ((DescriptionAttribute)customAttribute[0]).Description : name;
        }

        public static LogLevel GetEnumFromDescription(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new InvalidOperationException();
            }
            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                    {
                        return (LogLevel)field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Name == description)
                    {
                        return (LogLevel)field.GetValue(null);
                    }
                }
            }
            throw new Exception();
        }

        public static IEnumerable<string> GetDiscriptionsAsList(Type type)
        {
            return type.GetEnumNames().Select(GetEnumDescription).ToList();
        }
    }
}