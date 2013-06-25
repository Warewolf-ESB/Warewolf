using System;
using System.Linq;
using System.Reflection;

namespace Dev2.Common.ExtMethods
{
    public static class FieldInfoExtensionMethods
    {
        public static T GetCustomAttribute<T>(this FieldInfo fieldinfo) where T : Attribute
        {
            return fieldinfo.GetCustomAttributes(typeof(T), true).OfType<T>().FirstOrDefault();
        }
    }
}
