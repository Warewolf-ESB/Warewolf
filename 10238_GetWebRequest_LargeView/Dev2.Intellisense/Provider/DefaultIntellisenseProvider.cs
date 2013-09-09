using System.Text.RegularExpressions;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Parsing.Intellisense;
using System.Text;
using System.Windows;

namespace Dev2.Studio.InterfaceImplementors
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
        private readonly SyntaxTreeBuilder _builder = new SyntaxTreeBuilder();

        #endregion

        #region Constructor
        public DefaultIntellisenseProvider()
        {
            Optional = false;
            HandlesResultInsertion = true;
            EventPublishers.Aggregator.Subscribe(this);
            if (DesignTestObject.Dispatcher.CheckAccess() && !DesignerProperties.GetIsInDesignMode(DesignTestObject))
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
            if (_textBox != null) _textBox.UpdateErrorState();
        }
        #endregion

        #region Datalist Handling
        private void CreateDataList()
        {
            StringBuilder result = new StringBuilder("<ADL>");
            OptomizedObservableCollection<IDataListItemModel> dataList = null;

            if (DataListSingleton.ActiveDataList != null && DataListSingleton.ActiveDataList.DataList != null)
            {
                dataList = DataListSingleton.ActiveDataList.DataList;
            }

            bool wasRebuilt = false;

            if (dataList != null)
            {
                if (!_hasCachedDatalist || _isUpdated)
                {
                    wasRebuilt = true;

                    _hasCachedDatalist = true;
                    _isUpdated = false;
                }
            }

            result.Append("</ADL>");

            if (wasRebuilt)
            {
                //   _cachedDataList = result.ToString();
                if (DataListSingleton.ActiveDataList != null && DataListSingleton.ActiveDataList.Resource != null &&
                    DataListSingleton.ActiveDataList.Resource.DataList != null)
                {
                    _cachedDataList = DataListSingleton.ActiveDataList.Resource.DataList;
                }
            }
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
            int startIndex = 0;

            if (!string.IsNullOrEmpty(input))
            {
                if (subStringToReplace.Contains("("))
                {
                    int innerStartIndex = subStringToReplace.IndexOf("(", StringComparison.Ordinal) + 1;
                    int innerEndIndex = subStringToReplace.Length;

                    preString = subStringToReplace.Substring(0, innerStartIndex);

                    if (subStringToReplace.Contains(")"))
                    {
                        innerEndIndex = subStringToReplace.IndexOf(")", StringComparison.Ordinal);

                        postString = subStringToReplace.Substring(innerEndIndex, subStringToReplace.Length - innerEndIndex);
                    }

                    if (context.CaretPosition > innerStartIndex && context.CaretPosition <= innerEndIndex)
                    {
                        if (!string.IsNullOrEmpty(preString))
                        {
                            subStringToReplace = subStringToReplace.Replace(preString, "");
                        }
                        if (!string.IsNullOrEmpty(postString))
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
                if (string.IsNullOrEmpty(preString) && string.IsNullOrEmpty(postString))
                {
                    if (context.CaretPosition < context.InputText.Length)
                    {
                        if (context.InputText[context.CaretPosition] != ']' && context.InputText[context.CaretPosition + 1] != ']')
                        {
                            postString = subStringToReplace.Substring(context.CaretPosition);
                            context.InputText = subStringToReplace.Remove(context.CaretPosition);
                        }
                    }
                }

                subStringToReplace = CalculateReplacmentForNonRecordsetIndex(context, subStringToReplace, out startIndex);

                if (input.Contains(subStringToReplace.Replace("[[", "").Replace("]]", "")))
                {
                    int indexToRemoveFrom = startIndex;
                    if (indexToRemoveFrom == 0 && !string.IsNullOrEmpty(preString))
                    {
                        indexToRemoveFrom = preString.Length;
                    }

                    result = context.InputText.Remove(indexToRemoveFrom);
                    context.CaretPositionOnPopup = preString.Length + result.Length + input.Length;
                    result = result + input + postString;

                }
            }

            return result;
        }

        string CalculateReplacmentForNonRecordsetIndex(IntellisenseProviderContext context, string subStringToReplace, out int startIndex)
        {
            char[] trimCharArray = { ' ', ',' };

            startIndex = context.InputText.LastIndexOf("[[", StringComparison.Ordinal);
            int tmpIndex = context.InputText.LastIndexOf("]]", StringComparison.Ordinal);

            if (startIndex > tmpIndex)
            {
                subStringToReplace = context.InputText.Substring(startIndex, context.InputText.Length - startIndex);
            }
            else if (subStringToReplace.Contains("]]"))
            {
                startIndex = subStringToReplace.LastIndexOf("]]", StringComparison.Ordinal) + 2;
                if (startIndex > -1)
                {
                    if (startIndex != subStringToReplace.Length)
                    {
                        subStringToReplace = context.InputText.Substring(startIndex, context.InputText.Length - startIndex);
                        string tmpString = subStringToReplace.TrimStart(trimCharArray);
                        if (tmpString.Length < subStringToReplace.Length)
                        {
                            startIndex = startIndex + (subStringToReplace.Length - tmpString.Length);
                            subStringToReplace = tmpString;
                        }
                    }
                    else
                    {
                        startIndex = 0;
                    }
                }
            }

            if (startIndex == -1)
            {
                startIndex = 0;
            }
            return subStringToReplace;
        }

        private string RemoveOpeningBrackets(string text)
        {
            if (text.StartsWith("[["))
            {
                text = text.Substring(2);
            }

            return text;
        }

        private string CleanupInput(string value, int pos, out int newPos)
        {
            newPos = pos;
            if (!value.StartsWith("{{")) while (IsBetweenBraces(value, pos - 1))
                {
                    value = getBetweenBraces(value, pos, out newPos);
                    pos = newPos;
                }
            return value;
        }

        // Travis.Frisinger : Rolled-back to CI 8121 since it was over-writen by 3 other check-ins
        private bool IsBetweenBraces(string value, int pos)
        {
            if (pos < 0 || pos > value.Length - 1) return false;
            return (value.LastIndexOf('(', pos) != -1 && value.IndexOf(')', pos) != -1);
        }

        private string getBetweenBraces(string value, int pos, out int newPos)
        {
            newPos = pos;
            if (pos < 0 || pos > value.Length - 1) return "";

            int start = value.LastIndexOf('(', pos) + 1;
            int length = value.IndexOf(')', pos) - value.LastIndexOf('(', pos) - 1;

            if (start < 0 || length <= 0 || start + length > value.Length - 1) return "";

            newPos = pos - value.LastIndexOf('(', pos) - 1;

            return value.Substring(start, length);
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            if (_isDisposed) throw new ObjectDisposedException("DefaultIntellisenseProvider");
            if (!Equals(_textBox, context.TextBox)) _textBox = context.TextBox as IntellisenseTextBox;
            IList<IIntellisenseResult> results;
            string inputText = context.InputText;
            if (context.CaretPosition > context.InputText.Length || context.CaretPosition < 0)
            {
                return new List<IntellisenseProviderResult>();
            }

            int originalCaretPosition = context.CaretPosition;
            //string input = context.InputText;
            enIntellisensePartType filterType = context.FilterType;
            IntellisenseDesiredResultSet desiredResultSet = context.DesiredResultSet;

            switch (desiredResultSet)
            {
                case IntellisenseDesiredResultSet.EntireSet: results = GetIntellisenseResultsImpl("[[", filterType); break;
                default:
                    {
                        int newPos;
                        inputText = CleanupInput(inputText, context.CaretPosition, out newPos); //2013.01.30: Ashley Lewis Added this part for Bug 6103
                        context.CaretPosition = newPos;
                        string removeCsv;
                        if (context.CaretPosition > 0 && inputText.Length > 0 && context.CaretPosition < inputText.Length)
                        {
                            char letter = context.InputText[context.CaretPosition];

                            if (char.IsWhiteSpace(letter))
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
                            if (csv.Count() < 2)
                            {
                                //non csv 
                                removeCsv = inputText;
                            }
                            else
                            {
                                //only handle the last csv
                                removeCsv = csv.Last();
                            }
                            results = GetIntellisenseResultsImpl(removeCsv, filterType);
                        }

                        if (results == null || results.Count == 0 && HandlesResultInsertion)
                        {
                            //Reset carret position before searching for minimum, 
                            //This is only needed because the caret position is modified,
                            //This is a HACK, the caret position should never be modified,
                            context.CaretPosition = originalCaretPosition;
                            IList<IIntellisenseResult> previousResults = results;

                            string appendText = null;
                            int foundMinimum = -1;
                            int foundLength = 0;

                            for (int i = context.CaretPosition - 1; i >= 0; i--)
                            {
                                char currentChar = context.InputText[i];

                                if (Char.IsWhiteSpace(currentChar) || !Char.IsLetterOrDigit(currentChar))
                                {
                                    i = -1;
                                }
                                else
                                {
                                    if (currentChar == '[')
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

                            if (foundMinimum != -1)
                            {
                                appendText = context.InputText.Substring(foundMinimum, foundLength);
                            }

                            if (!String.IsNullOrEmpty(appendText))
                            {
                                inputText = "[[" + appendText;
                                results = GetIntellisenseResultsImpl(inputText, filterType);

                                if (results != null)
                                {
                                    context.State = true;

                                    for (int i = 0; i < results.Count; i++)
                                    {
                                        IIntellisenseResult currentResult = results[i];

                                        if (currentResult.ErrorCode != enIntellisenseErrorCode.None)
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
            if (openParts.Length != closeParts.Length)
            {
                results.Add(IntellisenseFactory.CreateCalculateIntellisenseResult(2, 2, "Invalid Expression", "", StringResources.IntellisenseErrorMisMacthingBrackets));
            }

            if (results != null)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    IIntellisenseResult currentResult = results[i];

                    if (currentResult.ErrorCode != enIntellisenseErrorCode.None)
                    {
                        if (currentResult.Type == enIntellisenseResultType.Error && currentResult.IsClosedRegion)
                        {
                            trueResults.Add(new IntellisenseProviderResult(this, currentResult.Option.DisplayValue, currentResult.Message, currentResult.Message, true));
                        }
                    }


                    if (currentResult.Type == enIntellisenseResultType.Selectable)
                    {
                        trueResults.Add(new IntellisenseProviderResult(this, currentResult.Option.DisplayValue, currentResult.Option.Description, currentResult.Option.Description, false));
                    }
                }
            }

            return trueResults;
        }

        IList<IIntellisenseResult> GetIntellisenseResultsRecsetField(string inputText, enIntellisensePartType filterType)
        {
            //remove index
            var recordSetIndex = DataListUtil.ExtractIndexRegionFromRecordset(inputText);
            IList<IIntellisenseResult> results = new List<IIntellisenseResult>();
            if (!string.IsNullOrEmpty(recordSetIndex))
            {
                results = GetIntellisenseResultsImpl(inputText.Replace(recordSetIndex, string.Empty), filterType);
                IList<IIntellisenseResult> recsetResults = new List<IIntellisenseResult>();
                foreach (var result in results)
                {
                    if (!result.Option.IsScalar)
                    {
                        recsetResults.Add(result);
                    }
                }
                foreach (var recsetResult in recsetResults)
                {
                    //replace index
                    IDataListVerifyPart newPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(recsetResult.Option.Recordset, recsetResult.Option.Field, recsetResult.Option.Description, recordSetIndex);
                    var newrecsetResult = IntellisenseFactory.CreateSelectableResult(recsetResult.StartIndex, recsetResult.EndIndex, newPart, recsetResult.Message);
                    results.Remove(recsetResult);
                    results.Add(newrecsetResult);
                }
            }
            else
            {
                results = GetIntellisenseResultsImpl(inputText, filterType);
            }
            return results;
        }

        private IList<IIntellisenseResult> GetIntellisenseResultsImpl(string input, enIntellisensePartType filterType)
        {
            IList<IIntellisenseResult> results = new List<IIntellisenseResult>();

            CreateDataList();

            IntellisenseFilterOpsTO filterTO = new IntellisenseFilterOpsTO();
            filterTO.FilterType = filterType;
            filterTO.FilterCondition = FilterCondition;

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

            if (input.Trim().EndsWith("]"))
            {
                var bracketNumber = Regex.Matches(input, @"\[\[[0-9]");
                if (bracketNumber.Count > 0)
                {
                    results.Add(IntellisenseFactory.CreateCalculateIntellisenseResult(1, 1, "Invalid Expression", "", StringResources.IntellisenseErrorExpressionStartingWithANumber));
                }

                if (results.Count < 0)
                {
                    var tmpResults = parser.ParseDataLanguageForIntellisense(input, _cachedDataList, false, filterTO, true);
                    tmpResults.ToList().ForEach(r =>
                    {
                        if (r.Type == enIntellisenseResultType.Error)
                        {
                            results.Add(r);
                        }
                    });
                }
            }
            else
            {
                results = parser.ParseDataLanguageForIntellisense(input, _cachedDataList, true, filterTO, true);
            }
            return results;
        }

        private static bool ContainsPart(string identifier, string fieldName, int kind, IList<IDev2DataLanguageIntellisensePart> parts)
        {
            foreach (IDev2DataLanguageIntellisensePart current in parts)
            {
                if (current.Name.Equals(identifier, StringComparison.Ordinal))
                {
                    if (kind != 3) return true;
                    return current.Children != null && current.Children.Count > 0 && ContainsPart(fieldName, null, 1, current.Children);
                }
            }

            return false;
        }
        #endregion

        #region Disposal Handling
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            EventPublishers.Aggregator.Unsubscribe(this);
            _cachedDataList = null;
            GC.SuppressFinalize(this);
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
