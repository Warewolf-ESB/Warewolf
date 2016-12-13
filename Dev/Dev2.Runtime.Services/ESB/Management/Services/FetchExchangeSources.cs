using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
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
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FetchExchangeSources : IEsbManagementEndpoint
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
            return "FetchExchangeSources";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            // ReSharper disable MaximumChainedReferences
            var list = Resources.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(a => a.ResourceType == "ExchangeSource").Select(a =>
            {
                var res = Resources.GetResource<ExchangeSource>(GlobalConstants.ServerWorkspaceID, a.ResourceID);
                if (res != null)
                {
                    return new ExchangeSourceDefinition
                    {
                        ResourceID = res.ResourceID,
                        ResourceName = res.ResourceName,
                        Path = res.GetSavePath(),
                        UserName = res.UserName,
                        Password = res.Password,
                        AutoDiscoverUrl = res.AutoDiscoverUrl,
                        Timeout = res.Timeout,
                        Type = res.Type,
                        ResourceType = res.ResourceType
                    };
                }
                return null;
            }).ToList();
            return serializer.SerializeToBuilder(new ExecuteMessage() { HasError = false, Message = serializer.SerializeToBuilder(list) });
            // ReSharper restore MaximumChainedReferences
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        private ResourceCatalog Resources => ResourceCatalog.Instance;
    }
}
