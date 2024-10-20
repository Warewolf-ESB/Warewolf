/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Newtonsoft.Json.Linq;

namespace Dev2.Common
{
    public static class JPropertyExtensionMethods
    {
        public static bool IsEnumerable(this JProperty property) => property.Value is JArray;

        public static bool IsEnumerableOfPrimitives(this JProperty property)
        {
            var returnValue = false;

            if (property.Value is JArray array && array.Count > 0)
            {
                returnValue = array[0].IsPrimitive();
            }

            return returnValue;
        }

        public static bool IsPrimitive(this JProperty property) => property.Value is JValue;
    }
}