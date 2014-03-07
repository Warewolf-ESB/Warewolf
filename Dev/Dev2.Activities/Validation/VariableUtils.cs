using Dev2.Data.Compilers;
using Dev2.Data.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;

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

        public static IActionableErrorInfo TryParseRecordsetVariables(this string inputValue, Action onError, string labelText = null, string variableValue = "a", ObservableCollection<ObservablePair<string, string>> inputs = null)
        {
            if(!string.IsNullOrWhiteSpace(inputValue))
            {
                var isValid = true;
                var v = inputValue;

                if(!string.IsNullOrEmpty(v))
                {
                    if(DataListUtil.IsValueRecordset(v))
                    {
                        var index = DataListUtil.ExtractIndexRegionFromRecordset(v);

                        if(!string.IsNullOrEmpty(index))
                        {
                            if(DataListUtil.IsEvaluated(index))
                            {
                                return index.TryParseRecordsetVariables(onError);
                            }

                            int indexForRecset;
                            int.TryParse(index, out indexForRecset);
                            if(indexForRecset <= 0)
                            {
                                isValid = false;
                            }
                        }
                    }
                }
                else
                {
                    isValid = false;
                }

                if(!isValid)
                {
                    return new ActionableErrorInfo(onError)
                    {
                        ErrorType = ErrorType.Critical,
                        Message = (string.IsNullOrEmpty(labelText) ? "" : labelText + " - ")
                                  + "Invalid expression: Recordset index is invalid."
                    };
                }
            }
            return null;
        }

        public static IActionableErrorInfo TryParseVariableSpecialChars(this string inputValue, Action onError, string labelText = null, string variableValue = "a", ObservableCollection<ObservablePair<string, string>> inputs = null)
        {
            if(!string.IsNullOrWhiteSpace(inputValue))
            {
                var isValid = true;

                var evaluatedExpression = inputValue;
                if(!string.IsNullOrEmpty(evaluatedExpression))
                {
                    try
                    {
                        if(DataListUtil.IsEvaluated(evaluatedExpression))
                        {
                            string name;

                            if(DataListUtil.IsValueRecordset(evaluatedExpression))
                            {
                                var index = DataListUtil.ExtractIndexRegionFromRecordset(evaluatedExpression);

                                if(!string.IsNullOrEmpty(index))
                                {
                                    if(DataListUtil.IsEvaluated(index))
                                    {
                                        evaluatedExpression = evaluatedExpression.Replace(index, "");
                                        var errorInfo = index.TryParseVariableSpecialChars(onError);

                                        if(errorInfo != null)
                                        {
                                            return errorInfo;
                                        }
                                    }
                                }

                                var extractRecordsetNameFromValue = DataListUtil.ExtractRecordsetNameFromValue(evaluatedExpression);
                                var extractFieldNameFromValue = DataListUtil.ExtractFieldNameFromValue(evaluatedExpression);
                                name = string.Format("{0}{1}", extractRecordsetNameFromValue, extractFieldNameFromValue);
                            }
                            else
                            {
                                name = evaluatedExpression;
                            }

                            name = DataListUtil.StripBracketsFromValue(name);

                            XmlConvert.VerifyName(name);
                        }
                    }
                    catch(Exception)
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
                                  + "Invalid expression: Variable has special characters."
                    };
                }
            }
            return null;
        }
    }
}


