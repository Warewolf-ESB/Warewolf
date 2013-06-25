using System.Linq;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Utils
{
    public static class ServerUtil
    {
        public static IServer GetLocalhostServer()
        {
            var servers = ServerProvider.Instance.Load();
            var localHost = servers.FirstOrDefault(s => s.IsLocalHost);
            if(localHost != null && localHost.Environment.IsConnected)
                return localHost;
            return null;
        }
    }
}
