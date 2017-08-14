using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common.Interfaces.Enums;

// ReSharper disable InconsistentNaming

namespace Dev2.Runtime.ESB.Management.Services
{

    public class TestRabbitMQServiceSource : IEsbManagementEndpoint
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
                Dev2Logger.Info("Test RabbitMQ Service Source");
                StringBuilder resourceDefinition;
                msg.HasError = false;

                values.TryGetValue("RabbitMQServiceSource", out resourceDefinition);

                RabbitMQServiceSourceDefinition rabbitMQServiceSourceDefinition = serializer.Deserialize<RabbitMQServiceSourceDefinition>(resourceDefinition);
                RabbitMQSources rabbitMQSources = new RabbitMQSources();
                ValidationResult result = rabbitMQSources.Test(new RabbitMQSource
                {
                    ResourceID = rabbitMQServiceSourceDefinition.ResourceID,
                    ResourceName = rabbitMQServiceSourceDefinition.ResourceName,
                    HostName = rabbitMQServiceSourceDefinition.HostName,
                    Port = rabbitMQServiceSourceDefinition.Port,
                    UserName = rabbitMQServiceSourceDefinition.UserName,
                    Password = rabbitMQServiceSourceDefinition.Password,
                    VirtualHost = rabbitMQServiceSourceDefinition.VirtualHost
                });
                
                if (!result.IsValid)
                {
                    msg.HasError = true;
                    msg.Message = new StringBuilder(result.ErrorMessage);
                }
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err);
            }
            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><RabbitMQServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "TestRabbitMQServiceSource";
        }
    }
}