/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using Dev2.Comparer;
using Dev2.Data.Util;
using Dev2.Runtime.ServiceModel;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using Newtonsoft.Json.Linq;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("WebMethods", "GET", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038E", "Dev2.Activities", "1.0.0.0", "Legacy", "HTTP Web Methods", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_WebMethod_Get")]
    public class WebGetActivity : DsfActivity, IEquatable<WebGetActivity>
    {
        public IList<INameValue> Headers { get; set; }

        public string QueryString { get; set; }

        public IOutputDescription OutputDescription { get; set; }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            base.GetDebugInputs(env, update);
            var head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Value, update)))).Where(a => !(String.IsNullOrEmpty(a.Name) && String.IsNullOrEmpty(a.Value)));
            var query = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(QueryString, update));
            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            var headerString = string.Join(" ", head.Select(a => a.Name + " : " + a.Value));

            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "URL"), debugItem);
            AddDebugItem(new DebugEvalResult(url.Address, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Query String"), debugItem);
            AddDebugItem(new DebugEvalResult(query, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", nameof(Headers)), debugItem);
            AddDebugItem(new DebugEvalResult(headerString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            return _debugInputs;
        }

        public IResponseManager ResponseManager { get; set; }

        public bool IsResponseBase64 { get; set; }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            var webRequestResult = string.Empty;
            try
            {
                if (Headers == null)
                {
                    tmpErrors.AddError(ErrorResource.HeadersAreNull);
                    return;
                }

                if (QueryString == null)
                {
                    tmpErrors.AddError(ErrorResource.QueryIsNull);
                    return;
                }

                var (head, query, _) = ConfigureHttp(dataObject, update);

                var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
                if (url == null
                    && AmbientSourceLoader.Current?.EnsureSourceLoaded(SourceId) == true
                    && ResourceCatalog.WorkspaceResources
                           .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
                {
                    lock (ws)
                        url = ws.OfType<WebSource>().FirstOrDefault(r => r.ResourceID == SourceId);
                }

                if (dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugEvalResult(query, "URL", dataObject.Environment, update));
                    AddDebugInputItem(new DebugEvalResult(url.Address, "Query String", dataObject.Environment, update));
                }

                webRequestResult = PerformWebRequest(head, query, url);
            }
            catch (Exception ex)
            {
                tmpErrors.AddError(ex.Message);
            }
            finally
            {
                tmpErrors.MergeErrors(_errorsTo);

                ResponseManager = new ResponseManager
                {
                    OutputDescription = OutputDescription,
                    Outputs = Outputs,
                    IsObject = IsObject,
                    ObjectName = ObjectName
                };
            }

            if (IsResponseBase64)
            {
                ResponseManager.PushResponseIntoEnvironment(webRequestResult, update, dataObject);
                return;
            }

            webRequestResult = Scrubber.Scrub(webRequestResult);
            ResponseManager.PushResponseIntoEnvironment(webRequestResult, update, dataObject);
        }

        private (IEnumerable<NameValue> head, string query, string data) ConfigureHttp(IDSFDataObject dataObject, int update)
        {
            var head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Value, update))));
            var query = ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(QueryString, update));

            return (head, query, null);
        }

        protected virtual string PerformWebRequest(IEnumerable<INameValue> head, string query, WebSource url)
        {
            return WebSources.Execute(url, WebRequestMethod.Get, query, String.Empty, true, out _errorsTo, head.Select(h => h.Name + ":" + h.Value).ToArray());
        }

        public WebGetActivity()
        {
            Type = "GET Web Method";
            DisplayName = "GET Web Method";
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public bool Equals(WebGetActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var headersAreEqual = CommonEqualityOps.CollectionEquals(Headers, other.Headers, new NameValueComparer());
            return base.Equals(other)
                   && headersAreEqual
                   && string.Equals(QueryString, other.QueryString)
                   && Equals(OutputDescription, other.OutputDescription);
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

            return Equals((WebGetActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Headers != null ? Headers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (QueryString != null ? QueryString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputDescription != null ? OutputDescription.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();
            base.ToX6Json(cell);

            cell.shape = Constants.WEBGETACTIVITY;
            cell.data[Constants.TYPE] = Constants.WEBGETACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_WEBGET;
            cell.data[Constants.UNIQUEID] = UniqueID;
            cell.data[Constants.WEBMETHOD_HEADERS] = Headers;
            cell.data[Constants.WEBMETHOD_QUERYSTRING] = QueryString;
            cell.data[Constants.WEBMETHOD_SOURCEID] = SourceId;
            cell.data[Constants.WEBMETHOD_OUTPUTDESCRIPTION] = OutputDescription;
            cell.data[Constants.WEBMETHOD_INPUTS] = Inputs;
            cell.data[Constants.WEBMETHOD_OUTPUTS] = Outputs;
            cell.data[Constants.WEBMETHOD_ISOBJECT] = IsObject;
            cell.data[Constants.WEBMETHOD_OBJECTNAME] = ObjectName;
            cell.data[Constants.WEBMETHOD_OBJECTRESULT] = ObjectResult;
            cell.data[Constants.WEBMETHOD_ISRESPONSEBASE64] = IsResponseBase64;
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;
            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) UniqueID = uniqueId;
            if (cell.data.TryGetString(Constants.WEBMETHOD_OBJECTRESULT, out var objectResult)) ObjectResult = objectResult;
            if (cell.data.TryGetString(Constants.WEBMETHOD_QUERYSTRING, out var queryString)) QueryString = queryString;
            if (cell.data.TryGetGuid(Constants.WEBMETHOD_SOURCEID, out var sourceId)) SourceId = sourceId;
            if (cell.data.TryGetBool(Constants.WEBMETHOD_ISOBJECT, out var isObject)) IsObject = isObject;
            if (cell.data.TryGetString(Constants.WEBMETHOD_OBJECTNAME, out var objectName)) ObjectName = objectName;

            if (cell.data.TryGetHeaders(out var headers)) Headers = headers;
            if (cell.data.TryGetInputs(out var inputs)) Inputs = inputs;
            if (cell.data.TryGetOutputs(out var outputs)) Outputs = outputs;
            if (cell.data.TryGetOutputDescription(out var outputDesc)) OutputDescription = outputDesc;
            if (cell.data.TryGetBool(Constants.WEBMETHOD_ISRESPONSEBASE64, out var isResponseBase64)) IsResponseBase64 = isResponseBase64;
        }

    }
}