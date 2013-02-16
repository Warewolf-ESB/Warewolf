using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Network;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Network;
using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Network.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DataListClientChannelTests
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
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<WriteDataListMessage>();

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, _studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(null);
        }

        [TestMethod]
        public void Instantiate_Expected_Success()
        {
            Mock<IStudioNetworkMessageAggregator> _studioNetworkMessageAggregator = Dev2MockFactory.SetupStudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<WriteDataListMessage>();

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, _studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));
        }

        #endregion Initialization Tests

        #region WriteDataList Tests

        [TestMethod]
        public void WriteDataList_Where_ResultTrue_Expected_True()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<WriteDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            _networkMessageBroker.Setup(e => e.Send<WriteDataListMessage>(It.IsAny<WriteDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
                {
                    Task t = new Task(new Action(() =>
                        {
                            studioNetworkMessageAggregator.Publish(new WriteDataListResultMessage { Errors = new ErrorResultTO(), Handle = 1, Result = true }, _studioNetworkChannelContext.Object, true);
                        }));

                    t.Start();
                });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors = new ErrorResultTO();

            bool expected = true;
            bool actual = channel.WriteDataList(dataList.UID, dataList, errors);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WriteDataList_Where_ResultFalse_Expected_False()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<WriteDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            _networkMessageBroker.Setup(e => e.Send<WriteDataListMessage>(It.IsAny<WriteDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    studioNetworkMessageAggregator.Publish(new WriteDataListResultMessage { Errors = new ErrorResultTO(), Handle = 1, Result = false }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors = new ErrorResultTO();

            bool expected = false;
            bool actual = channel.WriteDataList(dataList.UID, dataList, errors);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WriteDataList_Where_ResultIsErrorMessage_Expected_False()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<WriteDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            _networkMessageBroker.Setup(e => e.Send<WriteDataListMessage>(It.IsAny<WriteDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    studioNetworkMessageAggregator.Publish(new ErrorMessage { Handle = 1 }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors = new ErrorResultTO();

            bool expected = false;
            bool actual = channel.WriteDataList(dataList.UID, dataList, errors);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WriteDataList_Where_ResultContainsErrors_Expected_Errors()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<WriteDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            _networkMessageBroker.Setup(e => e.Send<WriteDataListMessage>(It.IsAny<WriteDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    ErrorResultTO testErrors = new ErrorResultTO();
                    testErrors.AddError("Cake");
                    studioNetworkMessageAggregator.Publish(new WriteDataListResultMessage { Errors = testErrors, Handle = 1, Result = false }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors = new ErrorResultTO();

            string expected = "Cake";
            channel.WriteDataList(dataList.UID, dataList, errors);

            Assert.AreEqual(expected, errors.FetchErrors().First());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteDataList_Where_DataListIsNull_Expected_ArgumentNullException()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<WriteDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors = new ErrorResultTO();

            bool expected = true;
            bool actual = channel.WriteDataList(dataList.UID, null, errors);

            Assert.AreEqual(expected, actual);
        }

        #endregion WriteDataList Tests

        #region ReadDatalist Tests

        [TestMethod]
        public void ReadDatalist_Where_ResultIsValid_Expected_ValidResult()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ReadDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();

            _networkMessageBroker.Setup(e => e.Send<ReadDataListMessage>(It.IsAny<ReadDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    studioNetworkMessageAggregator.Publish(new ReadDataListResultMessage { Errors = new ErrorResultTO(), Datalist = testDataList, Handle = 1 }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            ErrorResultTO errors = new ErrorResultTO();

            IBinaryDataList actual = channel.ReadDatalist(testDataList.UID, errors);

            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void ReadDatalist_Where_ResultIsNull_Expected_Null()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ReadDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();

            _networkMessageBroker.Setup(e => e.Send<ReadDataListMessage>(It.IsAny<ReadDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    studioNetworkMessageAggregator.Publish(new ReadDataListResultMessage { Errors = new ErrorResultTO(), Datalist = null, Handle = 1 }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            ErrorResultTO errors = new ErrorResultTO();

            IBinaryDataList actual = channel.ReadDatalist(testDataList.UID, errors);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadDatalist_Where_ResultIsErrorMessage_Expected_Null()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ReadDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();

            _networkMessageBroker.Setup(e => e.Send<ReadDataListMessage>(It.IsAny<ReadDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    studioNetworkMessageAggregator.Publish(new ErrorMessage { Handle = 1 }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            ErrorResultTO errors = new ErrorResultTO();

            IBinaryDataList actual = channel.ReadDatalist(testDataList.UID, errors);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadDatalist_Where_ResultContainsErrors_Expected_Errors()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<ReadDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            IBinaryDataList testDataList = Dev2BinaryDataListFactory.CreateDataList();

            _networkMessageBroker.Setup(e => e.Send<ReadDataListMessage>(It.IsAny<ReadDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    ErrorResultTO testErrors = new ErrorResultTO();
                    testErrors.AddError("Cake");
                    studioNetworkMessageAggregator.Publish(new ReadDataListResultMessage { Errors = testErrors, Datalist = testDataList, Handle = 1 }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            ErrorResultTO errors = new ErrorResultTO();

            string expected = "Cake";
            IBinaryDataList actual = channel.ReadDatalist(testDataList.UID, errors);

            Assert.AreEqual(expected, errors.FetchErrors().First());
        }

        #endregion ReadDatalist Tests

        #region DeleteDatalist

        [TestMethod]
        public void DeleteDatalist_Where_ResultIsValid_Expected_ValidResult()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<DeleteDataListMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            //This send callback mock verifies that the delete has run correctly
            _networkMessageBroker.Setup(e => e.Send<DeleteDataListMessage>(It.IsAny<DeleteDataListMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    studioNetworkMessageAggregator.Publish(new DeleteDataListResultMessage { Handle = 1 }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            ErrorResultTO errors = new ErrorResultTO();

            channel.DeleteDataList(Guid.Empty, true);

            //Look at the other comment in this test for pass criteria ;)
        }

        #endregion DeleteDatalist

        #region PersistChildChain

        [TestMethod]
        public void PersistChildChain_Where_ResultTrue_Expected_True()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<PersistChildChainMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            _networkMessageBroker.Setup(e => e.Send<PersistChildChainMessage>(It.IsAny<PersistChildChainMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    studioNetworkMessageAggregator.Publish(new PersistChildChainResultMessage { Handle = 1, Result = true }, _studioNetworkChannelContext.Object, true);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            ErrorResultTO errors = new ErrorResultTO();

            bool expected = true;
            bool actual = channel.PersistChildChain(Guid.Empty);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PersistChildChain_Where_ResultFalse_Expected_False()
        {
            StudioNetworkMessageAggregator studioNetworkMessageAggregator = new StudioNetworkMessageAggregator();
            Mock<INetworkMessageBroker> _networkMessageBroker = Dev2MockFactory.SetupNetworkMessageBroker<PersistChildChainMessage>();
            Mock<IStudioNetworkChannelContext> _studioNetworkChannelContext = Dev2MockFactory.SetupStudioNetworkChannelContext();

            _networkMessageBroker.Setup(e => e.Send(It.IsAny<PersistChildChainMessage>(), It.IsAny<INetworkOperator>())).Callback(() =>
            {
                Task t = new Task(new Action(() =>
                {
                    studioNetworkMessageAggregator.Publish(new PersistChildChainResultMessage { Handle = 1, Result = false }, _studioNetworkChannelContext.Object);
                }));

                t.Start();
            });

            ImportService.CurrentContext = CompositionInitializer.InitializeForDataListChannelTests(_networkMessageBroker, studioNetworkMessageAggregator);

            DataListClientChannel channel = new DataListClientChannel(new TCPDispatchedClient(""));

            bool expected = false;
            bool actual = channel.PersistChildChain(Guid.Empty);

            Assert.AreEqual(expected, actual);
        }

        #endregion PersistChildChain
    }
}
