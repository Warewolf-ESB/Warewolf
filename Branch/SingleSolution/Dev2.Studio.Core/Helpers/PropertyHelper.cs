using System;
using System.Linq;
using System.Reflection;

namespace Dev2.Helpers
{
    public static class PropertyHelper
    {
        public static void SetValues(Object instance, Tuple<string, object>[] properties)
        {
            if (instance == null) return;
            if (properties == null || !properties.Any()) return;

            foreach (var property in properties)
            {
                SetValue(instance, property);
            }
        }

        public static void SetValue(Object instance, Tuple<string, object> tuple)
        {
            if (instance == null) return;

            var prop = instance.GetType().GetProperty(tuple.Item1, BindingFlags.Public | BindingFlags.Instance);
            if (null != prop && prop.CanWrite)
            {
                prop.SetValue(instance, tuple.Item2, null);
            }
        }
    }
}
