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
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities
{
    [ToolDescriptorInfo("WebMethods", "GET", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "HTTP Web Methods", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_WebMethod_Get")]
    public class DsfWebGetActivity : DsfActivity
    {

        public IList<INameValue> Headers { get; set; }

        public string QueryString { get; set; }

        public IOutputDescription OutputDescription { get; set; }


        #region Overrides of DsfNativeActivity<bool>

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            base.GetDebugInputs(env, update);
            var head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Value, update)))).Where(a=>!(String.IsNullOrEmpty(a.Name)&&String.IsNullOrEmpty(a.Value)));
            var query = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(QueryString, update));
            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            string headerString = string.Join(" ", head.Select(a => a.Name+" : "+a.Value));

            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("","URL"), debugItem);
            AddDebugItem(new DebugEvalResult(url.Address, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Query String"), debugItem);
            AddDebugItem(new DebugEvalResult(query, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Headers"), debugItem);
            AddDebugItem(new DebugEvalResult(headerString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            
            return _debugInputs;
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            if (Headers == null)
            {
                errors.AddError(ErrorResource.HeadersAreNull);
                return;
            }
            if (QueryString == null)
            {
                errors.AddError(ErrorResource.QueryIsNull);
                return;
            }
            var head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Value, update))));
            var query =  ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(QueryString,update));

                     
            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);

            if (dataObject.IsDebugMode())
            {
                AddDebugInputItem(new DebugEvalResult(query, "URL", dataObject.Environment, update));
                AddDebugInputItem(new DebugEvalResult(url.Address, "Query String", dataObject.Environment, update));
            }
            var webRequestResult = PerformWebRequest(head, query, url);
            ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName};
            ResponseManager.PushResponseIntoEnvironment(webRequestResult, update, dataObject);
        }

        public IResponseManager ResponseManager { get; set; }

        protected virtual string PerformWebRequest(IEnumerable<NameValue> head, string query, WebSource url)
        {
            var client = CreateClient(head, query, url);
            var result = client.DownloadString(url.Address + query);
            return result;
        }

        private WebClient CreateClient(IEnumerable<NameValue> head, string query, WebSource source)
        {
            var webclient = new WebClient();
            foreach(var nameValue in head)
            {
                if(!String.IsNullOrEmpty( nameValue.Name) && !String.IsNullOrEmpty( nameValue.Value))
                webclient.Headers.Add(nameValue.Name,nameValue.Value);
            }

            if (source.AuthenticationType == AuthenticationType.User)
            {
                webclient.Credentials = new NetworkCredential(source.UserName, source.Password);
            }
          
            webclient.Headers.Add("user-agent", GlobalConstants.UserAgentString);
            webclient.BaseAddress = source.Address + query;
            return webclient;
        }

        #endregion

        // ReSharper disable once MemberCanBeProtected.Global
        public DsfWebGetActivity()
        {
            Type = "GET Web Method";
            DisplayName = "GET Web Method";
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

    }
}
