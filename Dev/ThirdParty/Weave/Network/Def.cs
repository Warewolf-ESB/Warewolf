
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

namespace System.Network
{
    public enum NetworkState : byte
    {
        Offline = 0,
        Connecting = 1,
        Online = 2
    }

    [Flags]
    public enum NetworkDirection : byte
    {
        None = 0,
        Inbound = 1,
        Outbound = 2,
        Bidirectional = 3
    }

    [Flags]
    public enum PacketHeaderFlags : byte
    {
        Extended = 16,
        Identifier16 = 32,
        Length16 = 64,
        Length32 = 128
    }

    public enum LoginReply : byte
    {
        Invalid = 0x00,
        InUse = 0x01,
        Blocked = 0x02,
        Valid = 0x04,
        NetworkLockdown = 0x05,
        BFProtected = 0x06,
        Compromised = 0x07,
        ServiceDown = 0x08,
        Logout = 0xFE,
        BadComm = 0xFF
    }

    public interface INetworkOperator
    {
        void Send(Packet p);
    }

    public interface INetworkSerializable
    {
        void Serialize(IByteWriterBase writer);
        void Deserialize(IByteReaderBase reader);
    }

    public struct PacketData
    {
        public int Channel;
        public int Length;
        public byte[] Data;
        public ushort ID;

        public PacketData(int channel, int length, byte[] data, ushort id)
        {
            Channel = channel;
            Length = length;
            Data = data;
            ID = id;
        }
    }

    public sealed class SocketConnectEventArgs : EventArgs
    {
        #region Instance Fields
        private Socket _socket;
        private IPAddress _address;
        private string _addressString;
        private bool _accepted;
        #endregion

        #region Public Properties
        public Socket Socket { get { return _socket; } }
        public IPAddress Address { get { return _address; } }
        public string AddressString { get { return _addressString; } }
        public bool Accepted { get { return _accepted; } set { _accepted = value; } }
        #endregion

        #region Constructor
        public SocketConnectEventArgs(Socket socket, IPAddress address, string addressString)
        {
            _socket = socket;
            _address = address;
            _addressString = addressString;
            _accepted = true;
        }
        #endregion
    }

    public sealed class NetworkStateEventArgs : EventArgs
    {
        #region Instance Fields
        private NetworkState _fromState;
        private NetworkState _toState;
        private bool _isError;
        private string _message;
        #endregion

        #region Public Properties
        public NetworkState FromState { get { return _fromState; } }
        public NetworkState ToState { get { return _toState; } }
        public bool IsError { get { return _isError; } }
        public string Message { get { return _message; } }
        #endregion

        #region Constructors
        public NetworkStateEventArgs(NetworkState fromState, NetworkState toState, bool isError, string message)
        {
            _fromState = fromState;
            _toState = toState;
            _isError = isError;
            _message = message;
        }

        public NetworkStateEventArgs(NetworkState fromState, NetworkState toState)
            : this(fromState, toState, false, "")
        {
        }
        #endregion
    }

    public sealed class LoginStateEventArgs : EventArgs
    {
        #region Instance Fields
        private AuthenticationResponse _reply;
        private bool _loggedIn;
        private bool _isError;
        private bool _expectDisconnect;
        private string _message;
        #endregion

        #region Public Properties
        public AuthenticationResponse Reply { get { return _reply; } }
        public bool LoggedIn { get { return _loggedIn; } }
        public bool IsError { get { return _isError; } }
        public bool ExpectDisconnect { get { return _expectDisconnect; } }
        public string Message { get { return _message; } }
        #endregion

        #region Constructors
        public LoginStateEventArgs(AuthenticationResponse reply, bool loggedIn, bool isError, string message)
        {
            _reply = reply;
            _loggedIn = loggedIn;
            _isError = isError;
            _message = message;
        }

        public LoginStateEventArgs(AuthenticationResponse reply, bool expectDisconnect)
        {
            _reply = reply;
            _loggedIn = (_reply == AuthenticationResponse.Success);
            _isError = (!_loggedIn && (_reply != AuthenticationResponse.Logout));
            _expectDisconnect = expectDisconnect;

            switch (_reply)
            {
                case AuthenticationResponse.Unspecified: _message = "Login attempt failed due to bad communication with the server."; break;
                case AuthenticationResponse.MachineBan: _message = "Due to a terms of service violation, this computer has been banned and cannot be used to connect to the server, regardless of the account used. Please contact a member of staff for more information."; break;
                case AuthenticationResponse.InUse: _message = "The login credentials you provided are already in use."; break;
                case AuthenticationResponse.InvalidCredentials: _message = "The login credentials you provided are invalid."; break;
                case AuthenticationResponse.NetworkLockdown: _message = "The server is undergoing maintenance, please try again later."; break;
                case AuthenticationResponse.GeneralBan: _message = "That account has been blocked by the server, please contact a member of staff for more information."; break;
                case AuthenticationResponse.Logout: _message = "You have logged out from the server."; break;
                case AuthenticationResponse.Success: _message = "You have logged in to the server."; break;
            }
        }
        #endregion
    }

    public sealed class NetworkAsyncEventArgs : SocketAsyncEventArgs
    {
        internal int Index;
        internal byte[] BufferInternal;

        public NetworkAsyncEventArgs(int capacity)
        {
            BufferInternal = new byte[capacity];
        }
    }
        
    public delegate void SocketEventHandler(NetworkHost host, SocketConnectEventArgs args);
    public delegate void ConnectionEventHandler(NetworkHost host, Connection connection);
    public delegate void NetworkStateEventHandler(NetworkHost client, NetworkStateEventArgs args);
    public delegate void LoginStateEventHandler(NetworkHost client, LoginStateEventArgs args);
    public delegate void EnvoyReturnedEventHandler(Envoy envoy, bool successful, string sourceHostNameOrAddress, IPHostEntry resolvedHostEntry);
    public delegate void NetworkContextEventHandler<T>(TCPServer<T> server, T context) where T: NetworkContext, new();
    public delegate void PacketEventHandler(INetworkOperator op, ByteBuffer reader);
    public delegate void PacketEventHandler<T>(INetworkOperator op, T context, ByteBuffer reader) where T: NetworkContext;
}
