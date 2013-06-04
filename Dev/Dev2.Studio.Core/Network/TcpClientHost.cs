using System;
using System.Net;
using System.Net.Sockets;
using System.Network;
using Caliburn.Micro;

namespace Dev2.Studio.Core.Network
{
    // PBI 9228: TWR - 2013.04.17 - refactored to test server reconnect heartbeat

    public class TcpClientHost : TcpClientHostBase
    {
        public TcpClientHost(bool isAuxiliary = false)
            : base(isAuxiliary)
        {
        }

        #region Ping

        public override bool Ping(EndPoint endPoint)
        {
            if(endPoint == null)
            {
                return false;
            }

            // If you need to determine the current state of the connection,  make a 
            // nonblocking, zero-byte Send call. If the call returns successfully or 
            // throws a WAEWOULDBLOCK error code (10035), then the socket is still 
            // connected; otherwise, the socket is no longer connected.
            //
            // See http://msdn.microsoft.com/en-us/library/vstudio/system.net.sockets.socket.connected(v=vs.100).aspx
            //

            bool result;
            var socket = CreateSocket();
            try
            {
                // Connect throws exception when blocking is true, if server is not online 
                socket.Blocking = true;
                socket.Connect(endPoint);

                var ping = new byte[1];
                var bytes = new byte[256];

                // Blocks until send returns.
                var bytesSent = socket.Send(ping, 1, SocketFlags.None);

                // MUST wait until data is available!!!
                while(socket.Available == 0)
                {
                }

                // Get reply from the server.
                var byteReceived = socket.Receive(bytes, socket.Available, SocketFlags.None);

                result = bytesSent == byteReceived;
            }
            catch(SocketException e)
            {
                // If the error code is 10035 (WSAEWOULDBLOC) then
                // we are still Connected, but the Send would block
                result = e.NativeErrorCode.Equals(10035);
            }
            catch(Exception)
            {
                result = false;
            }
            finally
            {
                NetworkHelper.ReleaseSocket(ref socket);
            }
            return result;
        }

        #endregion

    }
}
