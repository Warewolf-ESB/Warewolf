using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Dev2
{
    public static class TypeExtensionMethods
    {
        public static bool IsEnumerable(this Type type)
        {
            return type != typeof(string) && type.GetInterfaces().Contains(typeof(IEnumerable));
        }

        public static bool IsPrimitive(this Type type)
        {
            return type == typeof(string) || type.IsValueType || type.IsPrimitive;
        }
    }
}
