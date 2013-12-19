using System.Collections.Generic;
using Dev2.Services.Security;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestSecurityServiceBase : SecurityServiceBase
    {
        protected override void OnDisposed()
        {
        }

        public List<WindowsGroupPermission> ReadPermissionsResult { get; set; }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
            return ReadPermissionsResult ?? null;
        }
    }
}