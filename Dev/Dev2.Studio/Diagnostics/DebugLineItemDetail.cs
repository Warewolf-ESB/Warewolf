
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Studio.Diagnostics
{
    public class DebugLineItemDetail
    {
        public DebugLineItemDetail()
        {
        }

        public DebugLineItemDetail(DebugItemResultType type, string value, string moreLink, string variable)
        {
            Type = type;
            Value = value;
            MoreLink = moreLink;
            Variable = variable;
        }

        public string MoreLink { get; set; }
        public DebugItemResultType Type { get; set; }
        public string Value { get; set; }
        public string Variable { get; set; }
    }
}
