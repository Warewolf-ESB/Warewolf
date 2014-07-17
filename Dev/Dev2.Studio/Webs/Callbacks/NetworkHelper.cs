using System.Net;

namespace Dev2.Webs.Callbacks
{
    public interface INetworkHelper
    {
        bool HasConnection(string uri);
    }

    public class NetworkHelper : INetworkHelper
    {
        public bool HasConnection(string uri)
        {
            try
            {
                using(var client = new WebClient())
                using(client.OpenRead(uri))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
