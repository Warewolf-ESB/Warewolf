using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("WebMethods", "POST", ToolType.Native, "6AEB1038-6332-46F9-8BDD-752DE4EA038E", "Dev2.Activities", "1.0.0.0", "Legacy", "HTTP Web Methods", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_WebMethod_Post")]
    public class DsfWebPostActivity:DsfActivity
    {
        public IList<INameValue> Headers { get; set; }
        public string QueryString { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public string PostData { get; set; }

        public DsfWebPostActivity()
        {
            Type = "POST Web Method";
            DisplayName = "POST Web Method";
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null) return _debugInputs;
            base.GetDebugInputs(env, update);

            IEnumerable<NameValue> head = null;
            if (Headers != null)
            {
                head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Value, update)))).Where(a => !(String.IsNullOrEmpty(a.Name) && String.IsNullOrEmpty(a.Value)));
            }

            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            string headerString=string.Empty;
            if (head != null)
                headerString = string.Join(" ", head.Select(a => a.Name + " : " + a.Value));

            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "URL"), debugItem);
            AddDebugItem(new DebugEvalResult(url.Address, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Query String"), debugItem);
            AddDebugItem(new DebugEvalResult(QueryString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Post Data"), debugItem);
            AddDebugItem(new DebugEvalResult(PostData, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Headers"), debugItem);
            AddDebugItem(new DebugEvalResult(headerString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            return _debugInputs;
        }

        #region Overrides of DsfActivity
       

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
            var postData = "";
            if (PostData != null)
            {
                postData = ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(PostData, update));    
            }
            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            var webRequestResult = PerformWebPostRequest(head, query, url, postData);


            ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName};
            ResponseManager.PushResponseIntoEnvironment(webRequestResult, update, dataObject);

        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IResponseManager ResponseManager { get; set; }

        

        protected virtual string PerformWebPostRequest(IEnumerable<NameValue> head, string query, WebSource source, string postData)
        {
            var webclient = CreateClient(head, query, source);
            if (webclient != null)
            {
                var address = source.Address;
                if (query != null)
                {
                    address = address + query;
                }
                return webclient.UploadString(address, postData);
            }
            return null;
        }

        public WebClient CreateClient(IEnumerable<NameValue> head, string query, WebSource source)
        {
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => true;
            var webclient = new WebClient();
            if (head != null)
            {
                foreach (var nameValue in head)
                {
                    if (!String.IsNullOrEmpty(nameValue.Name) && !String.IsNullOrEmpty(nameValue.Value))
                        webclient.Headers.Add(nameValue.Name, nameValue.Value);
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
                address = address + query;
            }
            webclient.BaseAddress = address;
            return webclient;
        }

        #endregion
    }
}
