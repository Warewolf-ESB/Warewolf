
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Communication;
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace Dev2.Runtime.WebServer.Hubs
{
    [AuthorizeHub]
    [HubName("resources")]
    public class ResourcesHub : ServerHub
    {
        public void SendMemo(Memo memo)
        {
            var serializedMemo = JsonConvert.SerializeObject(memo);
            Server.SendMemo(serializedMemo, Context.ConnectionId);
        }
    }
}
