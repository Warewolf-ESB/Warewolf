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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchPluginSources : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            
            var list = Resources.GetResourceList<PluginSource>(GlobalConstants.ServerWorkspaceID).Select(a =>
            {
                if (a is PluginSource res)
                {
                    var pluginSourceDefinition = new PluginSourceDefinition
                    {
                        Id = res.ResourceID,
                        Name = res.ResourceName,
                        Path = res.GetSavePath(),
                        SelectedDll = new DllListing
                        {
                            Name = res.AssemblyName,
                            FullName = res.AssemblyLocation,
                            Children = new IFileListing[0],
                        },
                        ConfigFilePath = res.ConfigFilePath
                    };
                    if (!string.IsNullOrEmpty(res.AssemblyLocation) && res.AssemblyLocation.EndsWith(".dll"))
                    {
                        pluginSourceDefinition.FileSystemAssemblyName = res.AssemblyLocation;
                        pluginSourceDefinition.GACAssemblyName = string.Empty;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(res.AssemblyLocation) && res.AssemblyLocation.StartsWith("GAC:"))
                        {
                            pluginSourceDefinition.GACAssemblyName = res.AssemblyLocation;
                            pluginSourceDefinition.FileSystemAssemblyName = string.Empty;
                        }
                    }
                    return pluginSourceDefinition;
                }
                return null;
            }).ToList();
            return serializer.SerializeToBuilder(new ExecuteMessage { HasError = false, Message = serializer.SerializeToBuilder(list) });            
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchPluginSources";
    }
}
