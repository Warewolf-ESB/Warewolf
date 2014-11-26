
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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;

namespace Dev2.Activities.Debug
{
    public class DebugItemStaticDataParams : DebugOutputBase
    {
        readonly string _value;
        readonly string _labelText;
        readonly string _operand;
        readonly string _variable;
        readonly DebugItemResultType _type;

        public DebugItemStaticDataParams(string value, string labelText)
        {
            _value = value;
            _labelText = labelText;
            _type = DebugItemResultType.Value;
        }

        public DebugItemStaticDataParams(string value, string variable, string labelText)
        {
            _value = value;
            _labelText = labelText;
            _variable = variable;
            _type = DebugItemResultType.Variable;
        }

        public DebugItemStaticDataParams(string value, string variable, string labelText, string operand)
        {
            _value = value;
            _labelText = labelText;
            _operand = operand;
            _variable = variable;
            _type = DebugItemResultType.Variable;
        }

        public string Value
        {
            get
            {
                return _value;
            }
        }

        public override string LabelText
        {
            get
            {
                return _labelText;
            }
        }

        public string Variable
        {
            get
            {
                return _variable;
            }
        }
        public DebugItemResultType Type
        {
            get
            {
                return _type;
            }
        }

        public override List<IDebugItemResult> GetDebugItemResult()
        {

            var debugItemsResults = new List<IDebugItemResult>{
                 new DebugItemResult
                    {
                        Type = Type, 
                        Value = Value,
                        Label = LabelText,
                        Variable = Variable,
                        Operator = string.IsNullOrWhiteSpace(_operand) ? "" : "="
                    }};
            return debugItemsResults;
        }
    }
}
