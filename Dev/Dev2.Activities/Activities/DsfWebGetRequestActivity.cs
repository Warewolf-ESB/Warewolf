#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.State;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
   //[ToolDescriptorInfo("WebMethods", "Web Request", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfWebGetRequestActivity : DsfActivityAbstract<string>,IEquatable<DsfWebGetRequestActivity>
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

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "Url",
                    Value = Url,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Headers",
                    Value = Headers,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="Result",
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
            };
        }


        public override List<string> GetOutputs() => new List<string> { Result };


        #region Overrides of DsfNativeActivity<string>
        
        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugOutputs.Clear();
            _debugInputs.Clear();
            if(WebRequestInvoker == null)
            {
                return;
            }

            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);
            try
            {
                allErrors = TryExecute(dataObject, update, allErrors);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFWebGetRequest", e, GlobalConstants.WarewolfError);
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
                    PushResultsToDataList(expression, null, dataObject, update);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        ErrorResultTO TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            allErrors.MergeErrors(_errorsTo);
            if (dataObject.IsDebugMode())
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(Url, "URL", dataObject.Environment, update), debugItem);
                _debugInputs.Add(debugItem);
            }
            var colItr = new WarewolfListIterator();
            var urlitr = new WarewolfIterator(dataObject.Environment.Eval(Url, update));
            var headerItr = new WarewolfIterator(dataObject.Environment.Eval(Headers, update));
            colItr.AddVariableToIterateOn(urlitr);
            colItr.AddVariableToIterateOn(headerItr);
            const int IndexToUpsertTo = 1;
            while (colItr.HasMoreData())
            {
                var c = colItr.FetchNextValue(urlitr);
                var headerValue = colItr.FetchNextValue(headerItr);
                var headers = string.IsNullOrEmpty(headerValue)
                    ? new string[0]
                    : headerValue.Split(new[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries);

                var headersEntries = new List<Tuple<string, string>>();

                foreach (var header in headers)
                {
                    var headerSegments = header.Split(':');
                    headersEntries.Add(new Tuple<string, string>(headerSegments[0], headerSegments[1]));

                    if (dataObject.IsDebugMode())
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugEvalResult(Headers, "Header", dataObject.Environment, update), debugItem);
                        _debugInputs.Add(debugItem);
                    }
                }

                var result = WebRequestInvoker.ExecuteRequest(Method, c, headersEntries);
                allErrors.MergeErrors(_errorsTo);
                var expression = GetExpression(IndexToUpsertTo);
                PushResultsToDataList(expression, result, dataObject, update);
            }
            return allErrors;
        }

        string GetExpression(int indexToUpsertTo)
        {
            string expression;
            expression = DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star ? Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture)) : Result;
            return expression;
        }

        void PushResultsToDataList(string expression,  string result, IDSFDataObject dataObject,int update)
        {
            UpdateResultRegions(expression, dataObject.Environment, result, update);
            if(dataObject.IsDebugMode())
            {
                
                    AddDebugOutputItem(new DebugEvalResult(expression,"",dataObject.Environment, update));
            }
        }

        void UpdateResultRegions(string expression, IExecutionEnvironment environment, string result, int update)
        {
            foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
            {
                environment.Assign(region, result, update);
            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #endregion

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if(itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(Url);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        #endregion
        #endregion

        public bool Equals(DsfWebGetRequestActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) 
                && string.Equals(Method, other.Method) 
                && string.Equals(Url, other.Url) 
                && string.Equals(Headers, other.Headers) 
                && string.Equals(Result, other.Result);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfWebGetRequestActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Method != null ? Method.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Url != null ? Url.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Headers != null ? Headers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
