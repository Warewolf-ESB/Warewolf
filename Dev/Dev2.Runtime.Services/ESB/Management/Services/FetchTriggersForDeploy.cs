/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Warewolf.Triggers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchTriggersForDeploy : DefaultEsbManagementEndpoint
    {
        private ITriggersCatalog _triggersCatalog;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Fetch Triggers for Deploy Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("resourceID", out var resourceIdString);
                if (resourceIdString == null)
                {
                    throw new InvalidDataContractException("resourceID is missing");
                }
                if (!Guid.TryParse(resourceIdString.ToString(), out var resourceId))
                {
                    throw new InvalidDataContractException("resourceID is not a valid GUID.");
                }

                var triggerQueues = TriggersCatalog.LoadQueuesByResourceId(resourceId);
                var message = new CompressedExecuteMessage
                {
                    HasError = false
                };
                message.SetMessage(serializer.Serialize(triggerQueues));

                return serializer.SerializeToBuilder(message);
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Fetch Triggers For Deploy Failed: " + err.Message, GlobalConstants.WarewolfError);
                var msg = new CompressedExecuteMessage {HasError = true, Message = new StringBuilder(err.Message)};
                return serializer.SerializeToBuilder(msg);
            }
        }

        public ITriggersCatalog TriggersCatalog
        {
            get => _triggersCatalog ?? Hosting.TriggersCatalog.Instance;
            set => _triggersCatalog = value;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => nameof(FetchTriggersForDeploy);
    }
}
