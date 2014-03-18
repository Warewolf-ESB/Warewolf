using Dev2.Data.Enums;
using Dev2.Data.Parsers;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dev2.Validation
{
    public class IsValidExpressionRule : Rule<string>
    {
        readonly string _variableValue;
        // ReSharper disable NotAccessedField.Local
        readonly ObservableCollection<ObservablePair<string, string>> _inputs;
        // ReSharper restore NotAccessedField.Local
        readonly string _datalist;
        string _outputValue;

        public IsValidExpressionRule(Func<string> getValue, string datalist, string variableValue = "a", ObservableCollection<ObservablePair<string, string>> inputs = null)
            : base(getValue)
        {
            _variableValue = variableValue;
            _inputs = inputs;
            _datalist = datalist;
        }

        public string ExpressionValue { get { return _outputValue; } }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();

            if(string.IsNullOrEmpty(value))
            {
                return null;
            }

            var result = value.TryParseVariables(out _outputValue, DoError, LabelText, _variableValue, _inputs);

            if(result != null)
            {
                if(string.Equals(value, _outputValue))
                {
                    _outputValue = _variableValue;
                }
                return result;
            }

            var parser = new Dev2DataLanguageParser();

            var intellisenseResults = parser.ParseDataLanguageForIntellisense(value, _datalist);

            var error = intellisenseResults.FirstOrDefault(e => e.ErrorCode != enIntellisenseErrorCode.None);

            if(error != null)
            {
                if(string.Equals(value, _outputValue))
                {
                    _outputValue = _variableValue;
                }

                return new ActionableErrorInfo(DoError)
                {
                    ErrorType = ErrorType.Critical,
                    Message = (string.IsNullOrEmpty(LabelText) ? "" : LabelText + " - ")
                              + error.Message
                };
            }
            return null;
        }




    }
}