using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;

namespace Dev2.Activities.Debug
{
    public class DebugItemServiceTestStaticDataParams : DebugOutputBase
    {
        readonly string _operand;

        public DebugItemServiceTestStaticDataParams(string value)
        {
            Value = value;
            Type = DebugItemResultType.Value;
        }

        public DebugItemServiceTestStaticDataParams(string value, string variable)
        {
            Value = value;
            Variable = variable;
            Type = DebugItemResultType.Variable;
        }

        public DebugItemServiceTestStaticDataParams(string value, string variable, string operand)
        {
            Value = value;
            _operand = operand;
            Variable = variable;
            Type = DebugItemResultType.Variable;
        }

        public string Value { get; }

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
                        Operator = string.IsNullOrWhiteSpace(_operand) ? "" : "="
                    }};
            return debugItemsResults;
        }
    }
}