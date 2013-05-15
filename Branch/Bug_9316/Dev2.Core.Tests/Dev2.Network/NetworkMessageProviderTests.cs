using System;
using System.Network;
using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Tests.Dev2.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable CheckNamespace
namespace Unlimited.UnitTest.Framework.Dev2.Network
// ReSharper restore CheckNamespace
{
    [TestClass]
    [Ignore]
    public class NetworkMessageProviderTests
    {
        #region Start

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartWithNullAggregatorExpectedThrowsArgumentNullException()
        {
            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            provider.Object.Start<MockNetworkContext>(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartWithNullMessageBrokerExpectedThrowsArgumentNullException()
        {
            var aggregator = new Mock<IServerNetworkMessageAggregator<MockNetworkContext>>();
            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            provider.Object.Start(aggregator.Object, null);
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedInvokesSubscribeMethod()
        {
            var aggregator = new Mock<IServerNetworkMessageAggregator<MockNetworkContext>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<MockNetworkMessage, NetworkContext>>())).Verifiable();

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            aggregator.Verify(a => a.Subscribe(It.IsAny<Action<MockNetworkMessage, NetworkContext>>()));
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedSetsSubscriptionToken()
        {
            var expectedToken = Guid.NewGuid();
            var aggregator = new Mock<IServerNetworkMessageAggregator<MockNetworkContext>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<MockNetworkMessage, NetworkContext>>())).Returns(expectedToken);

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            Assert.AreEqual(expectedToken, provider.Object.SubscriptionToken);
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedSetsMessageResponseHandleToRequestHandle()
        {
            const int MessageRequestHandle = 32;

            //var messageResponse = new Mock<INetworkMessage>();
            //messageResponse.SetupProperty(m => m.Handle);
            var messageResponse = new MockNetworkMessage();

            //var messageRequest = new Mock<INetworkMessage>();
            //messageRequest.Setup(m => m.Handle).Returns(MessageRequestHandle);
            var messageRequest = new MockNetworkMessage
            {
                Handle = MessageRequestHandle
            };

            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<MockNetworkContext>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<MockNetworkMessage, NetworkContext>>()))
                //.Callback<Action<MockNetworkMessage, NetworkContext>>(action => action(messageRequest.Object, context.Object));
                .Callback<Action<MockNetworkMessage, NetworkContext>>(action => action(messageRequest, context.Object));

            long actualHandle = -1;
            var messageBroker = new Mock<INetworkMessageBroker>();
            messageBroker.Setup(m => m.Send(It.IsAny<MockNetworkMessage>(), It.IsAny<INetworkOperator>()))
                         .Callback<MockNetworkMessage, INetworkOperator>((sm, no) => actualHandle = sm.Handle);

            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            //provider.Setup(p => p.ProcessMessage(It.IsAny<MockNetworkMessage>())).Returns(messageResponse.Object);
            provider.Setup(p => p.ProcessMessage(It.IsAny<MockNetworkMessage>())).Returns(messageResponse);
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            Assert.AreEqual(MessageRequestHandle, actualHandle);
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedInvokesSendMethodWithResult()
        {
            //var message = new Mock<INetworkMessage>();
            //var messageResponse = new Mock<INetworkMessage>();
            var message = new MockNetworkMessage();
            var messageResponse = new MockNetworkMessage();
            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<MockNetworkContext>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<MockNetworkMessage, NetworkContext>>()))
                //.Callback<Action<MockNetworkMessage, NetworkContext>>(action => action(message.Object, context.Object));
                .Callback<Action<MockNetworkMessage, NetworkContext>>(action => action(message, context.Object));

            var messageBroker = new Mock<INetworkMessageBroker>();
            messageBroker.Setup(m => m.Send(It.IsAny<MockNetworkMessage>(), It.IsAny<INetworkOperator>())).Verifiable();

            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            //provider.Setup(p => p.ProcessMessage(It.IsAny<MockNetworkMessage>())).Returns(messageResponse.Object);
            provider.Setup(p => p.ProcessMessage(It.IsAny<MockNetworkMessage>())).Returns(messageResponse);
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            messageBroker.Verify(m => m.Send(It.IsAny<MockNetworkMessage>(), It.IsAny<INetworkOperator>()));
        }

        [TestMethod]
        public void StartWithValidArgumentsExpectedInvokesProcessMessage()
        {
            //var message = new Mock<INetworkMessage>();
            var message = new MockNetworkMessage();
            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<MockNetworkContext>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<MockNetworkMessage, NetworkContext>>()))
                //.Callback<Action<INetworkMessage, NetworkContext>>(action => action(message.Object, context.Object));
                .Callback<Action<MockNetworkMessage, NetworkContext>>(action => action(message, context.Object));

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            provider.Setup(m => m.ProcessMessage(It.IsAny<MockNetworkMessage>())).Verifiable();
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            provider.Verify(m => m.ProcessMessage(It.IsAny<MockNetworkMessage>()));
        }

        [TestMethod]
        public void StartWithValidArgumentsAndExecutionErrorExpectedInvokesSendMethodWithErrorMessage()
        {
            //var message = new Mock<INetworkMessage>();
            var message = new MockNetworkMessage();
            var context = new Mock<NetworkContext>();

            var aggregator = new Mock<IServerNetworkMessageAggregator<MockNetworkContext>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<MockNetworkMessage, NetworkContext>>()))
                //.Callback<Action<MockNetworkMessage, NetworkContext>>(action => action(message.Object, context.Object));
                .Callback<Action<MockNetworkMessage, NetworkContext>>(action => action(message, context.Object));

            var messageBroker = new Mock<INetworkMessageBroker>();
            messageBroker.Setup(m => m.Send(It.IsAny<ErrorMessage>(), It.IsAny<INetworkOperator>())).Verifiable();

            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            provider.Setup(m => m.ProcessMessage(It.IsAny<MockNetworkMessage>())).Callback(() => { throw new Exception("Exception occurred"); });
            provider.Object.Start(aggregator.Object, messageBroker.Object);

            messageBroker.Verify(m => m.Send(It.IsAny<ErrorMessage>(), It.IsAny<INetworkOperator>()));
        }

        #endregion

        #region Stop

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StopWithNullArgumentsExpectedThrowsArgumentNullException()
        {
            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            provider.Object.Stop<MockNetworkContext>(null);
        }

        [TestMethod]
        public void StopWithAggregatorExpectedExecuteUnsubscribeMethodOfAggregator()
        {
            var expectedToken = Guid.NewGuid();
            var aggregator = new Mock<IServerNetworkMessageAggregator<MockNetworkContext>>();
            aggregator.Setup(a => a.Subscribe(It.IsAny<Action<MockNetworkMessage, NetworkContext>>())).Returns(expectedToken);
            aggregator.Setup(a => a.Unsubscibe(It.IsAny<Guid>())).Verifiable();

            var messageBroker = new Mock<INetworkMessageBroker>();

            var provider = new Mock<NetworkMessageProviderBase<MockNetworkMessage>>();
            provider.Object.Start(aggregator.Object, messageBroker.Object);
            provider.Object.Stop(aggregator.Object);

            aggregator.Verify(a => a.Unsubscibe(It.IsAny<Guid>()));
        }

        #endregion


    }
}
