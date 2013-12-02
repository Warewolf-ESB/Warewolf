using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.WebServer.Security;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    public class TestAuthorizationProvider : AuthorizationProvider
    {
        public TestAuthorizationProvider(ISecurityConfigProvider securityConfigProvider)
            : base(securityConfigProvider)
        {
        }
    }
}