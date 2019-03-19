#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Dev2.Common.Utils
{
    public sealed class JsonNetValueSystem : IJsonPathValueSystem
    {
        public bool HasMember(object value, string member)
        {
            if (value is JObject)
            {
                return (value as JObject).Properties().Any(property => property.Name == member);
            }

            if (value is JArray)
            {
                var index = ParseInt(member, -1);
                return index >= 0 && index < (value as JArray).Count;
            }
            return false;
        }

        public object GetMemberValue(object value, string member)
        {
            if (value is JObject)
            {
                var memberValue = (value as JObject)[member];
                return memberValue;
            }
            if (value is JArray)
            {
                var index = ParseInt(member, -1);
                return (value as JArray)[index];
            }
            return null;
        }

        public IEnumerable GetMembers(object value)
        {
            var jobject = value as JObject;
            return jobject?.Properties().Select(property => property.Name);
        }

        public bool IsObject(object value) => value is JObject;

        public bool IsArray(object value) => value is JArray;

        public bool IsPrimitive(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return !(value is JObject) && !(value is JArray);
        }

        int ParseInt(string s, int defaultValue) => int.TryParse(s, out int result) ? result : defaultValue;
    }
}
