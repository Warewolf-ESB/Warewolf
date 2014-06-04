using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.ESB
{
    public class EsbServicesEndpointMock : EsbServicesEndpoint
    {
        readonly IEsbServiceInvoker _esbServiceInvoker;

        public EsbServicesEndpointMock(IEsbServiceInvoker esbServiceInvoker)
        {
            VerifyArgument.IsNotNull("esbServiceInvoker", esbServiceInvoker);
            _esbServiceInvoker = esbServiceInvoker;
        }

        protected override IEsbServiceInvoker CreateEsbServicesInvoker(IWorkspace theWorkspace)
        {
            return _esbServiceInvoker;
        }
    }
}