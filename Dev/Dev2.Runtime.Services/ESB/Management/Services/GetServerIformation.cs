using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Vestris.ResourceLib;
using System.Diagnostics;
using ServiceStack.ServiceModel;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServerInformation : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            return serialiser.SerializeToBuilder(GetServerInformationData());
        }

        private static Dictionary<string, string> GetServerInformationData()
        {
            var toReturn = ConfigurationManager.AppSettings.ToDictionary();
            toReturn.Add("Version", GetVersion());

            if (!toReturn.ContainsKey("MinSupportedVersion"))
            {
                toReturn.Add("MinSupportedVersion", MinSupportedVersion());
            }
            return toReturn;
        }

        private static string MinSupportedVersion()
        {
            var min =  ConfigurationManager.AppSettings["MinSupportedVersion"];
            if( min != null)
            {
                return min;
            }
            var asm = Assembly.GetExecutingAssembly();
            var versionResource = new VersionResource();
            var fileName = asm.Location;
            versionResource.LoadFrom(fileName);
            Version v = new Version(versionResource.FileVersion);
            return v.ToString();
        }

        private static string GetVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var fileName = asm.Location;
            var versionResource = FileVersionInfo.GetVersionInfo(fileName);
            return versionResource.FileVersion;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetServerInformation";
    }
}