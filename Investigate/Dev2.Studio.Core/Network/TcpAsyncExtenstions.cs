using System.Net.Sockets;
using System.Network;

namespace Dev2.Studio.Core.Network
{
    public static class TcpAsyncExtenstions
    {
        #region Release

        public static void Release(this Socket socket)
        {
            NetworkHelper.ReleaseSocket(ref socket);
        }

        #endregion

    }
}
