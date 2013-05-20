using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Network;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Dev2.Common;
using Dev2.Diagnostics;
using Dev2.ExtMethods;
using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Studio.Core.Network
{
    public abstract class TcpClientHostBase : NetworkHost, ITcpClientHost
    {
        long _nextMessageHandle;
        readonly ConcurrentDictionary<long, TaskCompletionSource<INetworkMessage>> _messages = new ConcurrentDictionary<long, TaskCompletionSource<INetworkMessage>>();
        readonly ConcurrentDictionary<Guid, IDebugWriter> _debugWriters = new ConcurrentDictionary<Guid, IDebugWriter>();
        readonly AsyncPacketHandlerCollection[] _channels = new AsyncPacketHandlerCollection[16];
        volatile Connection _primaryConnection;
        volatile bool _isLoggedIn;
        volatile NetworkState _networkState;
        volatile IStudioNetworkMessageAggregator _messageAggregator = new StudioNetworkMessageAggregator();
        volatile INetworkMessageBroker _messageBroker = new NetworkMessageBroker();

        const int DefaultConnectionRetryInterval = 10000; // milliseconds
        volatile System.Timers.Timer _reconnectHeartbeat;
        volatile EndPoint _reconnectEndPoint;
        volatile bool _isDisconnecting;

        #region CTOR

        protected TcpClientHostBase()
            : this(false)
        {
        }

        protected TcpClientHostBase(bool isAuxiliary)
            : base("TcpClientHost")
        {
            _isLoggedIn = false;
            _networkState = NetworkState.Offline;
            ConnectionRetryInterval = DefaultConnectionRetryInterval;
            IsAuxiliary = isAuxiliary;

            if(!isAuxiliary)
            {
                _channels[0] = new AsyncPacketHandlerCollection();
                _channels[0].Register(0, PacketTemplates.Client_OnAuxiliaryConnectionReply, OnAuxiliaryConnectionReply);
                _channels[0].Register(1, PacketTemplates.Client_OnDebugWriterWrite, OnDebugWriterWrite);

                _channels[1] = new AsyncPacketHandlerCollection();
                _channels[1].Register(0, PacketTemplates.Both_OnNetworkMessageReceived, OnNetworkMessageReceived);
            }

            _channels[15] = new AsyncPacketHandlerCollection();
            _channels[15].Register(0, InternalTemplates.Client_LogoutReceived, OnLogoutReceived);
            _channels[15].Register(1, InternalTemplates.Client_OnExecuteStringCommandReceived, OnExecuteCommandReceived);
            _channels[15].Register(2, InternalTemplates.Client_OnExecuteBinaryCommandReceived, OnExecuteCommandReceived);
            _channels[15].Register(3, InternalTemplates.Client_OnClientDetailsReceived, OnClientDetailsReceived);
        }

        #endregion

        #region Events

        public event EventHandler<LoginStateEventArgs> LoginStateChanged;
        public event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        public event EventHandler<ServerStateEventArgs> ServerStateChanged;

        #endregion

        #region Properties

        public int ConnectionRetryInterval { get; set; }

        public bool IsAuxiliary { get; private set; }

        public Guid ServerID { get; private set; }

        public Guid AccountID { get; private set; }

        public bool IsConnected { get { return _networkState == NetworkState.Online && _isLoggedIn; } }

        public IStudioNetworkMessageAggregator MessageAggregator { get { return _messageAggregator; } }

        public INetworkMessageBroker MessageBroker { get { return _messageBroker; } }

        #endregion

        #region Add/RemoveDebugWriter

        public void AddDebugWriter(IDebugWriter writer)
        {
            ValidateState();
            if(writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if(_debugWriters.TryAdd(AccountID, writer))
            {
                var p = new Packet(PacketTemplates.Server_OnDebugWriterAddition);
                Send(p);
            }
        }

        public void RemoveDebugWriter(IDebugWriter writer)
        {
            if(writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            RemoveDebugWriter(AccountID);
        }

        public void RemoveDebugWriter(Guid writerID)
        {
            IDebugWriter debugWriter;
            if(_debugWriters.TryRemove(writerID, out debugWriter))
            {
                var p = new Packet(PacketTemplates.Server_OnDebugWriterSubtraction);
                Send(p);
            }
        }

        #endregion

        #region ConnectAsync

        public async Task<bool> ConnectAsync(string hostNameOrAddress, int port)
        {
            if(_networkState != NetworkState.Offline)
            {
                return true;
            }

            var connectTask = ConnectAsyncImpl(hostNameOrAddress, port);
            await connectTask;
            return connectTask.Result;
        }

        #endregion

        #region LoginAsync

        public async Task<bool> LoginAsync(IIdentity identity)
        {
            if(identity == null)
            {
                return false;
            }

            // THIS IS NOT A CLEVER IDEA, WIN2K8 FALLS FLAT USING THE SAME IDENTITY FOR BOTH LOCAL AND REMOTE SERVER, BUT USING A RANDOM GUID WORKS JUST FINE ;)
            // internal location data ;)
            //var winIdentity = identity as WindowsIdentity;
            //var userName = (winIdentity != null && winIdentity.User != null) ? winIdentity.User.Value : identity.Name;

      
            // THIS IS REQUIRED TO PROPERLY ALLOW MULT CONNECTIONS TO SERVER ;)
            string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string savePath = @"Dev2\Studio\";
            string finalPath = Path.Combine(RootPath, savePath);

            var fileLoc = Path.Combine(finalPath, "ID.Studio");

            var tmp = Path.Combine(RootPath, "Dev2");

            if (!Directory.Exists(tmp))
            {
                Directory.CreateDirectory(tmp);
            }

            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }

            Guid id;

            if (!File.Exists(fileLoc))
            {
                id = Guid.NewGuid();

                File.WriteAllText(fileLoc, id.ToString());

            }
            else
            {
                var tt = File.ReadAllText(fileLoc);
                Guid.TryParse(tt, out id);
            }
          


            // TODO: Remove LoginAsync using hard-coded password
            // See StudioAccountProvider in Dev2.Runtime assembly
            return await LoginAsync(id.ToString(), "abc123xyz");
        }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            if(_networkState != NetworkState.Online)
            {
                return false;
            }

            if(_isLoggedIn)
            {
                return true;
            }

            if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var loginTask = LoginAsyncImpl(userName, password);
            await loginTask;
            return loginTask.Result;
        }

        #endregion

        #region Send

        public void Send(Packet p)
        {
            if(_primaryConnection == null)
            {
                throw new InvalidOperationException("Client is not connected.");
            }
            _primaryConnection.Send(p);
        }

        #endregion

        #region SendNetworkMessage

        public void SendNetworkMessage(INetworkMessage message)
        {
            _messageBroker.Send(message, this);
        }

        #endregion

        #region RecieveNetworkMessage

        public INetworkMessage RecieveNetworkMessage(IByteReaderBase reader)
        {
            return _messageBroker.Receive(reader);
        }

        #endregion

        #region SendReceiveNetworkMessage

        public INetworkMessage SendReceiveNetworkMessage(INetworkMessage message)
        {
            return SendReceive(message, PacketTemplates.Both_OnNetworkMessageReceived, true);
        }

        #endregion

        #region ExecuteCommand

        public string ExecuteCommand(string payload, Guid workspaceID, Guid dataListID)
        {
            var result = SendReceive(new ExecuteCommandMessage { DataListID = dataListID, Payload = payload }, InternalTemplates.Server_OnExecuteStringCommandReceived);

            return result.HasError ? result.ErrorMessage : result.Payload;
        }

        #endregion

        #region Overrides of NetworkHost

        public virtual new void Disconnect()
        {
            if(_isDisconnecting)
            {
                return;
            }
            _isDisconnecting = true;
            try
            {
                DisconnectImpl();
            }
            finally
            {
                _isDisconnecting = false;
            }
        }

        #region DisconnectImpl

        protected void DisconnectImpl()
        {
            RaiseLoginStateChanged(AuthenticationResponse.Logout, true);
            if(_primaryConnection != null)
            {
                _primaryConnection.Dispose();
                _primaryConnection = null;
            }
            RaiseNetworkStateChanged(NetworkState.Offline);
            base.Disconnect();
        }

        #endregion

        protected override void OnLoginSuccess(Connection connection, NetworkAccount account, AuthenticationBroker broker)
        {
            CompleteLogin((TcpLoginBroker)broker);
        }

        protected override void OnLoginFailed(Connection connection, OutboundAuthenticationBroker broker, AuthenticationResponse reason, bool expectDisconnect)
        {
            StudioLogger.LogMessage("Login Failed To [ " + connection.Address + " ] because of [ " + reason.ToString() + " ]");
            CancelLogin((TcpLoginBroker)broker, reason, expectDisconnect);
        }

        protected override bool ValidateChannel(int channel)
        {
            return _channels[channel] != null;
        }

        protected override PacketTemplate AcquirePacketTemplate(int channel, ushort packetID)
        {
            var handler = _channels[channel].Handlers[packetID];
            return handler == null ? null : handler.Template;
        }

        protected override void Dispatch(Connection connection, int channel, ushort packetID, int dataLength, byte[] data, bool compressed)
        {
            _channels[channel].Dispatch(packetID, connection, new ByteBuffer(data, dataLength));
        }

        #endregion

        #region Channel PacketEventHandlers

        void OnClientDetailsReceived(INetworkOperator op, ByteBuffer reader)
        {
            ServerID = reader.ReadGuid();
            AccountID = reader.ReadGuid();
        }

        void OnLogoutReceived(INetworkOperator op, ByteBuffer reader)
        {
            var reason = (AuthenticationResponse)reader.ReadByte();
            RaiseLoginStateChanged(reason);
        }

        void OnDebugWriterWrite(INetworkOperator op, ByteBuffer reader)
        {
            IDebugWriter writer;
            if(_debugWriters.TryGetValue(AccountID, out writer) && writer != null)
            {
                var debugState = new DebugState(reader);
                writer.Write(debugState);
            }
        }

        void OnExecuteCommandReceived(INetworkOperator op, ByteBuffer reader)
        {
            CompleteSendReceive(delegate
            {
                var message = new ExecuteCommandMessage { Handle = -1 };
                message.Handle = reader.ReadInt64();
                message.Read(reader);
                return message;
            });
        }

        void OnNetworkMessageReceived(INetworkOperator op, ByteBuffer reader)
        {
            CompleteSendReceive(delegate
            {
                var message = _messageBroker.Receive(reader);
                _messageAggregator.Publish(message, new StudioNetworkChannelContext(op, AccountID, ServerID));
                return message;
            });
        }

        void OnAuxiliaryConnectionReply(INetworkOperator op, ByteBuffer reader)
        {
            if(IsAuxiliary)
            {
                throw new InvalidOperationException("Auxiliary connections cannot receive this packet.");
            }
            var commandSerial = reader.ReadInt32();
        }

        #endregion

        #region StateChanged Event Handlers

        protected void RaiseServerStateChanged(ServerState state)
        {
            if(ServerStateChanged != null)
            {
                ServerStateChanged(this, new ServerStateEventArgs(state));
            }
            if(state == ServerState.Offline)
            {
                RaiseNetworkStateChanged(NetworkState.Offline);
            }
        }

        protected void RaiseLoginStateChanged(AuthenticationResponse response, bool expectDisconnect = false)
        {
            _isLoggedIn = response == AuthenticationResponse.Success;
            if(LoginStateChanged != null)
            {
                LoginStateChanged(this, new LoginStateEventArgs(response, expectDisconnect));
            }
        }

        protected void RaiseNetworkStateChanged(NetworkState toState, bool isError = false, string message = "")
        {
            if(_networkState != toState)
            {
                var fromState = _networkState;
                _networkState = toState;
                if(NetworkStateChanged != null)
                {
                    NetworkStateChanged(this, new NetworkStateEventArgs(fromState, toState, isError, message));
                }
            }
            if(toState == NetworkState.Offline && _isLoggedIn)
            {
                RaiseLoginStateChanged(AuthenticationResponse.Logout);
            }
        }

        #endregion

        #region ValidateState

        void ValidateState()
        {
            if(Disposed)
            {
                throw new ObjectDisposedException("TcpClientHost");
            }

            if(IsAuxiliary)
            {
                throw new InvalidOperationException("Auxiliary clients are not supported.");
            }

            if(_networkState != NetworkState.Online)
            {
                throw new InvalidOperationException("Client is not online.");
            }

            if(!_isLoggedIn)
            {
                throw new InvalidOperationException("Client is not logged in.");
            }
        }

        #endregion

        #region ConnectAsyncImpl

        Task<bool> ConnectAsyncImpl(string hostNameOrAddress, int port)
        {
            RaiseNetworkStateChanged(NetworkState.Connecting);

            var tcs = new TaskCompletionSource<bool>();

            IPAddress ipAddress;
            if(IPAddress.TryParse(hostNameOrAddress, out ipAddress))
            {
                TryConnect(tcs, ipAddress, port);
            }
            else
            {
                TryConnectDns(tcs, hostNameOrAddress, port);
            }
            return tcs.Task;
        }

        #endregion

        #region TryConnectDns

        void TryConnectDns(TaskCompletionSource<bool> tcs, string hostNameOrAddress, int port)
        {
            try
            {
                Dns.BeginGetHostEntry(hostNameOrAddress, ar =>
                {
                    try
                    {
                        var resolvedEntry = Dns.EndGetHostEntry(ar);
                        var resolvedAddress = resolvedEntry.AddressList.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);
                        if(resolvedAddress != null)
                        {
                            TryConnect(tcs, resolvedAddress, port);
                        }
                        else
                        {
                            CancelConnect(tcs, "The specified server could not be found.");
                        }
                    }
                    catch(Exception ex)
                    {
                        CancelConnect(tcs, ex.Message);
                    }
                }, null);
            }
            catch(Exception ex)
            {
                CancelConnect(tcs, ex.Message);
            }
        }

        #endregion

        #region TryConnect

        void TryConnect(TaskCompletionSource<bool> tcs, IPAddress address, int port)
        {
            Socket socket = null;
            try
            {
                socket = CreateSocket();
                var endPoint = new IPEndPoint(address, port);

                socket.BeginConnect(endPoint, iar =>
                {
                    var iarSocket = (Socket)iar.AsyncState;
                    try
                    {
                        iarSocket.EndConnect(iar);
                        Connection connection = new AsyncConnection(this, socket, NetworkDirection.Outbound);
                        connection.Start();

                        if(connection.Alive)
                        {
                            if(_primaryConnection == null)
                            {
                                _primaryConnection = connection;
                            }
                            else if(_networkState == NetworkState.Offline)
                            {
                                connection.Dispose();
                            }
                            CompleteConnect(tcs);
                        }
                        else
                        {
                            connection.Dispose();
                            if(_primaryConnection == null)
                            {
                                base.Disconnect();
                            }
                            CancelConnect(tcs, "Connection is not alive.");
                        }
                    }
                    catch(Exception ex)
                    {
                        iarSocket.Release();
                        CancelConnect(tcs, ex.Message);
                    }
                }, socket);
            }
            catch(Exception ex)
            {
                socket.Release();
                CancelConnect(tcs, ex.Message);
            }
        }

        #endregion

        #region Cancel/CompleteConnect

        void CancelConnect(TaskCompletionSource<bool> tcs, string message)
        {
            RaiseNetworkStateChanged(NetworkState.Offline, true, message);
            tcs.TrySetResult(false);
        }

        void CompleteConnect(TaskCompletionSource<bool> tcs)
        {
            RaiseNetworkStateChanged(NetworkState.Online);
            tcs.TrySetResult(true);
        }

        #endregion

        #region LoginAsyncImpl

        Task<bool> LoginAsyncImpl(string userName, string password)
        {
            // TcpLoginBroker invokes the following methods of this class:
            //
            // - this.OnLoginFailed();
            // - this.OnLoginSuccess();
            //
            var tcs = new TaskCompletionSource<bool>();
            var loginBroker = new TcpLoginBroker(userName, password, tcs);
            loginBroker.BeginAuthentication(_primaryConnection);
            return tcs.Task;
        }

        #endregion

        #region Cancel/CompleteLogin

        void CancelLogin(TcpLoginBroker broker, AuthenticationResponse reason, bool expectDisconnect)
        {
            if(reason == AuthenticationResponse.Success)
            {
                reason = AuthenticationResponse.Unspecified;
            }
            RaiseLoginStateChanged(reason, expectDisconnect);
            broker.TaskCompletionSource.TrySetResult(false);
        }

        void CompleteLogin(TcpLoginBroker broker)
        {
            RaiseLoginStateChanged(AuthenticationResponse.Success);
            broker.TaskCompletionSource.TrySetResult(true);
        }

        #endregion

        #region SendReceive

        TMessage SendReceive<TMessage>(TMessage message, PacketTemplate template, bool writeMessageTypeBeforeHandle = false)
            where TMessage : INetworkMessage
        {
            ValidateState();

            var handle = Interlocked.Increment(ref _nextMessageHandle);
            var tcs = new TaskCompletionSource<INetworkMessage>();

            message.Handle = handle;
            _messages.TryAdd(handle, tcs);

            TMessage result;
            try
            {
                var p = new Packet(template);
                if(writeMessageTypeBeforeHandle)
                {
                    var typeName = message.GetType().AssemblyQualifiedName;
                    p.Write(typeName);
                }
                p.Write(handle);
                message.Write(p);

                //
                // Start async call 
                // The response is received by the relevant channel packet event handler
                // which is responsible for invoking CompleteSendReceive<TMessage>
                //
                _primaryConnection.Send(p);

                // DO NOT block the UI thread by using Wait()!!
                if(tcs.Task.WaitWithPumping(GlobalConstants.NetworkTimeOut))
                {
                    result = (TMessage)tcs.Task.Result;
                }
                else
                {
                    result = Activator.CreateInstance<TMessage>();
                    result.HasError = true;
                    result.ErrorMessage = "Connection to server timed out.";
                }
            }
            catch(AggregateException aex)
            {
                var errors = new StringBuilder("Connection to server could not be established : ");
                aex.Handle(ex =>
                {
                    errors.AppendLine(ex.Message);
                    return true;
                });
                result = Activator.CreateInstance<TMessage>();
                result.HasError = true;
                result.ErrorMessage = errors.ToString();
            }
            finally
            {
                _messages.TryRemove(handle, out tcs);
            }
            return result;
        }

        #endregion

        #region CompleteSendReceive

        void CompleteSendReceive<TMessage>(Func<TMessage> read)
            where TMessage : INetworkMessage
        {
            TMessage message;
            try
            {
                message = read();
            }
            catch(Exception ex)
            {
                message = Activator.CreateInstance<TMessage>();
                message.HasError = true;
                message.ErrorMessage = "An error occured while trying to interpret network message from the server. " + ex.Message;
                TraceWriter.WriteTrace(message.ErrorMessage);
            }

            TaskCompletionSource<INetworkMessage> tcs;
            if(_messages.TryGetValue(message.Handle, out tcs))
            {
                tcs.TrySetResult(message);
            }
            else
            {
                TraceWriter.WriteTrace("Message handle not found : " + message.Handle);
            }
        }

        #endregion

        #region CreateSocket

        protected Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #endregion

        #region Server Reconnect Heartbeat

        // PBI 9228: TWR - 2013.04.17 - added server reconnect heartbeat

        #region OnConnectionDisposed

        protected override void OnConnectionDisposed(Connection connection)
        {
            base.OnConnectionDisposed(connection);

            RaiseServerStateChanged(ServerState.Offline);
            if(!_isDisconnecting)
            {
                StartReconnectHeartbeat(connection);
            }
        }

        #endregion

        #region StartReconnectHeartbeat

        public virtual bool StartReconnectHeartbeat(Connection connection)
        {
            if(connection != null)
            {
                _reconnectEndPoint = new IPEndPoint(connection.Address, connection.Port);
                _reconnectHeartbeat = new System.Timers.Timer();
                _reconnectHeartbeat.Elapsed += OnReconnectHeartbeatElapsed;
                _reconnectHeartbeat.Interval = ConnectionRetryInterval;
                _reconnectHeartbeat.AutoReset = false;
                _reconnectHeartbeat.Start();
                return true;
            }
            return false;
        }

        #endregion

        #region StopReconnectHeartbeat

        public virtual void StopReconnectHeartbeat()
        {
            if(_reconnectHeartbeat != null)
            {
                _reconnectHeartbeat.Stop();
                _reconnectHeartbeat.Dispose();
                _reconnectHeartbeat = null;
            }
            _reconnectEndPoint = null;
        }

        #endregion

        #region OnReconnectHeartbeatElapsed

        void OnReconnectHeartbeatElapsed(object sender, ElapsedEventArgs args)
        {
            var connected = Ping(_reconnectEndPoint);
            if(connected)
            {
                StopReconnectHeartbeat();
                RaiseServerStateChanged(ServerState.Online);
            }
            else
            {
                _reconnectHeartbeat.Start();
            }
        }

        #endregion

        public abstract bool Ping(EndPoint endPoint);

        #endregion
    }
}
