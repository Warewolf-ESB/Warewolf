using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
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
        private readonly IResourceCatalog _resourceCatalog;
        private readonly IExplorerServerResourceRepository _resourceRepository;

        public DuplicateResourceService(IResourceCatalog resourceCatalog, IExplorerServerResourceRepository resourceRepository)
        {
            _resourceCatalog = resourceCatalog;
            _resourceRepository = resourceRepository;
        }

        public DuplicateResourceService()
            : this(ResourceCatalog.Instance, ServerExplorerRepository.Instance)
        {

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
            var resourceId = Guid.Empty;

            if (tmp != null)
            {
                if (Guid.TryParse(tmp.ToString(), out resourceId))
                {
                    if (!string.IsNullOrEmpty(newResourceName?.ToString()))
                    {

                        var explorerItem = _resourceRepository.Find(resourceId);
                        var resource = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

                        try
                        {

                            if (!explorerItem.IsFolder)
                            {
                                var xElement = resource.ToXml();
                                var newResourceXml = new XElement(xElement);
                                //Allocate new ID
                                var newGuid = Guid.NewGuid();
                                var newResource = new Resource(newResourceXml)
                                {
                                    ResourceName = newResourceName.ToString(),
                                    ResourceID = newGuid
                                };

                                _resourceCatalog.CopyResource(newResource, GlobalConstants.ServerWorkspaceID);
                            }
                            else
                            {

                                var explorerItems = explorerItem.Children;
                                var folderResource = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, explorerItem.ResourceId);

                                var newFolder = new Resource(folderResource)
                                {
                                    ResourceName = newResourceName.ToString(),
                                    ResourceID = Guid.NewGuid()
                                };
                                _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, newFolder);
                                var guidIds =new StringBuilder() ;
                              
                                foreach (var guidId in explorerItems.Select(item => item.ResourceId))
                                {
                                    guidIds.Append(guidId + ",");
                                }
                                var resourceList = _resourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID,
                                    new Dictionary<string, string> { { "guidCsv", guidIds.ToString() } });
                                var recourceClones = new List<IResource>(resourceList);
                                foreach (var recourceClone in recourceClones)
                                {
                                    recourceClone.ResourceID = Guid.NewGuid();
                                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, recourceClone);
                                }

                            }

                        }
                        catch (Exception x)
                        {
                            Dev2Logger.Error(x.Message + " DuplicateResourceService", x);
                            var result = new ExecuteMessage { HasError = true };
                            return serializer.SerializeToBuilder(result);
                        }

                    }
                }
            }
            var success = new ExecuteMessage { HasError = false };
            return serializer.SerializeToBuilder(success);
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
