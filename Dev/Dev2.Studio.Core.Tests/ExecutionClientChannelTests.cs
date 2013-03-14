using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Network;
using Dev2.Composition;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Network.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ExecutionClientChannelTests
    {
        #region MyTestInitialize

        [TestInitialize()]
        public void Initialize()
        {
        }

        #endregion

        #region Initialization Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_ClientIsNull_Expected_Exception()
        {
            ExecutionClientChannel channel = new ExecutionClientChannel(null);
        }

        [TestMethod]
        public void Instantiate_Expected_Success()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
        }

        [TestMethod]
        [ExpectedException(typeof(ImportCardinalityMismatchException))]
        public void Instantiate_Where_RequiredMefImportsAreMissing_Expected_Exception()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);
            
            ImportService.CurrentContext = CompositionInitializer.EmptyInitialize();
            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
        }

        #endregion Initialization Tests

        #region Execution Status Callback

        [TestMethod]
        public void AddExecutionStatusCallback_Where_AddToDispatcherSuceeds_Expected_AddedToDispatcherAndMessageSentAndTrue()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            bool result = channel.AddExecutionStatusCallback(Guid.NewGuid(), new Action<ExecutionStatusCallbackMessage>(m => {}));

            _executionStatusCallbackDispatcher.Verify(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>()), Times.Exactly(1));
            _networkMessageBroker.Verify(e => e.Send(It.IsAny<ExecutionStatusCallbackMessage>(), It.IsAny<INetworkOperator>()), Times.Exactly(1));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AddExecutionStatusCallback_Where_AddToDispatcherSuceedsAndMessageBrokerSendThrowsException_Expected_AddedAndRemovedFromDispatcherAndFalse()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>(true);
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            bool result = channel.AddExecutionStatusCallback(Guid.NewGuid(), new Action<ExecutionStatusCallbackMessage>(m => { }));

            _executionStatusCallbackDispatcher.Verify(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>()), Times.Exactly(1));
            _executionStatusCallbackDispatcher.Verify(e => e.Remove(It.IsAny<Guid>()), Times.Exactly(1));
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AddExecutionStatusCallback_Where_AddToDispatcherFails_Expected_False()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher(false);

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            bool result = channel.AddExecutionStatusCallback(Guid.NewGuid(), new Action<ExecutionStatusCallbackMessage>(m => { }));

            _executionStatusCallbackDispatcher.Verify(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>()), Times.Exactly(1));

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddExecutionStatusCallback_Where_CallbackIsNull_Expected_Exception()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            channel.AddExecutionStatusCallback(Guid.NewGuid(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void AddExecutionStatusCallback_Where_DispatcherIsNull_Expected_Exception()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            channel.ExecutionStatusCallbackDispatcher = null;
            channel.AddExecutionStatusCallback(Guid.NewGuid(), new Action<ExecutionStatusCallbackMessage>(m => { }));
        }


        [TestMethod]
        public void RemoveExecutionStatusCallback_Where_RemoveFromDispatcherSuceeds_Expected_RemovedFromDispatcherAndMessageSentAndTrue()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            bool result = channel.RemoveExecutionStatusCallback(Guid.NewGuid());

            _executionStatusCallbackDispatcher.Verify(e => e.Remove(It.IsAny<Guid>()), Times.Exactly(1));
            _networkMessageBroker.Verify(e => e.Send(It.IsAny<ExecutionStatusCallbackMessage>(), It.IsAny<INetworkOperator>()), Times.Exactly(1));

            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void RemoveExecutionStatusCallback_Where_RemoveFromDispatcherSuceedsAndMessageBrokerThrowsException_Expected_Exception()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>(true);
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            channel.RemoveExecutionStatusCallback(Guid.NewGuid());
        }

        [TestMethod]
        public void RemoveExecutionStatusCallback_Where_RemoveFromDispatcherFails_Expected_False()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher(true, false);

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            bool result = channel.RemoveExecutionStatusCallback(Guid.NewGuid());

            _executionStatusCallbackDispatcher.Verify(e => e.Remove(It.IsAny<Guid>()), Times.Exactly(1));

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void RemoveExecutionStatusCallback_Where_DispatcherIsNull_Expected_Exception()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            channel.ExecutionStatusCallbackDispatcher = null;
            channel.RemoveExecutionStatusCallback(Guid.NewGuid());
        }

        #endregion Execution Status Callback

        #region Message Recieving

        [TestMethod]
        public void ExecutionStatusCallbackMessage_Where_MessageAppliesToChannel_Expected_PostRun()
        {
            NetworkMessageBroker tmpNetworkMessageBroker = new NetworkMessageBroker();
            StudioNetworkMessageAggregator tmpStudioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, tmpNetworkMessageBroker, tmpStudioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));

            tmpStudioNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(), new StudioNetworkChannelContext(null, Guid.Empty, Guid.Empty), false);

            _executionStatusCallbackDispatcher.Verify(e => e.Post(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Exactly(1));
        }

        [TestMethod]
        public void ExecutionStatusCallbackMessage_Where_MessageDoesntApplyToChannel_Expected_PostNotRun()
        {
            NetworkMessageBroker tmpNetworkMessageBroker = new NetworkMessageBroker();
            StudioNetworkMessageAggregator tmpStudioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelMessaegRecievingTests(tmpNetworkMessageBroker, tmpStudioNetworkMessageAggregator);

            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));

            tmpStudioNetworkMessageAggregator.Publish(new ExecutionStatusCallbackMessage(), new StudioNetworkChannelContext(null, Guid.NewGuid(), Guid.NewGuid()), false);

            _executionStatusCallbackDispatcher.Verify(e => e.Post(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Exactly(0));
        }

        [TestMethod]
        public void NetworkContextDetachedMessage_Where_MessageAppliesToChannel_Expected_RemoveRangeRun()
        {
            NetworkMessageBroker tmpNetworkMessageBroker = new NetworkMessageBroker();
            StudioNetworkMessageAggregator tmpStudioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, tmpNetworkMessageBroker, tmpStudioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            channel.AddExecutionStatusCallback(Guid.NewGuid(), new Action<ExecutionStatusCallbackMessage>(m => { }));

            tmpStudioNetworkMessageAggregator.Publish(new NetworkContextDetachedMessage(), new StudioNetworkChannelContext(null, Guid.Empty, Guid.Empty), false);

            _executionStatusCallbackDispatcher.Verify(e => e.RemoveRange(It.IsAny<IList<Guid>>()), Times.Exactly(1));
        }

        [TestMethod]
        public void NetworkContextDetachedMessage_Where_MessageDoesntApplyToChannel_Expected_RemoveRangeNotRun()
        {
            NetworkMessageBroker tmpNetworkMessageBroker = new NetworkMessageBroker();
            StudioNetworkMessageAggregator tmpStudioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelMessaegRecievingTests(tmpNetworkMessageBroker, tmpStudioNetworkMessageAggregator);

            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ExecutionStatusCallbackMessage>();
            Mock<IExecutionStatusCallbackDispatcher> _executionStatusCallbackDispatcher = Dev2MockFactory.SetupExecutionStatusCallbackDispatcher();

            ImportService.CurrentContext = CompositionInitializer.InitializeForExecutionChannelTests(_executionStatusCallbackDispatcher, _networkMessageBroker, _studioNetworkMessageAggregator);

            ExecutionClientChannel channel = new ExecutionClientChannel(new TCPDispatchedClient(""));
            channel.AddExecutionStatusCallback(Guid.NewGuid(), new Action<ExecutionStatusCallbackMessage>(m => { }));

            tmpStudioNetworkMessageAggregator.Publish(new NetworkContextDetachedMessage(), new StudioNetworkChannelContext(null, Guid.NewGuid(), Guid.NewGuid()), false);

            _executionStatusCallbackDispatcher.Verify(e => e.RemoveRange(It.IsAny<IList<Guid>>()), Times.Exactly(0));
        }

        #endregion Message Recieving
    }
}
