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
    public class DsfCountRecordsetActivity : DsfActivityAbstract<string>
    {
        private string _recordsetName;
        private string _countNumber;

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

        //private Variable<string> _recordSetName = new Variable<string>();
        //private Variable<string> _countNumber2 = new Variable<string>();

        //public InOutArgument<string> RecordsetName { get; set; }

        /// <summary>
        /// Gets or sets the count number.
        /// </summary>  
        [Outputs("CountNumber")]
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

        //public InOutArgument<string> CountNumber { get; set; }

        public DsfCountRecordsetActivity()
            : base("Count Records")
        {
            RecordsetName = string.Empty;
            CountNumber = string.Empty;
            this.DisplayName = "Count Records";
        }

        //public override void PreConfigureActivity()
        //{
        //    base.PreConfigureActivity();

        //    RecordsetName = new InOutArgument<string>(_recordSetName);
        //    CountNumber = new InOutArgument<string>(_countNumber2);
        //}

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

        }

        protected override void OnExecute(NativeActivityContext context)
        {

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = dlID;
           // Guid executionId = compiler.Shape(dlID, enDev2ArgumentType.Input, InputMapping, out errors);
            allErrors.MergeErrors(errors);

            // Process if no errors
            try
            {
                if (!string.IsNullOrWhiteSpace(RecordsetName))
                {


                    IBinaryDataList bdl = compiler.FetchBinaryDataList(executionId, out errors);
                    allErrors.MergeErrors(errors);

                    string err;
                    IBinaryDataListEntry recset;

                    string rs = DataListUtil.ExtractRecordsetNameFromValue(RecordsetName);

                    bdl.TryGetEntry(rs, out recset, out err);
                    allErrors.AddError(err);

                    //if(entry != null)
                    //{
                        
                    //}

                    //IBinaryDataListEntry recset = compiler.Evaluate(executionId, enActionType.User, RecordsetName, false, out errors);
                    if(recset != null)
                    {

                        allErrors.MergeErrors(errors);

                        if(recset.Columns != null && CountNumber != string.Empty)
                        {
                            string error;
                            // Travis.Frisinger - Re-did work for bug 7853 
                            if(recset.IsEmpty())
                            {
                                compiler.Upsert(executionId, CountNumber, "0", out errors);
                            }
                            else if(recset.FetchRecordAt(1, out error).Count > 0)
                            {
                                compiler.Upsert(executionId, CountNumber, recset.FetchLastRecordsetIndex().ToString(), out errors);
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

                        //compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
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
                // now delete executionID
                //compiler.DeleteDataListByID(executionId);

                // Handle Errors
                if (allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfCountRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
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
                var itemToAdd = new DebugItem
                {
                    new DebugItemResult { Type = DebugItemResultType.Label, Value = "Recordset" }, 
                    new DebugItemResult { Type = DebugItemResultType.Variable, Value = RecordsetName }
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

            if (!string.IsNullOrEmpty(CountNumber))
            {
                DebugItem itemToAdd = new DebugItem();
                foreach (IDebugItemResult debugItemResult in CreateDebugItems(CountNumber, dataList))
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
            if (updates.Count == 1)
            {
                RecordsetName = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                CountNumber = updates[0].Item2;
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, CountNumber);
        }

        #endregion

    }
}
