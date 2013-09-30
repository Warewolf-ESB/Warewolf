using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
    public class DsfWebGetRequestActivity : DsfActivityAbstract<string>
    {
        IWebRequestInvoker _webRequestInvoker;
        

        public IWebRequestInvoker WebRequestInvoker
        {
            get
            {
                return _webRequestInvoker ?? (_webRequestInvoker = new WebRequestInvoker());
            }
            set
            {
                _webRequestInvoker = value;
            }
        }


        [FindMissing]
        public string Method { get; set; }
        [Inputs("Url")]
        [FindMissing]
        public string Url { get; set; }
        [FindMissing]
        public string Headers { get; set; }
        /// <summary>
        /// The property that holds the result string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]       
        public new string Result { get; set; }

        public DsfWebGetRequestActivity()
            : base("Web Request")
        {
            Method = "GET";
        }

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        ///     When overridden runs the activity's execution logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugOutputs.Clear();
            _debugInputs.Clear();
            if(WebRequestInvoker == null)
            {
                return;
            }
            var dataObject = context.GetExtension<IDSFDataObject>();
            var compiler = DataListFactory.CreateDataListCompiler();
            var dlID = dataObject.DataListID;
            var allErrors = new ErrorResultTO();
            var executionId = DataListExecutionID.Get(context);
            try
            {
                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionId, enActionType.User, Url, false, out errors);
                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    AddUrlDebugInputItem(Url, expressionsEntry, executionId);
                }
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                int indexToUpsertTo = 1;
                while(itr.HasMoreRecords())
                {
                    IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                    foreach(IBinaryDataListItem c in cols)
                    {
                        string result = WebRequestInvoker.ExecuteRequest(Method, c.TheValue);
                        allErrors.MergeErrors(errors);
                        string expression;
                        if(DataListUtil.IsValueRecordset(Result) &&
                           DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                        {
                            expression = Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            expression = Result;
                        }
                        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                        foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
                        {
                            toUpsert.Add(region, result);
                            toUpsert.FlushIterationFrame();
                            if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                            {
                                AddDebugOutputItem(region, result, executionId, indexToUpsertTo);
                            }
                            indexToUpsertTo++;
                        }
                    }
                    compiler.Upsert(executionId, toUpsert, out errors);
                    allErrors.MergeErrors(errors);
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors

                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfWebGetRequestActivity", allErrors);
                    compiler.UpsertSystemTag(dlID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        private void AddUrlDebugInputItem(string expression, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "URL To Execute" });

            itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));

            _debugInputs.Add(itemToAdd);
        }

        void AddDebugOutputItem(string region, string result, Guid executionId, int indexToUpsertTo)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = (indexToUpsertTo).ToString(CultureInfo.InvariantCulture) });
            itemToAdd.AddRange(CreateDebugItemsFromString(region, result, executionId, indexToUpsertTo, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }
        #region Get Debug Inputs/Outputs

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

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {

                    if (t.Item1 == Url)
                    {
                        Url = t.Item2;
                    }               
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Url);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
        #endregion
    }
}