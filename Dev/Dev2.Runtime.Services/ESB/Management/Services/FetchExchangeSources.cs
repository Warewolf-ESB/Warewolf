﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchExchangeSources : IEsbManagementEndpoint
    {
        public string HandlesType()
        {
            return "FetchExchangeSources";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            // ReSharper disable MaximumChainedReferences
            var list = Resources.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(a => a.ResourceType == ResourceType.ExchangeSource).Select(a =>
            {
                var res = Resources.GetResource<ExchangeSource>(GlobalConstants.ServerWorkspaceID, a.ResourceID);
                if (res != null)
                {
                    return new ExchangeSource()
                    {
                        ResourceID = res.ResourceID,
                        ResourceName = res.Name,
                        Path = res.Path,
                        UserName = res.UserName,
                        Password = res.Password,
                        AutoDiscoverUrl = res.AutoDiscoverUrl,
                        Timeout = res.Timeout,
                        Type = res.Type
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

        public ResourceCatalog Resources
        {
            get
            {
                return ResourceCatalog.Instance;
            }
        }
    }
}
