using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.ESB
{
    public class EsbServicesEndpointMock : EsbServicesEndpoint
    {
        readonly IDynamicServicesInvoker _dynamicServicesInvoker;

        public EsbServicesEndpointMock(IDynamicServicesInvoker dynamicServicesInvoker)
        {
            VerifyArgument.IsNotNull("dynamicServicesInvoker", dynamicServicesInvoker);
            _dynamicServicesInvoker = dynamicServicesInvoker;
        }

        protected override IDynamicServicesInvoker CreateDynamicServicesInvoker(IWorkspace theWorkspace)
        {
            return _dynamicServicesInvoker;
        }
    }
}