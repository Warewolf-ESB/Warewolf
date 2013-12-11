using System.Collections.Generic;
using System.Net;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Dev2.Runtime.ServiceModel.Data;

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

        protected override string ExecuteGetRequest(Connection connection, string serviceName, string payload)
        {
            GetRequestUri = connection.WebAddress;
            return GetRequestRespsonse;
        }

        #region Overrides of RemoteWorkflowExecutionContainer

        protected override void ExecuteWebRequestAsync(WebRequest buildGetWebRequest)
        {
            LogExecutionUrl = buildGetWebRequest.RequestUri.ToString();
        }

        #endregion

        protected override IList<DebugState> FetchRemoteDebugItems(Connection connection)
        {
            FetchRemoteDebugItemsUri = connection.WebAddress;
            return new List<DebugState>();
        }

    }
}
