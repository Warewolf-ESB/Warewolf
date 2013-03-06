using System;
using System.Network;
using Dev2.Network;
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
            var provider = new SettingsProvider();
            Assert.IsNotNull(provider.Backup);
            Assert.IsNotNull(provider.Logging);
            Assert.IsNotNull(provider.Security);
        }

        #endregion

        #region Start

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartWithNullArgumentsExpectedThrowsArgumentNullException()
        {
            var provider = new SettingsProvider();
            provider.Start<NetworkContextMock>(null, null);
        }

        [TestMethod]
        public void StartWithAggregatorExpectedInvokesExecuteSubscribeMethodOfAggregator()
        {
            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>())).Verifiable();

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new SettingsProvider();
            provider.Start(aggregator.Object, messageBroker.Object);

            aggregator.Verify(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>()));
        }

        [TestMethod]
        public void StartWithAggregatorExpectedSetsSubscriptionToken()
        {
            var expectedToken = Guid.NewGuid();
            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>())).Returns(expectedToken);

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new SettingsProvider();
            provider.Start(aggregator.Object, messageBroker.Object);

            Assert.AreEqual(expectedToken, provider.SubscriptionToken);
        }

        [TestMethod]
        public void StartWithAggregatorAndMessageBrokerExpectedInvokesSendMethodOfMessageBroker()
        {
            var setttingsMessage = new Mock<ISettingsMessage>();
            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>()))
                .Callback<Action<ISettingsMessage, NetworkContext>>(action => action(setttingsMessage.Object, context.Object));

            var messageBroker = new Mock<INetworkMessageBroker>();
            messageBroker.Setup(m => m.Send(It.IsAny<ISettingsMessage>(), It.IsAny<INetworkOperator>())).Verifiable();

            var provider = new SettingsProvider();
            provider.Start(aggregator.Object, messageBroker.Object);

            messageBroker.Verify(m => m.Send(It.IsAny<ISettingsMessage>(), It.IsAny<INetworkOperator>()));
        }

        [TestMethod]
        public void StartWithAggregatorAndMessageBrokerExpectedInvokesExecuteMethodOfProvider()
        {
            var setttingsMessage = new Mock<ISettingsMessage>();
            setttingsMessage.Setup(m => m.Execute()).Verifiable();

            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>()))
                .Callback<Action<ISettingsMessage, NetworkContext>>(action => action(setttingsMessage.Object, context.Object));

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new SettingsProvider();
            provider.Start(aggregator.Object, messageBroker.Object);

            setttingsMessage.Verify(m => m.Execute());
        }

        #endregion

        #region Stop

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StopWithNullArgumentsExpectedThrowsArgumentNullException()
        {
            var provider = new SettingsProvider();
            provider.Stop<NetworkContextMock>(null);
        }

        [TestMethod]
        public void StopWithAggregatorExpectedExecuteUnsubscribeMethodOfAggregator()
        {
            var expectedToken = Guid.NewGuid();
            var aggregator = new Mock<IServerNetworkMessageAggregator<NetworkContextMock>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<ISettingsMessage, NetworkContext>>())).Returns(expectedToken);
            aggregator.Setup(a => a.Unsubscibe(It.IsAny<Guid>())).Verifiable();

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new SettingsProvider();
            provider.Start(aggregator.Object, messageBroker.Object);
            provider.Stop(aggregator.Object);

            aggregator.Verify(a => a.Unsubscibe(It.IsAny<Guid>()));
        }

        #endregion


    }
}
