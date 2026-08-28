/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.X6;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.WorkflowConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("WebMethods", "PUT", ToolType.Native, "6C5F6D7E-4B42-4874-8197-DBE86D4A9F2D", "Dev2.Activities", "1.0.0.0", "Legacy", "HTTP Web Methods", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_WebMethod_Put")]
    public class WebPutActivity : DsfWebActivityBase, IEquatable<WebPutActivity>
    {
        public WebPutActivity()
            : base(WebRequestDataDto.CreateRequestDataDto(WebRequestMethod.Put, "PUT Web Method", "PUT Web Method"))
        {
        }

        public string PutData { get; set; }

        public bool IsPutDataBase64 { get; set; }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return _debugInputs;
            }

            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Put Data"), debugItem);
            AddDebugItem(new DebugEvalResult(PutData, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            if (IsPutDataBase64)
            {
                debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", nameof(IsPutDataBase64)), debugItem);
                AddDebugItem(new DebugEvalResult(IsPutDataBase64.ToString(), "", env, update), debugItem);
                _debugInputs.Add(debugItem);
            }
            base.GetDebugInputs(env, update);
            return _debugInputs;
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            var webRequestResult = string.Empty;
            try
            {
                var (head, query, putData) = ConfigureHttp(dataObject, update);

                var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
                if (url == null
                    && AmbientSourceLoader.Current?.EnsureSourceLoaded(SourceId) == true
                    && ResourceCatalog.WorkspaceResources
                           .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
                {
                    lock (ws)
                        url = ws.OfType<WebSource>().FirstOrDefault(r => r.ResourceID == SourceId);
                }
                webRequestResult = PerformWebRequest(head, query, url, putData, IsPutDataBase64);
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

                ResponseManager.PushResponseIntoEnvironment(response, update, dataObject);
            }
        }

        private (IEnumerable<NameValue> head, string query, string data) ConfigureHttp(IDSFDataObject dataObject, int update)
        {
            IEnumerable<NameValue> head = null;
            if (Headers != null)
            {
                head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Value, update))));
            }
            var query = "";
            if (QueryString != null)
            {
                query = ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(QueryString, update));
            }
            var putData = "";
            if (PutData != null)
            {
                putData = ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(PutData, update));
            }

            return (head, query, putData);
        }

        public override HttpClient CreateClient(IEnumerable<INameValue> head, string query, WebSource source)
        {
            var httpClient = new HttpClient();
            if (source.AuthenticationType == AuthenticationType.User)
            {
                var byteArray = Encoding.ASCII.GetBytes($"{source.UserName}:{source.Password}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", byteArray.ToBase64String());
            }

            if (head != null)
            {
                var nameValues = head.Where(nameValue => !String.IsNullOrEmpty(nameValue.Name) && !String.IsNullOrEmpty(nameValue.Value));
                foreach (var nameValue in nameValues)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(nameValue.Name, nameValue.Value);
                }
            }

            var address = source.Address;
            if (!string.IsNullOrEmpty(query))
            {
                address += query;
            }
            try
            {
                var baseAddress = new Uri(address);
                httpClient.BaseAddress = baseAddress;
            }
            catch (UriFormatException e)
            {
                Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                return httpClient;
            }

            return httpClient;
        }

        public bool Equals(WebPutActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                && string.Equals(PutData, other.PutData)
                && IsPutDataBase64 == other.IsPutDataBase64;
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

            return Equals((WebPutActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (PutData != null ? PutData.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (IsPutDataBase64.GetHashCode());
                return hashCode;
            }
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();
            base.ToX6Json(cell);

            cell.shape = Constants.WEBPUTACTIVITY;
            cell.data[Constants.TYPE] = Constants.WEBPUTACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_WEBPUT;
            cell.data[Constants.UNIQUEID] = UniqueID;
            cell.data[Constants.WEBMETHOD_HEADERS] = Headers;
            cell.data[Constants.WEBMETHOD_QUERYSTRING] = QueryString;
            cell.data[Constants.WEBMETHOD_ISPUTDATABASE64] = IsPutDataBase64;
            cell.data[Constants.WEBMETHOD_POSTDATA] = PutData;
            cell.data[Constants.WEBMETHOD_SOURCEID] = SourceId;
            cell.data[Constants.WEBMETHOD_OUTPUTDESCRIPTION] = OutputDescription;
            cell.data[Constants.WEBMETHOD_INPUTS] = Inputs;
            cell.data[Constants.WEBMETHOD_OUTPUTS] = Outputs;
            cell.data[Constants.WEBMETHOD_ISOBJECT] = IsObject;
            cell.data[Constants.WEBMETHOD_OBJECTNAME] = ObjectName;
            cell.data[Constants.WEBMETHOD_OBJECTRESULT] = ObjectResult;
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;
            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) UniqueID = uniqueId;
            if (cell.data.TryGetString(Constants.WEBMETHOD_OBJECTRESULT, out var objectResult)) ObjectResult = objectResult;
            if (cell.data.TryGetString(Constants.WEBMETHOD_QUERYSTRING, out var queryString)) QueryString = queryString;
            if (cell.data.TryGetString(Constants.WEBMETHOD_POSTDATA, out var putData)) PutData = putData;
            if (cell.data.TryGetGuid(Constants.WEBMETHOD_SOURCEID, out var sourceId)) SourceId = sourceId;
            if (cell.data.TryGetBool(Constants.WEBMETHOD_ISPUTDATABASE64, out var isPutDataBase64)) IsPutDataBase64 = isPutDataBase64;
            if (cell.data.TryGetBool(Constants.WEBMETHOD_ISOBJECT, out var isObject)) IsObject = isObject;
            if (cell.data.TryGetString(Constants.WEBMETHOD_OBJECTNAME, out var objectName)) ObjectName = objectName;

            if (cell.data.TryGetHeaders(out var headers)) Headers = headers;
            if (cell.data.TryGetInputs(out var inputs)) Inputs = inputs;
            if (cell.data.TryGetOutputs(out var outputs)) Outputs = outputs;
            if (cell.data.TryGetOutputDescription(out var outputDesc)) OutputDescription = outputDesc;

            // Omitting the optional headers key left Headers null; ConfigureHttp then silently
            // sent no headers instead of hard-failing, but null vs. empty should not be a
            // distinction callers have to know about (F10).
            Headers ??= new List<INameValue>();
        }
    }
}