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
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network.Channels;
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
        public void InstantiateWhereClientIsNullExpectedException()
        {
            ExecutionClientChannel channel = new ExecutionClientChannel(null);
        }

        [TestMethod]
        public void InstantiateExpectedSuccess()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            //------------------------Execute Test------------------------------------------------------------
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Assert Result-----------------------------------------------------------
            Assert.IsNotNull(channel);
            
        }
        #endregion Initialization Tests

        #region Execution Status Callback

        [TestMethod]
        public void AddExecutionStatusCallbackWhereAddToDispatcherSuceedsExpectedAddedToDispatcherAndMessageSentAndTrue()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            var actual = channel.AddExecutionStatusCallback(callbackID, message => { });
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Once());
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void AddExecutionStatusCallbackWhereAddRemoveSucess()
        {

            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            channel.AddExecutionStatusCallback(callbackID, message => { });
            var actual = channel.RemoveExecutionStatusCallback(callbackID);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Exactly(2));
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void AddExecutionStatusCallbackWhereAddToDispatcherFailsExpectedFalse()
        {

            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            channel.AddExecutionStatusCallback(callbackID,message => {});
            var actual = channel.AddExecutionStatusCallback(callbackID,message => {});
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()),Times.Once());
            Assert.IsFalse(actual);
        }

       
        static Mock<IEnvironmentConnection> CreateEnvironmentConnectionMockWithMessageAggregator()
        {
            var environmentConnectionMock = new Mock<IEnvironmentConnection>();
            environmentConnectionMock.Setup(connection => connection.MessageAggregator).Returns(() => new Mock<IStudioNetworkMessageAggregator>().Object);
            return environmentConnectionMock;
        }
        
        static Mock<IEnvironmentConnection> CreateEnvironmentConnection()
        {
            var environmentConnectionMock = new Mock<IEnvironmentConnection>();
            return environmentConnectionMock;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddExecutionStatusCallbackWhereCallbackIsNullExpectedException()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            channel.AddExecutionStatusCallback(callbackID,null);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Once());
            //Exception is thrown and verified by the attribute [ExpectedException(typeof(ArgumentNullException))]
            
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void AddExecutionStatusCallbackWhereDispatcherIsNullExpectedException()
        {
            //-----------------Setup test-----------------------------------
            var environmentConnectionMock = new Mock<IEnvironmentConnection>();
            ExecutionClientChannel channel = new ExecutionClientChannel(environmentConnectionMock.Object);
            //-----------------Execute test---------------------------------
            channel.AddExecutionStatusCallback(Guid.NewGuid(), new Action<ExecutionStatusCallbackMessage>(m => { }));
            //-----------------Assert Result--------------------------------
            //Exception is thrown and verified by the attribute [ExpectedException(typeof(NullReferenceException))]
        }


        [TestMethod]
        public void RemoveExecutionStatusCallbackWhereRemoveFromDispatcherSuceedsExpectedRemovedFromDispatcherAndMessageSentAndTrue()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            channel.AddExecutionStatusCallback(callbackID, message => { });
            var actual = channel.RemoveExecutionStatusCallback(callbackID);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Exactly(2));
            Assert.IsTrue(actual);
         }


        [TestMethod]
        public void RemoveExecutionStatusCallbackWhereRemoveFromDispatcherFailsExpectedFalse()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            var actual = channel.RemoveExecutionStatusCallback(callbackID);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Never());
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void RemoveExecutionStatusCallbackWhereDispatcherIsNullExpectedException()
        {
            //-----------------Setup test-----------------------------------
            var environmentConnectionMock = new Mock<IEnvironmentConnection>();
            ExecutionClientChannel channel = new ExecutionClientChannel(environmentConnectionMock.Object);
            //-----------------Execute test---------------------------------
            channel.RemoveExecutionStatusCallback(Guid.NewGuid());
            //-----------------Assert Result--------------------------------
            //Exception is thrown and verified by the attribute [ExpectedException(typeof(NullReferenceException))]
        }

        #endregion Execution Status Callback

        #region Message Recieving

        [TestMethod]
        public void ExecutionStatusCallbackMessageWhereMessageAppliesToChannelExpectedPostRun()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            channel.AddExecutionStatusCallback(callbackID, message => { });
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Once());

        }

        [TestMethod]
        public void ExecutionStatusCallbackMessageWhereMessageDoesntApplyToChannelExpectedPostNotRun()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            channel.AddExecutionStatusCallback(callbackID, message => { });
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<ExecutionStatusCallbackMessage>()), Times.Once());
        }

        [TestMethod]
        public void NetworkContextDetachedMessageWhereMessageAppliesToChannelExpectedRemoveRangeRun()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMockWithMessageAggregator();
            connectionMock.Setup(connection => connection.SendNetworkMessage(It.IsAny<NetworkContextDetachedMessage>()));
            ExecutionClientChannel channel = new ExecutionClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var callbackID = Guid.NewGuid();
            var actual = channel.AddNetworkContextDetachedMessageCallback(callbackID, message => { });
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            connectionMock.Verify(connection => connection.SendNetworkMessage(It.IsAny<NetworkContextDetachedMessage>()), Times.Once());
            Assert.IsTrue(actual);

        }

        #endregion Message Recieving
    }
}
