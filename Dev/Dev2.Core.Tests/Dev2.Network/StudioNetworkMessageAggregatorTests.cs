using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using System;

namespace Unlimited.UnitTest.Framework.Dev2.Network
{
    [TestClass][ExcludeFromCodeCoverage]
    public class StudioNetworkMessageAggregatorTests
    {
        [TestMethod]
        public void Subscribe_WhereCallbackValid_Expect_NonEmptyToken()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            Guid subscriptionToken = studioNetworkMessageAggregator.Subscribe(new Action<TestMessage, IStudioNetworkChannelContext>((message, context) =>
            {
            }));

            Assert.AreNotEqual(subscriptionToken, Guid.Empty);
        }

        [TestMethod]
        public void Unsubscribe_WhereTokenIsRegistered_Expect_True()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            Guid subscriptionToken = studioNetworkMessageAggregator.Subscribe(new Action<TestMessage, IStudioNetworkChannelContext>((message, context) =>
            {
            }));

            bool result = studioNetworkMessageAggregator.Unsubscibe(subscriptionToken);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Unsubscribe_WhereTokenIsntRegistered_Expect_False()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            bool result = studioNetworkMessageAggregator.Unsubscibe(Guid.NewGuid());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SubscribeUnsubscribe_WhereCallbackIsTheSameInstance_Expected_True()
        {
            StudioNetworkMessageAggregator serverNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            Action<TestMessage, IStudioNetworkChannelContext> callback = new Action<TestMessage, IStudioNetworkChannelContext>((message, context) =>
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
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            TestMessage actual = null;
            TestMessage expected = new TestMessage("cake", 1);

            Guid subscriptionToken = studioNetworkMessageAggregator.Subscribe(new Action<TestMessage, IStudioNetworkChannelContext>((message, context) =>
            {
                actual = message;
            }));

            studioNetworkMessageAggregator.Publish(expected, false);

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Publish_TestMessage_WhereNetworkOperatorNotNull_Expected_NetworkContextInSubscriptionCallback()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            IStudioNetworkChannelContext actual = null;
            IStudioNetworkChannelContext expected = new MockStudioNetworkChannelContext(null, Guid.Empty, Guid.Empty);

            Guid subscriptionToken = studioNetworkMessageAggregator.Subscribe(new Action<TestMessage, IStudioNetworkChannelContext>((message, context) =>
            {
                actual = context;
            }));

            studioNetworkMessageAggregator.Publish(new TestMessage(), expected, false);

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Publish_NullMessage_Expected_CallbackNotRun()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();

            TestMessage actual = null;

            Guid subscriptionToken = studioNetworkMessageAggregator.Subscribe(new Action<TestMessage, IStudioNetworkChannelContext>((message, context) =>
            {
                actual = new TestMessage();
            }));

            studioNetworkMessageAggregator.Publish(null, false);

            Assert.IsNull(actual);
        }

        public object ServerNetworkMessageAggregator { get; set; }
    }
}
