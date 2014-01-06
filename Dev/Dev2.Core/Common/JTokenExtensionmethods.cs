using Newtonsoft.Json.Linq;

namespace Dev2
{
    public static class JTokenExtensionmethods
    {
        public static bool IsEnumerable(this JToken token)
        {
            return token is JArray;
        }

        public static bool IsPrimitive(this JToken token)
        {
            return token is JValue;
        }

        public static bool IsObject(this JToken token)
        {
            return token is JObject;
        }

        public static bool IsEnumerableOfPrimitives(this JToken property)
        {
            bool returnValue = false;
            JArray array = property as JArray;

            if (array != null && array.Count > 0)
            {
                returnValue = array[0].IsPrimitive();
            }

            return returnValue;
        }
    }
}
