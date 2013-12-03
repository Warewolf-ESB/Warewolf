using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.WebServer.Security;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    public class TestAuthorizationService : AuthorizationService
    {
        public TestAuthorizationService(ISecurityConfigProvider securityConfigProvider)
            : base(securityConfigProvider)
        {
        }
    }
}