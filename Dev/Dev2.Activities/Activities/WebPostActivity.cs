/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Data.Options;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{

    [ToolDescriptorInfo("WebMethods", "POST", ToolType.Native, "6AEB1038-6332-46F9-8BDD-752DE4EA038E", "Dev2.Activities", "1.0.0.0", "Legacy", "HTTP Web Methods", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_WebMethod_Post")]
    public class WebPostActivity : DsfActivity, IEquatable<WebPostActivity>
    {
        private IDSFDataObject _dataObject;

        public IList<INameValue> Headers { get; set; }
        public bool IsFormDataChecked { get; set; }
        public bool IsManualChecked { get; set; }
        public IList<FormDataConditionExpression> Conditions { get; set; }
        public string QueryString { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public IResponseManager ResponseManager { get; set; }
        public string PostData { get; set; }

        public WebPostActivity()
        {
            Type = "POST Web Method";
            DisplayName = "POST Web Method";
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return _debugInputs;
            }

            base.GetDebugInputs(env, update);

            var (head, parameters, _, conditions) = GetEnvironmentInputVariables(env, update);

            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            var headerString = string.Empty;
            if (head != null)
            {
                headerString = string.Join(" ", head.Select(a => a.Name + " : " + a.Value));
            }

            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "URL"), debugItem);
            AddDebugItem(new DebugEvalResult(url.Address, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Query String"), debugItem);
            AddDebugItem(new DebugEvalResult(QueryString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", nameof(Headers)), debugItem);
            AddDebugItem(new DebugEvalResult(headerString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            if (IsManualChecked)
            {
                debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", "Post Data"), debugItem);
                AddDebugItem(new DebugEvalResult(PostData, "", env, update), debugItem);
                _debugInputs.Add(debugItem);
            }

            if (IsFormDataChecked)
            {
                AddDebugFormDataInputs(conditions);
            }

            return _debugInputs;
        }


        private void AddDebugFormDataInputs(IEnumerable<IFormDataParameters> conditions)
        {
            var allErrors = new ErrorResultTO();

            try
            {
                var dds = conditions.GetEnumerator();
                var text = new StringBuilder();
                while (dds.MoveNext())
                {
                    var conditionExpression = dds.Current;

                    text.Append("\n");
                    if(conditionExpression != null)
                        conditionExpression.RenderDescription(text);
                }

                var debugItem = new DebugItem();
                var sb = text.ToString();
                AddDebugItem(new DebugItemStaticDataParams(sb, "Parameters"), debugItem);
                _debugInputs.Add(debugItem);
            }
            catch (JsonSerializationException e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                if (allErrors.HasErrors())
                {
                    var serviceName = GetType().Name;
                    DisplayAndWriteError(_dataObject,serviceName, allErrors);
                }
            }
        }


        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            _dataObject = dataObject;
            tmpErrors = new ErrorResultTO();
            var webRequestResult = string.Empty;
            try
            {
                var (head, query, postData, conditions) = GetEnvironmentInputVariables(_dataObject.Environment, update);

                var source = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);

                if (IsFormDataChecked)
                {
                    webRequestResult = PerformFormDataWebPostRequest(source, WebRequestMethod.Post, query, head, conditions);
                }
                else if (IsManualChecked)
                {
                    webRequestResult = PerformManualWebPostRequest(head, query, source, postData);
                }
            }
            catch (Exception ex)
            {
                tmpErrors.AddError(ex.Message);
            }
            finally
            {
                tmpErrors.MergeErrors(_errorsTo);

                var bytes = webRequestResult.Base64StringToByteArray();
                var response = bytes.ReadToString();

                ResponseManager = new ResponseManager
                {
                    OutputDescription = OutputDescription,
                    Outputs = Outputs,
                    IsObject = IsObject,
                    ObjectName = ObjectName
                };
                response = Scrubber.Scrub(response);
                ResponseManager.PushResponseIntoEnvironment(response, update, dataObject);
            }
        }

        private (IEnumerable<INameValue> head, string query, string data, IEnumerable<IFormDataParameters> conditions) GetEnvironmentInputVariables(IExecutionEnvironment environment, int update)
        {
            IEnumerable<INameValue> head = null;
            if (Headers != null)
            {
                head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval(a.Value, update))));
                if (IsFormDataChecked)
                {
                    var headersHelper = new WebRequestHeadersHelper(notEvaluatedHeaders: Headers, evaluatedHeaders: head);
                    head = headersHelper.CalculateFormDataContentType();
                }
            }
            var query = string.Empty;
            if (QueryString != null)
            {
                query = ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval(QueryString, update));
            }
            var postData = string.Empty;
            if (PostData != null && IsManualChecked)
            {
                postData = ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval(PostData, update, true));
            }
            var conditions = new List<IFormDataParameters>();
            if ((Conditions ?? (Conditions = new List<FormDataConditionExpression>())).Any() && IsFormDataChecked)
            {
                _errorsTo = new ErrorResultTO();
                conditions = Conditions.SelectMany(o => o.Eval(GetArgumentsFunc, _errorsTo.HasErrors())).ToList();
            }

            IEnumerable<string[]> GetArgumentsFunc(string col1s, string col2s, string col3s)
            {
                var col1 = environment.EvalAsList(col1s, 0, true);
                var col2 = environment.EvalAsList(col2s ?? "", 0, true);
                var col3 = environment.EvalAsList(col3s ?? "", 0, true);

                var iter = new WarewolfListIterator();
                var c1 = new WarewolfAtomIterator(col1);
                var c2 = new WarewolfAtomIterator(col2);
                var c3 = new WarewolfAtomIterator(col3);
                iter.AddVariableToIterateOn(c1);
                iter.AddVariableToIterateOn(c2);
                iter.AddVariableToIterateOn(c3);

                while (iter.HasMoreData())
                {
                    var item = new[] { iter.FetchNextValue(c1), iter.FetchNextValue(c2), iter.FetchNextValue(c3) };
                    yield return item;
                }
                yield break;
            }

            return (head, query, postData, conditions);
        }

        protected virtual string PerformManualWebPostRequest(IEnumerable<INameValue> head, string query, IWebSource source, string postData)
        {
            return WebSources.Execute(source, WebRequestMethod.Post, query, postData, throwError: true, out _errorsTo, head.Select(h => h.Name + ":" + h.Value).ToArray());
        }

        protected virtual string PerformFormDataWebPostRequest(IWebSource source, WebRequestMethod method, string query, IEnumerable<INameValue> head, IEnumerable<IFormDataParameters> parameters)
        {
            return WebSources.Execute(source, method, head.Select(h => h.Name + ":" + h.Value).ToArray(), query, isNoneChecked: false, isFormDataChecked: true, data: string.Empty, throwError: true, out _errorsTo, parameters);
        }

        public static WebClient CreateClient(IEnumerable<INameValue> head, string query, WebSource source)
        {
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => true;
            var webclient = new WebClient();
            if (head != null)
            {
                foreach (var nameValue in head)
                {
                    if (!string.IsNullOrEmpty(nameValue.Name) && !String.IsNullOrEmpty(nameValue.Value))
                    {
                        webclient.Headers.Add(nameValue.Name, nameValue.Value);
                    }
                }
            }

            if (source.AuthenticationType == AuthenticationType.User)
            {
                webclient.Credentials = new NetworkCredential(source.UserName, source.Password);
            }

            webclient.Headers.Add("user-agent", GlobalConstants.UserAgentString);
            var address = source.Address;
            if (query != null)
            {
                address += query;
            }
            webclient.BaseAddress = address;
            return webclient;
        }

        public bool Equals(WebPostActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && Equals(Headers, other.Headers) &&
                   string.Equals(QueryString, other.QueryString) &&
                   Equals(OutputDescription, other.OutputDescription) &&
                   string.Equals(PostData, other.PostData) &&
                   Equals(IsManualChecked, other.IsManualChecked) &&
                   Equals(IsFormDataChecked, other.IsFormDataChecked);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
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

            return Equals((WebPostActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Headers != null ? Headers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (QueryString != null ? QueryString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputDescription != null ? OutputDescription.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PostData != null ? PostData.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (IsManualChecked.GetHashCode());
                hashCode = (hashCode * 397) ^ (IsFormDataChecked.GetHashCode());
                return hashCode;
            }
        }
    }

}