using Caliburn.Micro;
using Dev2.Data.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Intellisense.Provider;
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
        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            if(string.IsNullOrEmpty(input))
            {
                return context.InputText;
            }

            string replace = context.FindTextToSearch();

            var substr = string.IsNullOrEmpty(context.InputText) ? "" : context.InputText.Replace(replace, "");

            replace = substr.StartsWith("(") ? context.InputText : replace;

            if(replace.StartsWith("=") && context.InputText.StartsWith("="))
            {
                replace = replace.Remove(0, 1);
            }

            var region = context.InputText.RegionInPostion(context.CaretPosition);

            if(replace.Equals(region.Name))
            {
                string updatedInputText = context.InputText
                                                 .Remove(region.StartIndex, region.Name.Length)
                                                 .Insert(region.StartIndex, input);

                context.CaretPosition = context.CaretPosition + (updatedInputText.Length - context.InputText.Length);
                return updatedInputText;
            }

            if(DataListUtil.IsValueRecordset(region.Name))
            {
                return PerformResultInsertionForRecordsets(input, context, region, replace);
            }
            if(string.IsNullOrEmpty(replace))
            {
                return context.InputText.Insert(context.CaretPosition, input);
            }
            return PerformResultInsertionForScalars(input, context, replace);
        }

        static string PerformResultInsertionForScalars(string input, IntellisenseProviderContext context, string replace)
        {
            int replaceStringIndex = 0;
            IList<int> stringIndexes = context.InputText.AllIndexesOf(replace).ToList();

            if(stringIndexes.Count > 0)
            {
                replaceStringIndex = stringIndexes.Where(i => i < context.CaretPosition).Max();
            }

            var region = context.InputText.RegionInPostion(context.CaretPosition);

            if(region.Name.EndsWith("]]"))
            {
                var indexOfClosingBracket = input.LastIndexOf("]]", StringComparison.Ordinal);
                input = input.Substring(0, indexOfClosingBracket);
                context.CaretPosition += 2;
            }

            string updatedInputText = context.InputText.Remove(replaceStringIndex, replace.Length)
                                             .Insert(replaceStringIndex, input);

            context.CaretPosition = context.CaretPosition + (updatedInputText.Length - context.InputText.Length);
            return updatedInputText;
        }

        static string PerformResultInsertionForRecordsets(string input, IntellisenseProviderContext context, Region region, string replace)
        {
            string updatedInputText;
            var indexOfStringToReplace = region.Name.IndexOf(replace, StringComparison.Ordinal);
            var recordsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(region.Name) ?? "";

            if(!string.IsNullOrEmpty(recordsetIndex) && indexOfStringToReplace == region.Name.IndexOf(recordsetIndex, StringComparison.Ordinal))
            {
                return ReplaceAnIndexOfARecordset(input, context,replace, region, recordsetIndex);
            }

            if(indexOfStringToReplace + replace.Length == context.CaretPosition)
            {
                if(replace.Contains(").") && !DataListUtil.IsValueRecordset(replace))
                {
                    var substitude = context.InputText
                                            .Replace(context.InputText, input);

                    if(DataListUtil.IsValueRecordset(input))
                    {
                        updatedInputText = recordsetIndex == "*" ? substitude : substitude.Insert(context.InputText.IndexOf(recordsetIndex, StringComparison.Ordinal), recordsetIndex);
                        context.CaretPosition = context.CaretPosition + (updatedInputText.Length - context.InputText.Length);
                        return updatedInputText;
                    }

                    if(input.Equals(substitude))
                    {
                        updatedInputText = context.InputText
                                                  .Remove(indexOfStringToReplace, replace.Length)
                                                  .Insert(indexOfStringToReplace, ")." + input);

                        context.CaretPosition = context.CaretPosition + (updatedInputText.Length - context.InputText.Length);
                        return updatedInputText;
                    }
                }
            }

            updatedInputText = context.InputText.Remove(region.StartIndex, region.Name.Length)
                                      .Insert(region.StartIndex, input);
            context.CaretPosition = context.CaretPosition + (updatedInputText.Length - context.InputText.Length);
            return updatedInputText;
        }

        static string ReplaceAnIndexOfARecordset(string input, IntellisenseProviderContext context, string replace, Region region, string recordsetIndex)
        {
            var indexOfIndex = region.Name.IndexOf("(" + replace, StringComparison.Ordinal);

            var updatedRegion = region.Name
                                      .Remove(indexOfIndex + 1, recordsetIndex.Length)
                                      .Insert(indexOfIndex + 1, input);

            string updatedInputText = context.InputText
                                             .Remove(region.StartIndex, region.Name.Length)
                                             .Insert(region.StartIndex, updatedRegion);

            context.CaretPosition = context.CaretPosition + (updatedInputText.Length - context.InputText.Length);
            return updatedInputText;
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
            if(context.CaretPosition > context.InputText.Length || context.CaretPosition < 0)
            {
                return new List<IntellisenseProviderResult>();
            }

            enIntellisensePartType filterType = context.FilterType;
            enIntellisensePartType altfilterType = context.FilterType;
            IntellisenseDesiredResultSet desiredResultSet = context.DesiredResultSet;

            string searchText = context.FindTextToSearch();
            string recordsetIndex = string.Empty;
            string recordserName = string.Empty;
            var region = context.InputText.RegionInPostion(context.CaretPosition);
            if (DataListUtil.IsValueRecordset(region.Name))
            {
                recordsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(region.Name);
                recordserName = DataListUtil.ExtractRecordsetNameFromValue(region.Name);
                if (!string.IsNullOrEmpty(recordsetIndex) && searchText.Contains(")"))
                {
                    //searchText = region.Name;
                    altfilterType = enIntellisensePartType.RecordsetFields;
                }
            }

            if(context.InputText.Equals("[["))
            {
                searchText = context.InputText;
            }
            else
            {
                searchText = searchText.StartsWith("[[") || string.IsNullOrEmpty(searchText) ? searchText : "[[" + searchText;
            }

            switch(desiredResultSet)
            {
                case IntellisenseDesiredResultSet.EntireSet: results = GetIntellisenseResultsImpl("[[", filterType); break;
                default:
                    {
                        results = GetIntellisenseResultsImpl(searchText, filterType);
                        if(results == null || results.Count == 0 && HandlesResultInsertion)
                        {
                            results = new List<IIntellisenseResult>();

                            string inputText = context.InputText;
                            int newPos;
                            inputText = CleanupInput(inputText, context.CaretPosition, out newPos);

                            var getErrors = GetIntellisenseResultsImpl(inputText, filterType)
                                            .Where(i => i.ErrorCode != enIntellisenseErrorCode.None)
                                            .ToList();

                            getErrors.ForEach(results.Add);
                        }
                        break;
                    }
            }

            if (altfilterType == enIntellisensePartType.RecordsetFields)
            {
                var filteredRecordsetFields = results.Where(r => r.Option.Recordset.Equals(recordserName) || r.Option.IsScalar);

                if (!string.IsNullOrEmpty(recordsetIndex))
                {
                    results = filteredRecordsetFields.ToList().Where(r => !r.Option.HasRecordsetIndex ||  r.Option.IsScalar).ToList();
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
