using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            return ReadPermissionsResult;
        }

        protected override void WritePermissions(List<WindowsGroupPermission> permissions)
        {
        }

        protected override void LogStart([CallerMemberName]string methodName = null)
        {
        }

        protected override void LogEnd([CallerMemberName]string methodName = null)
        {
        }
    }
}