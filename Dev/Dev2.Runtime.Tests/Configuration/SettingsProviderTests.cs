using System;
using System.Network;
using Dev2.Network;
using Dev2.Network.Messaging.Messages;
using Dev2.Runtime.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Configuration
{
    [TestClass]
    public class SettingsProviderTests
    {
        #region Instance

        [TestMethod]
        public void InstanceExpectedReturnsSingletonInstance()
        {
            var provider1 = SettingsProvider.Instance;
            var provider2 = SettingsProvider.Instance;
            Assert.AreSame(provider1, provider2);
        }

        [TestMethod]
        public void InstanceExpectedIsNotNull()
        {
            var provider = SettingsProvider.Instance;
            Assert.IsNotNull(provider);
        }

        #endregion

        #region CTOR

        [TestMethod]
        public void ConstructorExpectedInitializesProperties()
        {
            var provider = new Mock<SettingsProviderBase>();
            Assert.IsNotNull(provider.Object.Backup);
            Assert.IsNotNull(provider.Object.Logging);
            Assert.IsNotNull(provider.Object.Security);
        }

        #endregion

        #region Start

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartWithNullArgumentsExpectedThrowsArgumentNullException()
        {
            var provider = new Mock<SettingsProviderBase>();
            provider.Object.Start<NetworkContextMock>(null, null);
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedInvokesSubscribeMethod()
        {
            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>())).Verifiable();

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new Mock<SettingsProviderBase>();
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            aggregator.Verify(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>()));
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedSetsSubscriptionToken()
        {
            var expectedToken = Guid.NewGuid();
            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>())).Returns(expectedToken);

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new Mock<SettingsProviderBase>();
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            Assert.AreEqual(expectedToken, provider.Object.SubscriptionToken);
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedSetsMessageResponseHandleToRequestHandle()
        {
            const int MessageRequestHandle = 32;

            var messageResponse = new Mock<ISettingsMessage>();
            messageResponse.SetupProperty(m => m.Handle);

            var messageRequest = new Mock<ISettingsMessage>();
            messageRequest.Setup(m => m.Handle).Returns(MessageRequestHandle);

            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>()))
                .Callback<Action<ISettingsMessage, NetworkContext>>(action => action(messageRequest.Object, context.Object));

            long actualHandle = -1;
            var messageBroker = new Mock<INetworkMessageBroker>();
            messageBroker.Setup(m => m.Send(It.IsAny<ISettingsMessage>(), It.IsAny<INetworkOperator>()))
                         .Callback<ISettingsMessage, INetworkOperator>((sm, no) => actualHandle = sm.Handle);

            var provider = new Mock<SettingsProviderBase>();
            provider.Setup(p => p.ProcessMessage(It.IsAny<ISettingsMessage>())).Returns(messageResponse.Object);
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            Assert.AreEqual(MessageRequestHandle, actualHandle);
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedInvokesSendMethodWithResult()
        {
            var message = new Mock<ISettingsMessage>();
            var messageResponse = new Mock<ISettingsMessage>();
            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>()))
                .Callback<Action<ISettingsMessage, NetworkContext>>(action => action(message.Object, context.Object));

            var messageBroker = new Mock<INetworkMessageBroker>();
            messageBroker.Setup(m => m.Send(It.IsAny<ISettingsMessage>(), It.IsAny<INetworkOperator>())).Verifiable();

            var provider = new Mock<SettingsProviderBase>();
            provider.Setup(p => p.ProcessMessage(It.IsAny<ISettingsMessage>())).Returns(messageResponse.Object);
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            messageBroker.Verify(m => m.Send(It.IsAny<ISettingsMessage>(), It.IsAny<INetworkOperator>()));
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedInvokesProcessMessage()
        {
            var message = new Mock<ISettingsMessage>();
            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>()))
                .Callback<Action<ISettingsMessage, NetworkContext>>(action => action(message.Object, context.Object));

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new Mock<SettingsProviderBase>();
            provider.Setup(m => m.ProcessMessage(It.IsAny<ISettingsMessage>())).Verifiable();
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            provider.Verify(m => m.ProcessMessage(It.IsAny<ISettingsMessage>()));
        }

        [TestMethod]
        public void StartWithValidArgumentsAndExecutionErrorExpectedInvokesSendMethodWithErrorMessage()
        {
            var message = new Mock<ISettingsMessage>();
            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>()))
                .Callback<Action<ISettingsMessage, NetworkContext>>(action => action(message.Object, context.Object));

            var messageBroker = new Mock<INetworkMessageBroker>();
            messageBroker.Setup(m => m.Send(It.IsAny<ErrorMessage>(), It.IsAny<INetworkOperator>())).Verifiable();

            var provider = new Mock<SettingsProviderBase>();
            provider.Setup(m => m.ProcessMessage(It.IsAny<ISettingsMessage>())).Callback(() => { throw new Exception("Exception occurred"); });
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            messageBroker.Verify(m => m.Send(It.IsAny<ErrorMessage>(), It.IsAny<INetworkOperator>()));
        }

        #endregion

        #region Stop

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StopWithNullArgumentsExpectedThrowsArgumentNullException()
        {
            var provider = new Mock<SettingsProviderBase>();
            provider.Object.Stop<NetworkContextMock>(null);
        }

        [TestMethod]
        public void StopWithAggregatorExpectedExecuteUnsubscribeMethodOfAggregator()
        {
            var expectedToken = Guid.NewGuid();
            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>())).Returns(expectedToken);
            aggregator.Setup(a => a.Unsubscibe(It.IsAny<Guid>())).Verifiable();

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new Mock<SettingsProviderBase>();
            provider.Object.Start(aggregator.Object, messageBroker.Object);
            provider.Object.Stop(aggregator.Object);

            aggregator.Verify(a => a.Unsubscibe(It.IsAny<Guid>()));
        }

        #endregion


    }
}
