using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;


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
        public DuplicateResourceService()
        {

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
            values.TryGetValue("ResourceID", out tmp);
            values.TryGetValue("NewResourceName", out newResourceName);
            values.TryGetValue("FixRefs", out fixRefs);

            if (tmp != null)
            {
                Guid resourceId;
                if (Guid.TryParse(tmp.ToString(), out resourceId))
                {
                    if (!string.IsNullOrEmpty(newResourceName?.ToString()))
                    {

                        var explorerItem = GetItemExplorer().Find(resourceId);
                        var resource = GetResourceCatalog().GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

                        try
                        {

                            if (!explorerItem.IsFolder)
                            {
                                // ReSharper disable once UnusedVariable
                                SaveSingleResource(resource, newResourceName);

                            }
                            else
                            {
                                // ReSharper disable once UnusedVariable
                                SaveFolders(explorerItem, newResourceName);
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

        private void SaveSingleResource(IResource resource, StringBuilder newResourceName)
        {
            var newResourceClone = new Resource(resource);
            //Allocate new ID
            var newGuid = Guid.NewGuid();
            newResourceClone.ResourceName = newResourceName.ToString();
            newResourceClone.ResourceID = newGuid;
            newResourceClone.ResourcePath = string.Empty;
            newResourceClone.VersionInfo = null;
            GetResourceCatalog().SaveResource(GlobalConstants.ServerWorkspaceID, newResourceClone);
        }

        private static IEnumerable<T> Traverse<T>(T item, Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>();
            stack.Push(item);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in childSelector(next))
                    stack.Push(child);
            }
        }
        private void SaveFolders(IExplorerItem explorerItem, StringBuilder newResourceName)
        {
            IEnumerable<IExplorerItem> explorerItems;
            try
            {
                explorerItems = Traverse(explorerItem, item => item?.Children ?? new List<IExplorerItem>())
                    .Where(item => item.ResourceId != explorerItem.ResourceId);
            }
            // ReSharper disable once UnusedVariable
            catch (Exception ex)
            {
                explorerItems = explorerItem.Children;
                Console.WriteLine(ex.Message);
            }
            var guidIds = new StringBuilder();

            foreach (var guidId in explorerItems.Select(item => item.ResourceId))
            {
                guidIds.Append(guidId + ",");
            }
            var resourceList = GetResourceCatalog().GetResourceList(GlobalConstants.ServerWorkspaceID, new Dictionary<string, string> { { "guidCsv", guidIds.ToString() } });
            var recourceClones = new List<IResource>(resourceList);
            foreach (var recourceClone in recourceClones)
            {
                StringBuilder fixedResourcename = new StringBuilder();
                var folderName = recourceClone.ResourcePath.Split('\\');

                for (int index = 0; index < folderName.Length; index++)
                {
                    var value = folderName[index];
                    if (index > 0)
                    {
                        if (value.ToLower() == explorerItem.DisplayName.ToLower())
                        {
                            fixedResourcename.Append("\\" + newResourceName);
                        }
                        else
                        {
                            fixedResourcename.Append("\\" + value);
                        }
                    }
                    else
                    {
                        if (value.ToLower() == explorerItem.DisplayName.ToLower())
                        {
                            fixedResourcename.Append("\\" + newResourceName);
                        }
                        else
                        {
                            fixedResourcename.Append(value);
                        }
                    }
                }

                recourceClone.ResourceID = Guid.NewGuid();
                recourceClone.ResourcePath = fixedResourcename.ToString();
                recourceClone.VersionInfo = null;
                GetResourceCatalog().SaveResource(GlobalConstants.ServerWorkspaceID, recourceClone);
            }
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
