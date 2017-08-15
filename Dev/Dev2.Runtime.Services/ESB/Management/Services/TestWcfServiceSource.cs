using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class TestWcfServiceSource : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Resource Service", "Warewolf Info");
                StringBuilder resourceDefinition;

                values.TryGetValue("WcfSource", out resourceDefinition);

                var src = serializer.Deserialize<WcfServiceSourceDefinition>(resourceDefinition);

                var source = new WcfSource()
                {
                    EndpointUrl = src.EndpointUrl
                };

                source.Execute();
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, "Warewolf Error");
            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "TestWcfServiceSource";
        }
    }
}
