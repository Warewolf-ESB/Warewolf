using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Designers2.Core
{
    public static class VariableUtils
    {
        public static List<IActionableErrorInfo> TryParseVariables(this string inputValue, out string outputValue, Action onError, ObservableCollection<ObservablePair<string, string>> inputs = null)
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
                            s = "a"; // random text to replace variable
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
                    return new List<IActionableErrorInfo>
                    {
                        new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = "Invalid expression: opening and closing brackets don't match." }
                    };
                }
            }
            return new List<IActionableErrorInfo>();
        }
    }
}
