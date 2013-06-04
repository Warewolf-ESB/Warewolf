using System.Globalization;
using System.Text.RegularExpressions;
using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfCountRecordsetActivity : DsfActivityAbstract<string>
    {
        #region Fields

        private string _recordsetName;
        private string _countNumber;
        private IList<IDebugItem> _debugInputs = new List<IDebugItem>();
        private IList<IDebugItem> _debugOutputs = new List<IDebugItem>();

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
            this.DisplayName = "Count Records";
        }       

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

        }

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<IDebugItem>();
            _debugOutputs = new List<IDebugItem>();
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

                    if(dataObject.IsDebug)
                    {
//                        var dev2Columns = recset.Columns;
//                        foreach(var dev2Column in dev2Columns)
//                        {
//                            var columnName = dev2Column.ColumnName;
//                            var expression = RecordsetName.Insert(RecordsetName.IndexOf("()", System.StringComparison.Ordinal)+2, "." + columnName);
//                            if (expression.Contains("()."))
//                            {
//                                expression = expression.Replace("().", "(*).");
//                            }
//                            
//                        }

                        AddDebugInputItem(RecordsetName, "Recordset", recset, executionId);
                    }

                    if (recset != null)
                    {
                        if (recset.Columns != null && CountNumber != string.Empty)
                        {
                            // Travis.Frisinger - Re-did work for bug 7853 
                            if(recset.IsEmpty())
                            {
                                compiler.Upsert(executionId, CountNumber, "0", out errors);
                                if (dataObject.IsDebug)
                                {
                                    AddDebugOutputItem(CountNumber, "0", executionId);
                                }
                                allErrors.MergeErrors(errors);
                            }
                            else
                            {
                                int cnt = recset.ItemCollectionSize();
                                compiler.Upsert(executionId, CountNumber, cnt.ToString(CultureInfo.InvariantCulture), out errors);
                                if (dataObject.IsDebug)
                                {
                                    AddDebugOutputItem(CountNumber, cnt.ToString(CultureInfo.InvariantCulture), executionId);
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
                    DisplayAndWriteError("DsfCountRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebug)
                {
                    DispatchDebugState(context,StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
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
                IList<IDebugItemResult> res = CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input);
                itemToAdd.AddRange(res);
            }

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId,0, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        #endregion

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
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
