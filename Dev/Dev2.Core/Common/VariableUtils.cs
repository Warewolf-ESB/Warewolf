/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using Warewolf.Resource.Errors;
using Dev2.Data.Parsers;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;

namespace Dev2.Validation
{
    public class VariableUtils: IVariableUtils
    {
        public void AddError(List<IActionableErrorInfo> errors, IActionableErrorInfo error)
        {
            if(errors != null && error != null)
            {
                errors.Add(error);
            }
        }

        public bool IsEvaluated(string value) => DataListUtil.IsEvaluated(value);
        public bool IsValueRecordset(string value) => DataListUtil.IsValueRecordset(value);

        public IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string value, string datalist)
        {
            var parser = new Dev2DataLanguageParser();
            return parser.ParseDataLanguageForIntellisense(value, datalist);
        }

        public string RemoveLanguageBrackets(string region) => DataListUtil.RemoveLanguageBrackets(region);
        public List<string> SplitIntoRegions(string value) => DataListCleaningUtils.SplitIntoRegions(value);
        public IActionableErrorInfo TryParseVariables(string inputValue, out string outputValue, Action onError)=>TryParseVariables(inputValue,out outputValue, onError, null, "a", null);

        public IActionableErrorInfo TryParseVariables(string inputValue, out string outputValue, Action onError, string variableValue) => TryParseVariables(inputValue,out outputValue, onError, null, variableValue, null);

        public IActionableErrorInfo TryParseVariables(string inputValue, out string outputValue, Action onError, string variableValue, string labelText) => TryParseVariables(inputValue,out outputValue, onError, labelText, variableValue, null);

        public IActionableErrorInfo TryParseVariables(string inputValue, out string outputValue, Action onError, string labelText, string variableValue, ObservableCollection<ObservablePair<string, string>> inputs)
        {
            outputValue = inputValue;

            if(!string.IsNullOrWhiteSpace(outputValue))
            {
                var isValid = true;
                var variableList = DataListCleaningUtils.SplitIntoRegions(outputValue);
                foreach(var v in variableList)
                {
                    if(v != null)
                    {
                        outputValue = ParseVariables(outputValue, variableValue, inputs, v);
                    }
                    else
                    {
                        isValid = false;
                    }
                }

                if(!isValid)
                {
                    return new ActionableErrorInfo(onError)
                    {
                        ErrorType = ErrorType.Critical,
                        Message = (string.IsNullOrEmpty(labelText) ? "" : labelText + " - ")
                                  + ErrorResource.ResultOpeningClosingBracketMismatch
                    };
                }
            }
            return null;
        }

        static string ParseVariables(string outputValue, string variableValue, ObservableCollection<ObservablePair<string, string>> inputs, string v)
        {
            string s;
            if (inputs != null)
            {
                var input = inputs.FirstOrDefault(p => p.Key == v);
                s = input == null ? string.Empty : input.Value;
            }
            else
            {
                s = variableValue; // random text to replace variable
            }
            return outputValue.Replace(v, s);
        }

        public IIntellisenseResult ValidateName(string name, string displayName)
        {
            var parser = new Dev2DataLanguageParser();
            return parser.ValidateName(name, displayName);
        }
    }
}


