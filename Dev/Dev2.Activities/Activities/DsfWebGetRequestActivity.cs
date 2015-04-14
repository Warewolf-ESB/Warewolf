
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

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
            Headers = string.Empty;
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
            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);
            try
            {
                allErrors.MergeErrors(errorsTo);
                if(dataObject.IsDebugMode())
                {
                    DebugItem debugItem = new DebugItem();
                    AddDebugItem(new DebugEvalResult(Url, "URL", dataObject.Environment), debugItem);
                    _debugInputs.Add(debugItem);
                }
                var colItr = new WarewolfListIterator();
                var urlitr = new WarewolfIterator(dataObject.Environment.Eval(Url));
                var headerItr = new WarewolfIterator(dataObject.Environment.Eval(Headers));
                colItr.AddVariableToIterateOn(urlitr);
                colItr.AddVariableToIterateOn(headerItr);
                const int IndexToUpsertTo = 1;
                while(colItr.HasMoreData())
                {
                    var c = colItr.FetchNextValue(urlitr);
                    var headerValue = colItr.FetchNextValue(headerItr);
                    var headers = string.IsNullOrEmpty(headerValue)
                                      ? new string[0]
                                      : headerValue.Split(new[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries);

                    var headersEntries = new List<Tuple<string, string>>();

                    foreach(var header in headers)
                    {
                        var headerSegments = header.Split(':');
                        headersEntries.Add(new Tuple<string, string>(headerSegments[0], headerSegments[1]));

                        if(dataObject.IsDebugMode())
                        {
                            DebugItem debugItem = new DebugItem();
                            AddDebugItem(new DebugEvalResult(Headers, "Header", dataObject.Environment), debugItem);
                            _debugInputs.Add(debugItem);
                        }
                    }

                    var result = WebRequestInvoker.ExecuteRequest(Method, c, headersEntries);
                    allErrors.MergeErrors(errorsTo);
                    var expression = GetExpression(IndexToUpsertTo);
                    PushResultsToDataList(expression, result, dataObject);
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFWebGetRequest", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfWebGetRequestActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                    var expression = GetExpression(1);
                    PushResultsToDataList(expression, null, dataObject);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        string GetExpression(int indexToUpsertTo)
        {
            string expression;
            if(DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
            {
                expression = Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                expression = Result;
            }
            return expression;
        }

        void PushResultsToDataList(string expression,  string result, IDSFDataObject dataObject)
        {
            UpdateResultRegions(expression, dataObject.Environment, result);
            if(dataObject.IsDebugMode())
            {
                
                    AddDebugOutputItem(new DebugEvalResult(expression,"",dataObject.Environment));
            }
        }

        void UpdateResultRegions(string expression, IExecutionEnvironment environment, string result)
        {
            foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
            {
                environment.Assign(region, result);
            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
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
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == Url)
                    {
                        Url = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
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
