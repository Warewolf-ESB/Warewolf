#pragma warning disable
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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Driver.Serilog;
using Warewolf.Auditing;
using Warewolf.Logging;
using System.Threading;
using Warewolf.Interfaces.Auditing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetLogDataService : LogDataServiceBase, IEsbManagementEndpoint
    {
        private readonly IWebSocketServerFactory _webSocketServerFactory;
        private readonly IWebSocketFactory _webSocketFactory;

        public GetLogDataService()
        {
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var _client = WebSocketWrapper.Create(Config.Auditing.Endpoint);
            _client.Connect();                  

            Dev2Logger.Info("Get Log Data Service", GlobalConstants.WarewolfInfo);
            var serializer = new Dev2JsonSerializer();
            var result = new List<Audit>();
            var response = "";
            var message = new AuditCommand()
            {
                Type = "LogQuery",
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
                    var items = serializer.Deserialize<dynamic>(response);
                    socket.Close();
                });

                ewh.WaitOne();
                LogDataCache.CurrentResults = result;
                return serializer.SerializeToBuilder(result);
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Get Log Data ServiceError", e, GlobalConstants.WarewolfInfo);
            }
            return serializer.SerializeToBuilder("");
        }

        T GetValue<T>(string key, Dictionary<string, StringBuilder> values)
        {
            var toReturn = default(T);
            if (values.TryGetValue(key, out StringBuilder value))
            {
                var item = value.ToString();
                return (T)Convert.ChangeType(item, typeof(T));
            }
            return toReturn;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "GetLogDataService";
    }

    public static class LogDataCache
    {
        public static IEnumerable<dynamic> CurrentResults { get; set; }
    }
}