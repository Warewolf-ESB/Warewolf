using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class FetchWcfAction : IEsbManagementEndpoint
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
            return "FetchWcfAction";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var source = serializer.Deserialize<IWcfServerSource>(values["WcfSource"]);

                var src = ResourceCatalog.Instance.GetResource<WcfSource>(GlobalConstants.ServerWorkspaceID, source.Id);

                var methods = src.GetServiceActions(source);

                return serializer.SerializeToBuilder(new ExecuteMessage
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder(methods)
                });
            }
            catch (Exception e)
            {
                return serializer.SerializeToBuilder(new ExecuteMessage
                {
                    HasError = true,
                    Message = new StringBuilder(e.Message)
                });
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
    }
}
