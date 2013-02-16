using System;
using System.Collections.Generic;
using System.Network;
using Dev2.Diagnostics;
using Dev2.DynamicServices.Network;
using Dev2.DynamicServices.Network.Auxiliary;
using Dev2.Network;
using Dev2.Network.Messages;
using Dev2.Network.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Network;

namespace Dev2.DynamicServices {
    public sealed class StudioNetworkServer : TCPServer<StudioNetworkSession> {
        #region Instance Fields
        private StudioFileSystem _fileSystem;
        private StudioAccountProvider _accountProvider;
        private DynamicServicesEndpoint _channel;
        private Guid _serverID;

        private object _auxiliaryLock = new object();
        private StudioAuxiliaryServer _auxiliaryServer;
        private List<ListenerConfig> _auxiliaryConfigurations;

        #endregion

        #region Public Properties
        public StudioFileSystem FileSystem { get { return _fileSystem; } }
        public StudioAccountProvider AccountProvider { get { return _accountProvider; } }
        public DynamicServicesEndpoint Channel { get { return _channel; } }
        #endregion

        #region Constructor
        public StudioNetworkServer(string serverName, StudioFileSystem fileSystem, DynamicServicesEndpoint channel, Guid serverID)
            : this(serverName, fileSystem, channel, serverID, true) {
        }

        public StudioNetworkServer(string serverName, StudioFileSystem fileSystem, DynamicServicesEndpoint channel, Guid serverID, bool autoAccountCreation)
            : base(serverName, new StudioAuthenticationBroker()) {
            _channel = channel;
            _serverID = serverID;
            if ((_fileSystem = fileSystem) == null)
                throw new ArgumentNullException("fileSystem");
            _accountProvider = new StudioAccountProvider(null, autoAccountCreation, this);
            _accountProvider.Load();
            ((StudioAuthenticationBroker)_authenticationBroker).Server = this;

            _channels[0] = new IOCPPacketHandlerCollection<StudioNetworkSession>(0, null);
            _channels[1] = new IOCPPacketHandlerCollection<StudioNetworkSession>(1, null);

            _channels[0].Register(0, PacketTemplates.Server_OnAuxiliaryConnectionRequested, new PacketEventHandler<StudioNetworkSession>(OnAuxiliaryConnectionRequested));
            _channels[0].Register(1, PacketTemplates.Server_OnDebugWriterAddition, new PacketEventHandler<StudioNetworkSession>(OnDebugWriterAddition));
            _channels[0].Register(2, PacketTemplates.Server_OnDebugWriterSubtraction, new PacketEventHandler<StudioNetworkSession>(OnDebugWriterSubtraction));

            _channels[1].Register(0, PacketTemplates.Both_OnNetworkMessageRevieved, new PacketEventHandler<StudioNetworkSession>(OnNetworkMessageRevieved));

            ContextAttached += StudioNetworkServer_ContextAttached;
            ContextDetached += StudioNetworkServer_ContextDetached;
        }

        #endregion

        #region DebugWriter Support
        private void OnDebugWriterAddition(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader) {
            Guid accountID = context.AccountID;
            TransparentDebugWriter writer = new TransparentDebugWriter(this, accountID);
            DebugDispatcher.Instance.Add(accountID, writer);
        }

        private void OnDebugWriterSubtraction(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader) {
            Guid accountID = context.AccountID;
            DebugDispatcher.Instance.Remove(accountID);
        }
        #endregion

        #region Network Message Support

        private void OnNetworkMessageRevieved(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader) {
            try
            {
                INetworkMessage message = StudioMessaging.MessageBroker.Recieve(reader);
                StudioMessaging.MessageAggregator.Publish(message, new ServerNetworkChannelContext(context, this));
            }
            catch (Exception e)
            {
                TraceWriter.WriteTrace("An error occured while trying to interpret network message from the studio. " + e.Message);

                try
                {
                    ErrorMessage errorMessage = new ErrorMessage(0, e.Message);
                    StudioMessaging.MessageBroker.Send(errorMessage, op);
                }
                catch (Exception ex)
                {
                    TraceWriter.WriteTrace("An error occured while trying to send an error message to the studio. " + ex.Message);
                }
            }

        }

        #endregion

        #region ExecuteCommand Handling
        protected override string OnExecuteCommand(StudioNetworkSession context, string payload, Guid datalistID) {
            return _channel.ExecuteCommand(payload, Workspaces.WorkspaceRepository.Instance.Get(context.AccountID) ?? Workspaces.WorkspaceRepository.Instance.ServerWorkspace, datalistID);
        }

        protected override void OnExecuteCommand(StudioNetworkSession context, ByteBuffer payload, Packet writer) {

        }
        #endregion

        #region Auxiliary Conection Handling
        protected override void OnStarted(ListenerConfig[] configs) {
            lock (_auxiliaryLock) 
            {
                if (_auxiliaryConfigurations == null)
                {
                    _auxiliaryConfigurations = new List<ListenerConfig>();
                }

                if (configs == null)
                {
                    return;
                }

                foreach (ListenerConfig listenerConfig in configs)
                {
                    _auxiliaryConfigurations.Add(new ListenerConfig(listenerConfig.Address, listenerConfig.Port + 20000, listenerConfig.Backlog));
                }
                //_auxiliaryConfigurations = configs ?? new ListenerConfig[0];
                //for (int i = 0; i < _auxiliaryConfigurations.Length; i++)
                    //_auxiliaryConfigurations[i] = new ListenerConfig(_auxiliaryConfigurations[i].Address, _auxiliaryConfigurations[i].Port + 20000, _auxiliaryConfigurations[i].Backlog);
            }
        }

        protected override void OnStopped() {
            lock (_auxiliaryLock) {
                if (_auxiliaryServer != null)
                    _auxiliaryServer.Dispose();
                _auxiliaryServer = null;
                _auxiliaryConfigurations = null;
            }
        }

        private void OnAuxiliaryConnectionRequested(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader) {
            int request = reader.ReadInt32();
            Guid guid = Guid.NewGuid();// context.NotifyAuxiliaryConnectionRequested();

            lock (_auxiliaryLock) {
                if (_auxiliaryServer == null) {
                    _auxiliaryServer = new StudioAuxiliaryServer(this);
                    _auxiliaryServer.Start(_auxiliaryConfigurations.ToArray());
                }

                _auxiliaryServer.AccountProvider.CreateAccount(guid);
            }

            Packet p = new Packet(PacketTemplates.Client_OnAuxiliaryConnectionReply);
            p.Write(request);
            p.Write(guid);
            op.Send(p);
        }
        #endregion

        #region Context Handling
        private void StudioNetworkServer_ContextDetached(TCPServer<StudioNetworkSession> server, StudioNetworkSession context)
        {
            StudioMessaging.MessageAggregator.Publish(new NetworkContextDetachedMessage(), new ServerNetworkChannelContext(context, server));
        }

        private void StudioNetworkServer_ContextAttached(TCPServer<StudioNetworkSession> server, StudioNetworkSession context) {
            Packet p = new Packet(InternalTemplates.Server_SendClientDetails);
            p.Write(_serverID);
            p.Write(context.AccountID);
            context.Send(p);
        }
        #endregion

        #region Disposal Handling
        protected override void OnDisposing(bool disposing) {
            base.OnDisposing(disposing);

            if (disposing) {
                //_accountProvider.Save();

                lock (_auxiliaryLock) {
                    if (_auxiliaryServer != null)
                        _auxiliaryServer.Dispose();
                    _auxiliaryServer = null;
                    _auxiliaryConfigurations = null;
                }
            }
        }
        #endregion

        #region StudioAuthenticationBroker
        private sealed class StudioAuthenticationBroker : InboundSRPAuthenticationBroker {
            private StudioNetworkServer _server;

            public StudioNetworkServer Server { get { return _server; } set { _server = value; } }

            public StudioAuthenticationBroker() {
                _localIdentifier = new FourOctetUnion('D', 'E', 'V', '2').Int32;
                _localVersion = new Version(1, 0, 0, 0);
            }

            protected override InboundAuthenticationBroker OnInstantiate() {
                return new StudioAuthenticationBroker() { Server = _server };
            }

            protected override NetworkAccount ResolveAccount(string account) {
                return _server._accountProvider.GetAccount(account);
            }
        }
        #endregion

        #region TransparentDebugWriter
        private sealed class TransparentDebugWriter : IDebugWriter {
            readonly StudioNetworkServer _server;
            readonly Guid _accountID;
            readonly StudioAccount _account;

            public Guid ID { get { return _accountID; } }

            public TransparentDebugWriter(StudioNetworkServer server, Guid accountID) {
                _server = server;
                _accountID = accountID;
                _account = server.AccountProvider.GetAccount(accountID);
            }

            public void Write(IDebugState debugState) {
                if (_account.InUse && _account.Owner != null) {
                    debugState.ServerID = _server._serverID;
                    var p = new Packet(PacketTemplates.Client_OnDebugWriterWrite);
                    debugState.Write(p);
                    _account.Owner.Send(p);
                } else {
                    DebugDispatcher.Instance.Remove(_accountID);
                }
            }
        }
        #endregion
    }
}
