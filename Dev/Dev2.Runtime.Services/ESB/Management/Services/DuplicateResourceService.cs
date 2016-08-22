using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
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

        public DuplicateResourceService(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public DuplicateResourceService()
            : this(ResourceCatalog.Instance)
        {

        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            StringBuilder tmp;
            StringBuilder newResourceName;
            values.TryGetValue("ResourceID", out tmp);
            values.TryGetValue("NewResourceName", out newResourceName);
            var resourceId = Guid.Empty;

            if (tmp != null)
            {
                if (Guid.TryParse(tmp.ToString(), out resourceId))
                {
                    if (!string.IsNullOrEmpty(newResourceName?.ToString()))
                    {
                        var resource = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
                        if (!resource.IsFolder)
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
                            //Allocate new Path
                            _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, newResource);
                        }
                    }
                }
            }
            var result = new ExecuteMessage { HasError = false };
            return serializer.SerializeToBuilder(result);
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
