using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
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
    public class DsfMultiAssignActivity : DsfActivityAbstract<string>
    {
        #region Constants
        public const string CalculateTextConvertPrefix = GlobalConstants.CalculateTextConvertPrefix;
        public const string CalculateTextConvertSuffix = GlobalConstants.CalculateTextConvertSuffix;
        public const string CalculateTextConvertFormat = GlobalConstants.CalculateTextConvertFormat;
        #endregion

        private IList<ActivityDTO> _fieldsCollection;
        public IList<ActivityDTO> FieldsCollection
        {
            get
            {
                return _fieldsCollection;
            }
            set
            {
                _fieldsCollection = value;
                OnPropertyChanged("FieldsCollection");
            }
        }


        public bool UpdateAllOccurrences { get; set; }
        public bool CreateBookmark { get; set; }
        public string ServiceHost { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public DsfMultiAssignActivity()
            : base("Assign")
        {
            _fieldsCollection = new List<ActivityDTO>();
        }

        //private bool _IsDebug = false;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            //metadata.AddDelegate(_delegate);
        }

        protected override void OnExecute(NativeActivityContext context)
        {

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListBinder binder = context.GetExtension<IDataListBinder>();
            // 2012.11.05 : Travis.Frisinger - Added for Binary DataList -- Shape Input
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);

            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionID = DataListExecutionID.Get(context);

            try
            {
                if (!errors.HasErrors())
                {
                    for (int i = 0; i < FieldsCollection.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(FieldsCollection[i].FieldName))
                        {
                            string eval = FieldsCollection[i].FieldValue;

                            if (eval.Contains("@"))
                            {
                                if (dataObject != null)
                                {
                                    string bookmarkName = Guid.NewGuid().ToString();
                                    eval = eval.Replace("@Service", dataObject.ServiceName).Replace("@Instance", context.WorkflowInstanceId.ToString()).Replace("@Bookmark", bookmarkName);
                                    Uri hostUri = null;
                                    if (Uri.TryCreate(ServiceHost, UriKind.Absolute, out hostUri))
                                    {
                                        eval = eval.Replace("@Host", ServiceHost);
                                    }
                                    eval = binder.BindEnvironmentVariables(eval, dataObject.ServiceName);
                                }
                            }

                            toUpsert.Add(FieldsCollection[i].FieldName, eval);
                        }
                    }

                    compiler.Upsert(executionID, toUpsert, out errors);
                    allErrors.MergeErrors(errors);

                    // Merge Back into list
                    compiler.Shape(executionID, enDev2ArgumentType.Output, OutputMapping, out errors);
                    allErrors.MergeErrors(errors);
                }

            }
            finally
            {
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfWebpageActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }
            }

        }

        public void RemoveItem()
        {
            List<int> BlankCount = new List<int>();
            foreach (dynamic item in FieldsCollection)
            {
                if (String.IsNullOrEmpty(item.FieldName) && String.IsNullOrEmpty(item.FieldValue))
                {
                    BlankCount.Add(item.IndexNumber);
                }
            }
            if (BlankCount.Count > 1 && FieldsCollection.Count > 2)
            {
                FieldsCollection.Remove(FieldsCollection[BlankCount[0] - 1]);
                for (int i = BlankCount[0] - 1; i < FieldsCollection.Count; i++)
                {
                    dynamic tmp = FieldsCollection[i] as dynamic;
                    tmp.IndexNumber = i + 1;
                }
            }
        }

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            string recordsetName = "FieldsCollection";
            result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column>() { DataListFactory.CreateDev2Column("FieldName", string.Empty), DataListFactory.CreateDev2Column("FieldValue", string.Empty) }, true, out error);
            foreach (ActivityDTO item in FieldsCollection)
            {
                result.TryCreateRecordsetValue(item.FieldName, "FieldName", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.FieldValue, "FieldValue", recordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods


        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            var result = new List<IDebugItem>();
            int indexCounter = 1;
            foreach (ActivityDTO activityDto in FieldsCollection.Where(c => !c.CanRemove()))
            {
                string variable = activityDto.FieldValue;

                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCounter.ToString(CultureInfo.InvariantCulture) });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = activityDto.FieldName });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });

                foreach (IDebugItemResult debugItemResult in CreateDebugItems(variable, dataList))
                {

                    if (debugItemResult.Type == DebugItemResultType.Variable && debugItemResult.Value.ContainsSafe("!~calculation~!"))
                    {
                        debugItemResult.Value = debugItemResult.Value.Replace("!~calculation~!", "").Replace("!~~calculation~!", "");
                    }

                    itemToAdd.Add(debugItemResult);
                }
                indexCounter++;
                result.Add(itemToAdd);
            }
            return result;
        }

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldValue) && c.FieldValue.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.FieldValue = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldName) && c.FieldValue.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.FieldName = t.Item2;
                }
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldValue) && c.FieldValue.Contains("[[")).Select(c => c.FieldValue).ToArray();
            return GetForEachItems(context, StateType.Before, items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldName)).Select(c => c.FieldName).ToArray();
            return GetForEachItems(context, StateType.After, items);
        }

        #endregion
    }
}
