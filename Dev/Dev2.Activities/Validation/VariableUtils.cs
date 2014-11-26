
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;

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


