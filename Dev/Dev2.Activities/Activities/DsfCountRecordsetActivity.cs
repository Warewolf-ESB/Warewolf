using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfCountRecordsetActivity : DsfActivityAbstract<string>
    {
        #region Fields

        private string _recordsetName;
        private string _countNumber;

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
        [Outputs("CountNumber")]
        [FindMissing]
        public string CountNumber
        {
            get
            {
                return _countNumber;
            }
            set
            {
                _countNumber = value;
            }
        }

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

                        AddDebugInputItem(RecordsetName, "Recordset", recset, executionId);
                    }

                    if(recset != null)
                    {
                        if(recset.Columns != null && CountNumber != string.Empty)
                        {
                            // Travis.Frisinger - Re-did work for bug 7853 
                            if(recset.IsEmpty())
                            {
                                foreach(var region in DataListCleaningUtils.SplitIntoRegions(CountNumber))
                                {
                                    compiler.Upsert(executionId, region, "0", out errors);
                                    if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                                    {
                                        AddDebugOutputItem(region, "0", executionId);
                                    }
                                    allErrors.MergeErrors(errors);
                                }
                            }
                            else
                            {
                                foreach(var region in DataListCleaningUtils.SplitIntoRegions(CountNumber))
                                {
                                    int cnt = recset.ItemCollectionSize();
                                    compiler.Upsert(executionId, region, cnt.ToString(CultureInfo.InvariantCulture), out errors);
                                    if(dataObject.IsDebugMode())
                                    {
                                        AddDebugOutputItem(region, cnt.ToString(CultureInfo.InvariantCulture), executionId);
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
                        else if(CountNumber == string.Empty)
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
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfCountRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            if(!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if(valueEntry != null)
            {
                var res = CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input);
                itemToAdd.AddRange(res);
            }

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId)
        {
            var itemToAdd = new DebugItem();

            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId, 0, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        #endregion

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
                CountNumber = updates[0].Item2;
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
