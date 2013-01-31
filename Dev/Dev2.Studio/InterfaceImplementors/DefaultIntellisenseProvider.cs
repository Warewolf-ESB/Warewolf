using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Unlimited.Framework;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

using System.Windows;
using System.Parsing.Intellisense;
using Dev2.UI;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.InterfaceImplementors
{
    /// <summary>
    /// Provides a concrete implementation of IIntellisenseProvider that provides that same functionality
    /// as the previously implemented IntellisenseTextBox.
    /// </summary>
    public class DefaultIntellisenseProvider : DependencyObject, IIntellisenseProvider
    {
        #region Readonly Members
        internal static readonly DependencyObject _designTestObject = new DependencyObject();
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
        //private object _mediatorKey;
        private string _mediatorKey;
        private string _cachedDataList;
        private static IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        private IntellisenseTextBox _textBox;
        private SyntaxTreeBuilder _builder = new SyntaxTreeBuilder();

        //private StringValueCollection<IntellisenseTokenDefinition> _recordDefinitions;
        //private StringValueCollection<IntellisenseTokenDefinition> _entryDefinitions;
        #endregion

        #region Constructor
        public DefaultIntellisenseProvider()
        {
            //_recordDefinitions = new StringValueCollection<IntellisenseTokenDefinition>(null);
            //_entryDefinitions = new StringValueCollection<IntellisenseTokenDefinition>(null);
            Optional = false;
            HandlesResultInsertion = true;
            
            if (_designTestObject.Dispatcher.CheckAccess() && !DesignerProperties.GetIsInDesignMode(_designTestObject))
            {
                _isUpdated = true;
                CreateDataList();
                //_mediatorKey = Mediator.RegisterToReceiveDispatchedMessage(MediatorMessages.UpdateIntelisense, this, OnUpdateIntellisense);
                _mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateIntelisense, OnUpdateIntellisense);
            }
        }
        #endregion

        #region Update Handling
        private void OnUpdateIntellisense(object state)
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
                _cachedDataList = DataListSingleton.ActiveDataList.Resource.DataList;
            }
        }
        #endregion

        #region Result Handling
        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            string appendText = input + context.InputText.Substring(context.CaretPosition, context.InputText.Length - context.CaretPosition);
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

            int minimum = Math.Max(0, index - appendText.Length);
            int foundMinimum = -1;
            int foundLength = 0;
            int lastIndex = 0;

            for (int i = index - 1; i >= 0; i--)
            {
                if (appendText.StartsWith(currentText.Substring(i, index - i), StringComparison.OrdinalIgnoreCase))
                {
                    lastIndex = index;
                    foundMinimum = i;
                    foundLength = index - i;
                }
                else if (foundMinimum != -1 || appendText.IndexOf(currentText[i].ToString(), StringComparison.OrdinalIgnoreCase) == -1)
                {
                    i = -1;
                }
            }

            if (foundMinimum != -1)
            {
                currentText = currentText.Substring(0, foundMinimum) + appendText.Substring(0, foundLength) + currentText.Substring(foundMinimum + foundLength, currentText.Length - (foundMinimum + foundLength));
                appendText = appendText.Remove(0, foundLength);

                int nextIndex = currentText.IndexOf("]]", lastIndex);

                if (nextIndex >= index)
                {
                    int previousIndex = currentText.IndexOf("[[", index);

                    if (previousIndex == -1 || previousIndex > nextIndex)
                    {
                        currentText = currentText.Substring(0, lastIndex) + currentText.Substring(nextIndex + 2, currentText.Length - (nextIndex + 2));
                    }
                }

            }
            else
            {
                lastIndex = currentText.LastIndexOf("[[", index);
                
                if (lastIndex != -1 && index >= lastIndex + 2)
                {
                    int previousIndex = currentText.LastIndexOf("]]", index);

                    if (lastIndex + 2 < currentText.Length && previousIndex < lastIndex)
                    {
                        int nextIndex = currentText.IndexOf("]]", lastIndex + 2);

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
                string recsetName = input.Contains(")") ? input.Substring(2, input.IndexOf(')')-2) : null;
                if (!string.IsNullOrEmpty(recsetName))//2013.01.29: Ashley Lewis - Bug 8105 Added conditions for overwrite
                    if (recsetName.ToLower().StartsWith(currentText.ToLower()) || currentText.Substring(currentText.IndexOf('(') >= 0 ? currentText.IndexOf('(') + 1 : 0).StartsWith("[[")) //user typed a partial recordset name
                        if (currentText.Substring(currentText.IndexOf('(') >= 0 ? currentText.IndexOf('(') + 1 : 0).StartsWith("[["))
                        {
                            prepend = false;
                            currentText += appendText; //Append
                        }
                        else currentText += appendText; //Append
                    else if (appendText.ToLower().Contains(currentText.ToLower())) //confirm user typed a partial field
                        currentText = appendText; //Overwrite
                    else
                    {//avoid dangling else
                        currentText += appendText; //Append
                    }
                else if(currentText.Substring(currentText.IndexOf('(') >= 0 ? currentText.IndexOf('(')+1 : 0).StartsWith("[["))
                {
                    prepend = false;
                    currentText += appendText; //Append
                }
                else currentText += appendText; //Append

                if (prepend)
                {
                    if (foundMinimum == -1) foundMinimum = currentText.Length - appendText.Length;
                    currentText = currentText.Insert(foundMinimum, "[[");
                }

                context.CaretPositionOnPopup = currentText.Length;
            }
            else
            {
                var firstBrace = currentText.LastIndexOf('(', index);
                var secondBrace = currentText.LastIndexOf(')');
                var depthIndex = currentText.Substring(firstBrace+1, secondBrace-firstBrace-1);
                if (depthIndex.StartsWith("[["))
                    prepend = false;
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

            return currentText.Substring(currentText.IndexOf("[["), currentText.Length - currentText.IndexOf("[["));//Trim
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            if (_isDisposed) throw new ObjectDisposedException("DefaultIntellisenseProvider");
            if (_textBox != context.TextBox) _textBox = context.TextBox as IntellisenseTextBox;
            IList<IIntellisenseResult> results = null;

            string input = context.InputText;
            enIntellisensePartType filterType = context.FilterType;
            IntellisenseDesiredResultSet desiredResultSet = context.DesiredResultSet;

            switch (desiredResultSet)
            {
                case IntellisenseDesiredResultSet.EntireSet: results = GetIntellisenseResultsImpl("[[", filterType); break;
                case IntellisenseDesiredResultSet.ClosestMatch:
                default:
                    {

                        if (context.CaretPosition > 0 && context.InputText.Length > 0 && context.CaretPosition < context.InputText.Length)
                        {
                            char letter = context.InputText[context.CaretPosition];

                            if (char.IsWhiteSpace(letter))
                            {
                                results = GetIntellisenseResultsImpl(input.Substring(0, context.CaretPosition), filterType);
                            }
                            else results = GetIntellisenseResultsImpl(input, filterType);
                        }
                        else
                        {
                            results = GetIntellisenseResultsImpl(input, filterType);
                        }

                        if (results == null || results.Count == 0 && HandlesResultInsertion)
                        {
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
                                input = "[[" + appendText;
                                results = GetIntellisenseResultsImpl(input, filterType);

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
            IList<IIntellisenseResult> results = null;
            CreateDataList();

            IntellisenseFilterOpsTO filterTO = new IntellisenseFilterOpsTO();
            filterTO.FilterType = filterType;
            filterTO.FilterCondition = FilterCondition;

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

            if(!input.StartsWith("{{"))
            {
                var getIndex = input;
                while(DataListUtil.IsValueRecordset(getIndex)) getIndex = DataListUtil.ExtractIndexRegionFromRecordset(getIndex); //2013.01.30: Ashley Lewis  Added this part for Bug 6103
                results = parser.ParseDataLanguageForIntellisense(getIndex, _cachedDataList, false, filterTO);
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

                                if (refNode.NestedIdentifier == null)
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
                                identifier = refNode.Identifier.Content;
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

            if (_mediatorKey != null)
            {
                Mediator.DeRegister(MediatorMessages.UpdateIntelisense, _mediatorKey);
                _mediatorKey = null;
            }

            _cachedDataList = null;
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Properties

        public bool Optional { get; set; }
        public bool HandlesResultInsertion { get; set; }

        #endregion Properties
    }
}
