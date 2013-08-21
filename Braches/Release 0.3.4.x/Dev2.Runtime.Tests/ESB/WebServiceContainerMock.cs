using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.ESB
{
    public class WebServiceContainerMock : WebServiceContainer
    {
        public WebServiceContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
        }

        public string WebRequestRespsonse { get; set; }

        protected override void ExecuteWebRequest(WebService service)
        {
            service.RequestResponse = WebRequestRespsonse;
        }
    }
}
