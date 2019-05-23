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

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServerInformationalVersion : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serialiser = new Dev2JsonSerializer();
            return serialiser.SerializeToBuilder(GetVersion());
        }

        static string GetVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var fileName = asm.Location;
            var versionResource = FileVersionInfo.GetVersionInfo(fileName);
            return versionResource.ProductVersion;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetServerInformationalVersion";
    }
}
