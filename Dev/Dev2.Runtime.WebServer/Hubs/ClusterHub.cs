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

using System.Threading.Tasks;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Dev2.Runtime.WebServer.Hubs
{
    public interface IClusterHub
    {
        bool SetFollowingAllowed();
    }
    // Instances of the Hub class are transient, you can't use them 
    // to maintain state from one method call to the next. Each time 
    // the server receives a method call from a client, a new instance
    // of your Hub class processes the message.
    [AuthorizeHub]
    [HubName("esbpeer")]
    public class ClusterHub : Hub<IClusterHub>
    {
        private readonly IClusterCatalog _clusterCatalog;

        public ClusterHub()
            : this(Server.Instance)
        {
            _clusterCatalog = ClusterCatalog.Instance;
        }

        public ClusterHub(Server server)
        {
            Server = server;
        }

        private Server Server { get; }

        public override Task OnConnected()
        {
            
            return base.OnConnected().ContinueWith((task) =>
            {
                var key = Context.QueryString["key"];
                _clusterCatalog.AddFollower(key);
                Clients.Caller.SetFollowingAllowed();
            });
        }
    }

    class asdf
    {
        public void Tests()
        {
            var c = new ClusterHub();
            
        }
    }
}
