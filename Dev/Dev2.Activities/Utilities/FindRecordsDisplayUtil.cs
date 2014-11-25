
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Linq;

namespace Dev2.Utilities
{
    public static class FindRecordsDisplayUtil
    {
        private static Dictionary<string, string> changedOptions;

        public static string ConvertForDisplay(string key)
        {
            InitDictionary();

            string value;

            if(!changedOptions.TryGetValue(key, out value))
            {
                value = key;
            }

            return value;
        }

        public static string ConvertForWriting(string key)
        {
            InitDictionary();            

            KeyValuePair<string, string> firstOrDefault = changedOptions.FirstOrDefault(c => c.Value == key);
            if(firstOrDefault.Key != null && firstOrDefault.Value!= null)
            {
                return firstOrDefault.Key;
            }

            return key;
        }

        private static void InitDictionary()
        {
            if(changedOptions == null)
            {
                changedOptions = new Dictionary<string, string>();
                changedOptions.Add("Equals", "=");
                changedOptions.Add("Not Equals", "<> (Not Equal)");               
                changedOptions.Add("Not Contains", "Doesn't Contain");
                changedOptions.Add("Regex", "Is Regex");
            }
        }

    }
}
