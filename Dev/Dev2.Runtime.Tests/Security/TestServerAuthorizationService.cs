using System;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Tests.Runtime.Security
{
    public class TestServerAuthorizationService : ServerAuthorizationService
    {
        public TestServerAuthorizationService(ISecurityService securityService)
            : base(securityService)
        {
        }

        public Guid RemovedResourceID { get; set; }

        public override void Remove(Guid resourceID)
        {
            RemovedResourceID = resourceID;
            base.Remove(resourceID);
        }
    }
}