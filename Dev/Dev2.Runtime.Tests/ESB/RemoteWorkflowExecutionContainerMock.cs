using System.Collections.Generic;
using System.Net;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.DynamicServices.Objects;
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
        public WebRequest LogExecutionWebRequest { get; private set; }

        protected override string ExecuteGetRequest(Connection connection, string serviceName, string payload)
        {
            GetRequestUri = connection.Address;
            return GetRequestRespsonse;
        }

        #region Overrides of RemoteWorkflowExecutionContainer

        protected override void ExecuteWebRequestAsync(WebRequest buildGetWebRequest)
        {
            LogExecutionUrl = buildGetWebRequest.RequestUri.ToString();
            LogExecutionWebRequest = buildGetWebRequest;
        }

        #endregion

        protected override IList<DebugState> FetchRemoteDebugItems(Connection connection)
        {
            FetchRemoteDebugItemsUri = connection.Address;
            return new List<DebugState>();
        }

    }
}
