using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfRecordsetLengthActivity : DsfActivityAbstract<string>
    {
        #region Fields

        private string _recordsetName;
        private string _recordsLength;

        #endregion

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
        [Outputs("Length")]
        [FindMissing]
        public string RecordsLength
        {
            get
            {
                return _recordsLength;
            }
            set
            {
                _recordsLength = value;
            }
        }

        public DsfRecordsetLengthActivity()
            : base("Length")
        {
            RecordsetName = string.Empty;
            RecordsLength = string.Empty;
            DisplayName = "Length";
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

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = dlID;
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                if(!string.IsNullOrWhiteSpace(RecordsetName))
                {
                    IBinaryDataList bdl = compiler.FetchBinaryDataList(executionId, out errors);
                    allErrors.MergeErrors(errors);

                    string err;
                    IBinaryDataListEntry recset;

                    string rs = DataListUtil.ExtractRecordsetNameFromValue(RecordsetName);

                    bdl.TryGetEntry(rs, out recset, out err);
                    allErrors.AddError(err);

                    if(dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(new DebugItemVariableParams(RecordsetName, "Recordset", recset, executionId));
                    }

                    if(recset != null)
                    {
                        if(recset.Columns != null && RecordsLength != string.Empty)
                        {
                            // Travis.Frisinger - Re-did work for bug 7853 
                            if(recset.IsEmpty())
                            {
                                foreach(var region in DataListCleaningUtils.SplitIntoRegions(RecordsLength))
                                {
                                    compiler.Upsert(executionId, region, "0", out errors);
                                    if(dataObject.IsDebugMode())
                                    {
                                        AddDebugOutputItem(new DebugOutputParams(region, "0", executionId, 0));
                                    }
                                    allErrors.MergeErrors(errors);
                                }
                            }
                            else
                            {
                                foreach(var region in DataListCleaningUtils.SplitIntoRegions(RecordsLength))
                                {
                                    int cnt = recset.FetchAppendRecordsetIndex() - 1;
                                    compiler.Upsert(executionId, region, cnt.ToString(CultureInfo.InvariantCulture), out errors);
                                    if(dataObject.IsDebugMode())
                                    {
                                        AddDebugOutputItem(new DebugOutputParams(region, cnt.ToString(CultureInfo.InvariantCulture), executionId, 0));
                                    }
                                    allErrors.MergeErrors(errors);
                                }
                            }

                            allErrors.MergeErrors(errors);
                        }
                        else if(recset.Columns == null)
                        {
                            allErrors.AddError(RecordsetName + " is not a recordset");
                        }
                        else if(RecordsLength == string.Empty)
                        {
                            allErrors.AddError("Blank result variable");
                        }

                        allErrors.MergeErrors(errors);
                    }
                }
                else
                {
                    allErrors.AddError("No recordset given");
                }
            }
            finally
            {

                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfRecordsetLengthActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", ""));
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
            if(updates != null && updates.Count == 1)
            {
                RecordsetName = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null && updates.Count == 1)
            {
                RecordsLength = updates[0].Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(RecordsLength);
        }

        #endregion

    }
}
