using System;
using System.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Unlimited.UnitTest.Framework.Dev2.Network
{
    [TestClass][ExcludeFromCodeCoverage]
    public class NetworkMessageBrokerTests
    {
        [TestMethod]
        public void SendRecieve_Expected_RecievedMessageMatchedSendMessage()
        {
            Packet packet = null;

            Mock<INetworkOperator> no = new Mock<INetworkOperator>();
            no.Setup(n => n.Send(It.IsAny<Packet>())).Callback<Packet>(new Action<Packet>(p =>
                {
                    packet = p;
                }));

            NetworkMessageBroker networkMessageBroker = new NetworkMessageBroker();

            TestMessage sendMessage = new TestMessage("cake", 0);
            networkMessageBroker.Send(sendMessage, no.Object);
            ByteBuffer byteBuffer = new ByteBuffer(packet.Buffer);
            
            TestMessage recievedMessage = networkMessageBroker.Receive(byteBuffer) as TestMessage;
            
            string actual = recievedMessage.StringVal + recievedMessage.IntVal.ToString();
            string expected = sendMessage.StringVal + sendMessage.IntVal.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Send_Where_MessageNull_Expected_InvalidOperationException()
        {
            Packet packet = null;

            Mock<INetworkOperator> no = new Mock<INetworkOperator>();
            no.Setup(n => n.Send(It.IsAny<Packet>())).Callback<Packet>(new Action<Packet>(p =>
            {
                packet = p;
            }));

            NetworkMessageBroker networkMessageBroker = new NetworkMessageBroker();
            networkMessageBroker.Send<TestMessage>(null, no.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Send_Where_NetworkOperatorNull_Expected_InvalidOperationException()
        {
            TestMessage sendMessage = new TestMessage("cake", 0);
            NetworkMessageBroker networkMessageBroker = new NetworkMessageBroker();
            networkMessageBroker.Send(sendMessage, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Recieve_Where_ReaderNull_Expected_InvalidOperationException()
        {
            NetworkMessageBroker networkMessageBroker = new NetworkMessageBroker();
            networkMessageBroker.Receive(null);
        }
    }
}
