
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace System.Network
{
    public class TCPClient : NetworkHost
    {
        #region Instance Fields
        private NetworkStateEventHandler _onNetworkStateChanged;
        private LoginStateEventHandler _onLoginStateChanged;
        protected AsyncPacketHandlerCollection[] _channels;
        protected AsyncExtensionHandler[] _extensions;

        private volatile bool _loggedIn;
        private volatile NetworkState _networkState;
        private volatile Connection _primaryConnection;
        #endregion

        #region Public Properties
        public AsyncPacketHandlerCollection[] Channels { get { return _channels; } }
        public AsyncExtensionHandler[] Extensions { get { return _extensions; } }
        public NetworkState NetworkState { get { return (NetworkState)_networkState; } }
        public bool LoggedIn { get { return _loggedIn; } }
        #endregion

        #region Events
        public event NetworkStateEventHandler NetworkStateChanged { add { if (!_disposing) _onNetworkStateChanged += value; } remove { if (!_disposing) _onNetworkStateChanged -= value; } }
        public event LoginStateEventHandler LoginStateChanged { add { if (!_disposing) _onLoginStateChanged += value; } remove { if (!_disposing) _onLoginStateChanged -= value; } }
        #endregion

        #region Constructor
        public TCPClient(string name)
            : base(name)
        {
            _channels = new AsyncPacketHandlerCollection[16];
            _extensions = new AsyncExtensionHandler[16];
        }
        #endregion

        #region [Get/Set] Handling
        public void SetNetworkState(NetworkState state)
        {
            SetNetworkState(state, false, "");
        }

        public void SetNetworkState(NetworkState state, string message)
        {
            SetNetworkState(state, true, message);
        }

        public void SetNetworkState(NetworkState state, bool isError, string message)
        {
            NetworkState oldState = _networkState;
            _networkState = state;

            if (_onNetworkStateChanged != null) _onNetworkStateChanged(this, new NetworkStateEventArgs(oldState, state, isError, message));

            if (state == NetworkState.Offline)
            {
                if (_loggedIn)
                {
                    _loggedIn = false;
                    if (_onLoginStateChanged != null) _onLoginStateChanged(this, new LoginStateEventArgs(AuthenticationResponse.Logout, false));
                }
            }
        }
        #endregion

        #region [Login/Logout] Handling
        public void Login(OutboundAuthenticationBroker broker)
        {
            Connection primaryConnection = _primaryConnection;

            if (primaryConnection == null || !primaryConnection.Alive)
            {
                if (_onLoginStateChanged != null) _onLoginStateChanged(this, new LoginStateEventArgs(AuthenticationResponse.Unspecified, false, true, "You must be connected to the server in order to login."));
                _loggedIn = false;
                return;
            }

            if (_loggedIn) return;
            broker.BeginAuthentication(primaryConnection);
        }

        public void Logout()
        {
            Connection primaryConnection = _primaryConnection;

            if (primaryConnection == null || !primaryConnection.Alive)
            {
                if (_onLoginStateChanged != null) _onLoginStateChanged(this, new LoginStateEventArgs(AuthenticationResponse.Unspecified, false, true, "You must be connected to the server in order to logout."));
                _loggedIn = false;
                return;
            }

            if (!_loggedIn) return;
        }

        protected override void OnLoginSuccess(Connection connection, NetworkAccount account, AuthenticationBroker broker)
        {
            _loggedIn = true;
            if (_onLoginStateChanged != null) _onLoginStateChanged(this, new LoginStateEventArgs(AuthenticationResponse.Success, false));
        }

        protected override void OnLoginFailed(Connection connection, OutboundAuthenticationBroker broker, AuthenticationResponse reason, bool expectDisconnect)
        {
            _loggedIn = false;
            if (_onLoginStateChanged != null) _onLoginStateChanged(this, new LoginStateEventArgs(reason, expectDisconnect));
        }
        #endregion

        #region Outgoing Connection Handling
        public void Connect(string hostNameOrAddress, int port)
        {
            if (_networkState != NetworkState.Offline) return;
            SetNetworkState(NetworkState.Connecting);

            IPAddress potentialAddress = null;

            if (IPAddress.TryParse(hostNameOrAddress, out potentialAddress))
            {
                Socket s = null;

                try
                {
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    s.BeginConnect(new IPEndPoint(potentialAddress, port), new AsyncCallback(OnEndConnect), s);
                }
                catch (Exception e)
                {
                    try { s.Shutdown(SocketShutdown.Both); }
                    catch { }

                    try { s.Close(); }
                    catch { }

                    SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : " + e.Message);
                }
            }
            else
            {
                Envoy envoy = new Envoy(hostNameOrAddress, port);
                envoy.EnvoyReturned += new EnvoyReturnedEventHandler(OnEnvoyReturned);
                Exception result = envoy.BeginResolution();

                if (result != null)
                {
                    envoy.EnvoyReturned -= new EnvoyReturnedEventHandler(OnEnvoyReturned);
                    envoy.Dispose();
                    SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : " + result.Message);
                }
            }
        }

        private void OnEnvoyReturned(Envoy envoy, bool successful, string sourceHostNameOrAddress, IPHostEntry resolvedHostEntry)
        {
            int port = envoy.Port;
            envoy.EnvoyReturned -= new EnvoyReturnedEventHandler(OnEnvoyReturned);
            envoy.Dispose();

            if (!successful)
                SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : The specified server could not be found.");
            else
            {
                Socket s = null;

                try
                {
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress address = null;

                    for (int i = 0; i < resolvedHostEntry.AddressList.Length; i++)
                        if (resolvedHostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                        {
                            address = resolvedHostEntry.AddressList[i];
                            break;
                        }

                    if (address == null)
                    {
                        SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : The specified server could not be found.");
                        try { s.Shutdown(SocketShutdown.Both); }
                        catch { }

                        try { s.Close(); }
                        catch { }
                    }
                    else s.BeginConnect(new IPEndPoint(address, port), new AsyncCallback(OnEndConnect), s);
                }
                catch (Exception e)
                {
                    try { s.Shutdown(SocketShutdown.Both); }
                    catch { }

                    try { s.Close(); }
                    catch { }

                    SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : " + e.Message);
                }
            }
        }

        private void OnEndConnect(IAsyncResult asyncResult)
        {
            Socket socket = null;

            try
            {
                socket = (Socket)asyncResult.AsyncState;
                socket.EndConnect(asyncResult);
            }
            catch (Exception e)
            {
                try { socket.Shutdown(SocketShutdown.Both); }
                catch { }

                try { socket.Close(); }
                catch { }

                SetNetworkState(NetworkState.Offline, false, "A connection to the server could not be established. Reason : " + e.Message);
                return;
            }

            ConstructConnection(socket);
        }


        private void ConstructConnection(Socket socket)
        {
            if (_disposing)
            {
                NetworkHelper.ReleaseSocket(ref socket);
                return;
            }

            bool error = false;

            try
            {
                Connection con = new AsyncConnection(this, socket, NetworkDirection.Outbound);
                con.Start();

                if (con.Alive)
                {
                    if (_primaryConnection == null)
                    {
                        _primaryConnection = con;
                        SetNetworkState(NetworkState.Online);
                    }
                    else if (NetworkState == NetworkState.Offline)
                        con.Dispose();
                }
                else
                {
                    con.Dispose();
                    if (_primaryConnection == null) error = true;
                }
            }
            catch { NetworkHelper.ReleaseSocket(ref socket); }

            if (error) Disconnect(NetworkDirection.Bidirectional);
        }

        protected override void OnConnectionDisposed(Connection connection)
        {
            if (connection == _primaryConnection)
            {
                _primaryConnection = null;
                Disconnect(NetworkDirection.Bidirectional);
                SetNetworkState(NetworkState.Offline);
            }
        }
        #endregion

        #region Outgoing Data Handling
        public void Send(byte[] data, int index, int length)
        {
            if (_primaryConnection == null) return;
            _primaryConnection.Send(data, index, length);
        }

        public void Send(Packet p)
        {
            if (_primaryConnection == null) return;
            _primaryConnection.Send(p);
        }

        public void SendExtended(Packet p, byte[] extension, int extensionLength)
        {
            if (_primaryConnection == null) return;
            _primaryConnection.SendExtended(p, extension, extensionLength);
        }

        public void SendRange(params Packet[] p)
        {
            if (_primaryConnection == null) return;
            _primaryConnection.SendRange(p);
        }
        #endregion

        #region Incoming Data Handling
        protected internal override bool ValidateChannel(int channel)
        {
            return _channels[channel] != null;
        }

        protected internal override PacketTemplate AcquirePacketTemplate(int channel, ushort packetID)
        {
            AsyncPacketHandler handler = _channels[channel].Handlers[packetID];
            return handler == null ? null : handler.Template;
        }

        protected internal override void Dispatch(Connection connection, int channel, ushort packetID, int dataLength, byte[] data, bool compressed)
        {
            _channels[channel].Dispatch(packetID, connection, new ByteBuffer(data, dataLength));
        }
        #endregion

        #region Extension Handling
        protected internal override PacketTemplate AcquireExtensionTemplate(int extension)
        {
            AsyncExtensionHandler handler = _extensions[extension];
            return handler == null ? null : handler.Template;
        }

        protected internal override void Dispatch(Connection connection, PacketData extension, PacketData packet, bool isResponse)
        {
            if (isResponse) _extensions[extension.Channel].Response(_channels[packet.Channel], connection, extension, packet);
            else _extensions[extension.Channel].Dispatch(_channels[packet.Channel], connection, extension, packet);
        }
        #endregion

        #region Disposal Handling
        protected override void OnDisposed(bool disposing)
        {
            _primaryConnection = null;
            _onNetworkStateChanged = null;
            _onLoginStateChanged = null;
        }
        #endregion
    }
}
