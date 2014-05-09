using Caliburn.Micro;
using Dev2.Data.Enums;
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
            var inputText = context.InputText ?? string.Empty;

            if(string.IsNullOrEmpty(input))
            {
                return inputText;
            }

            string replace = context.FindTextToSearch();

            if(replace.StartsWith("=") && inputText.StartsWith("="))
            {
                replace = replace.Remove(0, 1);
            }

            var caretPosition = context.CaretPosition;
            var region = inputText.RegionInPostion(caretPosition);

            var substr = string.IsNullOrEmpty(inputText) ? "" : inputText.Replace(replace, "");
            replace = DataListUtil.IsRecordsetOpeningBrace(substr) ? region.Name : replace;

            var regionName = region.Name ?? string.Empty;

            if(replace.Equals(regionName))
            {
                var regionStartIndex = region.StartIndex;
                string updatedInputText = inputText
                                                 .Remove(regionStartIndex, regionName.Length)
                                                 .Insert(regionStartIndex, input);

                context.CaretPosition = caretPosition + (updatedInputText.Length - inputText.Length);
                return updatedInputText;
            }

            if(DataListUtil.IsValueRecordset(regionName))
            {
                return PerformResultInsertionForRecordsets(input, context, region, replace);
            }
            if(string.IsNullOrEmpty(replace))
            {
                return inputText.Insert(caretPosition, input);
            }
            return PerformResultInsertionForScalars(input, context, replace);
        }

        static string PerformResultInsertionForScalars(string input, IntellisenseProviderContext context, string replace)
        {
            int replaceStringIndex = 0;
            var inputText = context.InputText ?? string.Empty;
            var caretPosition = context.CaretPosition;

            IList<int> stringIndexes = inputText.AllIndexesOf(replace).ToList();

            if(stringIndexes.Count > 0)
            {
                replaceStringIndex = stringIndexes.Where(i => i < caretPosition).Max();
            }

            var region = inputText.RegionInPostion(caretPosition);
            var regionName = region.Name ?? string.Empty;

            if(DataListUtil.IsClosedRegion(regionName))
            {
                var indexOfClosingBracket = DataListUtil.IndexOfClosingTags(input);
                input = input.Substring(0, indexOfClosingBracket);
                caretPosition += 2;
            }

            string updatedInputText = inputText.Remove(replaceStringIndex, replace.Length)
                                             .Insert(replaceStringIndex, input);

            context.CaretPosition = caretPosition + (updatedInputText.Length - inputText.Length);
            return updatedInputText;
        }

        static string PerformResultInsertionForRecordsets(string input, IntellisenseProviderContext context, Region region, string replace)
        {
            string updatedInputText;
            var caretPosition = context.CaretPosition;
            var inputText = context.InputText ?? string.Empty;
            var name = region.Name ?? string.Empty;

            var indexOfStringToReplace = name.IndexOf(replace, StringComparison.Ordinal);
            var recordsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(name) ?? string.Empty;
            
            if(indexOfStringToReplace + replace.Length == caretPosition)
            {
                if(DataListUtil.IsValueRecordsetWithFields(replace) && !DataListUtil.IsValueRecordset(replace))
                {
                    var substitude = inputText.Replace(inputText, input);

                    if(DataListUtil.IsValueRecordset(input))
                    {
                        if(DataListUtil.IsStarIndex(name))
                        {
                            updatedInputText = substitude;
                        }
                        else
                        {
                            updatedInputText = substitude.Insert(inputText.IndexOf(recordsetIndex, StringComparison.Ordinal), recordsetIndex);
                        }

                        context.CaretPosition = caretPosition + (updatedInputText.Length - inputText.Length);
                        return updatedInputText;
                    }

                    if(input.Equals(substitude))
                    {
                        updatedInputText = inputText
                                                  .Remove(indexOfStringToReplace, replace.Length)
                                                  .Insert(indexOfStringToReplace, ")." + input);

                        context.CaretPosition = caretPosition + (updatedInputText.Length - inputText.Length);
                        return updatedInputText;
                    }
                }
            }

            updatedInputText = inputText.Remove(region.StartIndex, name.Length)
                                      .Insert(region.StartIndex, input);
            context.CaretPosition = caretPosition + (updatedInputText.Length - inputText.Length);
            return updatedInputText;
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

            if(DataListUtil.IsValueRecordset(region.Name))
            {
                recordsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(region.Name);
                recordserName = DataListUtil.ExtractRecordsetNameFromValue(region.Name);
                if(!string.IsNullOrEmpty(recordsetIndex) && DataListUtil.IsValueRecordsetWithFields(searchText))
                {
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

                            var regionInPostion = inputText.RegionInPostion(context.CaretPosition);
                            inputText = !string.IsNullOrEmpty(regionInPostion.Name) ? regionInPostion.Name : inputText;

                            var getErrors = GetIntellisenseResultsImpl(inputText, filterType)
                                            .Where(i => i.ErrorCode != enIntellisenseErrorCode.None)
                                            .ToList();

                            getErrors.ForEach(results.Add);
                        }
                        break;
                    }
            }

            if(altfilterType == enIntellisensePartType.RecordsetFields)
            {
                var filteredRecordsetFields = results.Where(r => r.Option.Recordset.Equals(recordserName) || r.Option.IsScalar);

                if(!string.IsNullOrEmpty(recordsetIndex))
                {
                    results = filteredRecordsetFields.ToList().Where(r => !r.Option.HasRecordsetIndex || r.Option.IsScalar).ToList();
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
