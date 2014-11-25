
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;

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

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var result = value.TryParseVariables(out _outputValue, DoError, LabelText, _variableValue, _inputs);

            if (result != null)
            {
                if (string.Equals(value, _outputValue))
                {
                    _outputValue = _variableValue;
                }
                return result;
            }

            var parser = new Dev2DataLanguageParser();

            var results = parser.ParseDataLanguageForIntellisense(value, _datalist);

            if (DataListUtil.IsEvaluated(value) && !DataListUtil.IsValueRecordset(value))
            {
                var validRegions = DataList.Contract.DataListCleaningUtils.SplitIntoRegions(value);
                foreach (var region in validRegions)
                {
                    var intellisenseResult = parser.ValidateName(DataListUtil.RemoveLanguageBrackets(region), "");
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
