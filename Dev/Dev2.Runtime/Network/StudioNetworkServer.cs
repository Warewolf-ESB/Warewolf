using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices.Network;
using Dev2.DynamicServices.Network.Auxiliary;
using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Network;
using Dev2.Services;

namespace Dev2.DynamicServices
{
    public class StudioNetworkServer : TCPServer<StudioNetworkSession>, IContextManager<IStudioNetworkSession>
    {
        #region Instance Fields
        private StudioFileSystem _fileSystem;
        private StudioAccountProvider _accountProvider;
        private EsbServicesEndpoint _channel;
        private Guid _serverID;

        private object _auxiliaryLock = new object();
        private StudioAuxiliaryServer _auxiliaryServer;
        private List<ListenerConfig> _auxiliaryConfigurations;

        // PBI 6690 - 2013.07.04 - TWR : added
        readonly IPushService _pushService;
        readonly ISerializer _serializer = new Communication.JsonSerializer();

        #endregion

        #region Public Properties
        public StudioFileSystem FileSystem { get { return _fileSystem; } }
        public StudioAccountProvider AccountProvider { get { return _accountProvider; } }
        public EsbServicesEndpoint Channel { get { return _channel; } }
        #endregion

        #region Constructor

        public StudioNetworkServer(string serverName, StudioFileSystem fileSystem, EsbServicesEndpoint channel, Guid serverID)
            : this(serverName, fileSystem, channel, serverID, true)
        {
        }

        public StudioNetworkServer(string serverName, StudioFileSystem fileSystem, EsbServicesEndpoint channel, Guid serverID, bool autoAccountCreation)
            : this(serverName, fileSystem, channel, serverID, autoAccountCreation, new PushService())
        {
        }

        // PBI 6690 - 2013.07.04 - TWR : added IPushService parameter
        public StudioNetworkServer(string serverName, StudioFileSystem fileSystem, EsbServicesEndpoint channel, Guid serverID, bool autoAccountCreation, IPushService pushService)
            : base(serverName, new StudioAuthenticationBroker())
        {
            _channel = channel;
            _serverID = serverID;

            if((_fileSystem = fileSystem) == null)
            {
                throw new ArgumentNullException("fileSystem");
            }

            _accountProvider = new StudioAccountProvider(null, autoAccountCreation, this);
            _accountProvider.Load();
            ((StudioAuthenticationBroker)_authenticationBroker).Server = this;

            _channels[0] = new IOCPPacketHandlerCollection<StudioNetworkSession>(0, null);
            _channels[1] = new IOCPPacketHandlerCollection<StudioNetworkSession>(1, null);

            _channels[0].Register(0, PacketTemplates.Server_OnAuxiliaryConnectionRequested, OnAuxiliaryConnectionRequested);
            _channels[0].Register(1, PacketTemplates.Server_OnDebugWriterAddition, OnDebugWriterAddition);
            _channels[0].Register(2, PacketTemplates.Server_OnDebugWriterSubtraction, OnDebugWriterSubtraction);

            _channels[1].Register(0, PacketTemplates.Both_OnNetworkMessageReceived, OnNetworkMessageReceived);

            // PBI 6690 - 2013.07.04 - TWR : added
            VerifyArgument.IsNotNull("pushService", pushService);
            _pushService = pushService;
            _channels[2] = new IOCPPacketHandlerCollection<StudioNetworkSession>(2, null);
            _channels[2].Register(0, PacketTemplates.EventProviderServerMessage, OnEventProviderServerMessageReceived);

            ContextAttached += StudioNetworkServer_ContextAttached;
            ContextDetached += StudioNetworkServer_ContextDetached;

            CompileMessageRepo.Instance.AllMessages.Subscribe(OnCompilerMessageReceived);
        }

        #endregion

        #region DebugWriter Support
        private void OnDebugWriterAddition(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader)
        {
            var account = context.Account;
            AddTransparentDebugWriter(account);
        }

        private void AddTransparentDebugWriter(StudioAccount account)
        {
            TransparentDebugWriter writer = new TransparentDebugWriter(this, account);
            DebugDispatcher.Instance.Add(account.AccountID, writer);
        }

        private void OnDebugWriterSubtraction(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader)
        {
            Guid accountID = context.AccountID;
            DebugDispatcher.Instance.Remove(accountID);
        }
        #endregion

        #region Network Message Support

        private void OnNetworkMessageReceived(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader)
        {
            try
            {
                INetworkMessage message = StudioMessaging.MessageBroker.Receive(reader);
                StudioMessaging.MessageAggregator.Publish(message, new ServerNetworkChannelContext(context, this));
            }
            catch(Exception e)
            {
                ServerLogger.LogError("An error occured while trying to interpret network message from the studio. " + e.Message);

                try
                {
                    ErrorMessage errorMessage = new ErrorMessage(0, e.Message);
                    StudioMessaging.MessageBroker.Send(errorMessage, op);
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError("An error occured while trying to interpret network message from the studio. " + ex.Message);
                }
            }

        }

        #endregion

        #region ExecuteRequest Handling
        protected override string OnExecuteCommand(StudioNetworkSession context, string payload, Guid datalistID)
        {
            IDSFDataObject dataObject = null;
            ErrorResultTO errors = new ErrorResultTO();

            dataObject = new DsfDataObject(payload, datalistID);

            if(!dataObject.Errors.HasErrors())
            {
                string dlID = _channel.ExecuteRequest(dataObject, context.AccountID, out errors).ToString();
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                string result = compiler.ConvertFrom(new Guid(dlID), DataListFormat.CreateFormat(GlobalConstants._XML),
                                                     enTranslationDepth.Data, out errors);
                return result;
            }
            else return dataObject.Errors.MakeUserReady();
        }

        protected override void OnExecuteCommand(StudioNetworkSession context, ByteBuffer payload, Packet writer)
        {

        }
        #endregion

        #region Auxiliary Conection Handling
        protected override void OnStarted(ListenerConfig[] configs)
        {
            lock(_auxiliaryLock)
            {
                if(_auxiliaryConfigurations == null)
                {
                    _auxiliaryConfigurations = new List<ListenerConfig>();
                }

                if(configs == null)
                {
                    return;
                }

                foreach(ListenerConfig listenerConfig in configs)
                {
                    _auxiliaryConfigurations.Add(new ListenerConfig(listenerConfig.Address, listenerConfig.Port + 20000, listenerConfig.Backlog));
                }
                //_auxiliaryConfigurations = configs ?? new ListenerConfig[0];
                //for (int i = 0; i < _auxiliaryConfigurations.Length; i++)
                //_auxiliaryConfigurations[i] = new ListenerConfig(_auxiliaryConfigurations[i].Address, _auxiliaryConfigurations[i].Port + 20000, _auxiliaryConfigurations[i].Backlog);
            }
        }

        protected override void OnStopped()
        {
            lock(_auxiliaryLock)
            {
                if(_auxiliaryServer != null)
                    _auxiliaryServer.Dispose();
                _auxiliaryServer = null;
                _auxiliaryConfigurations = null;
            }
        }

        private void OnAuxiliaryConnectionRequested(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader)
        {
            int request = reader.ReadInt32();
            Guid guid = Guid.NewGuid();// context.NotifyAuxiliaryConnectionRequested();

            lock(_auxiliaryLock)
            {
                if(_auxiliaryServer == null)
                {
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

        private void StudioNetworkServer_ContextAttached(TCPServer<StudioNetworkSession> server, StudioNetworkSession context)
        {
            var p = new Packet(InternalTemplates.Server_SendClientDetails);
            p.Write(_serverID);
            p.Write(context.AccountID);
            context.Send(p);
        }

        #endregion

        #region Disposal Handling
        protected override void OnDisposing(bool disposing)
        {
            base.OnDisposing(disposing);

            if(disposing)
            {
                //_accountProvider.Save();

                lock(_auxiliaryLock)
                {
                    if(_auxiliaryServer != null)
                        _auxiliaryServer.Dispose();
                    _auxiliaryServer = null;
                    _auxiliaryConfigurations = null;
                }
            }
        }
        #endregion

        #region StudioAuthenticationBroker
        private sealed class StudioAuthenticationBroker : InboundSRPAuthenticationBroker
        {
            private StudioNetworkServer _server;

            public StudioNetworkServer Server { get { return _server; } set { _server = value; } }

            public StudioAuthenticationBroker()
            {
                _localIdentifier = new FourOctetUnion('D', 'E', 'V', '2').Int32;
                _localVersion = new Version(1, 0, 0, 0);
            }

            protected override InboundAuthenticationBroker OnInstantiate()
            {
                return new StudioAuthenticationBroker() { Server = _server };
            }

            protected override NetworkAccount ResolveAccount(string account)
            {
                return _server._accountProvider.GetAccount(account);
            }
        }
        #endregion

        #region TransparentDebugWriter
        private sealed class TransparentDebugWriter : IDebugWriter
        {
            readonly StudioNetworkServer _server;
            readonly StudioAccount _account;

            public TransparentDebugWriter(StudioNetworkServer server, StudioAccount account)
            {
                _server = server;
                _account = account;
            }

            public void Write(IDebugState debugState)
            {

                if(_account.InUse && _account.Owner != null)
                {
                    debugState.ServerID = _server._serverID;

                    if(debugState.ExecutionOrigin == ExecutionOrigin.Debug)
                    {
                        debugState.ExecutingUser = Environment.UserName;
                    }

                    var p = new Packet(PacketTemplates.Client_OnDebugWriterWrite);
                    debugState.Write(p);
                    _account.Owner.Send(p);
                }
                else
                {
                    DebugDispatcher.Instance.Remove(_account.AccountID);
                }
            }
        }
        #endregion

        #region Implementation of IContextManager<IStudioNetworkSession>

        public IList<IStudioNetworkSession> CurrentContexts
        {
            get
            {
                return AttachedContexts.Cast<IStudioNetworkSession>().ToList();
            }
        }

        #endregion

        #region OnEventProviderServerMessageReceived

        // PBI 6690 - 2013.07.04 - TWR : added
        void OnEventProviderServerMessageReceived(INetworkOperator op, StudioNetworkSession context, ByteBuffer reader)
        {
            var payLoad = XElement.Parse(reader.ReadString());
            var jsonObj = payLoad.Nodes().OfType<XCData>().FirstOrDefault();
            if(jsonObj != null)
            {
                //_pushService.ProcessRequest(context, jsonObj.Value)
                //            .ContinueWith(t =>
                //            {
                //                string result = t.Result;
                //                WriteEventProviderClientMessage(result, op);
                //            });
            }
        }

        #endregion

        #region OnCompilerMessageReceived

        protected void OnCompilerMessageReceived(IList<CompileMessageTO> messages)
        {
            WriteEventProviderClientMessage<DesignValidationMemo>(messages.Where(m => m.MessageType == CompileMessageType.MappingChange || m.MessageType == CompileMessageType.MappingIsRequiredChanged), CoalesceMappingChangedErrors);
            WriteEventProviderClientMessage<DesignValidationMemo>(messages.Where(m => m.MessageType == CompileMessageType.ResourceSaved), CoalesceResourceSavedErrors);
            //WriteEventProviderClientMessage<DesignValidationMemo>(messages.Where(m => m.MessageType == CompileMessageType.MappingIsRequiredChanged), CoalesceMappingChangedErrors);
        }

        #endregion

        #region CoalesceMappingChangedErrors

        static void CoalesceMappingChangedErrors(DesignValidationMemo memo, CompileMessageTO compilerMessage)
        {
            memo.ServiceName = compilerMessage.ServiceName;
            memo.IsValid = false;
            memo.Errors.Add(compilerMessage.ToErrorInfo());
        }

        #endregion

        #region CoalesceResourceSavedErrors

        static void CoalesceResourceSavedErrors(DesignValidationMemo memo, CompileMessageTO compilerMessage)
        {
            memo.ServiceName = compilerMessage.ServiceName;
            memo.IsValid = true;
        }

        #endregion

        #region WriteEventProviderClientMessage

        void WriteEventProviderClientMessage<TMemo>(IEnumerable<CompileMessageTO> messages, Action<TMemo, CompileMessageTO> coalesceErrors)
            where TMemo : IMemo, new()
        {
            var messageArray = messages.ToArray();
            if(messageArray.Length == 0)
            {
                return;
            }

            // we need to broadcast per service and per unique ID messages
            var serviceGroupings = messageArray.GroupBy(to => to.ServiceID);
            WriteEventProviderClientMessage(serviceGroupings, coalesceErrors);

            var instanceGroupings = messageArray.GroupBy(to => to.UniqueID);
            WriteEventProviderClientMessage(instanceGroupings, coalesceErrors);
        }

        void WriteEventProviderClientMessage<TMemo>(IEnumerable<IGrouping<Guid, CompileMessageTO>> groupings, Action<TMemo, CompileMessageTO> coalesceErrors)
            where TMemo : IMemo, new()
        {
            var memo = new TMemo { ServerID = _serverID };
            foreach(var grouping in groupings)
            {
                if(grouping.Key == Guid.Empty)
                {
                    continue;
                }
                memo.InstanceID = grouping.Key;
                foreach(var message in grouping)
                {
                    memo.WorkspaceID = message.WorkspaceID;
                    coalesceErrors(memo, message);
                }
                WriteEventProviderClientMessage(memo, Connections);
            }
        }

        void WriteEventProviderClientMessage(IMemo memo, params INetworkOperator[] operators)
        {
            WriteEventProviderClientMessage(memo, operators.ToList());
        }

        protected virtual void WriteEventProviderClientMessage(IMemo memo, IEnumerable<INetworkOperator> operators)
        {
            VerifyArgument.IsNotNull("memo", memo);
            if(operators == null)
            {
                return;
            }
            var envelope = memo.ToString(_serializer);

            var p = new Packet(PacketTemplates.EventProviderClientMessage);
            p.Write(envelope);

            foreach(var op in operators)
            {
                op.Send(p);
            }
        }

        #endregion

    }
}
