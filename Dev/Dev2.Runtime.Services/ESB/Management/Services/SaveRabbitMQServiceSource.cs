﻿using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable InconsistentNaming

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SaveRabbitMQServiceSource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save RabbitMQ Service Source");
                StringBuilder resourceDefinition;
                msg.HasError = false;

                values.TryGetValue("RabbitMQServiceSource", out resourceDefinition);

                RabbitMQServiceSourceDefinition rabbitMQServiceSourceDefinition = serializer.Deserialize<RabbitMQServiceSourceDefinition>(resourceDefinition);
                if (rabbitMQServiceSourceDefinition.ResourcePath.EndsWith("\\"))
                {
                    rabbitMQServiceSourceDefinition.ResourcePath = rabbitMQServiceSourceDefinition.ResourcePath.Substring(0, rabbitMQServiceSourceDefinition.ResourcePath.LastIndexOf("\\", StringComparison.Ordinal));
                }

                RabbitMQSource rabbitMQSource = new RabbitMQSource
                {
                    ResourceID = rabbitMQServiceSourceDefinition.ResourceID,
                    ResourceName = rabbitMQServiceSourceDefinition.ResourceName,
                    ResourcePath = rabbitMQServiceSourceDefinition.ResourcePath,
                    HostName = rabbitMQServiceSourceDefinition.HostName,
                    Port = rabbitMQServiceSourceDefinition.Port,
                    UserName = rabbitMQServiceSourceDefinition.UserName,
                    Password = rabbitMQServiceSourceDefinition.Password,
                    VirtualHost = rabbitMQServiceSourceDefinition.VirtualHost
                };

                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, rabbitMQSource);
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error("Save RabbitMQ Service Source Failed: " + err.Message);
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
            return "SaveRabbitMQServiceSource";
        }
    }
}