using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Vestris.ResourceLib;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServerVersion : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, Workspaces.IWorkspace theWorkspace)
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
            return "GetServerVersion";
        }


        static Version GetVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var versionResource = new VersionResource();
            var fileName = asm.Location;
            versionResource.LoadFrom(fileName);
            Version v = new Version(versionResource.FileVersion);
            return v;
        }
    }
}
