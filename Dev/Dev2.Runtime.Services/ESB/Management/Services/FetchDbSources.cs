using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
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

    public class FetchDbSources : IEsbManagementEndpoint
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
            return "FetchDbSources";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            
            List<DbSourceDefinition> list = Resources.GetResourceList<DbSource>(GlobalConstants.ServerWorkspaceID).Select(a =>
            {
                var res = a as DbSource;
                if (res != null)
                {
                    return new DbSourceDefinition
                    {
                        AuthenticationType = res.AuthenticationType,
                        DbName = res.DatabaseName,
                        Id = res.ResourceID,
                        Name = res.ResourceName,
                        Path = res.GetSavePath(),
                        Password = res.Password,
                        ServerName = res.Server,
                        Type = res.ServerType,
                        UserName = res.UserID
                    };
                }
                return null;
            }).ToList();
         
            return serializer.SerializeToBuilder(new ExecuteMessage { HasError = false, Message = serializer.SerializeToBuilder(list) });
            
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;
    }
}
