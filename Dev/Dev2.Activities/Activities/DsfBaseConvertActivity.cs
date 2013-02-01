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
using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfBaseConvertActivity : DsfActivityAbstract<string>
    {

        #region Fields
        private readonly Dev2BaseConversionFactory _fac = new Dev2BaseConversionFactory();
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
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionId = DataListExecutionID.Get(context);

            try
            {
                CleanArgs();
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);

                foreach (BaseConvertTO item in ConvertCollection)
                {

                    // Travis.Frisinger - This needs to be in the ViewModel not here ;)
                    if (item.ToExpression == string.Empty)
                    {
                        item.ToExpression = item.FromExpression;
                    }

                    IBinaryDataListEntry tmp = compiler.Evaluate(executionId, enActionType.User, item.FromExpression, false, out errors);
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
                                string val = broker.Convert(c.TheValue);
                                string expression = item.ToExpression;

                                if (DataListUtil.IsValueRecordset(item.ToExpression) && DataListUtil.GetRecordsetIndexType(item.ToExpression) == enRecordsetIndexType.Star)
                                {
                                    expression = item.ToExpression.Replace(GlobalConstants.StarExpression,
                                                                           indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                    indexToUpsertTo++;
                                }

                                toUpsert.Add(expression, val);
                            }
                            compiler.Upsert(executionId, toUpsert, out errors);
                            allErrors.MergeErrors(errors);
                            toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                            // Upsert the entire payload                            
                        }
                    }
                }

                compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                allErrors.MergeErrors(errors);

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
                    string err = DisplayAndWriteError("DsfBaseConvertActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }
        }

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

        // HACK: It appears that this activity overwrites the ConvertTO.FromExpression 
        //       during the course of its execution, so we store it for use on outputs
        string[] _debugItems = new string[0];

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            int indexCounter = 1;
            foreach (BaseConvertTO baseConvertTo in ConvertCollection.Where(c => !string.IsNullOrEmpty(c.FromExpression)))
            {
                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCounter.ToString(CultureInfo.InvariantCulture) });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Convert" });
                foreach (IDebugItemResult debugItemResult in CreateDebugItems(baseConvertTo.FromExpression, dataList))
                {
                    itemToAdd.Add(debugItemResult);
                }

                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "From" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = baseConvertTo.FromType });

                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "To" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = baseConvertTo.ToType });

                results.Add(itemToAdd);
                indexCounter++;
            }

            return results;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();
            int indexCounter = 1;
            foreach (BaseConvertTO baseConvertTo in ConvertCollection)
            {
                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCounter.ToString(CultureInfo.InvariantCulture) });
                foreach (IDebugItemResult debugItemResult in CreateDebugItems(baseConvertTo.ToExpression, dataList))
                {
                    itemToAdd.Add(debugItemResult);
                }
                results.Add(itemToAdd);
                indexCounter++;
            }

            return results;
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
                var items = ConvertCollection.Where(c => t1 != null && (!string.IsNullOrEmpty(c.ToExpression) && c.FromExpression.Equals(t1.Item1)));

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
            return GetForEachItems(context, StateType.After, _foreachItems);
        }

        #endregion

    }
}
