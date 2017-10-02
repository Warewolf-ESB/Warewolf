using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Hosting;
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

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("ResourceID", out StringBuilder tmp);
            if (tmp != null)
            {
                if (Guid.TryParse(tmp.ToString(), out Guid resourceId))
                {
                    return resourceId;
                }
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        private readonly IResourceCatalog _catalog;

        public DuplicateResourceService(IResourceCatalog catalog)
        {
            _catalog = catalog;
        }

        
        public DuplicateResourceService()
        {

        }
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            values.TryGetValue("ResourceID", out StringBuilder tmp);
            values.TryGetValue("NewResourceName", out StringBuilder newResourceName);
            values.TryGetValue("destinationPath", out StringBuilder destinationPath);

            if (tmp != null)
            {
                if (Guid.TryParse(tmp.ToString(), out Guid resourceId))
                {
                    if (!string.IsNullOrEmpty(newResourceName?.ToString()))
                    {
                        try
                        {
                            if (destinationPath == null)
                            {
                                var failure = new ResourceCatalogDuplicateResult { Status = ExecStatus.Fail, Message = "Destination Paths not specified" };
                                return serializer.SerializeToBuilder(failure);
                            }
                            var resourceCatalog = _catalog ?? ResourceCatalog.Instance;
                            var resourceCatalogResult = resourceCatalog.DuplicateResource(resourceId.ToString().ToGuid(), destinationPath.ToString(), newResourceName.ToString());
                            return serializer.SerializeToBuilder(resourceCatalogResult);
                        }
                        catch (Exception x)
                        {
                            Dev2Logger.Error(x.Message + " DuplicateResourceService", x, GlobalConstants.WarewolfError);
                            var result = new ExecuteMessage { HasError = true, Message = x.Message.ToStringBuilder() };
                            return serializer.SerializeToBuilder(result);
                        }

                    }
                }
            }
            var success = new ExecuteMessage { HasError = true, Message = new StringBuilder("ResourceId is required")};
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
