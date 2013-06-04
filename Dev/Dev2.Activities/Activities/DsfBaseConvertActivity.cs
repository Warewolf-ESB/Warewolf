using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Converters;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfBaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity
    {

        #region Fields
        private readonly Dev2BaseConversionFactory _fac = new Dev2BaseConversionFactory();
        private IList<IDebugItem> _debugInputs = new List<IDebugItem>();
        private IList<IDebugItem> _debugOutputs = new List<IDebugItem>();
        private int _indexCounter = 0;

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


        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary>       
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<IDebugItem>();
            _debugOutputs = new List<IDebugItem>();
            _indexCounter = 0;
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionId = DataListExecutionID.Get(context);

            try
            {
                CleanArgs();
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);

                foreach (BaseConvertTO item in ConvertCollection)
                {
                    _indexCounter++;
                    // Travis.Frisinger - This needs to be in the ViewModel not here ;)
                    if (item.ToExpression == string.Empty)
                    {
                        item.ToExpression = item.FromExpression;
                    }

                    IBinaryDataListEntry tmp = compiler.Evaluate(executionId, enActionType.User, item.FromExpression, false, out errors);
                    if (dataObject.IsDebug)
                    {
                        AddDebugInputItem(item.FromExpression, tmp, executionId, item.FromType, item.ToType);
                    }
                    allErrors.MergeErrors(errors);
                    if (tmp != null)
                    {

                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(tmp);

                        IBaseConverter from = _fac.CreateConverter(Dev2EnumConverter.GetEnumFromStringValue<enDev2BaseConvertType>(item.FromType));
                        IBaseConverter to = _fac.CreateConverter(Dev2EnumConverter.GetEnumFromStringValue<enDev2BaseConvertType>(item.ToType));
                        IBaseConversionBroker broker = _fac.CreateBroker(from, to);

                        int indexToUpsertTo = 1;
                        // process result information
                        while (itr.HasMoreRecords())
                        {

                            IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                            foreach (IBinaryDataListItem c in cols)
                            {

                                // set up live flushing iterator details
                                if (c.IsDeferredRead)
                                {
                                    toUpsert.HasLiveFlushing = true;
                                    toUpsert.LiveFlushingLocation = executionId;
                                }

                                indexToUpsertTo = c.ItemCollectionIndex;//2013.02.13: Ashley Lewis - Bug 8725, Task 8836
                                string val = broker.Convert(c.TheValue);
                                string expression = item.ToExpression;

                                if (DataListUtil.IsValueRecordset(item.ToExpression) && DataListUtil.GetRecordsetIndexType(item.ToExpression) == enRecordsetIndexType.Star)
                                {
                                    expression = item.ToExpression.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                    //indexToUpsertTo++;(2013.02.13: Ashley Lewis - Bug 8725, Task 8836)
                                }
                                
                                //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                                foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
                                {
                                    toUpsert.Add(region, val);
                                    if(dataObject.IsDebug)
                                    {
                                        AddDebugOutputItem(region, val, executionId);
                                    }
                                }



                                if (toUpsert.HasLiveFlushing)
                                {
                                    try
                                    {
                                        toUpsert.FlushIterationFrame();
                                        toUpsert = null;
                                        //toUpsert.PublishLiveIterationData();
                                    }
                                    catch (Exception e)
                                    {
                                        allErrors.AddError(e.Message);
                                    }
                                }
                            }


                            if (toUpsert.HasLiveFlushing)
                            {
                                try
                                {
                                    toUpsert.FlushIterationFrame(true);
                                    toUpsert = null;
                                    //toUpsert.PublishLiveIterationData();
                                }
                                catch (Exception e)
                                {
                                    allErrors.AddError(e.Message);
                                }
                            }
                            else
                            {
                                compiler.Upsert(executionId, toUpsert, out errors);
                                allErrors.MergeErrors(errors);
                            }

                            //compiler.Upsert(executionId, toUpsert, out errors);
                            //allErrors.MergeErrors(errors);
                            toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                            // Upsert the entire payload                            
                        }
                    }
                }                
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfBaseConvertActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject.IsDebug)
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

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            const string RecordsetName = "ConvertCollection";
            result.TryCreateRecordsetTemplate(RecordsetName, string.Empty, new List<Dev2Column> { DataListFactory.CreateDev2Column("FromExpression", string.Empty), DataListFactory.CreateDev2Column("FromType", string.Empty), DataListFactory.CreateDev2Column("ToType", string.Empty), DataListFactory.CreateDev2Column("Result", string.Empty) }, true, out error);
            foreach (BaseConvertTO item in ConvertCollection)
            {
                result.TryCreateRecordsetValue(item.FromExpression, "FromExpression", RecordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.FromType, "FromType", RecordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.ToType, "ToType", RecordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.ToExpression, "Result", RecordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
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
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = _indexCounter.ToString(CultureInfo.InvariantCulture) });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Convert" });

            itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId,enDev2ArgumentType.Input));

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "From" });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = fromType });

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "To" });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = toType });

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult{Type = DebugItemResultType.Label,Value = _indexCounter.ToString(CultureInfo.InvariantCulture)});
            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId,0,enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        private void InsertToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["ConvertCollection"].Collection;

            if (mic != null)
            {
                List<BaseConvertTO> listOfValidRows = ConvertCollection.Where(c => !c.CanRemove()).ToList();
                if (listOfValidRows.Count > 0)
                {
                    int startIndex = ConvertCollection.Last(c => !c.CanRemove()).IndexNumber;
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

        private void AddToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["ConvertCollection"].Collection;

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

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new BaseConvertTO(string.Empty, "Text", "Base 64", string.Empty, startIndex + 1));
            modelItem.Properties["DisplayName"].SetValue(CreateDisplayName(modelItem, startIndex + 1));
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            string currentName = modelItem.Properties["DisplayName"].ComputedValue as string;
            if (currentName.Contains("(") && currentName.Contains(")"))
            {
                if (currentName.Contains(" ("))
                {
                    currentName = currentName.Remove(currentName.IndexOf(" ("));
                }
                else
                {
                    currentName = currentName.Remove(currentName.IndexOf("("));
                }
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }

        #endregion

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                var items = ConvertCollection.Where(c => t1 != null && (!string.IsNullOrEmpty(c.FromExpression) && c.FromExpression.Equals(t1.Item1)));

                // issues updates
                foreach (var a in items)
                {
                    a.FromExpression = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                //TODO : This need to be changed when the expanded version comes in because the user can set the ToExpression
                //var items = ConvertCollection.Where(c => t1 != null && (!string.IsNullOrEmpty(c.ToExpression) && c.FromExpression.Equals(t1.Item1)));
                var items = ConvertCollection.Where(c => t1 != null && (!string.IsNullOrEmpty(c.FromExpression) && c.FromExpression.Equals(t1.Item1)));

                // issues updates
                foreach (var a in items)
                {
                    a.ToExpression = t.Item2;
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        // HACK: It appears that this activity overwrites the ConvertTO.FromExpression 
        //       during the course of its execution, so we store it for use on outputs
        string[] _foreachItems = new string[0];

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            _foreachItems = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.FromExpression)).Select(c => c.FromExpression).ToArray();
            return GetForEachItems(context, StateType.Before, _foreachItems);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            //TODO : This need to be changed when the expanded version comes in because the user can set the ToExpression
            _foreachItems = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.FromExpression)).Select(c => c.FromExpression).ToArray();
            return GetForEachItems(context, StateType.After, _foreachItems);
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return ConvertCollection.Count(caseConvertTO => !caseConvertTO.CanRemove());
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
