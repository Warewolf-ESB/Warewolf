using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
using Dev2.Enums;

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

        #region Fields

        private IList<ActivityDTO> _fieldsCollection;

        #endregion

        #region Properties

        public IList<ActivityDTO> FieldsCollection
        {
            get
            {
                return _fieldsCollection;
            }
            set
            {
                _fieldsCollection = value;
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

        #endregion

        #region Ctor

        public DsfMultiAssignActivity()
            : base("Assign")
        {
            _fieldsCollection = new List<ActivityDTO>();
        }

        #endregion

        #region Overridden NativeActivity Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugOutputs.Clear();

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            toUpsert.IsDebug = (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke);
            toUpsert.ResourceID = dataObject.ResourceID;

            if(dataObject.IsDebug || dataObject.RemoteInvoke)
            {
                DispatchDebugState(context, StateType.Before);
            }

            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionID = DataListExecutionID.Get(context);

            try
            {
                if(!errors.HasErrors())
                {
                    for(int i = 0; i < FieldsCollection.Count; i++)
                    {
                        if(!string.IsNullOrEmpty(FieldsCollection[i].FieldName))
                        {
                            string eval = FieldsCollection[i].FieldValue;

                            if(eval.StartsWith("@"))
                            {
                                eval = GetEnviromentVariable(dataObject, context, eval);
                            }

                            toUpsert.Add(FieldsCollection[i].FieldName, eval);
                        }
                    }

                    compiler.Upsert(executionID, toUpsert, out errors);

                    if(dataObject.IsDebugMode())
                    {
                        int innerCount = 0;
                        foreach(DebugOutputTO debugOutputTO in toUpsert.DebugOutputs)
                        {
                            AddDebugItem(debugOutputTO.TargetEntry, FieldsCollection[innerCount].FieldValue,
                                         debugOutputTO.FromEntry, executionID, innerCount,
                                         debugOutputTO.UsedRecordsetIndex, dataObject, context);
                            innerCount++;
                            if(debugOutputTO.FromEntry != null)
                                debugOutputTO.FromEntry.Dispose();
                            if(debugOutputTO.TargetEntry != null)
                                debugOutputTO.TargetEntry.Dispose();
                        }
                    }
                    allErrors.MergeErrors(errors);
                }

            }
            catch(Exception e)
            {
                this.LogError(e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfAssignActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldValue) && c.FieldValue.Contains(t.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.FieldValue = a.FieldValue.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(Tuple<string, string> t in updates)
            {

                // locate all updates for this tuple
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldName) && c.FieldName.Contains(t.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.FieldName = a.FieldName.Replace(t.Item1, t.Item2);
                }
            }
        }

        #endregion

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            string recordsetName = "FieldsCollection";
            result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column>() { DataListFactory.CreateDev2Column("FieldName", string.Empty), DataListFactory.CreateDev2Column("FieldValue", string.Empty) }, true, out error);
            foreach(ActivityDTO item in FieldsCollection)
            {
                result.TryCreateRecordsetValue(item.FieldName, "FieldName", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.FieldValue, "FieldValue", recordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> invalidDebugItems = new List<IDebugItem>();
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
                if(string.IsNullOrEmpty(debugOutput.FetchResultsList()[1].Value))
                {
                    invalidDebugItems.Add(debugOutput);
                }
            }
            foreach(DebugItem invalidDebugItem in invalidDebugItems)
            {
                _debugOutputs.Remove(invalidDebugItem);
            }

            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var result = new List<DsfForEachItem>();

            foreach(var item in FieldsCollection)
            {
                if(!string.IsNullOrEmpty(item.FieldValue) && item.FieldValue.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.FieldName, Value = item.FieldValue });
                }
            }

            return result;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var result = new List<DsfForEachItem>();

            foreach(var item in FieldsCollection)
            {
                if(!string.IsNullOrEmpty(item.FieldName) && item.FieldName.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.FieldValue, Value = item.FieldName });
                }
            }

            return result;
        }

        #endregion

        #region Methods

        public void RemoveItem()
        {
            List<int> BlankCount = new List<int>();
            foreach(dynamic item in FieldsCollection)
            {
                if(String.IsNullOrEmpty(item.FieldName) && String.IsNullOrEmpty(item.FieldValue))
                {
                    BlankCount.Add(item.IndexNumber);
                }
            }
            if(BlankCount.Count > 1 && FieldsCollection.Count > 2)
            {
                FieldsCollection.Remove(FieldsCollection[BlankCount[0] - 1]);
                for(int i = BlankCount[0] - 1; i < FieldsCollection.Count; i++)
                {
                    dynamic tmp = FieldsCollection[i] as dynamic;
                    tmp.IndexNumber = i + 1;
                }
            }
        }

        #endregion

        #region Private Method

        private string GetEnviromentVariable(IDSFDataObject dataObject, NativeActivityContext context, string eval)
        {
            if(dataObject != null)
            {
                string bookmarkName = Guid.NewGuid().ToString();
                eval = eval.Replace("@Service", dataObject.ServiceName).Replace("@Instance", context.WorkflowInstanceId.ToString()).Replace("@Bookmark", bookmarkName).Replace("@AppPath", Directory.GetCurrentDirectory());
                Uri hostUri = null;
                if(Uri.TryCreate(ServiceHost, UriKind.Absolute, out hostUri))
                {
                    eval = eval.Replace("@Host", ServiceHost);
                }
                eval = DataListUtil.BindEnvironmentVariables(eval, dataObject.ServiceName);
            }
            return eval;
        }

        private void AddDebugItem(IBinaryDataListEntry fieldEntry, string fieldValue, IBinaryDataListEntry valueEntry, Guid executionId, int indexNumToUse, int rsIdx, IDSFDataObject dataObject, NativeActivityContext context)
        {
            if(valueEntry != null)
            {
                if(fieldValue.Contains("@"))
                {
                    string eval = GetEnviromentVariable(dataObject, context, fieldValue);
                    var results = CreateDebugItemsFromString(FieldsCollection[indexNumToUse].FieldName, FieldsCollection[indexNumToUse].FieldValue, DataListExecutionID.Get(context), indexNumToUse, enDev2ArgumentType.Output);
                    var itemToAdd = new DebugItem();
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = (indexNumToUse + 1).ToString(CultureInfo.InvariantCulture) });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = eval });
                    itemToAdd.AddRange(results);
                    _debugOutputs.Add(itemToAdd);
                }
                else
                {
                    var itemToAdd = new DebugItem();
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = (indexNumToUse + 1).ToString(CultureInfo.InvariantCulture) });
                    string fieldName = FieldsCollection[indexNumToUse].FieldName;
                    if(fieldEntry != null && fieldEntry.IsRecordset &&
                        (DataListUtil.GetRecordsetIndexType(FieldsCollection[indexNumToUse].FieldName)
                        == enRecordsetIndexType.Blank))
                    {
                        fieldName = fieldName.Replace("().", string.Concat("(", rsIdx.ToString(CultureInfo.InvariantCulture), ")."));
                    }
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = fieldName });
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });

                    itemToAdd.AddRange(CreateDebugItemsFromEntry(fieldValue, valueEntry, executionId, enDev2ArgumentType.Output, valueEntry.FetchAppendRecordsetIndex()));

                    _debugOutputs.Add(itemToAdd);
                }
            }
        }

        #endregion
    }
}
