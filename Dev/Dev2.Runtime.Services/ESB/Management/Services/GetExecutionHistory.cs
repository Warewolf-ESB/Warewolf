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
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Warewolf.Auditing;
using Warewolf.Resource.Errors;
using Warewolf.Triggers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetExecutionHistory : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var serializer = new Dev2JsonSerializer();
                var result = new List<IExecutionHistory>();
                if (values == null)
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }
                values.TryGetValue("ResourceId", out StringBuilder triggerID);
                if (triggerID != null)
                {
                    var _client = WebSocketWrapper.Create("ws://localhost:5000/ws");
                    _client.Connect();

                    Dev2Logger.Info("Get Execution History Data from Logger Service. " + triggerID, GlobalConstants.WarewolfInfo);
                   
                    var response = "";
                    var message = new AuditCommand
                    {
                        Type = "TriggerQuery",
                        Query = values
                    };
                    try
                    {
                        var ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
                        _client.SendMessage(serializer.Serialize(message));
                        _client.OnMessage((msgResponse, socket) =>
                        {
                            ewh.Set();
                            response = msgResponse;
                            socket.Close();
                        });
                        ewh.WaitOne();
                        return serializer.SerializeToBuilder(response);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Info("Get Execution History Data ServiceError", e, GlobalConstants.WarewolfInfo);
                    }
                    return serializer.SerializeToBuilder(result);
                }
                Dev2Logger.Debug("No QueueName Provided", GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(new List<IExecutionHistory>());
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public new AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList></DataList>");

        public override string HandlesType() => "GetExecutionHistoryService";
    }
}
