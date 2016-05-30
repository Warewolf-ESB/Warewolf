/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable InconsistentNaming

namespace Dev2.Common.Utils
{
    public static class JSONUtils
    {
        /// <summary>
        ///     Scrubs the JSON.
        /// </summary>
        /// <param name="stringToScrub">The string to scrub.</param>
        /// <returns></returns>
        public static string ScrubJSON(string stringToScrub)
        {
            if (!string.IsNullOrEmpty(stringToScrub))
            {
                if (stringToScrub.StartsWith("\""))
                {
                    stringToScrub = stringToScrub.Remove(0, 1);
                }

                if (stringToScrub.EndsWith("\""))
                {
                    int indexTORemoveFrom = stringToScrub.Length - 1;
                    stringToScrub = stringToScrub.Remove(indexTORemoveFrom, 1);
                }
            }
            return stringToScrub;
        }

        /// <summary>
        ///     Replaces the slashes.
        /// </summary>
        /// <param name="stringToReplaceIn">The string to replace in.</param>
        /// <returns></returns>
        public static string ReplaceSlashes(string stringToReplaceIn)
        {
            string replaceSlashes = stringToReplaceIn.Replace(@"\", @"\\");
            return replaceSlashes;
        }

        public static IList<IServiceInput> CreateServiceInputMappingsFromJsonString(string jsonString)
        {
            var serviceInputMappingsFromJsonString = new List<IServiceInput>();
            var obj = JsonConvert.DeserializeObject(jsonString) as JObject;
            if (obj != null)
            {
                
            }
            return serviceInputMappingsFromJsonString;
        }
    }
}