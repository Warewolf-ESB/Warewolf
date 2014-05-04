using System;
using System.Activities;
using System.Collections.Generic;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfSortRecordsActivity : DsfActivityAbstract<string>
    {

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
            DisplayName = "Sort Records";
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
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ErrorResultTO errors;
            ErrorResultTO allErrors = new ErrorResultTO();
            string error;
            Guid executionID = DataListExecutionID.Get(context);

            InitializeDebug(dataObject);

            try
            {

                string rawRecsetName = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, SortField);
                string sortField = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, SortField);

                bool descOrder = String.IsNullOrEmpty(SelectedSort) || SelectedSort.Equals("Backwards");

                // Travis.Frisinger : New Stuff....
                if (!string.IsNullOrEmpty(rawRecsetName))
                {
                    IBinaryDataList bdl = compiler.FetchBinaryDataList(executionID, out errors);
                    IBinaryDataListEntry rsData;
                    bdl.TryGetEntry(rawRecsetName, out rsData, out error);
                    if (dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(SortField, "Sort Field", rsData, executionID);
                    }

                    allErrors.AddError(error);

                        // Check for fields
                        if (rsData != null && rsData.HasField(sortField))
                        {
                            rsData.Sort(sortField, descOrder, out error);
                            errors.AddError(error);

                            // Push back against the datalist
                            compiler.PushBinaryDataList(executionID, bdl, out errors);
                            allErrors.MergeErrors(errors);
                        if (dataObject.IsDebugMode())
                            {
                                bdl.TryGetEntry(rawRecsetName, out rsData, out error);
                                //Added for Bug 9479 
                                string tmpExpression = SortField;
                                if (tmpExpression.Contains("()."))
                                {
                                    tmpExpression = tmpExpression.Replace("().", "(*).");
                                }
                                AddDebugOutputItem(new DebugItemVariableParams(tmpExpression, "", rsData, executionID));
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
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject.IsDebugMode())
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
            if (valueEntry != null)
            {
                //Added for Bug 9479 - Massimo Guerrera
                if(expression.Contains("()."))
                {
                    expression = expression.Replace("().", "(*).");
                }
            }
            AddDebugInputItem(new DebugItemVariableParams(expression, labelText, valueEntry, executionId));
            AddDebugInputItem(new DebugItemStaticDataParams(SelectedSort, "Sort Order"));
        }        

        private string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {

            string rawRef = DataListUtil.StripBracketsFromValue(value);
            string objRef = string.Empty;

            if (partType == enIntellisensePartType.RecordsetsOnly)
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
            if(updates != null)
            {
            foreach (Tuple<string, string> t in updates)
            {

                if (t.Item1 == SortField)
                {
                    SortField = t.Item2;
                }
            }
        }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
           
                    if(t.Item1 == SortField)
                    {
                        SortField = t.Item2;
                    }
        }
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(SortField);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(SortField);
        }

        #endregion


        #region GetDebugInputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
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
