/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Diagnostics.Debug;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public class DebugLineItem : IDebugLineItem
    {
        public DebugLineItem()
        {
        }

        public DebugLineItem(IDebugItemResult result)
        {
            Type = result.Type;
            Value = result.Value;
            MoreLink = result.MoreLink;
            Label = result.Label;
            Variable = result.Variable;
            Operator = result.Operator;
            TestStepHasError = result.HasError;
        }

        public string MoreLink { get; set; }
        public DebugItemResultType Type { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public string Variable { get; set; }
        public string Operator { get; set; }
        public bool TestStepHasError { get; set; }
    }
}
