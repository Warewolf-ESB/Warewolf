#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Data.Interfaces.Enums;

namespace Dev2.Validation
{
    public class IsValidExpressionRule : Rule<string>
    {
        readonly string _variableValue;
        
        readonly ObservableCollection<ObservablePair<string, string>> _inputs;
        private readonly IVariableUtils _variableUtils;
        readonly string _datalist;
        string _outputValue;
        
        public IsValidExpressionRule(Func<string> getValue, string datalist, IVariableUtils variableUtils)
            : this(getValue, datalist, "a", null, variableUtils)
        {
        }

        public IsValidExpressionRule(Func<string> getValue, string datalist, string variableValue, IVariableUtils variableUtils)
            : this(getValue, datalist, variableValue, null, variableUtils)
        {
        }

        public IsValidExpressionRule(Func<string> getValue, string datalist, string variableValue, ObservableCollection<ObservablePair<string, string>> inputs,IVariableUtils variableUtils)
            : base(getValue)
        {
            _variableValue = variableValue;
            _inputs = inputs;
            _variableUtils = variableUtils;
            _datalist = datalist;
        }

        public string ExpressionValue => _outputValue;

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var result = _variableUtils.TryParseVariables(value,out _outputValue, DoError, LabelText, _variableValue, _inputs);

            if (result != null)
            {
                if (string.Equals(value, _outputValue))
                {
                    _outputValue = _variableValue;
                }
                return result;
            }

            

            var results = _variableUtils.ParseDataLanguageForIntellisense(value, _datalist);

            if (_variableUtils.IsEvaluated(value) && !_variableUtils.IsValueRecordset(value))
            {
                var validRegions = _variableUtils.SplitIntoRegions(value);
                foreach (var region in validRegions)
                {
                    var intellisenseResult = _variableUtils.ValidateName(_variableUtils.RemoveLanguageBrackets(region), "");
                    if (intellisenseResult != null && intellisenseResult.Type == enIntellisenseResultType.Error)
                    {
                        results.Add(intellisenseResult);
                    }
                }
            }

            var error = results.FirstOrDefault(r => r.Type == enIntellisenseResultType.Error);

            if (error != null)
            {
                if (string.Equals(value, _outputValue))
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
