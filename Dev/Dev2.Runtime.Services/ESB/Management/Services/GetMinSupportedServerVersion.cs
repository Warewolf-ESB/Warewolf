using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;
using Vestris.ResourceLib;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class GetMinSupportedServerVersion : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            return serialiser.SerializeToBuilder(GetVersion().ToString());
        }

        public DynamicService CreateServiceEntry()
        {
            var getServerVersion = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            var getServerVersionService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            getServerVersionService.Actions.Add(getServerVersion);

            return getServerVersionService;
        }

        public string HandlesType()
        {
            return "GetMinSupportedVersion";
        }


        static Version GetVersion()
        {
           var min =  ConfigurationManager.AppSettings["MinSupportedVersion"];
            if( min != null)
            {
                return Version.Parse(min);
            }
            var asm = Assembly.GetExecutingAssembly();
            var versionResource = new VersionResource();
            var fileName = asm.Location;
            versionResource.LoadFrom(fileName);
            Version v = new Version(versionResource.FileVersion);
            return v;
        }
    }
}