/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;

namespace Dev2.Activities.Debug
{
    public class DebugItemStaticDataParams : DebugOutputBase
    {
        readonly string _operand;

        public DebugItemStaticDataParams(string value, string labelText, bool mockSelected = false)
        {
            Value = value;
            LabelText = labelText;
            Type = DebugItemResultType.Value;
            MockSelected = mockSelected;
        }

        public DebugItemStaticDataParams(string value, string variable, string labelText, bool mockSelected = false)
        {
            Value = value;
            LabelText = labelText;
            Variable = variable;
            Type = DebugItemResultType.Variable;
            MockSelected = mockSelected;
        }

        public DebugItemStaticDataParams(string value, string variable, string labelText, string operand, bool mockSelected = false)
        {
            Value = value;
            LabelText = labelText;
            _operand = operand;
            Variable = variable;
            Type = DebugItemResultType.Variable;
            MockSelected = mockSelected;
        }

        public string Value { get; }

        public bool MockSelected { get; }

        public override string LabelText { get; }

        public string Variable { get; }

        public DebugItemResultType Type { get; }

        public override List<IDebugItemResult> GetDebugItemResult()
        {

            var debugItemsResults = new List<IDebugItemResult>{
                 new DebugItemResult
                    {
                        Type = Type, 
                        Value = Value,
                        Label = LabelText,
                        Variable = Variable,
                        Operator = string.IsNullOrWhiteSpace(_operand) ? "" : "=",
                        MockSelected = MockSelected
                    }};
            return debugItemsResults;
        }
    }
}
