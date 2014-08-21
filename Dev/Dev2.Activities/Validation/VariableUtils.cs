using Dev2.Common.Interfaces.Infrastructure;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dev2.Validation
{
    public static class VariableUtils
    {
        public static void AddError(this List<IActionableErrorInfo> errors, IActionableErrorInfo error)
        {
            if(errors != null && error != null)
            {
                errors.Add(error);
            }
        }

        public static IActionableErrorInfo TryParseVariables(this string inputValue, out string outputValue, Action onError, string labelText = null, string variableValue = "a", ObservableCollection<ObservablePair<string, string>> inputs = null)
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
                        string s;
                        if(inputs != null)
                        {
                            var input = inputs.FirstOrDefault(p => p.Key == v);
                            s = input == null ? string.Empty : input.Value;
                        }
                        else
                        {
                            s = variableValue; // random text to replace variable
                        }
                        outputValue = outputValue.Replace(v, s);
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
                                  + "Invalid expression: opening and closing brackets don't match."
                    };
                }
            }
            return null;
        }
    }
}


