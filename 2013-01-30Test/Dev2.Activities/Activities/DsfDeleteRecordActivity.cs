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
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            Guid executionId = DataListExecutionID.Get(context);

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();

            try
            {
                IBinaryDataListEntry entry = compiler.Evaluate(executionId, enActionType.Internal, RecordsetName, false, out errors);
                allErrors.MergeErrors(errors);
                //Guid parentId = compiler.FetchParentID(executionId);
                compiler.Upsert(executionId, Result, entry.FetchScalar().TheValue, out errors);
                allErrors.MergeErrors(errors);
                compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                allErrors.MergeErrors(errors);
            }
            finally
            {
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfDeleteRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            var result = new List<IDebugItem>();

            var item = new DebugItem("Records", RecordsetName, string.Empty);
            result.Add(item);

            var rs = GetRecordSet(dataList, RecordsetName);
            string error;
            int index;
            Int32.TryParse(DataListUtil.ExtractIndexRegionFromRecordset(RecordsetName), out index);
            if (index > 0)
            {
                if (index < rs.FetchLastRecordsetIndex())
                {
                    var record = rs.FetchRecordAt(index, out error);
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var recordField in record)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        result.Add(new DebugItem(index, recordField));
                    }
                }
            }
            else
            {
                var idxItr = rs.FetchRecordsetIndexes();
                while (idxItr.HasMore())
                {
                    index = idxItr.FetchNextIndex();
                    var record = rs.FetchRecordAt(index, out error);
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var recordField in record)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        result.Add(new DebugItem(index, recordField));
                    }
                }
            }

            return result;
        }

        #endregion

        #region GetDebugOutputs

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            var result = new List<IDebugItem>();
            var theValue = GetValue(dataList, Result);

            result.Add(new DebugItem("Result", Result.ContainsSafe("[[") ? Result : null, (" = " + theValue)));

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
