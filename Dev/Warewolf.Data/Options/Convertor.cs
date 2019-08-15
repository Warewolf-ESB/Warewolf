using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Warewolf.Data;

namespace Warewolf.Options
{
    public static class OptionConvertor
    {
        public static IOption[] Convert(object o)
        {
            var result = new List<IOption>();

            var type = o.GetType();
            var properties = type.GetProperties();
            foreach (var prop in properties)
            {
                result.Add(PropertyToOption(o, prop));
            }

            return result.ToArray();
        }

        private static IOption PropertyToOption(object instance, PropertyInfo prop)
        {
            if (prop.PropertyType.IsAssignableFrom(typeof(string)))
            {
                var attr = prop.GetCustomAttributes().Where(o => o is DataProviderAttribute).Cast<DataProviderAttribute>().FirstOrDefault();// typeof(DataProviderAttribute).IsAssignableFrom(o.GetType()));
                var result = new OptionAutocomplete()
                {
                    Name = prop.Name,
                    Value = (string)prop.GetValue(instance)
                };
                if (attr != null)
                {
                    result.Suggestions = ((IOptionDataList) attr.Get()).Options;
                }
                return result;
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(int)))
            {
                return new OptionInt
                {
                    Name = prop.Name,
                    Value = (int)prop.GetValue(instance)
                };
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(bool)))
            {
                return new OptionBool
                {
                    Name = prop.Name,
                    Value = (bool)prop.GetValue(instance)
                };
            }
            throw UnhandledException;
        }

        public static readonly System.Exception UnhandledException = new System.Exception("unhandled property type for option conversion");
    }
}
