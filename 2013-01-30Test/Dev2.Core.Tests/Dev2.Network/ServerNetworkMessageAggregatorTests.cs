using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unlimited.UnitTest.Framework.Dev2.Network
{
    [TestClass]
    public class ServerNetworkMessageAggregatorTests
    {
        [TestMethod]
        public void Subscribe_WhereCallbackValid_Expect_NonEmptyToken()
        {
            ServerNetworkMessageAggregator<MockNetworkContext> serverNetworkMessageAggregator = new ServerNetworkMessageAggregator<MockNetworkContext>();

            Guid subscriptionToken = serverNetworkMessageAggregator.Subscribe(new Action<TestMessage, IServerNetworkChannelContext<MockNetworkContext>>((message, context) =>
            {
            }));

            Assert.AreNotEqual(subscriptionToken, Guid.Empty);
        }

        [TestMethod]
        public void Subscribe_WhereCallbackNull_Expect_EmptyToken()
        {
            ServerNetworkMessageAggregator<MockNetworkContext> serverNetworkMessageAggregator = new ServerNetworkMessageAggregator<MockNetworkContext>();

            Guid subscriptionToken = serverNetworkMessageAggregator.Subscribe<TestMessage>(null);

            Assert.AreEqual(subscriptionToken, Guid.Empty);
        }

        [TestMethod]
        public void Unsubscribe_WhereTokenIsRegistered_Expect_True()
        {
            ServerNetworkMessageAggregator<MockNetworkContext> serverNetworkMessageAggregator = new ServerNetworkMessageAggregator<MockNetworkContext>();

            Guid subscriptionToken = serverNetworkMessageAggregator.Subscribe(new Action<TestMessage, IServerNetworkChannelContext<MockNetworkContext>>((message, context) =>
            {
            }));

            bool result = serverNetworkMessageAggregator.Unsubscibe(subscriptionToken);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Unsubscribe_WhereTokenIsntRegistered_Expect_False()
        {
            ServerNetworkMessageAggregator<MockNetworkContext> serverNetworkMessageAggregator = new ServerNetworkMessageAggregator<MockNetworkContext>();

            bool result = serverNetworkMessageAggregator.Unsubscibe(Guid.NewGuid());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SubscribeUnsubscribe_WhereCallbackIsTheSameInstance_Expected_True()
        {
            ServerNetworkMessageAggregator<MockNetworkContext> serverNetworkMessageAggregator = new ServerNetworkMessageAggregator<MockNetworkContext>();

            Action<TestMessage, IServerNetworkChannelContext<MockNetworkContext>> callback = new Action<TestMessage, IServerNetworkChannelContext<MockNetworkContext>>((message, context) =>
            {
            });

            Guid subscriptionToken1 = serverNetworkMessageAggregator.Subscribe(callback);
            Guid subscriptionToken2 = serverNetworkMessageAggregator.Subscribe(callback);

            bool result1 = serverNetworkMessageAggregator.Unsubscibe(subscriptionToken1);
            bool result2 = serverNetworkMessageAggregator.Unsubscibe(subscriptionToken2);

            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Publish_TestMessage_Expected_SameTestMessageInSubscriptionCallback()
        {
            ServerNetworkMessageAggregator<MockNetworkContext> serverNetworkMessageAggregator = new ServerNetworkMessageAggregator<MockNetworkContext>();

            TestMessage actual = null;
            TestMessage expected = new TestMessage("cake", 1);

            Guid subscriptionToken = serverNetworkMessageAggregator.Subscribe(new Action<TestMessage, IServerNetworkChannelContext<MockNetworkContext>>((message, context) =>
            {
                actual = message;
            }));

            serverNetworkMessageAggregator.Publish(expected, false);

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Publish_TestMessage_WhereNetworkContextNotNull_Expected_NetworkContextInSubscriptionCallback()
        {
            ServerNetworkMessageAggregator<MockNetworkContext> serverNetworkMessageAggregator = new ServerNetworkMessageAggregator<MockNetworkContext>();

            IServerNetworkChannelContext<MockNetworkContext> actual = null;
            IServerNetworkChannelContext<MockNetworkContext> expected = new MockServerNetworkChannelContext(null, null);

            Guid subscriptionToken = serverNetworkMessageAggregator.Subscribe(new Action<TestMessage, IServerNetworkChannelContext<MockNetworkContext>>((message, context) =>
            {
                actual = context;
            }));

            serverNetworkMessageAggregator.Publish(new TestMessage(), expected, false);

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Publish_NullMessage_Expected_CallbackNotRun()
        {
            ServerNetworkMessageAggregator<MockNetworkContext> serverNetworkMessageAggregator = new ServerNetworkMessageAggregator<MockNetworkContext>();

            TestMessage actual = null;

            Guid subscriptionToken = serverNetworkMessageAggregator.Subscribe(new Action<TestMessage, IServerNetworkChannelContext<MockNetworkContext>>((message, context) =>
            {
                actual = new TestMessage();
            }));

            serverNetworkMessageAggregator.Publish(null, false);

            Assert.IsNull(actual);
        }
    }
}
