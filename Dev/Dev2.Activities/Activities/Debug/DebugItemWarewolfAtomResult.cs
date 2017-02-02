using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.Diagnostics;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

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
        private readonly bool _mockSelected;

        public DebugItemWarewolfAtomResult(string value, string variable, string leftLabel, bool mockSelected = false)
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

        public DebugItemWarewolfAtomResult(string value, string newValue, string variable, string assignFromVariable, string leftLabel, string rightLabel, string operand, bool mockSelected = false)
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