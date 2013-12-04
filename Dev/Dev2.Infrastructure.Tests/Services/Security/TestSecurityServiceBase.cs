using System.Collections.Generic;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestSecurityServiceBase : SecurityServiceBase
    {
        protected override void OnDisposed()
        {
        }

        public List<WindowsGroupPermission> ReadPermissionsResult { get; set; }

        protected override string ReadPermissions()
        {
            return ReadPermissionsResult == null ? null : JsonConvert.SerializeObject(ReadPermissionsResult);
        }
    }
}