/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Triggers;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchTriggerQueues : IEsbManagementEndpoint
    {
        private ITriggersCatalog _triggersCatalog;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Fetch Trigger Queue Service", GlobalConstants.WarewolfInfo);

                TriggersCatalog.Load();
                var triggerQueues = TriggersCatalog.Queues;
                var message = new CompressedExecuteMessage
                {
                    HasError = false
                };
                message.SetMessage(serializer.Serialize(triggerQueues));
                message.HasError = false;

                return serializer.SerializeToBuilder(message);
            }
            catch (Exception err)
            {
                var msg = new ExecuteMessage
                {
                    HasError = true,
                    Message = new StringBuilder(err.Message)
                };
                Dev2Logger.Error("Fetch Queue Service Failed: " + err.Message, GlobalConstants.WarewolfError);
                return serializer.SerializeToBuilder(msg);
            }
        }

        public ITriggersCatalog TriggersCatalog
        {
            get => _triggersCatalog ?? Triggers.TriggersCatalog.Instance;
            set => _triggersCatalog = value;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => nameof(FetchTriggerQueues);
    }
}
