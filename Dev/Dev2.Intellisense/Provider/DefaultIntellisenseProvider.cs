using Caliburn.Micro;
using Dev2.Data.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.InterfaceImplementors
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Provides a concrete implementation of IIntellisenseProvider that provides that same functionality
    /// as the previously implemented IntellisenseTextBox.
    /// </summary>
    public class DefaultIntellisenseProvider : DependencyObject, IIntellisenseProvider, IHandle<UpdateIntellisenseMessage>
    {
        #region Readonly Members
        internal static readonly DependencyObject DesignTestObject = new DependencyObject();
        #endregion

        #region FilterCondition
        public static readonly DependencyProperty FilterConditionProperty = DependencyProperty.Register("FilterCondition", typeof(string), typeof(DefaultIntellisenseProvider), new UIPropertyMetadata(""));

        public string FilterCondition
        {
            get
            {
                return (string)GetValue(FilterConditionProperty);
            }
            set
            {
                SetValue(FilterConditionProperty, value);
            }
        }

        #endregion FilterCondition

        #region Instance Fields

        private bool _isDisposed;
        private bool _isUpdated;
        private bool _hasCachedDatalist;
        private string _cachedDataList;
        private IntellisenseTextBox _textBox;

        #endregion

        #region Constructor
        public DefaultIntellisenseProvider()
        {
            Optional = false;
            HandlesResultInsertion = true;
            EventPublishers.Aggregator.Subscribe(this);
            if(DesignTestObject.Dispatcher.CheckAccess() && !DesignerProperties.GetIsInDesignMode(DesignTestObject))
            {
                _isUpdated = true;
                CreateDataList();
            }
        }

        #endregion

        #region Update Handling
        private void OnUpdateIntellisense()
        {
            _isUpdated = true;
            if(_textBox != null) _textBox.UpdateErrorState();
        }
        #endregion

        #region Datalist Handling
        private bool CreateDataList(string input = "", IList<IIntellisenseResult> results = null)
        {
            bool succeeded = false;

            StringBuilder result = new StringBuilder("<ADL>");
            ObservableCollection<IDataListItemModel> dataList = null;

            var activeDataList = DataListSingleton.ActiveDataList;
            var activeDataListViewModels = activeDataList.DataList;

            if(activeDataList != null && activeDataListViewModels != null)
            {
                dataList = activeDataListViewModels;
            }

            if(dataList != null)
            {
                if(!_hasCachedDatalist || _isUpdated)
                {
                    _hasCachedDatalist = true;
                    _isUpdated = false;
                }
            }

            result.Append("</ADL>");

            if(activeDataList != null && activeDataList.Resource != null && activeDataList.Resource.DataList != null)
            {
                if(activeDataList.HasErrors)
                {
                    if(activeDataListViewModels != null && results != null)
                    {
                        var error = activeDataListViewModels
                            .FirstOrDefault(d => d.HasError && input.Contains(d.DisplayName.Replace("()", "")));

                        if(error != null)
                        {
                            results.Add(IntellisenseFactory.CreateErrorResult(1, 1, null, error.ErrorMessage, enIntellisenseErrorCode.SyntaxError, true));
                        }
                        else
                        {
                            _cachedDataList = activeDataList.Resource.DataList;
                            succeeded = true;
                        }
                    }
                }
                else
                {
                    _cachedDataList = activeDataList.Resource.DataList;
                    succeeded = true;
                }
            }

            return succeeded;
        }

        #endregion

        #region Result Handling
        // TODO Brendon.Page This methods needs a major refactor, while doign so please consider the single responsibility principle!!!!!!!!
        //                   Part
        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            string preString = string.Empty;
            string postString = string.Empty;
            string subStringToReplace = context.InputText;
            string result = context.InputText;

            if(!string.IsNullOrEmpty(input))
            {
                if(subStringToReplace.Contains("("))
                {
                    int innerStartIndex = subStringToReplace.IndexOf("(", StringComparison.Ordinal) + 1;
                    int innerEndIndex = subStringToReplace.Length;

                    preString = subStringToReplace.Substring(0, innerStartIndex);

                    if(subStringToReplace.Contains(")"))
                    {
                        innerEndIndex = subStringToReplace.IndexOf(")", StringComparison.Ordinal);

                        postString = subStringToReplace.Substring(innerEndIndex, subStringToReplace.Length - innerEndIndex);
                    }

                    if(context.CaretPosition > innerStartIndex && context.CaretPosition <= innerEndIndex)
                    {
                        if(!string.IsNullOrEmpty(preString))
                        {
                            subStringToReplace = subStringToReplace.Replace(preString, "");
                        }
                        if(!string.IsNullOrEmpty(postString))
                        {
                            subStringToReplace = subStringToReplace.Replace(postString, "");
                        }
                    }
                    else
                    {
                        preString = string.Empty;
                        postString = string.Empty;
                    }
                }
                if(string.IsNullOrEmpty(preString) && string.IsNullOrEmpty(postString))
                {
                    if(context.CaretPosition < context.InputText.Length)
                    {
                        postString = subStringToReplace.Substring(context.CaretPosition);
                        context.InputText = subStringToReplace.Remove(context.CaretPosition);
                    }
                }

                int startIndex;
                subStringToReplace = CalculateReplacmentForNonRecordsetIndex(context, subStringToReplace, out startIndex);

                //Need to remove case sensativity ;)
                var compareStr = input.ToLower();
                var replaceStr = subStringToReplace.ToLower();

                // compareStr contains [[, but resplaceStr does not?
                var toCmp = replaceStr.Replace("[[", "").Replace("]]", "");
                var matchFound = false;

                if(compareStr.Contains(toCmp))
                {
                    matchFound = true;
                }
                else
                {
                    // try replacing () with (*) or (*) with () ;)

                    var tmp = compareStr.Replace("()", "(*)");
                    if(tmp.Contains(toCmp))
                    {
                        matchFound = true;
                    }
                    else
                    {
                        tmp = compareStr.Replace("(*)", "()");
                        if(tmp.Contains(tmp))
                        {
                            matchFound = true;
                        }
                    }
                }

                if(matchFound)
                {
                    int indexToRemoveFrom = startIndex;
                    if(indexToRemoveFrom == 0 && !string.IsNullOrEmpty(preString))
                    {
                        indexToRemoveFrom = preString.Length;
                    }

                    var txt = context.InputText;

                    // we need to bounds check ;)
                    if(!string.IsNullOrEmpty(txt))
                    {
                        result = context.InputText.Remove(indexToRemoveFrom);
                    }
                    context.CaretPositionOnPopup = preString.Length + result.Length + input.Length;
                    result = result + input + postString;

                }

            }

            return result;
        }

        string CalculateReplacmentForNonRecordsetIndex(IntellisenseProviderContext context, string subStringToReplace, out int startIndex)
        {
            startIndex = context.InputText.LastIndexOf("[[", StringComparison.Ordinal);
            int tmpIndex = context.InputText.LastIndexOf("]]", StringComparison.Ordinal);

            if(startIndex > tmpIndex)
            {
                subStringToReplace = context.InputText.Substring(startIndex, context.InputText.Length - startIndex);
            }
            else if(subStringToReplace.Contains("]]"))
            {
                subStringToReplace = context.InputText.Substring(startIndex, context.InputText.Length - startIndex);
                var si = subStringToReplace.LastIndexOf("[[", StringComparison.Ordinal);
                var ei = subStringToReplace.LastIndexOf("]]", StringComparison.Ordinal) + 2;
                subStringToReplace = subStringToReplace.Substring(si, ei);
            }

            if(startIndex == -1)
            {
                startIndex = 0;
            }
            return subStringToReplace;
        }

        private string CleanupInput(string value, int pos, out int newPos)
        {

            var result = value;
            newPos = pos;

            // Use language parser to find open parts!
            var languageParser = new Dev2DataLanguageParser();

            try
            {
                var parts = languageParser.MakeParts(value);

                // continue to la-la land if we need do ;)
                if(parts.Any(c => c.HangingOpen))
                {
                    if(!value.StartsWith("{{")) while(IsBetweenBraces(result, pos - 1))
                        {
                            result = getBetweenBraces(result, pos, out newPos);
                            pos = newPos;
                        }

                    // if empty default back to original value ;)
                    if(string.IsNullOrEmpty(result))
                    {
                        result = value;
                    }

                    var rsName = DataListUtil.ExtractRecordsetNameFromValue(result);

                    // is it recordset notation and the only value present?
                    if(!String.IsNullOrEmpty(rsName))
                    {
                        var case1Len = 1;
                        var case2Len = 2;

                        if(result.Length == pos && !result.StartsWith("[["))
                        {
                            result = string.Concat("[[", result);
                            newPos += 2;
                        }
                        else
                        {
                            // adjust len checks for when [[ already exist ;)
                            case1Len += 2;
                            case2Len += 3;
                        }

                        // we have a rs( case ;)
                        if(rsName.Length + case1Len == value.Length)
                        {
                            // fake it to get recordset field data ;)
                            result = string.Concat(result, ").");
                            newPos += 2;
                        }

                        // we have a rs() case ;)
                        if(rsName.Length + case2Len == value.Length)
                        {
                            // fake it to get recordset field data ;)
                            result = string.Concat(result, ".");
                            newPos += 1;
                        }
                    }
                }
            }
            catch
            {
                result = value;
            }

            return result;
        }

        // Travis.Frisinger : Rolled-back to CI 8121 since it was over-writen by 3 other check-ins
        private bool IsBetweenBraces(string value, int pos)
        {
            if(pos < 0 || pos > value.Length - 1) return false;
            return (value.LastIndexOf('(', pos) != -1 && value.IndexOf(')', pos) != -1);
        }

        private string getBetweenBraces(string value, int pos, out int newPos)
        {
            newPos = pos;
            if(pos < 0 || pos > value.Length - 1) return "";

            int start = value.LastIndexOf('(', pos) + 1;
            int length = value.IndexOf(')', pos) - value.LastIndexOf('(', pos) - 1;

            if(start < 0 || length <= 0 || start + length > value.Length - 1) return "";

            newPos = pos - value.LastIndexOf('(', pos) - 1;

            return value.Substring(start, length);
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            if(_isDisposed) throw new ObjectDisposedException("DefaultIntellisenseProvider");
            if(!Equals(_textBox, context.TextBox)) _textBox = context.TextBox as IntellisenseTextBox;
            IList<IIntellisenseResult> results;
            string inputText = context.InputText;
            if(context.CaretPosition > context.InputText.Length || context.CaretPosition < 0)
            {
                return new List<IntellisenseProviderResult>();
            }

            int originalCaretPosition = context.CaretPosition;
            //string input = context.InputText;
            enIntellisensePartType filterType = context.FilterType;
            IntellisenseDesiredResultSet desiredResultSet = context.DesiredResultSet;

            switch(desiredResultSet)
            {
                case IntellisenseDesiredResultSet.EntireSet: results = GetIntellisenseResultsImpl("[[", filterType); break;
                default:
                    {
                        int newPos;
                        inputText = CleanupInput(inputText, context.CaretPosition, out newPos); //2013.01.30: Ashley Lewis Added this part for Bug 6103
                        context.CaretPosition = newPos;
                        if(context.CaretPosition > 0 && inputText.Length > 0 && context.CaretPosition < inputText.Length)
                        {
                            char letter = context.InputText[context.CaretPosition];

                            if(char.IsWhiteSpace(letter))
                            {
                                results = GetIntellisenseResultsImpl(inputText.Substring(0, context.CaretPosition), filterType);
                            }
                            else
                            {
                                results = GetIntellisenseResultsImpl(inputText, filterType);
                            }
                        }
                        else
                        {
                            //consider csv input
                            var csv = inputText.Split(',');
                            var index = inputText.IndexOf(',');

                            if(index < 4 || inputText.ElementAt(index - 1).ToString(CultureInfo.InvariantCulture) != "]" || inputText.ElementAt(index - 1).ToString(CultureInfo.InvariantCulture) != "]")
                            {
                                csv = new[] { inputText };
                            }

                            results = GetIntellisenseResultsImpl(csv.Last(), filterType);
                        }

                        if(results == null || results.Count == 0 && HandlesResultInsertion)
                        {
                            //Reset carret position before searching for minimum, 
                            //This is only needed because the caret position is modified,
                            //This is a HACK, the caret position should never be modified,
                            context.CaretPosition = originalCaretPosition;
                            IList<IIntellisenseResult> previousResults = results;

                            string appendText = null;
                            int foundMinimum = -1;
                            int foundLength = 0;

                            for(int i = context.CaretPosition - 1; i >= 0; i--)
                            {
                                char currentChar = context.InputText[i];

                                if(Char.IsWhiteSpace(currentChar) || !Char.IsLetterOrDigit(currentChar))
                                {
                                    i = -1;
                                }
                                else
                                {
                                    if(currentChar == '[')
                                    {
                                        i = -1;
                                    }
                                    else
                                    {
                                        foundMinimum = i;
                                        foundLength = context.CaretPosition - i;
                                    }
                                }
                            }

                            if(foundMinimum != -1)
                            {
                                appendText = context.InputText.Substring(foundMinimum, foundLength);
                            }

                            if(!String.IsNullOrEmpty(appendText))
                            {
                                inputText = "[[" + appendText;
                                results = GetIntellisenseResultsImpl(inputText, filterType);

                                if(results != null)
                                {
                                    context.State = true;

                                    // ReSharper disable PossibleNullReferenceException
                                    for(int i = 0; i < results.Count; i++)
                                    // ReSharper restore PossibleNullReferenceException
                                    {
                                        IIntellisenseResult currentResult = results[i];

                                        if(currentResult.ErrorCode != enIntellisenseErrorCode.None)
                                        {
                                            context.State = false;
                                            i = results.Count;
                                            results = previousResults;
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }
            }

            IList<IntellisenseProviderResult> trueResults = new List<IntellisenseProviderResult>();

            string[] openParts = Regex.Split(context.InputText, @"\[\[");
            string[] closeParts = Regex.Split(context.InputText, @"\]\]");
            if(openParts.Length != closeParts.Length)
            {
                if(results != null)
                {
                    results.Add(IntellisenseFactory.CreateCalculateIntellisenseResult(2, 2, "Invalid Expression", "", StringResources.IntellisenseErrorMisMacthingBrackets));
                }
            }

            if(results != null)
            {
                foreach(IIntellisenseResult currentResult in results)
                {
                    if(currentResult.ErrorCode != enIntellisenseErrorCode.None)
                    {
                        if(currentResult.Type == enIntellisenseResultType.Error && currentResult.IsClosedRegion)
                        {
                            var displayValue = currentResult.Option == null ? string.Empty : currentResult.Option.DisplayValue;
                            trueResults.Add(new IntellisenseProviderResult(this, displayValue, currentResult.Message, currentResult.Message, true));
                        }
                    }


                    if(currentResult.Type == enIntellisenseResultType.Selectable)
                    {
                        var displayValue = currentResult.Option == null ? string.Empty : currentResult.Option.DisplayValue;
                        var description = currentResult.Option == null ? string.Empty : currentResult.Option.Description;
                        trueResults.Add(new IntellisenseProviderResult(this, displayValue, description, description, false));
                    }
                }
            }

            return trueResults;
        }

        private IList<IIntellisenseResult> GetIntellisenseResultsImpl(string input, enIntellisensePartType filterType)
        {
            IList<IIntellisenseResult> results = new List<IIntellisenseResult>();

            if(!CreateDataList(input, results))
            {
                return results;
            }

            IntellisenseFilterOpsTO filterTO = new IntellisenseFilterOpsTO { FilterType = filterType, FilterCondition = FilterCondition };

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

            results = parser.ParseDataLanguageForIntellisense(input, _cachedDataList, false, filterTO, true);

            return results;
        }

        #endregion

        #region Disposal Handling

        public void Dispose()
        {
            if(_isDisposed)
                return;
            _isDisposed = true;

            EventPublishers.Aggregator.Unsubscribe(this);
            _cachedDataList = null;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                // get rid of managed resources
            }
            // get rid of unmanaged resources
        }

        ~DefaultIntellisenseProvider()
        {
            Dispose(false);

        }

        public void OnDispose()
        {

        }

        #endregion

        #region Properties

        public bool Optional { get; set; }
        public bool HandlesResultInsertion { get; set; }

        #endregion Properties

        #region Implementation of IHandle<UpdateIntellisenseMessage>

        public void Handle(UpdateIntellisenseMessage message)
        {
            OnUpdateIntellisense();
        }

        #endregion
    }
}
