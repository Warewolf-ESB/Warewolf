using System;
using System.Diagnostics.CodeAnalysis;
using System.Network;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Network;
using Dev2.DynamicServices.Network.DataList;
using Dev2.Network;
using Dev2.Network.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.DynamicServices.Test
{
    [TestClass][ExcludeFromCodeCoverage]
    public class DataListServerChannelTests
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
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _dataListServer.Object);

            channel.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_MessageBrokerIsNull_Expected_ArgumentNullException()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();

            DataListServerChannel channel = new DataListServerChannel(null, _serverNetworkMessageAggregator.Object, _dataListServer.Object);

            channel.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_MessageAggregatorIsNull_Expected_ArgumentNullException()
        {
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, null, _dataListServer.Object);
            channel.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_DataListServerIsNull_Expected_ArgumentNullException()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, null);
            channel.Dispose();
        }

        #endregion Initialization Tests

        #region WriteDataList Tests

        [TestMethod]
        public void WriteDataList_Where_InputsAreValid_Expected_True()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _dataListServer.Object);

            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors = new ErrorResultTO();

            bool expected = true;
            bool actual = channel.WriteDataList(dataList.UID, dataList, errors);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out resultErrors), Times.Exactly(1));

            channel.Dispose();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteDataList_Where_DataListIsNull_Expected_True()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _dataListServer.Object);

            IBinaryDataList dataList = null;
            ErrorResultTO errors = new ErrorResultTO();

            bool actual = channel.WriteDataList(Guid.NewGuid(), dataList, errors);

            channel.Dispose();
        }

        #endregion WriteDataList Tests

        #region ReadDataList Tests

        [TestMethod]
        public void ReadDataList_Expected_ValidDataListReturned()
        {
            IBinaryDataList expected = Dev2BinaryDataListFactory.CreateDataList();

            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer(true, true, expected);

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _dataListServer.Object);

            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList actual = channel.ReadDatalist(Guid.Empty, errors);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.ReadDatalist(It.IsAny<Guid>(), out resultErrors), Times.Exactly(1));
            
            channel.Dispose();

            Assert.AreEqual(expected, actual);
        }

        #endregion ReadDataList Tests

        #region DeleteDataList Tests

        [TestMethod]
        public void DeleteDataList_Expected_DeleteDataListRun()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> _serverNetworkMessageAggregator = Dev2MockFactory.SetupServerNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, _serverNetworkMessageAggregator.Object, _dataListServer.Object);

            channel.DeleteDataList(Guid.Empty, true);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.DeleteDataList(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));

            channel.Dispose();
        }

        #endregion DeleteDataList Tests

        #region Message Recieving

        [TestMethod]
        public void ReadDataListMessage_Expected_DataListServerReadAndResultMessageSent()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _dataListServer.Object);
            
            ErrorResultTO errors = new ErrorResultTO();
            tmpServerNetworkMessageAggregator.Publish(new ReadDataListMessage(1, Guid.Empty, errors), _context.Object, false);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.ReadDatalist(It.IsAny<Guid>(), out resultErrors), Times.Exactly(1));
            _networkMessageBroker.Verify(e => e.Send<ReadDataListResultMessage>(It.IsAny<ReadDataListResultMessage>(), It.IsAny<INetworkOperator>()), Times.Exactly(1));
            channel.Dispose();
        }

        [TestMethod]
        public void ReadDataListMessage_WhereDataListServerReadCausesException_Expected_ErrorsInReturnMessage()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer(true, true, null, true);
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            ReadDataListResultMessage resultMessage = null;
            _networkMessageBroker.Setup(e => e.Send<ReadDataListResultMessage>(It.IsAny<ReadDataListResultMessage>(), It.IsAny<INetworkOperator>())).Callback(new Action<ReadDataListResultMessage, INetworkOperator>((a, b) =>
            {
                resultMessage = a;
            }));

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _dataListServer.Object);

            ErrorResultTO errors = new ErrorResultTO();
            tmpServerNetworkMessageAggregator.Publish(new ReadDataListMessage(1, Guid.Empty, errors), _context.Object, false);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.ReadDatalist(It.IsAny<Guid>(), out resultErrors), Times.Exactly(1));

            channel.Dispose();

            Assert.IsTrue(resultMessage.Errors.FetchErrors().Count > 0);
        }


        [TestMethod]
        public void WriteDataListMessage_Expected_DataListServerWriteAndResultMessageSent()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _dataListServer.Object);

            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            tmpServerNetworkMessageAggregator.Publish(new WriteDataListMessage(1, Guid.Empty, dataList, errors), _context.Object, false);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out resultErrors), Times.Exactly(1));
            _networkMessageBroker.Verify(e => e.Send<WriteDataListResultMessage>(It.IsAny<WriteDataListResultMessage>(), It.IsAny<INetworkOperator>()), Times.Exactly(1));

            channel.Dispose();
        }

        [TestMethod]
        public void WriteDataListMessage_WhereDataListServerWriteCausesException_Expected_ErrorsInReturnMessage()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer(true, true, null, false, true);
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            WriteDataListResultMessage resultMessage = null;
            _networkMessageBroker.Setup(e => e.Send<WriteDataListResultMessage>(It.IsAny<WriteDataListResultMessage>(), It.IsAny<INetworkOperator>())).Callback(new Action<WriteDataListResultMessage, INetworkOperator>((a, b) =>
            {
                resultMessage = a;
            }));

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _dataListServer.Object);

            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            tmpServerNetworkMessageAggregator.Publish(new WriteDataListMessage(1, Guid.Empty, dataList, errors), _context.Object, false);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out resultErrors), Times.Exactly(1));

            channel.Dispose();

            Assert.IsTrue(resultMessage.Errors.FetchErrors().Count > 0);
        }


        [TestMethod]
        public void DeleteDataListMessage_Expected_DataListServerDeleteAndResultMessageSent()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer();
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _dataListServer.Object);

            ErrorResultTO errors = new ErrorResultTO();
            tmpServerNetworkMessageAggregator.Publish(new DeleteDataListMessage(1, Guid.Empty, true), _context.Object, false);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.DeleteDataList(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
            _networkMessageBroker.Verify(e => e.Send<DeleteDataListResultMessage>(It.IsAny<DeleteDataListResultMessage>(), It.IsAny<INetworkOperator>()), Times.Exactly(1));

            channel.Dispose();
        }

        [TestMethod]
        public void DeleteDataListMessage_WhereDataListServerDeleteCausesException_Expected_ErrorsInReturnMessage()
        {
            ServerNetworkMessageAggregator<StudioNetworkSession> tmpServerNetworkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();

            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker();
            Mock<IDataListServer> _dataListServer = Dev2MockFactory.SetupDataListServer(true, true, null, false, false, false, true);
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> _context = Dev2MockFactory.SetupServerNetworkChannelContext();

            DeleteDataListResultMessage resultMessage = null;
            _networkMessageBroker.Setup(e => e.Send<DeleteDataListResultMessage>(It.IsAny<DeleteDataListResultMessage>(), It.IsAny<INetworkOperator>())).Callback(new Action<DeleteDataListResultMessage, INetworkOperator>((a, b) =>
            {
                resultMessage = a;
            }));

            DataListServerChannel channel = new DataListServerChannel(_networkMessageBroker.Object, tmpServerNetworkMessageAggregator, _dataListServer.Object);

            ErrorResultTO errors = new ErrorResultTO();
            tmpServerNetworkMessageAggregator.Publish(new DeleteDataListMessage(1, Guid.Empty, true), _context.Object, false);

            ErrorResultTO resultErrors = new ErrorResultTO();
            _dataListServer.Verify(e => e.DeleteDataList(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));

            channel.Dispose();

            Assert.IsTrue(resultMessage.Errors.FetchErrors().Count > 0);
        }

        #endregion Message Recieving
    }
}
