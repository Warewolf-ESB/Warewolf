using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServerInformationalVersion : DefaultEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            return serialiser.SerializeToBuilder(GetVersion());
        }

        public override DynamicService CreateServiceEntry()
        {
            var getServerVersion = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            var getServerVersionService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            getServerVersionService.Actions.Add(getServerVersion);

            return getServerVersionService;
        }

        public override string HandlesType()
        {
            return "GetServerInformationalVersion";
        }


        static string GetVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var fileName = asm.Location;
            var versionResource = FileVersionInfo.GetVersionInfo(fileName);
            return versionResource.ProductVersion;
        }
    }
}
