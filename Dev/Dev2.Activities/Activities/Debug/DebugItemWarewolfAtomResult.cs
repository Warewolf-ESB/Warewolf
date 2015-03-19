using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;

namespace Dev2.Activities.Debug
{
    public class DebugItemWarewolfAtomResult : DebugOutputBase
    {
        readonly string _value;
        readonly string _labelText;
        readonly string _operand;
        readonly string _variable;
        readonly DebugItemResultType _type;

        public DebugItemWarewolfAtomResult(string value, string labelText)
        {
            _value = value;
            _labelText = labelText;
            _type = DebugItemResultType.Value;
        }

        public DebugItemWarewolfAtomResult(string value, string variable, string labelText)
        {
            _value = value;
            _labelText = labelText;
            _variable = variable;
            _type = DebugItemResultType.Variable;
        }

        public DebugItemWarewolfAtomResult(string value, string variable, string labelText, string operand)
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