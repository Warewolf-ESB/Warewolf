#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System.Diagnostics;
using ServiceStack.ServiceModel;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServerInformation : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serialiser = new Dev2JsonSerializer();
            return serialiser.SerializeToBuilder(GetServerInformationData());
        }

        static Dictionary<string, string> GetServerInformationData()
        {
            var toReturn = ConfigurationManager.AppSettings.ToDictionary();
            toReturn.Add("Version", GetVersion());

            if (!toReturn.ContainsKey("MinSupportedVersion"))
            {
                toReturn.Add("MinSupportedVersion", MinSupportedVersion());
            }
            return toReturn;
        }

        static string MinSupportedVersion()
        {
            var min = ConfigurationManager.AppSettings["MinSupportedVersion"];
            if (min != null)
            {
                return min;
            }
            var asm = Assembly.GetExecutingAssembly();
            var fileName = asm.Location;
            var versionResource = FileVersionInfo.GetVersionInfo(fileName);
            var v = new Version(versionResource.FileVersion);
            return v.ToString();
        }

        static string GetVersion()
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