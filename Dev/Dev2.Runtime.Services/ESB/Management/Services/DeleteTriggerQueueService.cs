﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Warewolf.Resource.Errors;
using Warewolf.Triggers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class DeleteTriggerQueueService : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if (values == null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }

            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Delete Trigger Queue Service", GlobalConstants.WarewolfInfo);
                msg.HasError = false;

                values.TryGetValue("TriggerQueue", out StringBuilder resourceDefinition);

                var triggerQueue = serializer.Deserialize<ITriggerQueue>(resourceDefinition);

                TriggersCatalog.Instance.DeleteTriggerQueue(triggerQueue);

                return serializer.SerializeToBuilder(msg);
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error("Delete Queue Service Failed: " + err.Message, GlobalConstants.WarewolfError);
                return serializer.SerializeToBuilder(msg);
            }
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => nameof(DeleteTriggerQueueService);
    }
}
