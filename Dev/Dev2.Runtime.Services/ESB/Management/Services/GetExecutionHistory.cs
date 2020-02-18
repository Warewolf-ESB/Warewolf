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
using Warewolf.Interfaces.Auditing;
using Warewolf.Resource.Errors;
using Warewolf.Triggers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetExecutionHistory : DefaultEsbManagementEndpoint
    {
        private readonly IWebSocketPool _webSocketPool;
        private readonly TimeSpan _waitTimeOut;

        public GetExecutionHistory()
             : this(new WebSocketPool(), TimeSpan.FromMinutes(5))
        {
        }

        public GetExecutionHistory(IWebSocketPool webSocketFactory, TimeSpan waitTimeOut)
        {
            _webSocketPool = webSocketFactory;
            _waitTimeOut = waitTimeOut;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IWebSocketWrapper client = null;
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
                    client = _webSocketPool.Acquire(Config.Auditing.Endpoint).Connect();

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
                        client.OnMessage((msgResponse, socket) =>
                        {
                            response = msgResponse;
                            result.AddRange(serializer.Deserialize<List<ExecutionHistory>>(response));
                            ewh.Set();
                        });
                        client.SendMessage(serializer.Serialize(message));
                        ewh.WaitOne(_waitTimeOut);
                        return serializer.SerializeToBuilder(result);
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
            finally
            {
                if (client != null)
                {
                    _webSocketPool.Release(client);
                }
            }
        }

        public new AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList></DataList>");

        public override string HandlesType() => "GetExecutionHistoryService";
    }
}
