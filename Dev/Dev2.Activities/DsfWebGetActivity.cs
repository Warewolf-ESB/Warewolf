using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.DataList.Contract;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2
{
    [ToolDescriptorInfo("Resources-Service", "Web Service Get", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfWebGetActivity : DsfActivity
    {

        public IList<INameValue> Headers { get; set; }

        public string QueryString { get; set; }

        public IWebServiceSource SavedSource { get; set; }

        #region Overrides of DsfNativeActivity<bool>


        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            if (Headers == null)
            {
                errors.AddError("Headers Are Null");
                return;
            }
            if (QueryString == null)
            {
                errors.AddError("Query is Null");
                return;
            }
            var head = Headers.Select(a => new NameValue(dataObject.Environment.Eval(a.Name, update).ToString(), dataObject.Environment.Eval(a.Value, update).ToString()));
            var query = dataObject.Environment.Eval(QueryString,update);
            var url = ResourceCatalog.Instance.GetResource<WebSource>(Guid.Empty, SourceId);
            var client = CreateClient(head, query, url);
            var result = client.DownloadString(url.Address+query);
            //ExecuteService(update, out errors, Method, Namespace, dataObject, OutputFormatterFactory.CreateOutputFormatter(OutputDescription));
        }

        private WebClient CreateClient(IEnumerable<NameValue> head, WarewolfDataEvaluationCommon.WarewolfEvalResult query, WebSource source)
        {
            var webclient = new WebClient();
            foreach(var nameValue in head)
            {
                webclient.Headers.Add(nameValue.Name,nameValue.Value);
            }

            if (source.AuthenticationType == AuthenticationType.User)
            {
                webclient.Credentials = new NetworkCredential(source.UserName, source.Password);
            }

            var contentType = webclient.Headers["Content-Type"];
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/x-www-form-urlencoded";
            }
            webclient.Headers["Content-Type"] = contentType;
            webclient.Headers.Add("user-agent", GlobalConstants.UserAgentString);
            webclient.BaseAddress = source.Address + query;
            return webclient;
        }

        #endregion

        public DsfWebGetActivity()
        {
            Type = "Web Get Request Connector";
            DisplayName = "Web Get Request Connector";
        }

    }
}
