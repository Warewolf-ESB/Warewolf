using System;
using System.Collections.ObjectModel;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;

namespace Dev2.Validation
{
    public class IsValidExpressionRule : Rule<string>
    {
        readonly string _variableValue;
        readonly ObservableCollection<ObservablePair<string, string>> _inputs;
        string _outputValue;

        public IsValidExpressionRule(Func<string> getValue, string variableValue = "a", ObservableCollection<ObservablePair<string, string>> inputs = null)
            : base(getValue)
        {
            _variableValue = variableValue;
            _inputs = inputs;
        }

        public string ExpressionValue { get { return _outputValue; } }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            var result = value.TryParseVariables(out _outputValue, DoError, LabelText, _variableValue, _inputs);
            if(!HasError(result, value))
            {
                result = value.TryParseRecordsetVariables(DoError, LabelText, _variableValue, _inputs);
                if(!HasError(result, value))
                {
                    result = value.TryParseVariableSpecialChars(DoError, LabelText, _variableValue, _inputs);
                    if(!HasError(result, value))
                    {
                        result = value.TryParseIsValidRecordset(DoError, LabelText, _variableValue, _inputs);
                        HasError(result, value);
                    }
                }
            }
            return result;
        }

        private bool HasError(IActionableErrorInfo result, string value)
        {
            if(result != null)
            {
                if(string.Equals(value, _outputValue))
                {
                    _outputValue = _variableValue;
                }
                return true;
            }
            return false;
        }
    }
}