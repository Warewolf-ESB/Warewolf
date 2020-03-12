#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
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

    public class TestRabbitMQServiceSource : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><RabbitMQServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "TestRabbitMQServiceSource";
    }
}