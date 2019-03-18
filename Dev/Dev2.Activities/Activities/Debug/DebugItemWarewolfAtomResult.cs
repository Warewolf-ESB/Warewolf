#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.Diagnostics;



namespace Dev2.Activities.Debug
{
    public class DebugItemWarewolfAtomResult : DebugOutputBase
    {
        readonly string _value;
        readonly string _newValue;
        readonly string _leftLabel;
        readonly string _rightLabel;
        readonly string _operand;
        readonly string _assignFromVariable;
        readonly bool _mockSelected;

        public DebugItemWarewolfAtomResult(string value, string variable, string leftLabel)
            : this(value, variable, leftLabel, false)
        {
        }

        public DebugItemWarewolfAtomResult(string value, string variable, string leftLabel, bool mockSelected)
        {
            _value = value;
            _leftLabel = leftLabel;
            Variable = variable;
            Type = DebugItemResultType.Variable;
            _operand = "=";
            _mockSelected = mockSelected;
            if (_mockSelected)
            {
                _value = variable + " " + _operand + " " + value;
            }

        }

        public DebugItemWarewolfAtomResult(string value, string variable, string leftLabel, string operand)
            : this(value, variable, leftLabel, operand, false)
        {
        }

        public DebugItemWarewolfAtomResult(string value, string variable, string leftLabel, string operand, bool mockSelected)
            :this(value, variable, leftLabel, mockSelected)
        {
            if(operand != null)
            {
                _operand = operand;
            }
        }

        public DebugItemWarewolfAtomResult(string value, string newValue, string variable, string assignFromVariable, string leftLabel, string rightLabel, string operand)
            : this(value, newValue, variable, assignFromVariable, leftLabel, rightLabel, operand, false)
        {
        }

        public DebugItemWarewolfAtomResult(string value, string newValue, string variable, string assignFromVariable, string leftLabel, string rightLabel, string operand, bool mockSelected)
        {
            _value = value;
            _newValue = newValue;
            _leftLabel = leftLabel;
            _rightLabel = rightLabel;
            _operand = operand;
            Variable = variable;
            _assignFromVariable = assignFromVariable;
            Type = DebugItemResultType.Variable;
            _mockSelected = mockSelected;
        }

        public string Value => _value;

        public override string LabelText => _leftLabel;

        public string Variable { get; }

        public DebugItemResultType Type { get; }

        public override List<IDebugItemResult> GetDebugItemResult()
        {

            var debugItemsResults = new List<IDebugItemResult>();
            if (!string.IsNullOrEmpty(_leftLabel))
            {

                var debugItem = new DebugItemResult
                {
                    Type = Type,
                    Value = _value,
                    Label = _leftLabel,
                    Variable = Variable,
                    Operator = _operand,
                    MockSelected = _mockSelected
                };
                debugItemsResults.Add(debugItem);
            }
            else
            {
                var debugItem = new DebugItemResult
                {
                    Type = Type,
                    Value = _value,
                    Variable = Variable,
                    Operator = _operand,
                    MockSelected = _mockSelected
                };
                debugItemsResults.Add(debugItem);
            }
            if (!string.IsNullOrEmpty(_rightLabel))
            {
                var debugItem =
                new DebugItemResult
                {
                    Type = Type,
                    Value = _newValue,
                    Variable = DataListUtil.IsEvaluated(_assignFromVariable)?_assignFromVariable:null,
                    Label = _rightLabel,
                    Operator = DataListUtil.IsEvaluated(_assignFromVariable) ? "=" : "",
                    MockSelected = _mockSelected
                };
                debugItemsResults.Add(debugItem);
            }
            return debugItemsResults;
        }
    }
}