﻿using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;



namespace Dev2.Runtime.ESB.Management.Services
{

    public class TestRabbitMQServiceSource : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Test RabbitMQ Service Source", GlobalConstants.WarewolfInfo);
                msg.HasError = false;

                values.TryGetValue("RabbitMQServiceSource", out StringBuilder resourceDefinition);

                var rabbitMQServiceSourceDefinition = serializer.Deserialize<RabbitMQServiceSourceDefinition>(resourceDefinition);
                var rabbitMQSources = new RabbitMQSources();
                var result = rabbitMQSources.Test(new RabbitMQSource
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
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }
            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><RabbitMQServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "TestRabbitMQServiceSource";
    }
}