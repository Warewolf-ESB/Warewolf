using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Vestris.ResourceLib;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetMinSupportedServerVersion : DefaultEsbManagementEndpoint
    {

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            return serialiser.SerializeToBuilder(GetVersion().ToString());
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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetMinSupportedVersion";
    }
}