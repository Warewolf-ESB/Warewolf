using System.Collections.Generic;
using System.Net;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.ESB
{
    public class RemoteWorkflowExecutionContainerMock : RemoteWorkflowExecutionContainer
    {
        public RemoteWorkflowExecutionContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, IResourceCatalog resourceCatalog)
            : base(sa, dataObj, theWorkspace, esbChannel, resourceCatalog)
        {
        }

        public string GetRequestRespsonse { get; set; }
        public string GetRequestUri { get; set; }
        public string FetchRemoteDebugItemsUri { get; set; }
        public string LogExecutionUrl { get; private set; }

        protected override string ExecuteGetRequest(string uri, string serviceName, string payload)
        {
            GetRequestUri = uri;
            return GetRequestRespsonse;
        }

        #region Overrides of RemoteWorkflowExecutionContainer

        protected override void ExecuteWebRequestAsync(WebRequest buildGetWebRequest)
        {
            LogExecutionUrl = buildGetWebRequest.RequestUri.ToString();
        }

        #endregion

        protected override IList<DebugState> FetchRemoteDebugItems(string uri)
        {
            FetchRemoteDebugItemsUri = uri;
            return new List<DebugState>();
        }

    }
}
