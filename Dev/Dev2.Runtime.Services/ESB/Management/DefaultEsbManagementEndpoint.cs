using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public abstract class DefaultEsbManagementEndpoint : IEsbManagementEndpoint
    {
        public abstract StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace);
        public abstract DynamicService CreateServiceEntry();
        public abstract string HandlesType();
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }
        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}