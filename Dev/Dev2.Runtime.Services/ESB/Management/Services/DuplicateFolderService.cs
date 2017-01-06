using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    // ReSharper disable once UnusedMember.Global
    public class DuplicateFolderService : IEsbManagementEndpoint
    {

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        private readonly IResourceCatalog _catalog;

        public DuplicateFolderService(IResourceCatalog catalog)
        {
            _catalog = catalog;
        }

        // ReSharper disable once MemberCanBeInternal
        public DuplicateFolderService()
        {

        }
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            StringBuilder newResourceName;
            StringBuilder fixRefs;
            StringBuilder sourcePath;
            StringBuilder destinationPath;
            values.TryGetValue("NewResourceName", out newResourceName);
            values.TryGetValue("FixRefs", out fixRefs);
            values.TryGetValue("sourcePath", out sourcePath);
            values.TryGetValue("destinationPath", out destinationPath);

            if (!string.IsNullOrEmpty(newResourceName?.ToString()))
            {
                try
                {
                    if (sourcePath == null || destinationPath == null)
                    {
                        throw new Exception("Source or Destination Paths not specified");
                    }

                    var resourceCatalog = _catalog ?? ResourceCatalog.Instance;
                    var resourceCatalogResult = resourceCatalog.DuplicateFolder(sourcePath.ToString(), destinationPath.ToString(), newResourceName.ToString(), bool.Parse(fixRefs?.ToString() ?? false.ToString()));
                    Dev2Logger.Error(resourceCatalogResult.Message);                    
                    return serializer.SerializeToBuilder(resourceCatalogResult);

                }
                catch (Exception x)
                {
                    Dev2Logger.Error(x.Message + " DuplicateResourceService", x);
                    var result = new ExecuteMessage { HasError = true, Message = x.Message.ToStringBuilder() };
                    return serializer.SerializeToBuilder(result);
                }

            }

            var success = new ExecuteMessage { HasError = true, Message = new StringBuilder("NewResourceName required")};
            return serializer.SerializeToBuilder(success);
        }

        public string HandlesType()
        {
            return "DuplicateFolderService";
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
