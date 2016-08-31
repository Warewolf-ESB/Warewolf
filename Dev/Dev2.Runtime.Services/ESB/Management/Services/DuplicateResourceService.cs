using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    // ReSharper disable once UnusedMember.Global
    public class DuplicateResourceService : IEsbManagementEndpoint
    {
        private readonly IResourceCatalog _catalog;

        public DuplicateResourceService(IResourceCatalog catalog)
        {
            _catalog = catalog;
        }

        // ReSharper disable once MemberCanBeInternal
        public DuplicateResourceService()
        {

        }
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            StringBuilder tmp;
            StringBuilder newResourceName;
            StringBuilder destinatioPath;
            values.TryGetValue("ResourceID", out tmp);
            values.TryGetValue("NewResourceName", out newResourceName);
            values.TryGetValue("destinatioPath", out destinatioPath);

            if (tmp != null)
            {
                Guid resourceId;
                if (Guid.TryParse(tmp.ToString(), out resourceId))
                {
                    if (!string.IsNullOrEmpty(newResourceName?.ToString()))
                    {
                        try
                        {
                            if (destinatioPath == null)
                            {
                                var faliure = new ExecuteMessage { HasError = true, Message = new StringBuilder("Destination Paths not specified") };
                                return serializer.SerializeToBuilder(faliure);
                            }
                            var resourceCatalog = _catalog ?? ResourceCatalog.Instance;
                            var resourceCatalogResult = resourceCatalog.DuplicateResource(resourceId.ToString().ToGuid(), destinatioPath.ToString(), newResourceName.ToString());
                            Dev2Logger.Error("DuplicateResourceService success");
                            var result = new ExecuteMessage { HasError = false, Message = resourceCatalogResult.Message.ToStringBuilder() };
                            return serializer.SerializeToBuilder(result);
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
