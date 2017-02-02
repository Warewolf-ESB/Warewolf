using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("WebMethods", "PUT", ToolType.Native, "6C5F6D7E-4B42-4874-8197-DBE86D4A9F2D", "Dev2.Acitivities", "1.0.0.0", "Legacy", "HTTP Web Methods", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_WebMethod_Put")]
    public class DsfWebPutActivity : DsfWebActivityBase
    {
        public DsfWebPutActivity()
            : base(WebRequestDataDto.CreateRequestDataDto(WebRequestMethod.Put, "PUT Web Method", "PUT Web Method"))
        {
        }
        public string PutData { get; set; }
        
        
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null) return _debugInputs;

            DebugItem debugItem = new DebugItem();

            AddDebugItem(new DebugItemStaticDataParams("", "Put Data"), debugItem);
            AddDebugItem(new DebugEvalResult(PutData, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            base.GetDebugInputs(env, update);
            return _debugInputs;
        }
        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
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

            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            var webRequestResult = PerformWebPostRequest(head, query, url, putData);

            ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName };
            ResponseManager.PushResponseIntoEnvironment(webRequestResult, update, dataObject);
        }
        public override HttpClient CreateClient(IEnumerable<NameValue> head, string query, WebSource source)
        {
            var httpClient = new HttpClient();
            if (source.AuthenticationType == AuthenticationType.User)
            {
                var byteArray = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", source.UserName, source.Password));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            if (head != null)
            {
                IEnumerable<NameValue> nameValues = head.Where(nameValue => !String.IsNullOrEmpty(nameValue.Name) && !String.IsNullOrEmpty(nameValue.Value));
                foreach (var nameValue in nameValues)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(nameValue.Name, nameValue.Value);
                }
            }

            var address = source.Address;
            if (!string.IsNullOrEmpty(query))
            {
                address = address + query;
            }
            try
            {
                var baseAddress = new Uri(address);
                httpClient.BaseAddress = baseAddress;
            }
            catch (UriFormatException e)
            {
                //CurrentDataObject.Environment.AddError(e.Message);// To investigate this
                Dev2Logger.Error(e.Message, e); // Error must be added on the environment
                return httpClient;
            }

            return httpClient;
        }


    }
}