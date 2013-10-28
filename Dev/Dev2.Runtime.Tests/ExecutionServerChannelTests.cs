using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.DynamicServices.Network.Execution;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.DynamicServices.Test
{
    [TestClass][ExcludeFromCodeCoverage]
    public class ExecutionServerChannelTests
    {
        #region MyTestInitialize

        [TestInitialize()]
        public void Initialize()
        {
        }

        #endregion

        #region Initialization Tests

        [TestMethod]
        public void Instantiate_Expected_Success()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);

            channel.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_MessageBrokerIsNull_Expected_ArgumentNullException()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ExecutionServerChannel channel = new ExecutionServerChannel(null, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);

            channel.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_MessageAggregatorIsNull_Expected_ArgumentNullException()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, null, _executionStatusCallbackDispatcher.Object);

            channel.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_DispatcherIsNull_Expected_ArgumentNullException()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, null);

            channel.Dispose();
        }

        #endregion Initialization Tests

        #region Execution Status Callback

        [TestMethod]
        public void AddExecutionStatusCallback_Where_DispatcherAddSucseeds_Expected_True()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);
            channel.Context = _context.Object;
            bool result = channel.AddExecutionStatusCallback(Guid.Empty, new Action<ExecutionStatusCallbackMessage>(m => {}));

            _executionStatusCallbackDispatcher.Verify(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>()), Times.Exactly(1));

            channel.Dispose();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AddExecutionStatusCallback_Where_DispatcherAddFails_Expected_False()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher(false);
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);
            channel.Context = _context.Object;
            bool result = channel.AddExecutionStatusCallback(Guid.Empty, new Action<ExecutionStatusCallbackMessage>(m => { }));

            _executionStatusCallbackDispatcher.Verify(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>()), Times.Exactly(1));
            channel.Dispose();
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddExecutionStatusCallback_Where_CallBackIsNull_Expected_ArgumentNullException()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);
            channel.Context = _context.Object;
            channel.AddExecutionStatusCallback(Guid.Empty, null);
            channel.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void AddExecutionStatusCallback_Where_ContextIsNull_Expected_NullReferenceException()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher(false);

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);
            channel.AddExecutionStatusCallback(Guid.Empty, new Action<ExecutionStatusCallbackMessage>(m => { }));
            channel.Dispose();
        }


        [TestMethod]
        public void RemoveExecutionStatusCallback_Where_DispatcherRemoveSucseeds_Expected_True()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);
            channel.Context = _context.Object;
            bool result = channel.RemoveExecutionStatusCallback(Guid.Empty);

            _executionStatusCallbackDispatcher.Verify(e => e.Remove(It.IsAny<Guid>()), Times.Exactly(1));
            channel.Dispose();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RemoveExecutionStatusCallback_Where_DispatcherRemoveFails_Expected_False()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher(false, false);
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);
            channel.Context = _context.Object;
            bool result = channel.RemoveExecutionStatusCallback(Guid.Empty);

            _executionStatusCallbackDispatcher.Verify(e => e.Remove(It.IsAny<Guid>()), Times.Exactly(1));
            channel.Dispose();
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void RemoveExecutionStatusCallback_Where_ContextIsNull_Expected_NullReferenceException()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher(false);

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _executionStatusCallbackDispatcher.Object);
            channel.RemoveExecutionStatusCallback(Guid.Empty);
            channel.Dispose();
        }

        #endregion Execution Status Callback

        #region Message Recieving

        [TestMethod]
        public void ExecutionStatusCallbackMessage_Where_MessageSignalsAdd_Expected_AddedToDispatcher()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _executionStatusCallbackDispatcher.Object);

            tmpServerNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(Guid.Empty, ExecutionStatusCallbackMessageType.Add), _context.Object, false);

            _executionStatusCallbackDispatcher.Verify(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>()), Times.Exactly(1));
            channel.Dispose();
        }

        [TestMethod]
        public void ExecutionStatusCallbackMessage_Where_MessageSignalsRemove_Expected_RemovedFromDispatcher()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _executionStatusCallbackDispatcher.Object);

            tmpServerNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(Guid.Empty, ExecutionStatusCallbackMessageType.Remove), _context.Object, false);

            _executionStatusCallbackDispatcher.Verify(e => e.Remove(It.IsAny<Guid>()), Times.Exactly(1));
            channel.Dispose();
        }

        [TestMethod]
        public void ExecutionStatusCallbackMessage_Where_MessageDoesntSignalsAddOrRemove_Expected_NoActionOnDispatcher()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _executionStatusCallbackDispatcher.Object);

            tmpServerNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(Guid.Empty, ExecutionStatusCallbackMessageType.Unknown), _context.Object, false);
            tmpServerNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(Guid.Empty, ExecutionStatusCallbackMessageType.BookmarkedCallback), _context.Object, false);
            tmpServerNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(Guid.Empty, ExecutionStatusCallbackMessageType.CompletedCallback), _context.Object, false);
            tmpServerNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(Guid.Empty, ExecutionStatusCallbackMessageType.StartedCallback), _context.Object, false);

            _executionStatusCallbackDispatcher.Verify(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>()), Times.Exactly(0));
            _executionStatusCallbackDispatcher.Verify(e => e.Remove(It.IsAny<Guid>()), Times.Exactly(0));

            channel.Dispose();
        }

        [TestMethod]
        public void NetworkContextDetachedMessage_Expected_RegisteredMessagesForContextDetached()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();
            
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ExecutionServerChannel channel = new ExecutionServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _executionStatusCallbackDispatcher.Object);

            tmpServerNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(Guid.NewGuid(), ExecutionStatusCallbackMessageType.Add), _context.Object, false);
            tmpServerNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(Guid.NewGuid(), ExecutionStatusCallbackMessageType.Add), _context.Object, false);

            tmpServerNetworkMessageAggregator.Publish(new NetworkContextDetachedMessage(), _context.Object, false);

            _executionStatusCallbackDispatcher.Verify(e => e.Remove(It.IsAny<Guid>()), Times.Exactly(2));

            channel.Dispose();
        }

        #endregion Message Recieving
    }
}
