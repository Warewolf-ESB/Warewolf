/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Diagnostics.Debug;


namespace Dev2.Diagnostics
{
    [Serializable]
    public class DebugItemResult : IDebugItemResult
    {
        public DebugItemResultType Type { get; set; }
        public string Label { get; set; }
        public string Variable { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public int GroupIndex { get; set; }
        public string MoreLink { get; set; }
        public bool HasError { get; set; }
        public bool TestStepHasError { get; set; }
        public bool MockSelected { get; set; }

        public string GetMoreLinkItem() => string.IsNullOrEmpty(Variable) ? Value : string.Format("{0} {1} {2}", Variable, Operator, Value);
    }
}
