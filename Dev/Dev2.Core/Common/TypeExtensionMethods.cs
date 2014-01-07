using System;
using System.Collections;
using System.Linq;

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
