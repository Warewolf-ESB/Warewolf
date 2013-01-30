using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Dev2
{
    public static class JPropertyExtensionMethods
    {
        public static bool IsEnumerable(this JProperty property)
        {
            return property.Value is JArray;
        }

        public static bool IsEnumerableOfPrimitives(this JProperty property)
        {
            bool returnValue = false;
            JArray array = property.Value as JArray;

            if (array != null && array.Count > 0)
            {
                returnValue = array[0].IsPrimitive();
            }

            return returnValue;
        }

        public static bool IsPrimitive(this JProperty property)
        {
            return property.Value is JValue;
        }
    }
}
