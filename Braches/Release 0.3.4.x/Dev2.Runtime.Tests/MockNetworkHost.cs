using System.Network;

namespace Dev2.DynamicServices.Test
{
    public abstract class MockNetworkHost : NetworkHost
    {
        protected MockNetworkHost()
            : base("TestHost")
        {
        }
    }
}