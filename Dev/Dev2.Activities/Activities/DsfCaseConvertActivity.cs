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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfCaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity
    {

        #region Properties

        /// <summary>
        /// The property that holds all the convertions
        /// </summary>
        public IList<ICaseConvertTO> ConvertCollection { get; set; }

        #endregion Properties

        #region Ctor

        /// <summary>
        /// The consructor for the activity 
        /// </summary>
        public DsfCaseConvertActivity()
            : base("Case Conversion")
        {
            ConvertCollection = new List<ICaseConvertTO>();
        }

        #endregion Ctor

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary>       
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            try
            {
                CleanArgs();
                //IList<string> expression = new List<string>();
                //IList<IBinaryDataListEntry> values = new List<IBinaryDataListEntry>();

                Dev2BaseConversionFactory fac = new Dev2BaseConversionFactory();

                ICaseConverter converter = CaseConverterFactory.CreateCaseConverter();
                string error = string.Empty;

                allErrors.MergeErrors(errors);

                foreach (ICaseConvertTO item in ConvertCollection)
                {
                    IBinaryDataListEntry tmp = compiler.Evaluate(executionId, enActionType.User, item.StringToConvert, false, out errors);

                    if (tmp != null)
                    {

                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(tmp);

                        string expression = string.Empty;
                        int indexToUpsertTo = 1;
                        while (itr.HasMoreRecords())
                        {

                            foreach (IBinaryDataListItem itm in itr.FetchNextRowData())
                            {
                                IBinaryDataListItem res = converter.TryConvert(item.ConvertType, itm);
                                //if (tmp.IsRecordset && DataListUtil.GetRecordsetIndexType(item.StringToConvert) == enRecordsetIndexType.Star)
                                if (DataListUtil.IsValueRecordset(item.Result) && DataListUtil.GetRecordsetIndexType(item.Result) == enRecordsetIndexType.Star)
                                {
                                    expression = item.Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString());
                                }
                                else
                                {
                                    expression = item.Result;
                                }
                                toUpsert.Add(expression, res.TheValue);

                            }
                            indexToUpsertTo++;
                        }

                        // Upsert the entire payload
                        compiler.Upsert(executionId, toUpsert, out errors);
                        allErrors.MergeErrors(errors);

                        compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                        allErrors.MergeErrors(errors);

                    }
                }
            }
            finally
            {

                // Handle Errors
                if (allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfCaseConvertActivity", allErrors);
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
                if (string.IsNullOrWhiteSpace(ConvertCollection[count].StringToConvert))
                {
                    ConvertCollection.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }

        private void InsertToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["ConvertCollection"].Collection;

            if (mic != null)
            {
                int startIndex = ConvertCollection.Last(c => !c.CanRemove()).IndexNumber;
                foreach (string s in listToAdd)
                {
                    mic.Insert(startIndex, new CaseConvertTO(s, ConvertCollection[startIndex - 1].ConvertType, s, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
        }

        private void AddToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["ConvertCollection"].Collection;

            if (mic != null)
            {
                int startIndex = 0;
                mic.Clear();
                foreach (string s in listToAdd)
                {
                    mic.Add(new CaseConvertTO(s, "UPPER", s, startIndex + 1));
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
            mic.Add(new CaseConvertTO(string.Empty, "UPPER", string.Empty, startIndex + 1));
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

        #endregion Private Methods

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            string recordsetName = "ConvertCollection";
            result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column>() { DataListFactory.CreateDev2Column("StringToConvert", string.Empty), DataListFactory.CreateDev2Column("ConvertType", string.Empty), DataListFactory.CreateDev2Column("Result", string.Empty) }, true, out error);
            foreach (CaseConvertTO item in ConvertCollection)
            {
                result.TryCreateRecordsetValue(item.StringToConvert, "StringToConvert", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.ConvertType, "ConvertType", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.Result, "Result", recordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            int indexCounter = 1;
            foreach (CaseConvertTO caseConvertTo in ConvertCollection.Where(c => !string.IsNullOrEmpty(c.StringToConvert)))
            {
                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCounter.ToString(CultureInfo.InvariantCulture) });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Convert" });

                foreach (IDebugItemResult debugItemResult in CreateDebugItems(caseConvertTo.StringToConvert, dataList))
                {
                    itemToAdd.Add(debugItemResult);
                }

                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "To" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = caseConvertTo.ConvertType });

                results.Add(itemToAdd);
                indexCounter++;
            }

            return results;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();
            int indexCounter = 1;
            foreach (CaseConvertTO caseConvertTo in ConvertCollection)
            {
                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCounter.ToString(CultureInfo.InvariantCulture) });

                foreach (IDebugItemResult debugItemResult in CreateDebugItems(caseConvertTo.Result, dataList))
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
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.StringToConvert) && c.StringToConvert.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.StringToConvert = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.Result = t.Item2;
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After);
        }

        IList<DsfForEachItem> GetForEachItems(NativeActivityContext context, StateType stateType)
        {
            var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.Result)).Select(c => c.Result).ToArray();
            return GetForEachItems(context, stateType, items);
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
