using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;

using Warewolf.Studio.ServerProxyLayer;

namespace ServerProxyLayerTests
{
    class ProxyImpl : ProxyBase {
        public ProxyImpl(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection)
            : base(communicationControllerFactory, connection)
        {
        }
    }
}
