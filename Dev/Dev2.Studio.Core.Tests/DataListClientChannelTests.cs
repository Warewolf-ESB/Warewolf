using System;
using System.Linq;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DataListClientChannelTests
    {
        #region MyTestInitialize

        [TestInitialize]
        public void Initialize()
        {
        }

        #endregion

        #region Initialization Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantiateWhereClientIsNullExpectedException()
        {
            //--------------------Setup Test-------------------------------------
            //---------------------Execute Test---------------------------------
// ReSharper disable ObjectCreationAsStatement
            new DataListClientChannel(null);
// ReSharper restore ObjectCreationAsStatement
            //---------------------Assert Result---------------------------------
            //An exception is thrown and verified by the attribute [ExpectedException(typeof(ArgumentNullException))]
        }

        [TestMethod]
        public void InstantiateExpectedSuccess()
        {
            //------------------------Setup Test-------------------------------------------------------
            var environmentConnectionMock = CreateEnvironmentConnectionMock();
            //-------------------------Execute Test --------------------------------------------------
            DataListClientChannel channel = new DataListClientChannel(environmentConnectionMock.Object);
            //--------------------------Assert Result-------------------------------------------------
            Assert.IsNotNull(channel);
        }

        static Mock<IEnvironmentConnection> CreateEnvironmentConnectionMock()
        {
            var environmentConnectionMock = new Mock<IEnvironmentConnection>();
            return environmentConnectionMock;
        }

        #endregion Initialization Tests

        #region WriteDataList Tests

        [TestMethod]
        public void WriteDataListWhereResultTrueExpectedTrue()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            var writeDataListResultMessage = new WriteDataListResultMessage { Errors = new ErrorResultTO(), Handle = 1, Result = true };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<WriteDataListMessage>())).Returns(writeDataListResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            //------------------------Execute Test------------------------------------------------------------
            var actual = channel.WriteDataList(testDataList.UID, testDataList, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void WriteDataListWhereResultFalseExpectedFalse()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            var writeDataListResultMessage = new WriteDataListResultMessage { Errors = new ErrorResultTO(), Handle = 1, Result = false };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<WriteDataListMessage>())).Returns(writeDataListResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            //------------------------Execute Test------------------------------------------------------------
            var actual = channel.WriteDataList(testDataList.UID, testDataList, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void WriteDataListWhereResultIsErrorMessageExpectedFalse()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            var errorMessage = new ErrorMessage { Handle = 1 };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<WriteDataListMessage>())).Returns(errorMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            //------------------------Execute Test------------------------------------------------------------
            var actual = channel.WriteDataList(testDataList.UID, testDataList, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void WriteDataListWhereResultContainsErrorsExpectedErrors()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            ErrorResultTO testErrors = new ErrorResultTO();
            testErrors.AddError("Cake");
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            var writeDataListResultMessage = new WriteDataListResultMessage { Errors = testErrors, Handle = 1, Result = false };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<WriteDataListMessage>())).Returns(writeDataListResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            //------------------------Execute Test------------------------------------------------------------
            var actual = channel.WriteDataList(testDataList.UID, testDataList, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            string expected = "Cake";
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, errors.FetchErrors().First());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteDataListWhereDataListIsNullExpectedArgumentNullException()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            var readDataListResultMessage = new ReadDataListResultMessage { Errors = new ErrorResultTO(), Datalist = testDataList, Handle = 1 };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ReadDataListMessage>())).Returns(readDataListResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            //------------------------Execute Test------------------------------------------------------------
            channel.WriteDataList(testDataList.UID,null, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            //An exception is thrown and verified by the attribute [ExpectedException(typeof(ArgumentNullException))]
        }

        #endregion WriteDataList Tests

        #region ReadDatalist Tests

        [TestMethod]
        public void ReadDatalistWhereResultIsValidExpectedValidResult()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            var readDataListResultMessage = new ReadDataListResultMessage { Errors = new ErrorResultTO(), Datalist = testDataList, Handle = 1 };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ReadDataListMessage>())).Returns(readDataListResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            //------------------------Execute Test------------------------------------------------------------
            IBinaryDataList actual = channel.ReadDatalist(testDataList.UID, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.IsNotNull(actual);
            Assert.AreEqual(testDataList.UID,actual.UID);
        }

        [TestMethod]
        public void ReadDatalistWhereResultIsNullExpectedNull()
        {

            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            var readDataListResultMessage = new ReadDataListResultMessage { Errors = new ErrorResultTO(), Datalist = null, Handle = 1 };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ReadDataListMessage>())).Returns(readDataListResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            //------------------------Execute Test------------------------------------------------------------
            IBinaryDataList actual = channel.ReadDatalist(testDataList.UID, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadDatalistWhereResultIsErrorMessageExpectedNull()
        {

            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            var errorMessage = new ErrorMessage { Handle = 1 };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ReadDataListMessage>())).Returns(errorMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            //------------------------Execute Test------------------------------------------------------------
            IBinaryDataList actual = channel.ReadDatalist(testDataList.UID, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadDatalistWhereResultContainsErrorsExpectedErrors()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO testErrors = new ErrorResultTO();
            testErrors.AddError("Cake");
            var readDataListResultMessage = new ReadDataListResultMessage { Errors = testErrors, Datalist = testDataList, Handle = 1 };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<ReadDataListMessage>())).Returns(readDataListResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            ErrorResultTO errors = new ErrorResultTO();
            string expected = "Cake";
            //------------------------Execute Test------------------------------------------------------------
            channel.ReadDatalist(testDataList.UID, errors);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.AreEqual(expected, errors.FetchErrors().First());
        }

        #endregion ReadDatalist Tests

        #region DeleteDatalist

        [TestMethod]
        public void DeleteDatalistWhereResultIsValidExpectedValidResult()
        {
            //--------------Setup Test ----------------------------------------------------
            var deleteDataListResultMessage = new DeleteDataListResultMessage { Handle = 1 };
            var environmentConnectionMock = CreateEnvironmentConnectionMock();
            environmentConnectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>()))
                                                                                        .Returns(() => deleteDataListResultMessage);
            var channel = new DataListClientChannel(environmentConnectionMock.Object);
            //---------------------Execute Test -----------------------------------------------
            channel.DeleteDataList(Guid.Empty, true);
            //------------------Assert Result-------------------------------------------------
            environmentConnectionMock.Verify();
            environmentConnectionMock.Verify(connection => connection.SendReceiveNetworkMessage(It.IsAny<DeleteDataListMessage>()),Times.Once());
        }

        #endregion DeleteDatalist

        #region PersistChildChain

        [TestMethod]
        public void PersistChildChainWhereResultTrueExpectedTrue()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            var persistChildChainResultMessage = new PersistChildChainResultMessage { Handle = 1, Result = true };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<PersistChildChainMessage>())).Returns(persistChildChainResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var actual = channel.PersistChildChain(Guid.Empty);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void PersistChildChainWhereResultFalseExpectedFalse()
        {
            //-----------------------Setup Test ---------------------------------------------------------------
            var connectionMock = CreateEnvironmentConnectionMock();
            var persistChildChainResultMessage = new PersistChildChainResultMessage { Handle = 1, Result = false };
            connectionMock.Setup(connection => connection.SendReceiveNetworkMessage(It.IsAny<PersistChildChainMessage>())).Returns(persistChildChainResultMessage);
            DataListClientChannel channel = new DataListClientChannel(connectionMock.Object);
            //------------------------Execute Test------------------------------------------------------------
            var actual = channel.PersistChildChain(Guid.Empty);
            //------------------------Assert Result-----------------------------------------------------------
            connectionMock.Verify();
            Assert.IsFalse(actual);
        }

        #endregion PersistChildChain
    }
}
