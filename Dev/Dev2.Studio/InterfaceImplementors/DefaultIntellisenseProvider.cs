using System.Text.RegularExpressions;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
            //_recordDefinitions = new StringValueCollection<IntellisenseTokenDefinition>(null);
            //_entryDefinitions = new StringValueCollection<IntellisenseTokenDefinition>(null);
            Optional = false;
            HandlesResultInsertion = true;
            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
            EventAggregator.Subscribe(this);
            if (DesignTestObject.Dispatcher.CheckAccess() && !DesignerProperties.GetIsInDesignMode(DesignTestObject))
            {
                _isUpdated = true;
                CreateDataList();
                //_mediatorKey = Mediator.RegisterToReceiveDispatchedMessage(MediatorMessages.UpdateIntelisense, this, OnUpdateIntellisense);
            }
        }

        protected IEventAggregator EventAggregator { get; set; }

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

                    //int count = dataList.Count;

                    //for (int i = 0; i < count; i++)
                    //{
                    //    result.Append(dataList[i].ToDataListXml());
                    //}
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
            string textToAppend = context.InputText.Substring(context.CaretPosition, context.InputText.Length - context.CaretPosition);
            if (textToAppend.Length >= 2 && textToAppend.Substring(0, 2) == "]]")
            {
                textToAppend = textToAppend.Substring(2, textToAppend.Length - 2);
            }
            string appendText = input + textToAppend;

            bool prepend = false;

            if (context.State != null && ((bool)context.State))
            {
                if (appendText.StartsWith("[["))
                {
                    appendText = appendText.Substring(2);
                    prepend = true;
                }
            }

            int index = context.CaretPosition;
            string currentText = context.InputText;

            int foundMinimum = -1;
            int foundLength = 0;
            int lastIndex = 0;

            for (int i = index - 1; i >= 0; i--)
            {
                var test = currentText.Substring(i, index - i);
                if (appendText.StartsWith(test, StringComparison.OrdinalIgnoreCase))
                {
                    lastIndex = index;
                    foundMinimum = i;
                    foundLength = index - i;
                }
                else if (foundMinimum != -1 || appendText.IndexOf(currentText[i].ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) == -1)
                {
                    i = -1;
                }
            }

            if (foundMinimum != -1)
            {
                currentText = currentText.Substring(0, foundMinimum) + appendText.Substring(0, foundLength) + currentText.Substring(foundMinimum + foundLength, currentText.Length - (foundMinimum + foundLength));
                appendText = appendText.Remove(0, foundLength);

                int nextIndex = currentText.IndexOf("]]", lastIndex, StringComparison.Ordinal);

                if (nextIndex >= index)
                {
                    int previousIndex = currentText.IndexOf("[[", index, StringComparison.Ordinal);

                    if (previousIndex == -1 || previousIndex > nextIndex)
                    {
                        currentText = currentText.Substring(0, lastIndex) + currentText.Substring(nextIndex + 2, currentText.Length - (nextIndex + 2));
                    }
                }

            }
            else
            {
                lastIndex = currentText.LastIndexOf("[[", index, StringComparison.Ordinal);

                if (lastIndex != -1 && index >= lastIndex + 2)
                {
                    int previousIndex = currentText.LastIndexOf("]]", index, StringComparison.Ordinal);

                    if (lastIndex + 2 < currentText.Length && previousIndex < lastIndex)
                    {
                        int nextIndex = currentText.IndexOf("]]", lastIndex + 2, StringComparison.Ordinal);

                        if (nextIndex > index)
                        {
                            currentText = currentText.Substring(0, lastIndex) + appendText + currentText.Substring(nextIndex + 2, currentText.Length - (nextIndex + 2));
                            context.CaretPositionOnPopup = lastIndex + appendText.Length;
                            return currentText;
                        }


                        foundLength = 0;

                        for (int i = currentText.Length; i > lastIndex + 2; i--)
                            if (appendText.Contains(currentText.Substring(lastIndex + 2, i - (lastIndex + 2))))
                            {
                                foundLength++;
                            }
                            else
                            {
                                i = 0;
                            }

                        if (foundLength != 0)
                        {
                            currentText = currentText.Substring(0, lastIndex) + appendText + currentText.Substring(lastIndex + 2 + foundLength, currentText.Length - (lastIndex + 2 + foundLength));
                            context.CaretPositionOnPopup = lastIndex + appendText.Length;
                            return currentText;
                        }
                    }
                }
            }

            if (currentText.Length == index)
            {
                var existingRegions = DataListCleaningUtils.SplitIntoRegions(currentText);
                if(existingRegions.Count == 1 && existingRegions[0] == null)
                {
                    existingRegions.RemoveAt(0);
                }
                //2013.06.14: Ashley lewis for bug 8760
                string recsetName = input.Contains("(") ? input.Substring(2, input.IndexOf('(') - 2) : null;
                if(!string.IsNullOrEmpty(recsetName) && existingRegions.Count == 0) //2013.01.29: Ashley Lewis - Bug 8105 Added conditions to allow for overwrite (previously only ever appended text)
                {
                    //if (recsetName.ToLower().StartsWith(!currentText.Substring(currentText.LastIndexOf('(') + 1).ToLower().StartsWith("[[") ? currentText.Substring(currentText.LastIndexOf('(') + 1).ToLower() : currentText.Substring(currentText.LastIndexOf('(') + 1).ToLower().Substring(2, currentText.Length - currentText.LastIndexOf('(') - 3))) //user typed a partial recordset name
                    if(IsPartialRecordSetOrRecorsSetWithOutField(currentText, recsetName))
                    {
                        prepend = !currentText.StartsWith("[[");
                        currentText += appendText; //Append
                    }
                    else
                    {
                        if(appendText.IndexOf(')') != -1 && !recsetName.ToLower().Contains(currentText.ToLower()))
                        {
                            prepend = false;
                            currentText = currentText.Remove(currentText.IndexOf(')') >= 0 ? currentText.IndexOf(')') : 0, currentText.Length - (currentText.IndexOf(')') != -1 ? currentText.IndexOf(')') : 0)) + appendText.Remove(0, appendText.IndexOf(')')); // User typed a partial fieldname, just append that fieldname
                        }
                        else currentText = appendText; // User typed a partial recset name within the index of a record set
                    }
                }
                else
                {
                    var latestRegion = existingRegions.Aggregate(currentText, (current, region) => current.Replace(region, string.Empty));
                    if(latestRegion.StartsWith("[[")) // Already starts with [[ - dont prepend
                    {
                        prepend = false;
                        currentText += appendText; //Append
                    }
                    else currentText += appendText; //Append
                }

                if (prepend)
                {
                    if (foundMinimum == -1) foundMinimum = currentText.Length - appendText.Length;
                    currentText = currentText.Insert(foundMinimum, "[[");
                }

                context.CaretPositionOnPopup = currentText.Length;
            }
            else
            {
                if (index < 0 || index > currentText.Length - 1)
                {
                    prepend = false;
                }
                else
                {
                    var firstBrace = currentText.LastIndexOf('(', index);
                    var secondBrace = currentText.LastIndexOf(')');

                    var length = secondBrace - firstBrace - 1;
                    if (length >= 0)
                    {
                        var depthIndex = currentText.Substring(firstBrace + 1, length);
                        if (depthIndex.StartsWith("[["))
                        {
                            prepend = false;
                        }
                    }
                }

                currentText = currentText.Substring(0, index);
                currentText = currentText.Insert(index, appendText);

                if (prepend)
                {
                    if (foundMinimum == -1) foundMinimum = index;
                    currentText = currentText.Insert(foundMinimum, "[[");
                    context.CaretPositionOnPopup = index + appendText.Length + 2;
                }
                else
                {
                    context.CaretPositionOnPopup = index + appendText.Length;
                }
            }

            //return !currentText.Contains('(') ? currentText.Substring(currentText.IndexOf("[["), currentText.Length - currentText.IndexOf("[[")) : currentText;//Trim if no brackets
            return currentText;//No Trim
        }

        private bool IsPartialRecordSetOrRecorsSetWithOutField(string currentText, string recsetName)
        {
            string testAfterOpenBracket = currentText.Substring(currentText.LastIndexOf('(') + 1).ToLower();

            if (testAfterOpenBracket.Contains(")."))
            {
                return true;
            }

            string workingRecordsetTextwithoutBrackets;
            if (string.IsNullOrWhiteSpace(testAfterOpenBracket))
            {
                workingRecordsetTextwithoutBrackets = RemoveOpeningBrackets(currentText);
            }
            else
            {
                workingRecordsetTextwithoutBrackets = RemoveOpeningBrackets(testAfterOpenBracket);
            }

            bool isPartialRecordset;
            if (workingRecordsetTextwithoutBrackets.Length <= recsetName.Length)
            {
                isPartialRecordset = recsetName.ToLower().StartsWith(workingRecordsetTextwithoutBrackets.ToLower());
            }
            else
            {
                isPartialRecordset = workingRecordsetTextwithoutBrackets.ToLower().StartsWith(recsetName.ToLower());
            }

            return isPartialRecordset;
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
                        if (context.CaretPosition > 0 && inputText.Length > 0 && context.CaretPosition < inputText.Length)
                        {
                            results = GetIntellisenseResultsImpl(inputText.Substring(0, context.CaretPosition), filterType);
                        }
                        else
                        {
                            //consider csv input
                            var csv = inputText.Split(',');
                            string removeCSV = string.Empty;
                            if (csv.Count() < 2)
                            {
                                //non csv 
                                removeCSV = inputText;
                            }
                            else
                            {
                                //only handle the last csv
                                removeCSV = csv.Last();
                            }
                            results = GetIntellisenseResultsImpl(removeCSV, filterType);
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

        private IList<IIntellisenseResult> GetIntellisenseResultsImpl(string input, enIntellisensePartType filterType)
        {
            IList<IIntellisenseResult> results = new List<IIntellisenseResult>();

            CreateDataList();

            IntellisenseFilterOpsTO filterTO = new IntellisenseFilterOpsTO();
            filterTO.FilterType = filterType;
            filterTO.FilterCondition = FilterCondition;

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

            //2013.04.26: Ashley Lewis - Bug 6103 the user just closed the datalist region, leave results clear
            if (!input.EndsWith("]"))
            {
                results = parser.ParseDataLanguageForIntellisense(input, _cachedDataList, false, filterTO);
            }
            else if(input.Contains(' ') && input.EndsWith("]]"))
            {

                var tmpResults = parser.ParseDataLanguageForIntellisense(input, _cachedDataList, true, filterTO);

                //06.03.2013: Ashley Lewis - BUG 6731
                foreach(var res in tmpResults)
                {
                    if(res.Option.DisplayValue.IndexOf(' ') >= 0)
                    {
                        results.Add(IntellisenseFactory.CreateErrorResult(0, 0, res.Option, res.Option.DisplayValue + " contains a space, this is an invalid character for a variable name", enIntellisenseErrorCode.SyntaxError, true));
                    }
                }
            }
            else
            {
                int num;
                if (int.TryParse(DataListUtil.StripBracketsFromValue(input)[0].ToString(CultureInfo.InvariantCulture), out num))
                {
                    results.Add(IntellisenseFactory.CreateErrorResult(0, 0, IntellisenseFactory.CreateDataListValidationScalarPart("Syntax Error"), "Invalid syntax - You have started a variable name with a number", enIntellisenseErrorCode.SyntaxError, true));
                }
                //2013.06.13: Ashley Lewis for bug 9611 - finally add mismatched region braces error if region braces are mismatched
                string[] openParts = Regex.Split(input, @"\[\[");
                string[] closeParts = Regex.Split(input, @"\]\]");
                if (openParts.Length != closeParts.Length)
                {
                    results.Add(IntellisenseFactory.CreateErrorResult(0, 0, IntellisenseFactory.CreateDataListValidationScalarPart("Invalid Expression"), StringResources.IntellisenseErrorMisMatchingBrackets, enIntellisenseErrorCode.SyntaxError, true));
                }
            }


            if (results != null)
            {
                if (filterType == enIntellisensePartType.RecordsetFields)
                {
                    IList<IIntellisenseResult> test = results.Where(n => n.Option.Field == string.Empty || n.Option.Recordset == string.Empty).ToList();
                    IIntellisenseResult current;

                    for (int i = results.Count - 1; i >= 0; i--)
                    {
                        if ((current = results[i]).Option.Field == String.Empty || current.Option.Recordset == String.Empty)
                        {
                            results.RemoveAt(i);
                        }
                    }
                }

                bool checkError = false;
                bool trueHasError = false;
                Node[] nodes = null;

                for (int i = results.Count - 1; i >= 0; i--)
                {
                    if (results[i].ErrorCode != enIntellisenseErrorCode.None)
                    {
                        if (!checkError)
                        {
                            checkError = true;
                            nodes = _builder.Build(input);

                            if (_builder.EventLog.HasEventLogs)
                            {
                                trueHasError = true;
                                _builder.EventLog.Clear();
                            }
                        }

                        if (!trueHasError) results.RemoveAt(i);
                    }
                }

                if (checkError || results.Count == 0)
                {
                    if (results.Count == 0)
                    {
                        if (nodes == null)
                        {
                            nodes = _builder.Build(input);
                        }
                    }

                    if (!trueHasError && nodes != null && nodes.Length == 1)
                    {
                        IList<IDev2DataLanguageIntellisensePart> parts = DataListFactory.GenerateIntellisensePartsFromDataList(_cachedDataList, filterTO);
                        List<Node> allNodes = new List<Node>();
                        nodes[0].CollectNodes(allNodes);

                        for (int i = allNodes.Count - 1; i >= 0; i--)
                        {
                            string identifier = null;
                            string fieldName = null;
                            int kind = -1;

                            if (allNodes[i] is DatalistRecordSetNode)
                            {
                                DatalistRecordSetNode refNode = allNodes[i] as DatalistRecordSetNode;

                                if (refNode.NestedIdentifier == null && filterType != enIntellisensePartType.RecorsetsOnly)
                                {
                                    identifier = refNode.Identifier.Content;
                                    kind = 2;
                                }
                            }
                            else if (allNodes[i] is DatalistRecordSetFieldNode)
                            {
                                DatalistRecordSetFieldNode refNode = allNodes[i] as DatalistRecordSetFieldNode;

                                if (refNode.RecordSet.NestedIdentifier == null)
                                {
                                    identifier = refNode.RecordSet.Identifier.Content;

                                    if (refNode.Field == null)
                                    {
                                        fieldName = refNode.Identifier.Content;
                                        kind = 3;
                                    }
                                }
                            }
                            else if (allNodes[i].GetType() == typeof(DatalistReferenceNode))
                            {
                                DatalistReferenceNode refNode = allNodes[i] as DatalistReferenceNode;
                                if (refNode != null)
                                {
                                identifier = refNode.Identifier.Content;
                                }
                                kind = 1;
                            }

                            if (kind > 0)
                            {
                                if (!ContainsPart(identifier, fieldName, kind, parts))
                                {
                                    string displayName = null;
                                    string message = null;

                                    switch (kind)
                                    {
                                        case 1:
                                            {
                                                displayName = "Missing Scalar";
                                                message = "Datalist scalar \"" + identifier + "\" does not exist in your datalist.";
                                                break;
                                            }
                                        case 2:
                                            {
                                                displayName = "Missing Recordset";
                                                message = "Datalist recordset \"" + identifier + "\" does not exist in your datalist.";
                                                break;
                                            }
                                        case 3:
                                            {
                                                displayName = "Missing Recordset Field";
                                                message = "Datalist recordset field \"" + fieldName + "\" does not exist in recordset \"" + identifier + "\".";
                                                break;
                                            }
                                    }

                                    if (message != null)
                                        results.Add(IntellisenseFactory.CreateCalculateIntellisenseResult(allNodes[i].Identifier.Start.SourceIndex, allNodes[i].Identifier.End.SourceIndex + allNodes[i].Identifier.End.SourceLength, displayName, "", message));
                                }
                            }
                        }
                    }
                }
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

            EventAggregator.Unsubscribe(this);
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
