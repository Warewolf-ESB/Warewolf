using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        #region Fields

        private IList<ActivityDTO> _fieldsCollection;

        #endregion

        #region Properties

        // ReSharper disable ConvertToAutoProperty
        public IList<ActivityDTO> FieldsCollection
        // ReSharper restore ConvertToAutoProperty
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

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugOutputs.Clear();
            _debugInputs.Clear();

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            toUpsert.IsDebug = (dataObject.IsDebugMode());
            toUpsert.ResourceID = dataObject.ResourceID;

            InitializeDebug(dataObject);

            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionID = DataListExecutionID.Get(context);

            try
            {
                if(!errors.HasErrors())
                {
                    var index = 1;
                    foreach(ActivityDTO t in FieldsCollection)
                    {
                        if(dataObject.IsDebugMode())
                        {
                            const string VariableLabelText = "Variable";
                            const string NewFieldLabelText = "New Value";
                            if(!string.IsNullOrEmpty(t.FieldName))
                            {
                                var debugItem = new DebugItem();
                                AddDebugItem(new DebugItemStaticDataParams("", index.ToString(CultureInfo.InvariantCulture)), debugItem);

                                var dataList = compiler.FetchBinaryDataList(executionID, out errors);


                                if(DataListUtil.IsEvaluated(t.FieldName))
                                {
                                    EvaluateEmptyRecordsetBeforeAddingToDebugOutput(dataList, t.FieldName, VariableLabelText, debugItem, compiler, executionID);
                                }
                                else
                                {
                                    AddDebugItem(new DebugItemStaticDataParams(t.FieldName, VariableLabelText), debugItem);
                                }

                                string calculationExpression;
                                if(DataListUtil.IsCalcEvaluation(t.FieldValue, out calculationExpression))
                                {
                                    var expression = string.Format("={0}", calculationExpression);
                                    var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, expression, false, out errors);
                                    AddDebugItem(new DebugItemVariableParams(expression, NewFieldLabelText, expressionsEntry, executionID), debugItem);
                                }
                                else
                                {
                                    if(DataListUtil.IsEvaluated(t.FieldValue))
                                    {
                                        var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, t.FieldValue, false, out errors);
                                        AddDebugItem(new DebugItemVariableParams(t.FieldValue, NewFieldLabelText, expressionsEntry, executionID), debugItem);
                                    }
                                    else
                                    {
                                        AddDebugItem(new DebugItemStaticDataParams(t.FieldValue, NewFieldLabelText), debugItem);
                                    }
                                }

                                _debugInputs.Add(debugItem);
                                index++;
                            }
                        }

                        if(!string.IsNullOrEmpty(t.FieldName))
                        {
                            string eval = t.FieldValue;

                            if(eval.StartsWith("@"))
                            {
                                eval = GetEnviromentVariable(dataObject, context, eval);
                            }

                            toUpsert.Add(t.FieldName, eval);
                        }
                    }

                    compiler.Upsert(executionID, toUpsert, out errors);

                    allErrors.MergeErrors(errors);

                    if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                    {
                        AddDebugTos(toUpsert);
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
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfAssignActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void EvaluateEmptyRecordsetBeforeAddingToDebugOutput(IBinaryDataList dataList, string field, string labelText, DebugItem debugItem, IDataListCompiler compiler, Guid executionID)
        {
            ErrorResultTO errors;
            string error;
            IBinaryDataListEntry expressionsEntry;

            if(DataListUtil.IsValueRecordset(field))
            {
                var found = dataList.TryGetEntry(DataListUtil.ExtractRecordsetNameFromValue(field), out expressionsEntry, out error);
                if((found && expressionsEntry.IsEmpty()) || !found)
                {
                    AddDebugItem(new DebugItemStaticDataParams("", field, labelText), debugItem);
                }
                else
                {
                    expressionsEntry = compiler.Evaluate(executionID, enActionType.User, field, false, out errors);
                    AddDebugItem(new DebugItemVariableParams(field, labelText, expressionsEntry, executionID), debugItem);
                }
            }
            else
            {
                expressionsEntry = compiler.Evaluate(executionID, enActionType.User, field, false, out errors);
                AddDebugItem(new DebugItemVariableParams(field, labelText, expressionsEntry, executionID), debugItem);
            }
        }

        void AddDebugTos(IDev2DataListUpsertPayloadBuilder<string> toUpsert)
        {
            int innerCount = 1;

            foreach(DebugOutputTO debugOutputTO in toUpsert.DebugOutputs)
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                AddDebugItem(new DebugItemVariableParams(debugOutputTO), debugItem);
                innerCount++;
                _debugOutputs.Add(debugItem);
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
                Tuple<string, string> t1 = t;
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldValue) && c.FieldValue.Contains(t1.Item1));

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
                Tuple<string, string> t1 = t;
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldName) && c.FieldName.Contains(t1.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.FieldName = a.FieldName.Replace(t.Item1, t.Item2);
                }
            }
        }

        #endregion

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return (from item in FieldsCollection
                    where !string.IsNullOrEmpty(item.FieldValue) && item.FieldValue.Contains("[[")
                    select new DsfForEachItem { Name = item.FieldName, Value = item.FieldValue }).ToList();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return (from item in FieldsCollection
                    where !string.IsNullOrEmpty(item.FieldName) && item.FieldName.Contains("[[")
                    select new DsfForEachItem { Name = item.FieldValue, Value = item.FieldName }).ToList();
        }

        #endregion

        #region Methods

        #endregion

        #region Private Method

        private string GetEnviromentVariable(IDSFDataObject dataObject, NativeActivityContext context, string eval)
        {
            if(dataObject != null)
            {
                string bookmarkName = Guid.NewGuid().ToString();
                eval = eval.Replace("@Service", dataObject.ServiceName).Replace("@Instance", context.WorkflowInstanceId.ToString()).Replace("@Bookmark", bookmarkName).Replace("@AppPath", Directory.GetCurrentDirectory());
                Uri hostUri;
                if(Uri.TryCreate(ServiceHost, UriKind.Absolute, out hostUri))
                {
                    eval = eval.Replace("@Host", ServiceHost);
                }
                eval = DataListUtil.BindEnvironmentVariables(eval, dataObject.ServiceName);
            }
            return eval;
        }
        #endregion
    }
}
