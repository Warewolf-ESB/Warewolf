
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
