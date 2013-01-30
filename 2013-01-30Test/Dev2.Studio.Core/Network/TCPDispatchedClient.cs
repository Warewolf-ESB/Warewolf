using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Network;
using System.Threading;
using System.Windows.Threading;
using Dev2.Diagnostics;
using Dev2.Network;
using Dev2.Network.Messages;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Network;

namespace Dev2.Studio.Core
{
    public class TCPDispatchedClient : NetworkHost, INetworkOperator
    {
        #region Constants
        private const long True = 1;
        private const long False = 0;
        #endregion

        #region Instance Fields
        private NetworkStateEventHandler _onNetworkStateChangedCore;
        private NetworkStateEventHandler _onNetworkStateChanged;
        private LoginStateEventHandler _onLoginStateChangedCore;
        private LoginStateEventHandler _onLoginStateChanged;
        protected AsyncPacketHandlerCollection[] _channels;
        protected AsyncExtensionHandler[] _extensions;

        private string _hostNameOrAddress;
        private int _port;
        private bool _isAuxiliary;

        private volatile bool _loggedIn;
        private volatile NetworkState _networkState;
        private volatile Connection _primaryConnection;

        private DispatcherFrame _frame;
        private NetworkStateEventArgs _connectFrameArgs;
        private LoginStateEventArgs _loginFrameArgs;
        private object _frameLock;
        private long _withinConnect;
        private Guid _serverID;
        private Guid _accountID;
        private ServerMessaging _serverMessaging;

        private int _commandSerial;
        private object _commandLock;
        private Dictionary<int, ExecuteCommandToken> _pendingCommands;
        #endregion

        #region Public Properties
        public AsyncPacketHandlerCollection[] Channels { get { return _channels; } }
        public AsyncExtensionHandler[] Extensions { get { return _extensions; } }
        public NetworkState NetworkState { get { return _networkState; } }
        public bool LoggedIn { get { return _loggedIn; } }
        public bool IsAuxiliary { get { return _isAuxiliary; } }

        public Guid ServerID { get { return _serverID; } }
        public Guid AccountID { get { return _accountID; } }
        #endregion

        #region Events
        public event NetworkStateEventHandler NetworkStateChanged { add { if(!_disposing) _onNetworkStateChanged += value; } remove { if(!_disposing) _onNetworkStateChanged -= value; } }
        public event LoginStateEventHandler LoginStateChanged { add { if(!_disposing) _onLoginStateChanged += value; } remove { if(!_disposing) _onLoginStateChanged -= value; } }
        #endregion

        #region Constructor
        public TCPDispatchedClient(string name)
            : this(name, false)
        {
        }

        private TCPDispatchedClient(string name, bool isAuxiliary)
            : base(name)
        {
            _frameLock = new object();
            _commandLock = new object();
            _channels = new AsyncPacketHandlerCollection[16];
            _extensions = new AsyncExtensionHandler[16];
            _isAuxiliary = isAuxiliary;

            _channels[0] = new AsyncPacketHandlerCollection();
            _channels[1] = new AsyncPacketHandlerCollection();

            _serverMessaging = new ServerMessaging();

            if(!isAuxiliary)
            {
                _channels[0].Register(0, PacketTemplates.Client_OnAuxiliaryConnectionReply, new PacketEventHandler(OnAuxiliaryConnectionReply));
                _channels[0].Register(1, PacketTemplates.Client_OnDebugWriterWrite, new PacketEventHandler(OnDebugWriterWrite));
                
                _channels[1].Register(0, PacketTemplates.Both_OnNetworkMessageRevieved, new PacketEventHandler(OnNetworkMessageRevieved));
            }

            _channels[15] = new AsyncPacketHandlerCollection();
            _channels[15].Register(0, InternalTemplates.Client_LogoutReceived, new PacketEventHandler(OnLogoutReceived));
            _channels[15].Register(1, InternalTemplates.Client_OnExecuteStringCommandReceived, new PacketEventHandler(OnExecuteCommandReceived));
            _channels[15].Register(2, InternalTemplates.Client_OnExecuteBinaryCommandReceived, new PacketEventHandler(OnExecuteCommandReceived));

            _channels[15].Register(3, InternalTemplates.Client_OnClientDetailsReceived, new PacketEventHandler(OnClientDetailsReceived));
            _pendingCommands = new Dictionary<int, ExecuteCommandToken>();
        }
        #endregion

        #region [Get/Set] Handling
        private void SetNetworkState(NetworkState state)
        {
            SetNetworkState(state, false, "");
        }

        private void SetNetworkState(NetworkState state, string message)
        {
            SetNetworkState(state, true, message);
        }

        private void SetNetworkState(NetworkState state, bool isError, string message)
        {
            NetworkState oldState = _networkState;
            _networkState = state;

            RaiseNetworkStateChanged(new NetworkStateEventArgs(oldState, state, isError, message));
            if(state == NetworkState.Offline && _loggedIn)
                RaiseLoginStateChanged(new LoginStateEventArgs(AuthenticationResponse.Logout, false));
        }
        #endregion

        #region DebugWriter Support

        private readonly object _writerGuard = new object();
        private readonly Dictionary<Guid, IDebugWriter> _debugWriters = new Dictionary<Guid, IDebugWriter>();

        public void AddDebugWriter(IDebugWriter writer)
        {
            if(Disposed)
                throw new ObjectDisposedException("TCPDispatchedClient");
            if(_isAuxiliary)
                throw new InvalidOperationException("Auxiliary clients cannot add debug writers.");
            if(_networkState != NetworkState.Online)
                throw new InvalidOperationException("TCPDispatchedClient is not online.");
            if(!_loggedIn)
                throw new InvalidOperationException("TCPDispatchedClient is not logged in.");
            if(writer == null)
                throw new ArgumentNullException("writer");
            Guid id = _accountID;

            lock(_writerGuard)
            {
                if(!_debugWriters.ContainsKey(id))
                {
                    _debugWriters.Add(id, writer);
                }
                else
                {
                    return;
                }
            }

            Packet p = new Packet(PacketTemplates.Server_OnDebugWriterAddition);
            Send(p);
        }

        public void RemoveDebugWriter(IDebugWriter writer)
        {
            if(writer == null)
                throw new ArgumentNullException("writer");
            Guid id = _accountID;

            lock(_writerGuard)
            {
                if(_debugWriters.Remove(id))
                {
                    Packet p = new Packet(PacketTemplates.Server_OnDebugWriterSubtraction);
                    Send(p);
                }
            }
        }

        public void RemoveDebugWriter(Guid writerID)
        {
            lock(_writerGuard)
            {
                if(_debugWriters.Remove(writerID))
                {
                    Packet p = new Packet(PacketTemplates.Server_OnDebugWriterSubtraction);
                    Send(p);
                }
            }
        }

        private void OnDebugWriterWrite(INetworkOperator op, ByteBuffer reader)
        {
            var debugState = new DebugState(reader);
            IDebugWriter writer;

            lock(_writerGuard)
            {
                _debugWriters.TryGetValue(_accountID, out writer);
            }

            if(writer != null)
            {
                writer.Write(debugState);
            }
        }

        #endregion

        #region NetworkMessage Handling

        private void OnNetworkMessageRevieved(INetworkOperator op, ByteBuffer reader)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException("TCPDispatchedClient");
            }

            if (_isAuxiliary)
            {
                throw new InvalidOperationException("Auxiliary clients don't support network messages.");
            }

            if (_networkState != NetworkState.Online)
            {
                throw new InvalidOperationException("TCPDispatchedClient is not online.");
            }

            if (!_loggedIn)
            {
                throw new InvalidOperationException("TCPDispatchedClient is not logged in.");
            }

            try
            {
                INetworkMessage message = _serverMessaging.MessageBroker.Recieve(reader);
                _serverMessaging.MessageAggregator.Publish(message, new StudioNetworkChannelContext(op, AccountID, ServerID));
            }
            catch (Exception e)
            {
                TraceWriter.WriteTrace("An error occured while trying to interpret network message from the server. " + e.Message);
            }
        }

        #endregion NetworkMessage Handling

        #region Auxiliary Connection Handling
        public TCPDispatchedClient CreateAuxiliaryClient()
        {
            if(Disposed)
                throw new ObjectDisposedException("TCPDispatchedClient");
            if(_isAuxiliary)
                throw new InvalidOperationException("Auxiliary clients cannot create nested auxiliary clients.");
            if(_networkState != NetworkState.Online)
                throw new InvalidOperationException("TCPDispatchedClient is not online.");
            if(!_loggedIn)
                throw new InvalidOperationException("TCPDispatchedClient is not logged in.");


            int commandSerial = Interlocked.Increment(ref _commandSerial);
            Packet p = new Packet(PacketTemplates.Server_OnAuxiliaryConnectionRequested);
            p.Write(commandSerial);

            ExecuteCommandToken token = new ExecuteCommandToken(commandSerial);

            lock(_commandLock)
                _pendingCommands.Add(commandSerial, token);

            _primaryConnection.Send(p);
            ByteBuffer result = token.Execute();

            if(result != null)
            {
                Guid connectionAuth = result.ReadGuid();
                TCPDispatchedClient client = new TCPDispatchedClient(Name + " (Auxiliary)", true);

                NetworkStateEventArgs netArgs = client.Connect(_hostNameOrAddress, _port + 20000);

                if(netArgs.ToState == System.Network.NetworkState.Online)
                {
                    byte[] raw = connectionAuth.ToByteArray();
                    string username = "Auth";
                    string password = "P";

                    for(int i = 0; i < 8; i++)
                    {
                        username += raw[i].ToString();
                        password += raw[i + 8].ToString();
                    }

                    LoginStateEventArgs loginArgs = client.Login(new TWALoginBroker(username, password));
                    if(loginArgs.LoggedIn)
                        return client;
                }

                client.Dispose();
                return null;
            }

            return null;
        }

        private void OnAuxiliaryConnectionReply(INetworkOperator op, ByteBuffer reader)
        {
            if(_isAuxiliary)
                throw new InvalidOperationException("Auxiliary connections cannot receive this packet.");
            int commandSerial = reader.ReadInt32();
            ExecuteCommandToken token = null;

            lock(_commandLock)
            {
                if(_pendingCommands.TryGetValue(commandSerial, out token))
                    _pendingCommands.Remove(commandSerial);
            }

            if(token != null)
            {
                token.Complete(reader);
            }
        }
        #endregion

        #region Event Handling
        private void OnClientDetailsReceived(INetworkOperator op, ByteBuffer reader)
        {
            _serverID = reader.ReadGuid();
            _accountID = reader.ReadGuid();
        }

        private void RaiseNetworkStateChanged(NetworkStateEventArgs args)
        {
            if(args.ToState != NetworkState.Online)
                CancelPending();

            if(_onNetworkStateChangedCore != null)
            {
                _onNetworkStateChangedCore(this, args);
            }

            if(_onNetworkStateChanged != null)
            {
                _onNetworkStateChanged(this, args);
            }
        }

        private void RaiseLoginStateChanged(LoginStateEventArgs args)
        {
            if(!args.LoggedIn)
            {
                CancelPending();
                //_accountID = Guid.Empty;
            }

            _loggedIn = args.LoggedIn;

            if(_onLoginStateChangedCore != null)
            {
                _onLoginStateChangedCore(this, args);
            }

            if(_onLoginStateChanged != null)
            {
                _onLoginStateChanged(this, args);
            }
        }
        #endregion

        #region [Login/Logout] Handling
        public LoginStateEventArgs Login(string username, string password)
        {
            password = "abc123xyz";
            return Login(new TWALoginBroker(username, password));
        }

        public LoginStateEventArgs Login(OutboundAuthenticationBroker broker)
        {
            if(Disposed)
                throw new ObjectDisposedException("TCPDispatchedClient");
            if(broker == null)
                throw new ArgumentNullException("broker");
            if(Interlocked.CompareExchange(ref _withinConnect, True, False) != False)
                throw new InvalidOperationException("TCPDispatchedClient does not support concurrent connect/login operations.");

            if(_networkState != NetworkState.Online)
            {
                if(Interlocked.CompareExchange(ref _withinConnect, False, True) != True)
                    throw new InvalidOperationException("Unexpected connection state encountered.");
                throw new InvalidOperationException("TCPDispatchedClient is not online.");
            }

            if(_loggedIn)
            {
                if(Interlocked.CompareExchange(ref _withinConnect, False, True) != True)
                    throw new InvalidOperationException("Unexpected connection state encountered.");
                throw new InvalidOperationException("TCPDispatchedClient is already logged in.");
            }

            LoginStateEventArgs result = null;
            NetworkStateEventArgs networkResult = null;

            try
            {
                DispatcherFrame frame = null;

                lock(_frameLock)
                {
                    _loginFrameArgs = null;
                    _connectFrameArgs = null;
                    _frame = frame = new DispatcherFrame();
                    _onLoginStateChangedCore = new LoginStateEventHandler(LoginOperation_LoginStateChangedCore);
                    _onNetworkStateChangedCore = new NetworkStateEventHandler(LoginOperation_NetworkStateChangedCore);
                }

                LoginImpl(broker);
                Dispatcher.PushFrame(frame);
            }
            finally
            {
                DispatcherFrame frame = null;

                lock(_frameLock)
                {
                    result = _loginFrameArgs;
                    networkResult = _connectFrameArgs;
                    frame = _frame;

                    _loginFrameArgs = null;
                    _connectFrameArgs = null;
                    _frame = null;

                    _onLoginStateChangedCore = null;
                    _onNetworkStateChangedCore = null;
                }

                if(frame != null)
                    frame.Continue = false;
                if(Interlocked.CompareExchange(ref _withinConnect, False, True) != True)
                    throw new InvalidOperationException("Unexpected connection state encountered.");
            }

            if(result == null)
            {
                if(networkResult != null)
                    result = new LoginStateEventArgs(AuthenticationResponse.Unspecified, false);
                else
                {
                    result = new LoginStateEventArgs(AuthenticationResponse.Unspecified, false);
                }
            }

            return result;
        }

        private void LoginImpl(OutboundAuthenticationBroker broker)
        {
            if(_loggedIn)
                return;
            broker.BeginAuthentication(_primaryConnection);
        }

        public LoginStateEventArgs Logout()
        {
            if(Disposed)
                throw new ObjectDisposedException("TCPDispatchedClient");
            if(Interlocked.CompareExchange(ref _withinConnect, True, False) != False)
                throw new InvalidOperationException("TCPDispatchedClient does not support concurrent connect/login operations.");

            if(_networkState != NetworkState.Online)
            {
                if(Interlocked.CompareExchange(ref _withinConnect, False, True) != True)
                    throw new InvalidOperationException("Unexpected connection state encountered.");
                throw new InvalidOperationException("TCPDispatchedClient is not online.");
            }

            if(!_loggedIn)
            {
                if(Interlocked.CompareExchange(ref _withinConnect, False, True) != True)
                    throw new InvalidOperationException("Unexpected connection state encountered.");
                throw new InvalidOperationException("TCPDispatchedClient is not logged in.");
            }

            LoginStateEventArgs result = null;
            NetworkStateEventArgs networkResult = null;

            try
            {
                DispatcherFrame frame = null;

                lock(_frameLock)
                {
                    _loginFrameArgs = null;
                    _connectFrameArgs = null;
                    _frame = frame = new DispatcherFrame();
                    _onLoginStateChangedCore = new LoginStateEventHandler(LoginOperation_LoginStateChangedCore);
                    _onNetworkStateChangedCore = new NetworkStateEventHandler(LoginOperation_NetworkStateChangedCore);
                }

                LogoutImpl();
                Dispatcher.PushFrame(frame);
            }
            finally
            {
                DispatcherFrame frame = null;

                lock(_frameLock)
                {
                    result = _loginFrameArgs;
                    networkResult = _connectFrameArgs;
                    frame = _frame;

                    _loginFrameArgs = null;
                    _connectFrameArgs = null;
                    _frame = null;

                    _onLoginStateChangedCore = null;
                    _onNetworkStateChangedCore = null;
                }

                if(frame != null)
                    frame.Continue = false;
                if(Interlocked.CompareExchange(ref _withinConnect, False, True) != True)
                    throw new InvalidOperationException("Unexpected connection state encountered.");
            }

            if(result == null)
            {
                if(networkResult != null)
                    result = new LoginStateEventArgs(AuthenticationResponse.Logout, false);
                else
                {
                    result = new LoginStateEventArgs(AuthenticationResponse.Unspecified, false);
                }
            }

            return result;
        }

        private void LogoutImpl()
        {
            if(!_loggedIn)
                return;

            Packet p = new Packet(InternalTemplates.Server_LogoutReceived);
            Send(p);
        }

        private void LoginOperation_NetworkStateChangedCore(NetworkHost client, NetworkStateEventArgs args)
        {
            DispatcherFrame frame = null;

            if(args.ToState == NetworkState.Offline)
            {
                lock(_frameLock)
                {
                    if(_frame != null && _connectFrameArgs == null)
                    {
                        _connectFrameArgs = args;

                        if(_loginFrameArgs == null)
                        {
                            frame = _frame;
                            _frame = null;
                        }
                    }
                }
            }

            if(frame != null)
                frame.Continue = false;
        }

        private void LoginOperation_LoginStateChangedCore(NetworkHost client, LoginStateEventArgs args)
        {
            DispatcherFrame frame = null;

            lock(_frameLock)
            {
                if(_frame != null && _loginFrameArgs == null)
                {
                    _onNetworkStateChangedCore = null;
                    _loginFrameArgs = args;
                    frame = _frame;
                    _frame = null;
                }
            }

            if(frame != null)
                frame.Continue = false;
        }
        #endregion

        #region ExecuteCommand Handling
        public string ExecuteCommand(string payload)
        {
            if(Disposed)
                throw new ObjectDisposedException("TCPDispatchedClient");
            if(payload == null)
                throw new ArgumentNullException("payload");
            if(_networkState != NetworkState.Online)
                throw new InvalidOperationException("TCPDispatchedClient is not online.");
            if(!_loggedIn)
                throw new InvalidOperationException("TCPDispatchedClient is not logged in.");

            int commandSerial = Interlocked.Increment(ref _commandSerial);
            Packet p = new Packet(InternalTemplates.Server_OnExecuteStringCommandReceived);

            p.Write(commandSerial);
            p.Write(payload);

            ExecuteCommandToken token = new ExecuteCommandToken(commandSerial);

            lock(_commandLock)
                _pendingCommands.Add(commandSerial, token);

            _primaryConnection.Send(p);
            ByteBuffer reader = token.Execute();

            string result = null;
            if(reader != null)
                result = reader.ReadString();
            return result;
        }

        public ByteBuffer ExecuteCommand(ByteBuffer payload)
        {
            if(payload == null)
                throw new ArgumentNullException("payload");
            return ExecuteCommand(payload, 0, (int)payload.Length);
        }

        public ByteBuffer ExecuteCommand(ByteBuffer payload, int index, int count)
        {
            if(Disposed)
                throw new ObjectDisposedException("TCPDispatchedClient");
            if(payload == null)
                throw new ArgumentNullException("payload");
            if(_networkState != NetworkState.Online)
                throw new InvalidOperationException("TCPDispatchedClient is not online.");
            if(!_loggedIn)
                throw new InvalidOperationException("TCPDispatchedClient is not logged in.");

            int commandSerial = Interlocked.Increment(ref _commandSerial);
            Packet p = new Packet(InternalTemplates.Server_OnExecuteBinaryCommandReceived);

            p.Write(commandSerial);
            p.Write(payload.Data, index, count);

            ExecuteCommandToken token = new ExecuteCommandToken(commandSerial);

            lock(_commandLock)
                _pendingCommands.Add(commandSerial, token);

            _primaryConnection.Send(p);
            ByteBuffer result = token.Execute();

            return result;
        }

        private void OnExecuteCommandReceived(INetworkOperator op, ByteBuffer reader)
        {
            int commandSerial = reader.ReadInt32();
            ExecuteCommandToken token = null;

            lock(_commandLock)
            {
                if(_pendingCommands.TryGetValue(commandSerial, out token))
                    _pendingCommands.Remove(commandSerial);
            }

            if(token != null)
            {
                token.Complete(reader);
            }
        }

        private void CancelPending()
        {
            Dictionary<int, ExecuteCommandToken> pending;

            lock(_commandLock)
            {
                pending = _pendingCommands;
                _pendingCommands = new Dictionary<int, ExecuteCommandToken>();
            }

            foreach(KeyValuePair<int, ExecuteCommandToken> kvp in pending)
            {
                kvp.Value.Cancel();
            }
        }
        #endregion

        #region Notification Handling
        protected override void OnLoginSuccess(Connection connection, NetworkAccount account, AuthenticationBroker broker)
        {
            RaiseLoginStateChanged(new LoginStateEventArgs(AuthenticationResponse.Success, false));
        }

        protected override void OnLoginFailed(Connection connection, OutboundAuthenticationBroker broker, AuthenticationResponse reason, bool expectDisconnect)
        {
            if(reason == AuthenticationResponse.Success)
                throw new InvalidOperationException();
            RaiseLoginStateChanged(new LoginStateEventArgs(reason, expectDisconnect));
        }

        private void OnLogoutReceived(INetworkOperator op, ByteBuffer reader)
        {
            if(_primaryConnection != null && _primaryConnection.Alive)
            {
                _primaryConnection.Crypt = NullCryptProvider.Singleton;
                _primaryConnection.Assembler = null;
                _primaryConnection.AuthBroker = null;
            }

            AuthenticationResponse reason = (AuthenticationResponse)reader.ReadByte();
            RaiseLoginStateChanged(new LoginStateEventArgs(reason, false));
        }
        #endregion

        #region Outgoing Connection Handling
        public NetworkStateEventArgs Connect(string hostNameOrAddress, int port)
        {
            if(Disposed)
                throw new ObjectDisposedException("TCPDispatchedClient");
            if(Interlocked.CompareExchange(ref _withinConnect, True, False) != False)
                throw new InvalidOperationException("TCPDispatchedClient does not support concurrent connect/login operations.");

            if(_networkState != NetworkState.Offline)
            {
                if(Interlocked.CompareExchange(ref _withinConnect, False, True) != True)
                    throw new InvalidOperationException("Unexpected connection state encountered.");
                throw new InvalidOperationException("TCPDispatchedClient is not offline.");
            }

            _hostNameOrAddress = hostNameOrAddress;
            _port = port;
            NetworkStateEventArgs result = null;

            try
            {
                DispatcherFrame frame = null;

                lock(_frameLock)
                {
                    _connectFrameArgs = null;
                    _loginFrameArgs = null;
                    _frame = frame = new DispatcherFrame();
                    _onNetworkStateChangedCore = new NetworkStateEventHandler(ConnectOperation_NetworkStateChangedCore);
                }

                ConnectImpl(hostNameOrAddress, port);
                Dispatcher.PushFrame(frame);
            }
            finally
            {
                DispatcherFrame frame = null;

                lock(_frameLock)
                {
                    result = _connectFrameArgs;
                    frame = _frame;

                    _connectFrameArgs = null;
                    _frame = null;

                    _onNetworkStateChangedCore = null;
                }

                if(frame != null)
                    frame.Continue = false;
                if(Interlocked.CompareExchange(ref _withinConnect, False, True) != True)
                    throw new InvalidOperationException("Unexpected connection state encountered.");
            }

            return result;
        }

        private void ConnectImpl(string hostNameOrAddress, int port)
        {
            if(_networkState != NetworkState.Offline)
                return;
            SetNetworkState(NetworkState.Connecting);

            IPAddress potentialAddress = null;

            if(IPAddress.TryParse(hostNameOrAddress, out potentialAddress))
            {
                Socket s = null;

                try
                {
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    s.BeginConnect(new IPEndPoint(potentialAddress, port), new AsyncCallback(OnEndConnect), s);
                }
                catch(Exception e)
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

                if(result != null)
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

            if(!successful)
                SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : The specified server could not be found.");
            else
            {
                Socket s = null;

                try
                {
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress address = null;

                    for(int i = 0; i < resolvedHostEntry.AddressList.Length; i++)
                        if(resolvedHostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                        {
                            address = resolvedHostEntry.AddressList[i];
                            break;
                        }

                    if(address == null)
                    {
                        try { s.Shutdown(SocketShutdown.Both); }
                        catch { }

                        try { s.Close(); }
                        catch { }

                        SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : The specified server could not be found.");
                    }
                    else
                        s.BeginConnect(new IPEndPoint(address, port), new AsyncCallback(OnEndConnect), s);
                }
                catch(Exception e)
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
            catch(Exception e)
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
            if(_disposing)
            {
                NetworkHelper.ReleaseSocket(ref socket);
                return;
            }

            bool error = false;

            try
            {
                Connection con = new AsyncConnection(this, socket, NetworkDirection.Outbound);
                con.Start();

                if(con.Alive)
                {
                    if(_primaryConnection == null)
                    {
                        _primaryConnection = con;
                        SetNetworkState(NetworkState.Online);
                    }
                    else if(NetworkState == NetworkState.Offline)
                        con.Dispose();
                }
                else
                {
                    con.Dispose();
                    if(_primaryConnection == null)
                        error = true;
                }
            }
            catch { NetworkHelper.ReleaseSocket(ref socket); }

            if(error)
                Disconnect(NetworkDirection.Bidirectional);
        }

        protected override void OnConnectionDisposed(Connection connection)
        {
            if(connection == _primaryConnection)
            {
                _serverMessaging.MessageAggregator.Publish(new NetworkContextDetachedMessage(), new StudioNetworkChannelContext(connection, AccountID, ServerID));
                _primaryConnection = null;
                Disconnect(NetworkDirection.Bidirectional);
                SetNetworkState(NetworkState.Offline);
            }
        }

        private void ConnectOperation_NetworkStateChangedCore(NetworkHost client, NetworkStateEventArgs args)
        {
            DispatcherFrame frame = null;

            if(args.ToState == NetworkState.Offline)
            {
                lock(_frameLock)
                {
                    if(_frame != null && _connectFrameArgs == null)
                    {
                        _connectFrameArgs = args;
                        frame = _frame;
                        _frame = null;
                    }
                }
            }
            else if(args.ToState == NetworkState.Online)
            {
                lock(_frameLock)
                {
                    if(_frame != null && _connectFrameArgs == null)
                    {
                        _connectFrameArgs = args;
                        frame = _frame;
                        _frame = null;
                    }
                }
            }

            if(frame != null)
                frame.Continue = false;
        }
        #endregion

        #region Outgoing Data Handling

        public void Send(byte[] data, int index, int length)
        {
            if(_primaryConnection == null)
                throw new InvalidOperationException("TCPDispatchedClient is not connected.");
            _primaryConnection.Send(data, index, length);
        }

        public void Send(Packet p)
        {
            if(_primaryConnection == null)
                throw new InvalidOperationException("TCPDispatchedClient is not connected.");
            _primaryConnection.Send(p);
        }

        public void SendExtended(Packet p, byte[] extension, int extensionLength)
        {
            if(_primaryConnection == null)
                throw new InvalidOperationException("TCPDispatchedClient is not connected.");
            _primaryConnection.SendExtended(p, extension, extensionLength);
        }

        public void SendRange(params Packet[] p)
        {
            if(_primaryConnection == null)
                throw new InvalidOperationException("TCPDispatchedClient is not connected.");
            _primaryConnection.SendRange(p);
        }
        #endregion

        #region Incoming Data Handling
        protected override bool ValidateChannel(int channel)
        {
            return _channels[channel] != null;
        }

        protected override PacketTemplate AcquirePacketTemplate(int channel, ushort packetID)
        {
            AsyncPacketHandler handler = _channels[channel].Handlers[packetID];
            return handler == null ? null : handler.Template;
        }

        protected override void Dispatch(Connection connection, int channel, ushort packetID, int dataLength, byte[] data, bool compressed)
        {
            _channels[channel].Dispatch(packetID, connection, new ByteBuffer(data, dataLength));
        }
        #endregion

        #region Extension Handling
        protected override PacketTemplate AcquireExtensionTemplate(int extension)
        {
            AsyncExtensionHandler handler = _extensions[extension];
            return handler == null ? null : handler.Template;
        }

        protected override void Dispatch(Connection connection, PacketData extension, PacketData packet, bool isResponse)
        {
            if(isResponse)
                _extensions[extension.Channel].Response(_channels[packet.Channel], connection, extension, packet);
            else
                _extensions[extension.Channel].Dispatch(_channels[packet.Channel], connection, extension, packet);
        }
        #endregion

        #region Disposal Handling
        protected override void OnDisposing(bool disposing)
        {
            if(disposing)
            {
                CancelPending();
                DispatcherFrame frame = null;

                lock(_frameLock)
                {
                    _onNetworkStateChangedCore = null;
                    _onLoginStateChangedCore = null;

                    if(_frame != null)
                    {
                        frame = _frame;
                        _frame = null;
                    }
                }

                if(frame != null)
                    frame.Continue = false;
            }
        }

        protected override void OnDisposed(bool disposing)
        {
            _primaryConnection = null;
            _onNetworkStateChanged = null;
            _onLoginStateChanged = null;
            _serverMessaging = null;
        }
        #endregion

        #region ExecuteCommandToken
        private sealed class ExecuteCommandToken
        {
            private int _serial;
            private ManualResetEventSlim _handle;
            private ByteBuffer _result;

            private CancellationTokenSource _source;

            public ExecuteCommandToken(int serial)
            {
                _source = new CancellationTokenSource();
                _serial = serial;
                _handle = new ManualResetEventSlim(false);
            }

            public ByteBuffer Execute()
            {
                bool cancelled = false;

                try
                {
                    _handle.Wait(_source.Token);
                }
                catch(OperationCanceledException)
                {
                    cancelled = true;
                }

                if(cancelled)
                {
                    _result = null;
                }
                else
                {
                    _source.Dispose();
                }

                _handle.Dispose();

                ByteBuffer result = _result;
                _result = null;
                return result;
            }

            public void Cancel()
            {
                _source.Cancel();
                _source.Dispose();

            }

            public void Complete(ByteBuffer result)
            {
                _result = result;
                _handle.Set();
            }
        }
        #endregion

        private sealed class TWALoginBroker : OutboundSRPAuthenticationBroker
        {
            public TWALoginBroker(string username, string password)
                : base(username, password)
            {
                _remoteFirewall = true;
                _localIdentifier = new FourOctetUnion('D', 'E', 'V', '2').Int32;
                _localVersion = new Version(1, 0, 0, 0);
                _localPlatform = Environment.OSVersion.Platform;
                _localServicePack = Environment.OSVersion.ServicePack;
                _localFingerprint = WeaveUtility.GetVolumeSerial(System.IO.Path.GetPathRoot(Environment.CurrentDirectory)[0].ToString().ToUpper());
            }

            protected override void OnAuthenticated(ByteBuffer buffer)
            {
            }
        }
    }
}
