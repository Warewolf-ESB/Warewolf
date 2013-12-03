using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    public class TestAuthorizationService : AuthorizationService
    {
        public TestAuthorizationService(ISecurityService securityService)
            : base(securityService)
        {
        }
    }
}