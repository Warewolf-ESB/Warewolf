using Dev2;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfSortRecordsActivity : DsfActivityAbstract<string>
    {

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        [Inputs("SortField")]
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
            //DataObject = context.GetExtension<IDSFDataObject>();
            IDataListBinder binder = context.GetExtension<IDataListBinder>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
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
                    allErrors.AddError(error);
                    //IBinaryDataListEntry rsData = compiler.Evaluate(executionID, enActionType.User, fetchStr, true, out errors);
                    if (errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }
                    else
                    {
                        // Check for fields
                        if (rsData.HasField(sortField))
                        {
                            rsData.Sort(sortField, descOrder, out error);
                            errors.AddError(error);

                            // Push back against the datalist
                            compiler.PushBinaryDataList(executionID, bdl, out errors);
                            allErrors.MergeErrors(errors);

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
                    string err = DisplayAndWriteError("DsfSortRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }

            // End Travis.Frisinger New Stuff
        }

        #region Private Methods

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
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }


        #region GetDebugInputs/Outputs

        #region GetDebugInputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            var result = new List<IDebugItem>();
            DebugItem itemToAdd;
            if (!string.IsNullOrEmpty(SortField))
            {
                itemToAdd = new DebugItem
                    {
                        new DebugItemResult {Type = DebugItemResultType.Label, Value = "Sort Field"}
                    };

                foreach (IDebugItemResult debugItemResult in CreateDebugItems(SortField, dataList))
                {
                    itemToAdd.Add(debugItemResult);
                }
                result.Add(itemToAdd);
            }
            itemToAdd = new DebugItem
                {
                    new DebugItemResult {Type = DebugItemResultType.Label, Value = "Sort Order"},
                    new DebugItemResult {Type = DebugItemResultType.Value, Value = SelectedSort}
                };
            result.Add(itemToAdd);

            return result;
        }

        #endregion

        #region GetDebugOutputs

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            var result = new List<IDebugItem>();
            if (!string.IsNullOrEmpty(SortField))
            {
                DebugItem itemToAdd = new DebugItem();
                foreach (IDebugItemResult debugItemResult in CreateDebugItems(SortField, dataList))
                {
                    itemToAdd.Add(debugItemResult);
                }
                result.Add(itemToAdd);
            }

            return result;
        }

        #endregion

        #endregion

    }
}
