
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Runtime.Hosting
{
    public class ResourceDelimiter
    {
        public int ID { get; set; }
        public string Start { get; set; }
        public string End { get; set; }

        #region TryGetValue

        public bool TryGetValue(string content, out string thisValue)
        {
            thisValue = string.Empty;
            var startTokenLength = Start.Length;
            var startIdx = content.IndexOf(Start, StringComparison.InvariantCultureIgnoreCase);
            if(startIdx == -1)
            {
                return false;
            }
            startIdx += startTokenLength;
            var endIdx = content.IndexOf(End, startIdx, StringComparison.InvariantCultureIgnoreCase);
            var length = endIdx - startIdx;
            if(length > 0)
            {
                thisValue = content.Substring(startIdx, length);
                return true;
            }
            return false;
        }

        #endregion
    }
}
