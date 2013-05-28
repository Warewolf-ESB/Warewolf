using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dev2.Utilities
{
    public class StringAttributeRefectionUtils
    {
        public static IEnumerable<PropertyInfo> ExtractAdornedProperties<T>(object objectToReflect)
        {
            Type sourceType = objectToReflect.GetType();
            IEnumerable<PropertyInfo> properties = sourceType.GetProperties().Where(c => c.GetCustomAttributes(typeof(T), true).Any());
            return properties;
        }    
    }
}
