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

        public bool IsDisposed {get; private set;}
        public string CachedDataList { get; private set; }
        public IntellisenseTextBox TextBox { get; private set; }
        public bool IsUpdated { get; private set; }

        private bool _hasCachedDatalist;

        #endregion

        #region Constructor
        public DefaultIntellisenseProvider()
        {
            Optional = false;
            HandlesResultInsertion = true;
            EventPublishers.Aggregator.Subscribe(this);
            if(DesignTestObject.Dispatcher.CheckAccess() && !DesignerProperties.GetIsInDesignMode(DesignTestObject))
            {
                IsUpdated = true;
                CreateDataList();
            }
        }

        #endregion

        #region Update Handling
        private void OnUpdateIntellisense()
        {
            IsUpdated = true;
            if(TextBox != null)
            {
                TextBox.UpdateErrorState();
            }
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
                if(!_hasCachedDatalist || IsUpdated)
                {
                    _hasCachedDatalist = true;
                    IsUpdated = false;
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
                            CachedDataList = activeDataList.Resource.DataList;
                            succeeded = true;
                        }
                    }
                }
                else
                {
                    CachedDataList = activeDataList.Resource.DataList;
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
            var caretPosition = context.CaretPosition;
            var region = inputText.RegionInPostion(caretPosition);

            string substr;

            if(string.IsNullOrEmpty(inputText))
            {
                substr = "";
            }
            else
            {
                substr = string.IsNullOrEmpty(replace) ? inputText : inputText.Replace(replace, "");
            }
            
            replace = DataListUtil.IsRecordsetOpeningBrace(substr) ? region.Name : replace;
            replace = replace ?? string.Empty;

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
                context.CaretPosition = caretPosition + input.Length;
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

            if(DataListUtil.EndsWithClosingTags(regionName))
            {
                var indexOfClosingBracket = DataListUtil.IndexOfClosingTags(input);
                indexOfClosingBracket = indexOfClosingBracket > 0 ? indexOfClosingBracket : 0;
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
            var startIndex = region.StartIndex;

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
            
            updatedInputText = inputText.Remove(startIndex, name.Length)
                                         .Insert(startIndex, input);

            context.CaretPosition = caretPosition + (updatedInputText.Length - inputText.Length);
            return updatedInputText;
        }
        
        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            if(context == null)
            {
                return new List<IntellisenseProviderResult>();
            }

            if(IsDisposed)
            {
                throw new ObjectDisposedException("DefaultIntellisenseProvider");
            }
           
            if(!Equals(TextBox, context.TextBox))
            {
                TextBox = context.TextBox as IntellisenseTextBox;
            }
            IList<IIntellisenseResult> results;
            var input = context.InputText ?? string.Empty;
            var caretPosition = context.CaretPosition;

            if(caretPosition > input.Length || caretPosition < 0)
            {
                return new List<IntellisenseProviderResult>();
            }

            enIntellisensePartType filterType = context.FilterType;
            enIntellisensePartType altfilterType = context.FilterType;
            IntellisenseDesiredResultSet desiredResultSet = context.DesiredResultSet;

            string searchText = context.FindTextToSearch();
            string recordsetIndex = string.Empty;
            string recordserName = string.Empty;
            var region = input.RegionInPostion(caretPosition);
            var regionName = region.Name;

            if(DataListUtil.IsValueRecordset(regionName))
            {
                recordsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(regionName);
                recordserName = DataListUtil.ExtractRecordsetNameFromValue(regionName);
                if(!string.IsNullOrEmpty(recordsetIndex) && DataListUtil.IsValueRecordsetWithFields(searchText))
                {
                    altfilterType = enIntellisensePartType.RecordsetFields;
                }
            }
            
            if(context.InputText.Equals(DataListUtil.OpeningSquareBrackets))
            {
                searchText = context.InputText;
            }
            else
            {
                searchText = searchText.StartsWith(DataListUtil.OpeningSquareBrackets) || string.IsNullOrEmpty(searchText) ? searchText : DataListUtil.OpeningSquareBrackets + searchText;
            }
            
            switch(desiredResultSet)
            {
                case IntellisenseDesiredResultSet.EntireSet: results = GetIntellisenseResultsImpl(DataListUtil.OpeningSquareBrackets, filterType); break;
                default:
                    {
                        results = GetIntellisenseResultsImpl(searchText, filterType);
                        if(results == null || results.Count == 0 && HandlesResultInsertion)
                        {
                            results = new List<IIntellisenseResult>();
                            string inputText = input;
                            var regionInPostion = inputText.RegionInPostion(caretPosition);

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

            string[] openParts = Regex.Split(input, @"\[\[");
            string[] closeParts = Regex.Split(input, @"\]\]");
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

            results = parser.ParseDataLanguageForIntellisense(input, CachedDataList, false, filterTO, true);

            return results;
        }

        #endregion

        #region Disposal Handling

        public void Dispose()
        {
            if(IsDisposed)
                return;
            IsDisposed = true;

            EventPublishers.Aggregator.Unsubscribe(this);
            CachedDataList = null;
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
