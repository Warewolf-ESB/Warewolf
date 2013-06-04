using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace System.Network
{
    public abstract class NetworkServer : NetworkHost
    {
        #region Constructor
        public NetworkServer(string name)
            : base(name)
        {
        }
        #endregion

        #region Socket Handling
        internal void NotifySocketConnected(Listener source, Socket socket)
        {
            IPAddress address = IPAddress.None;
            int port = 0;
            string addressString = null;

            try
            {
                IPEndPoint ipEP = (IPEndPoint)socket.RemoteEndPoint;
                address = ipEP.Address;
                port = ipEP.Port;
                addressString = address.ToString();
            }
            catch
            {
                address = IPAddress.None;
                port = -1;
                addressString = "(error)";
            }


            if (!ValidateSocketConnection(source, socket, address, addressString, port))
            {
                NetworkHelper.ReleaseSocket(ref socket);
                return;
            }

            try
            { 
                ApplySocketOptions(source, socket);
                OnSocketConnectionAccepted(source, socket, address, addressString, port);
            }
            catch { NetworkHelper.ReleaseSocket(ref socket); }
        }

        protected abstract bool ValidateSocketConnection(Listener source, Socket socket, IPAddress address, string addressString, int port);
        protected abstract void ApplySocketOptions(Listener source, Socket socket);
        protected abstract void OnSocketConnectionAccepted(Listener source, Socket socket, IPAddress address, string addressString, int port);
        #endregion

        #region Authentication Handling
        internal sealed override void NotifyDeauthenticated(Connection connection, InboundAuthenticationBroker broker, AuthenticationResponse reason)
        {
            OnLogoutRequired(connection, broker, reason);
        }

        protected abstract void OnLogoutRequired(Connection connection, InboundAuthenticationBroker broker, AuthenticationResponse reason);
        #endregion
    }
}
