using System.Linq;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Utils
{
    public static class ServerUtil
    {
        public static IEnvironmentModel GetLocalhostServer()
        {
            var servers = ServerProvider.Instance.Load();
            var localHost = servers.FirstOrDefault(s => s.IsLocalHost);
            if(localHost != null && localHost.IsConnected)
                return localHost;
            return null;
        }
    }
}
