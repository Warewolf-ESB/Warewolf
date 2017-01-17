using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchPluginSources : IEsbManagementEndpoint
    {

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public string HandlesType()
        {
            return "FetchPluginSources";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            // ReSharper disable MaximumChainedReferences
            List<PluginSourceDefinition> list = Resources.GetResourceList<PluginSource>(GlobalConstants.ServerWorkspaceID).Select(a =>
            {
                var res = a as PluginSource;
                if (res != null)
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
                    else if (!string.IsNullOrEmpty(res.AssemblyLocation) && res.AssemblyLocation.StartsWith("GAC:"))
                    {
                        pluginSourceDefinition.GACAssemblyName = res.AssemblyLocation;
                        pluginSourceDefinition.FileSystemAssemblyName = string.Empty;
                    }
                    return pluginSourceDefinition;
                }
                return null;
            }).ToList();
            return serializer.SerializeToBuilder(new ExecuteMessage { HasError = false, Message = serializer.SerializeToBuilder(list) });
            // ReSharper restore MaximumChainedReferences
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;
    }
}
