using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class DuplicateResourceService : IEsbManagementEndpoint
    {
        private IResourceCatalog _resourceCatalog;
        private IExplorerServerResourceRepository _resourceRepository;

        public DuplicateResourceService(IResourceCatalog resourceCatalog, IExplorerServerResourceRepository resourceRepository)
        {
            _resourceCatalog = resourceCatalog;
            _resourceRepository = resourceRepository;
        }
        /// <summary>
        /// USED, 
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        public DuplicateResourceService()
        {
            //

        }

        private IResourceCatalog GetResourceCatalog()
        {
            return _resourceCatalog ?? (_resourceCatalog = ResourceCatalog.Instance);
        }
        private IExplorerServerResourceRepository GetItemExplorer()
        {
            return _resourceRepository ?? (_resourceRepository = ServerExplorerRepository.Instance);
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            StringBuilder tmp;
            StringBuilder newResourceName;
            StringBuilder fixRefs;
            StringBuilder isFolder;
            StringBuilder lightExplorerItems;
            values.TryGetValue("ResourceID", out tmp);
            values.TryGetValue("NewResourceName", out newResourceName);
            values.TryGetValue("FixRefs", out fixRefs);
            values.TryGetValue("lightExplorerItems", out lightExplorerItems);
            values.TryGetValue("isFolder", out isFolder);

            if (tmp != null)
            {
                Guid resourceId;
                if (Guid.TryParse(tmp.ToString(), out resourceId))
                {
                    if (!string.IsNullOrEmpty(newResourceName?.ToString()))
                    {
                        var items = lightExplorerItems?.ToString();
                        var actualItems = serializer.Deserialize<List<LightExplorerItem>>(items);
                        try
                        {
                            var valueIsFolder = isFolder != null && bool.Parse(isFolder.ToString());
                            if (!valueIsFolder)
                            {
                                // ReSharper disable once UnusedVariable
                                //SaveSingleResource(resource, newResourceName, explorerItem);
                                SaveResource(actualItems.Single(), newResourceName);
                            }
                            else
                            {
                                // ReSharper disable once UnusedVariable
                                SaveFolders(actualItems, newResourceName);
                            }
                        }
                        catch (Exception x)
                        {
                            Dev2Logger.Error(x.Message + " DuplicateResourceService", x);
                            var result = new ExecuteMessage { HasError = true, Message = x.Message.ToStringBuilder() };
                            return serializer.SerializeToBuilder(result);
                        }

                    }
                }
            }
            var success = new ExecuteMessage { HasError = false };
            return serializer.SerializeToBuilder(success);
        }

        private void SaveResource(ILightExplorerItem lightResource, StringBuilder newResourceName, string resourcePath = null)
        {

            StringBuilder result = GetResourceCatalog().GetResourceContents(GlobalConstants.ServerWorkspaceID, lightResource.ResourceId);
            var resource = GetResourceCatalog().GetResource(GlobalConstants.ServerWorkspaceID, lightResource.ResourceId);
            var xElement = result.ToXElement();
            resource.ResourcePath = resourcePath ?? newResourceName.ToString();
            resource.IsUpgraded = true;
            var resourceID = Guid.NewGuid();
            var resourceName = newResourceName.ToString().Split('\\').Last();

            resource.ResourceName = lightResource.ResourceName != resourceName ? resourceName : lightResource.ResourceName;
            resource.ResourceID = resourceID;
            var displayName = xElement.Element("DisplayName")?.Value;
            var category = xElement.Element("Category")?.Value;
            xElement.SetElementValue("DisplayName", resourceName);
            xElement.SetElementValue("Category", category?.Replace(displayName ?? "", resourceName));
            xElement.SetElementValue("ID", resourceID.ToString());
            xElement.SetElementValue("Name", resourceName);
            xElement.SetElementValue("ResourcePath", resourcePath ?? newResourceName.ToString());
            var fixedResource = xElement.ToStringBuilder();
            GetResourceCatalog().SaveResource(GlobalConstants.ServerWorkspaceID, resource, fixedResource);

        }


        private void SaveFolders(IEnumerable<ILightExplorerItem> explorerItems, StringBuilder newResourceName)
        {

            var guidIds = new StringBuilder();

            var lightExplorerItems = explorerItems as ILightExplorerItem[] ?? explorerItems.ToArray();
            foreach (var guidId in lightExplorerItems.Select(item => item.ResourceId))
            {
                guidIds.Append(guidId + ",");
            }
            var resourceList = GetResourceCatalog().GetResourceList(GlobalConstants.ServerWorkspaceID, new Dictionary<string, string> { { "guidCsv", guidIds.ToString() } });
            var recourceClones = new List<IResource>(resourceList);
            foreach (var recourceClone in lightExplorerItems.Where(item => recourceClones.Any(resource => resource.ResourceID == item.ResourceId)))
            {
                // ReSharper disable once RedundantToStringCall
              /*  var indexOf = recourceClone.ResourcePath.IndexOf($"\\{newResourceName.ToString()}", StringComparison.CurrentCultureIgnoreCase);
                var resourcePath = recourceClone.ResourcePath.Substring(indexOf - 1, recourceClone.ResourcePath.Length);
                var stringBuilder = GetResourceName(recourceClone, newResourceName, recourceClone.ResourcePath.Split('\\'));*/
                SaveResource(recourceClone, newResourceName, recourceClone.ResourcePath);
            }
        }

        private static StringBuilder GetResourceName(ILightExplorerItem explorerItem, StringBuilder newResourceName, string[] folderName)
        {
            StringBuilder fixedResourcename = new StringBuilder();
            for (int index = 0; index < folderName.Length; index++)
            {
                var value = folderName[index];
                var stringBuilder = newResourceName.ToString().Split('\\').Last();
                if (index > 0)
                {
                    if (string.Equals(value, explorerItem.ResourceName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        fixedResourcename.Append("\\" + stringBuilder);
                        break;
                    }
                    fixedResourcename.Append("\\" + value);
                }
                else
                {
                    if (string.Equals(value, explorerItem.ResourceName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        fixedResourcename.Append("\\" + stringBuilder);
                        break;
                    }
                    fixedResourcename.Append(value);
                }
            }
            return fixedResourcename;
        }

        public string HandlesType()
        {
            return "DuplicateResourceService";
        }

        public DynamicService CreateServiceEntry()
        {
            var deleteResourceService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><ResourceName ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            var deleteResourceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            deleteResourceService.Actions.Add(deleteResourceAction);

            return deleteResourceService;
        }
    }
}
