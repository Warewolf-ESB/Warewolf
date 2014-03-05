using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfDeleteRecordActivity : DsfActivityAbstract<string>
    {
        private string _recordsetName;
        private string _result;

        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("RecordsetName")]
        [FindMissing]
        public string RecordsetName
        {
            get
            {
                return _recordsetName;
            }
            set
            {
                _recordsetName = value;
            }
        }

        /// <summary>
        /// Gets or sets the count number.
        /// </summary>  
        [Outputs("Result")]
        [FindMissing]
        public new string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }

        public DsfDeleteRecordActivity()
            : base("Delete Record")
        {
            RecordsetName = string.Empty;
            Result = string.Empty;
        }

// ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

        }
// ReSharper restore RedundantOverridenMember

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid executionID = dataObject.DataListID;


            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            InitializeDebug(dataObject);
            try
            {
                var tmpRecsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(RecordsetName);
                IBinaryDataListEntry indexEntry = compiler.Evaluate(executionID, enActionType.User, tmpRecsetIndex, false, out errors);

                if(DataListUtil.IsValueRecordset(RecordsetName))
                {
                    IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(indexEntry);
                    IDev2IteratorCollection collection = Dev2ValueObjectFactory.CreateIteratorCollection();
                    collection.AddIterator(itr);

                    if(!allErrors.HasErrors())
                    {
                        while(collection.HasMoreData())
                        {
                            var evaluatedRecordset = RecordsetName.Remove(RecordsetName.IndexOf("(", StringComparison.Ordinal) + 1) + collection.FetchNextRow(itr).TheValue + ")]]";
                            if(dataObject.IsDebugMode())
                            {
                                IBinaryDataListEntry tmpentry = compiler.Evaluate(executionID, enActionType.User, evaluatedRecordset.Replace("()", "(*)"), false, out errors);
                                AddDebugInputItem(new DebugItemVariableParams(RecordsetName, "Records", tmpentry, executionID));
                            }

                            IBinaryDataListEntry entry = compiler.Evaluate(executionID, enActionType.Internal, evaluatedRecordset, false, out errors);

                            allErrors.MergeErrors(errors);
                            compiler.Upsert(executionID, Result, entry.FetchScalar().TheValue, out errors);

                            if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                            {
                                AddDebugOutputItem(new DebugItemVariableParams(Result, "", entry, executionID));
                            }
                            allErrors.MergeErrors(errors);
                        }
                    }
                }
                else
                {
                    AddDebugInputItem(new DebugItemVariableParams(RecordsetName, "Records", indexEntry, executionID));
                    allErrors.AddError("Invalid Input : Input must be a recordset.");
                    compiler.Upsert(executionID, Result, "Failure", out errors);
                }
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfDeleteRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugOutputParams(Result, "", executionID, 0));
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }
        
        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(var t in updates)
                {

                    if(t.Item1 == RecordsetName)
                    {
                        RecordsetName = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
