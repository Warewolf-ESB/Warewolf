using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Enums;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfSortRecordsActivity : DsfActivityAbstract<string>
    {
        #region Fields

        private IList<IDebugItem> _debugInputs = new List<IDebugItem>();
        private IList<IDebugItem> _debugOutputs = new List<IDebugItem>();

        #endregion

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        [Inputs("SortField")]
        [FindMissing]
        public string SortField { get; set; }

        /// <summary>
        /// Gets or sets the selected sort.
        /// </summary>
        [Inputs("SelectedSort")]        
        public string SelectedSort { get; set; }

        public DsfSortRecordsActivity()
            : base("Sort Records")
        {
            SortField = string.Empty;
            SelectedSort = "Forward";
            this.DisplayName = "Sort Records";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }


        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<IDebugItem>();
            _debugOutputs = new List<IDebugItem>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            string error = string.Empty;
            //string preExecute = DataObject.XmlData;
            Guid executionID = DataListExecutionID.Get(context);

            try
            {

                string rawRecsetName = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, SortField);
                string sortField = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, SortField);

                bool descOrder = String.IsNullOrEmpty(SelectedSort) || SelectedSort.Equals("Backwards");

                // Travis.Frisinger : New Stuff....
                if (!string.IsNullOrEmpty(rawRecsetName))
                {
                    IBinaryDataList bdl = compiler.FetchBinaryDataList(executionID, out errors);
                    IBinaryDataListEntry rsData;
                    bdl.TryGetEntry(rawRecsetName, out rsData, out error);
                    if(dataObject.IsDebug)
                    {
                        AddDebugInputItem(SortField, "Sort Field", rsData, executionID);
                    }
                    allErrors.AddError(error);
                    //IBinaryDataListEntry rsData = compiler.Evaluate(executionID, enActionType.User, fetchStr, true, out errors);
                    if (errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }
                    else
                    {
                        // Check for fields
                        if (rsData != null && rsData.HasField(sortField))
                        {
                            rsData.Sort(sortField, descOrder, out error);
                            errors.AddError(error);

                            // Push back against the datalist
                            compiler.PushBinaryDataList(executionID, bdl, out errors);
                            allErrors.MergeErrors(errors);
                            if(dataObject.IsDebug)
                            {
                                bdl.TryGetEntry(rawRecsetName, out rsData, out error);
                                IDebugItem itemToAdd = new DebugItem();
                                itemToAdd.AddRange(CreateDebugItemsFromEntry(SortField,rsData,executionID,enDev2ArgumentType.Output));
                                _debugOutputs.Add(itemToAdd);
                            }

                        }
                        else
                        {
                            // no field, default behavior .... Do nothing
                        }
                    }
                }
                else
                {
                    allErrors.AddError("No recordset given");
                }
            }
            finally
            {

                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfSortRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebug)
                {
                    DispatchDebugState(context,StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

            // End Travis.Frisinger New Stuff
        }

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();            

            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);

              itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Sort Order" });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value =  SelectedSort});
            _debugInputs.Add(itemToAdd);
        }        

        private string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {

            string rawRef = DataListUtil.StripBracketsFromValue(value);
            string objRef = string.Empty;

            if (partType == enIntellisensePartType.RecorsetsOnly)
            {
                objRef = DataListUtil.ExtractRecordsetNameFromValue(rawRef);
            }
            else if (partType == enIntellisensePartType.RecordsetFields)
            {
                objRef = DataListUtil.ExtractFieldNameFromValue(rawRef);
            }

            return objRef;
        }

        #endregion Private Methods

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {

                if (t.Item1 == SortField)
                {
                    SortField = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
           
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, SortField);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return new List<DsfForEachItem>();
        }

        #endregion


        #region GetDebugInputs/Outputs

        #region GetDebugInputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #endregion

    }
}
