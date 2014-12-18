
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
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.Convertors.Base;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Converters;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Validation;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfBaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity
    {

        #region Fields
        private readonly Dev2BaseConversionFactory _fac = new Dev2BaseConversionFactory();
        private int _indexCounter;

        #endregion

        #region Properties

        /// <summary>
        /// The property that holds all the convertions
        /// </summary>
        public IList<BaseConvertTO> ConvertCollection { get; set; }

        #endregion Properties

        #region Ctor

        /// <summary>
        /// The consructor for the activity 
        /// </summary>
        public DsfBaseConvertActivity()
            : base("Base Conversion")
        {
            ConvertCollection = new List<BaseConvertTO>();
        }

        #endregion Ctor

        #region Overridden NativeActivity Methods

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

        private readonly IList<string> nonFramedTokens = new List<string>();
        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary>       
        // ReSharper disable MethodTooLong
        // ReSharper disable FunctionComplexityOverflow
        protected override void OnExecute(NativeActivityContext context)
        // ReSharper restore MethodTooLong
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            _indexCounter = 0;
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionId = DataListExecutionID.Get(context);
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);

            InitializeDebug(dataObject);

            try
            {
                CleanArgs();

                toUpsert.IsDebug = dataObject.IsDebugMode();

                foreach (var item in ConvertCollection)
                {
                    try
                    {
                        _indexCounter++;
                        // Travis.Frisinger - This needs to be in the ViewModel not here ;)
                        if (item.ToExpression == string.Empty)
                        {
                            item.ToExpression = item.FromExpression;
                        }
                        if (nonFramedTokens.Contains(item.ToExpression))
                        {
                            // reset the framing ;)
                            nonFramedTokens.Clear();
                            nonFramedTokens.Add(item.ToExpression);

                            compiler.Upsert(executionId, toUpsert, out errors);
                            allErrors.MergeErrors(errors);

                            if (!allErrors.HasErrors() && toUpsert != null)
                            {
                                var outIndex = 1;
                                foreach (var debugOutputTo in toUpsert.DebugOutputs)
                                {
                                    var debugItem = new DebugItem();
                                    AddDebugItem(new DebugItemStaticDataParams("", outIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                                    AddDebugItem(new DebugItemVariableParams(debugOutputTo), debugItem);
                                    _debugOutputs.Add(debugItem);
                                    outIndex++;
                                }
                            }
                        }
                        else
                        {
                            // current append indexes are alright ;)
                            nonFramedTokens.Add(item.ToExpression);
                        }

                        IsSingleValueRule.ApplyIsSingleValueRule(item.FromExpression, allErrors);
                        var fieldName = item.FromExpression;
                        fieldName = DataListUtil.IsValueRecordset(fieldName) ? DataListUtil.ReplaceRecordsetIndexWithBlank(fieldName) : fieldName;
                        var datalist = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), Dev2.DataList.Contract.enTranslationDepth.Shape, out errors);
                        if (!datalist.IsNullOrEmpty())
                        {
                            var isValidExpr = new IsValidExpressionRule(() => fieldName, datalist.ToString())
                            {
                                LabelText = fieldName
                            };

                            var errorInfo = isValidExpr.Check();
                            if (errorInfo != null)
                            {
                                item.FromExpression = "";
                                errors.AddError(errorInfo.Message);
                            }
                            allErrors.MergeErrors(errors);
                        }

                        IBinaryDataListEntry tmp = compiler.Evaluate(executionId, enActionType.User, item.FromExpression, false, out errors);
                        if (dataObject.IsDebugMode())
                        {
                            AddDebugInputItem(item.FromExpression, tmp, executionId, item.FromType, item.ToType);
                        }
                        allErrors.MergeErrors(errors);
                        if (tmp != null)
                        {

                            IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(tmp);

                            IBaseConverter from = _fac.CreateConverter((enDev2BaseConvertType)Dev2EnumConverter.GetEnumFromStringDiscription(item.FromType, typeof(enDev2BaseConvertType)));
                            IBaseConverter to = _fac.CreateConverter((enDev2BaseConvertType)Dev2EnumConverter.GetEnumFromStringDiscription(item.ToType, typeof(enDev2BaseConvertType)));
                            IBaseConversionBroker broker = _fac.CreateBroker(from, to);

                            // process result information
                            while (itr.HasMoreRecords())
                            {

                                IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                                foreach (IBinaryDataListItem c in cols)
                                {
                                    // set up live flushing iterator details
                                    int indexToUpsertTo = c.ItemCollectionIndex;

                                    string val = string.IsNullOrEmpty(c.TheValue) ? "" : broker.Convert(c.TheValue);
                                    string expression = item.ToExpression;

                                    if (DataListUtil.IsValueRecordset(item.ToExpression) && DataListUtil.GetRecordsetIndexType(item.ToExpression) == enRecordsetIndexType.Star)
                                    {
                                        expression = item.ToExpression.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                    }
                                    toUpsert.Add(expression, val);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        Dev2Logger.Log.Error("DSFBaseConvert", e);
                        allErrors.AddError(e.Message);
                    }
                    finally
                    {
                        if (allErrors.HasErrors())
                        {
                            toUpsert.Add(item.ToExpression, null);
                        }
                    }
                }
                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);

                if (!allErrors.HasErrors() && toUpsert != null)
                {
                    var outIndex = 1;
                    foreach (var debugOutputTo in toUpsert.DebugOutputs)
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", outIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugItemVariableParams(debugOutputTo), debugItem);
                        _debugOutputs.Add(debugItem);
                        outIndex++;
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error("DSFBaseConvert", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfBaseConvertActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        #endregion

        #region Private Methods

        private void CleanArgs()
        {
            int count = 0;
            while (count < ConvertCollection.Count)
            {
                if (string.IsNullOrWhiteSpace(ConvertCollection[count].FromExpression))
                {
                    ConvertCollection.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #region Private Methods

        private void AddDebugInputItem(string expression, IBinaryDataListEntry valueEntry, Guid executionId, string fromType, string toType)
        {
            var itemToAdd = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), itemToAdd);
            AddDebugItem(new DebugItemVariableParams(expression, "Convert", valueEntry, executionId), itemToAdd);
            AddDebugItem(new DebugItemStaticDataParams(fromType, "From"), itemToAdd);
            AddDebugItem(new DebugItemStaticDataParams(toType, "To"), itemToAdd);
            _debugInputs.Add(itemToAdd);
        }

        private void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            if (modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if (mic != null)
                {
                    List<BaseConvertTO> listOfValidRows = ConvertCollection.Where(c => !c.CanRemove()).ToList();
                    if (listOfValidRows.Count > 0)
                    {
                        BaseConvertTO baseConvertTo = ConvertCollection.Last(c => !c.CanRemove());
                        int startIndex = ConvertCollection.IndexOf(baseConvertTo) + 1;
                        foreach (string s in listToAdd)
                        {
                            mic.Insert(startIndex, new BaseConvertTO(s, ConvertCollection[startIndex - 1].FromType, ConvertCollection[startIndex - 1].ToType, string.Empty, startIndex + 1));
                            startIndex++;
                        }
                        CleanUpCollection(mic, modelItem, startIndex);
                    }
                    else
                    {
                        AddToCollection(listToAdd, modelItem);
                    }
                }
            }
        }

        private void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            if (modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if (mic != null)
                {
                    int startIndex = 0;
                    string firstRowConvertFromType = ConvertCollection[0].FromType;
                    string firstRowConvertToType = ConvertCollection[0].ToType;
                    mic.Clear();
                    foreach (string s in listToAdd)
                    {
                        mic.Add(new BaseConvertTO(s, firstRowConvertFromType, firstRowConvertToType, string.Empty, startIndex + 1));
                        startIndex++;
                    }
                    CleanUpCollection(mic, modelItem, startIndex);
                }
            }
        }

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new BaseConvertTO(string.Empty, "Text", "Base 64", string.Empty, startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                string currentName = modelProperty.ComputedValue as string;
                if (currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
                {
                    currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
                }
                currentName = currentName + " (" + (count - 1) + ")";
                return currentName;
            }

            return string.Empty;
        }

        #endregion

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.FromExpression) && c.FromExpression.Contains(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.FromExpression = a.FromExpression.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            foreach (Tuple<string, string> t in updates)
            {

                // locate all updates for this tuple
                //TODO : This need to be changed when the expanded version comes in because the user can set the ToExpression
                Tuple<string, string> t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.FromExpression) && c.FromExpression.Contains(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.ToExpression = a.FromExpression.Replace(t.Item1, t.Item2);
                }
            }

        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var result = new List<DsfForEachItem>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in ConvertCollection)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if (!string.IsNullOrEmpty(item.FromExpression) && item.FromExpression.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.FromExpression, Value = item.FromExpression });
                }
            }

            return result;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var result = new List<DsfForEachItem>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in ConvertCollection)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if (!string.IsNullOrEmpty(item.FromExpression) && item.FromExpression.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.FromExpression, Value = item.FromExpression });
                }
            }

            return result;
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return ConvertCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());
        }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem)
        {
            if (!overwrite)
            {
                InsertToCollection(listToAdd, modelItem);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        #endregion
    }
}
