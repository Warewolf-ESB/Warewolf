using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfDeleteRecordActivity : DsfActivityAbstract<string>
    {
        private string _recordsetName;
        private string _result;

        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("RecordsetName")]
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

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

        }

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            Guid executionID = dataObject.DataListID;
            

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;

            try
            {
                IBinaryDataListEntry entry = compiler.Evaluate(executionID, enActionType.Internal, RecordsetName, false, out errors);
                allErrors.MergeErrors(errors);
                //Guid parentId = compiler.FetchParentID(executionId);
                compiler.Upsert(executionID, Result, entry.FetchScalar().TheValue, out errors);
                allErrors.MergeErrors(errors);

            }
            finally
            {
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfDeleteRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }
            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            var result = new List<IDebugItem>();
            if (!string.IsNullOrEmpty(RecordsetName))
            {
                DebugItem itemToAdd = new DebugItem
                    {
                        new DebugItemResult {Type = DebugItemResultType.Label, Value = "Recordset"}
                    };
                foreach (IDebugItemResult debugItemResult in CreateDebugItems(RecordsetName, dataList))
                {
                    itemToAdd.Add(debugItemResult);
                }
                result.Add(itemToAdd);
            }
            return result;
        }

        #endregion

        #region GetDebugOutputs

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            var result = new List<IDebugItem>();
            if (!string.IsNullOrEmpty(Result))
            {
                DebugItem itemToAdd = new DebugItem();
                foreach (IDebugItemResult debugItemResult in CreateDebugItems(Result, dataList))
                {
                    itemToAdd.Add(debugItemResult);
                }
                result.Add(itemToAdd);
            }

            return result;
        }

        #endregion

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion
    }
}
