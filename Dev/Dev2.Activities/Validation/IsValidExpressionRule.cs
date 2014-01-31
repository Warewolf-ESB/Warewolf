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
            return value.TryParseVariables(out _outputValue, DoError, LabelText, _variableValue, _inputs);
        }

    }
}