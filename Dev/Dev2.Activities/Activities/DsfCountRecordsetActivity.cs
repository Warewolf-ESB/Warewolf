using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfCountRecordsetActivity : DsfActivityAbstract<string>
    {
        #region Fields

        #endregion

        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("RecordsetName"), FindMissing]
        public string RecordsetName { get; set; }

        /// <summary>
        /// Gets or sets the count number.
        /// </summary>  
        [Outputs("CountNumber"), FindMissing]
        public string CountNumber { get; set; }

        public DsfCountRecordsetActivity()
            : base("Count Records")
        {
            RecordsetName = string.Empty;
            CountNumber = string.Empty;
            DisplayName = "Count Records";
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
                ValidateRecordsetName(RecordsetName, errors);
                allErrors.MergeErrors(errors);

                IBinaryDataList bdl = compiler.FetchBinaryDataList(executionId, out errors);
                allErrors.MergeErrors(errors);
                if (!allErrors.HasErrors())
                {
                    string err;
                    IBinaryDataListEntry recset;

                    string rs = DataListUtil.ExtractRecordsetNameFromValue(RecordsetName);

                    bdl.TryGetEntry(rs, out recset, out err);
                    allErrors.AddError(err);

                    if (dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(new DebugItemVariableParams(RecordsetName, "Recordset", recset, executionId));
                    }
                    var rule = new IsSingleValueRule(() => CountNumber);
                    var single = rule.Check();
                    if (single != null)
                    {
                        allErrors.AddError(single.Message);
                    }
                    else
                    {
                        if (recset != null)
                        {
                            if (recset.Columns != null && CountNumber != string.Empty)
                            {
                                if (recset.IsEmpty())
                                {
                                  
                                        compiler.Upsert(executionId, CountNumber, "0", out errors);
                                        if (dataObject.IsDebugMode())
                                        {
                                            AddDebugOutputItem(new DebugOutputParams(CountNumber, "0", executionId, 0));
                                        }
                                        allErrors.MergeErrors(errors);
                                    
                                }
                                else
                                {
                                        int cnt = recset.ItemCollectionSize();
                                        compiler.Upsert(executionId, CountNumber, cnt.ToString(CultureInfo.InvariantCulture), out errors);
                                        if (dataObject.IsDebugMode())
                                        {
                                            AddDebugOutputItem(new DebugOutputParams(CountNumber, cnt.ToString(CultureInfo.InvariantCulture), executionId, 0));
                                        }
                                        allErrors.MergeErrors(errors);
                                

                                }

                                allErrors.MergeErrors(errors);
                            }
                            else if (recset.Columns == null)
                            {
                                allErrors.AddError(RecordsetName + " is not a recordset");
                            }
                            else if (CountNumber == string.Empty)
                            {
                                allErrors.AddError("Blank result variable");
                            }
                        }
                        allErrors.MergeErrors(errors);
                    }
                }
            }
            finally
            {

                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfCountRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject.IsDebugMode())
                {

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
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == CountNumber);
                if(itemUpdate != null)
                {
                    CountNumber = itemUpdate.Item2;
                }
            }

        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(CountNumber);
        }

        #endregion

    }
}
