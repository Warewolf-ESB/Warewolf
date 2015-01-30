using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ConnectControlViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_Constructor_NullServer_ThrowsNullException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            new ConnectControlViewModel(null,null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        public void ConnectControlViewModel_Constructor_WhenHasServer_ShouldGetListOfConnections()
        {
            //------------Setup for test--------------------------
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer>());
            var server = mockServer.Object;
            
            //------------Execute Test---------------------------
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlViewModel);
            Assert.IsNotNull(connectControlViewModel.Servers);
            mockServer.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_SelectedServer")]
        public void ConnectControlViewModel_SelectedServer_WhenSet_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IServer>();
            var connection = mockConnection.Object;
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer> { connection });
            var server = mockServer.Object;
            // ReSharper disable UseObjectOrCollectionInitializer
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object);
            // ReSharper restore UseObjectOrCollectionInitializer
            
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedConnection = connection;
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlViewModel.SelectedConnection);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_Connect")]
        public void ConnectControlViewModel_Connect_GivenServer_ShouldCallConnectOnServer()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IServer>();
            mockConnection.Setup(server1 => server1.Connect()).Verifiable();
            var connection = mockConnection.Object;
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer> { connection });
            var server = mockServer.Object;
            // ReSharper disable UseObjectOrCollectionInitializer
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object);
            // ReSharper restore UseObjectOrCollectionInitializer
            
            //------------Execute Test---------------------------
            connectControlViewModel.Connect(connection);
            //------------Assert Results-------------------------
            mockConnection.Verify();
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_Disconnect")]
        public void ConnectControlViewModel_Disconnect_GivenServer_ShouldCallDisconnectOnServer()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IServer>();
            mockConnection.Setup(server1 => server1.Disconnect()).Verifiable();
            var connection = mockConnection.Object;
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer> { connection });
            var server = mockServer.Object;
            // ReSharper disable UseObjectOrCollectionInitializer
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object);
            // ReSharper restore UseObjectOrCollectionInitializer
            
            //------------Execute Test---------------------------
            connectControlViewModel.Disconnect(connection);
            //------------Assert Results-------------------------
            mockConnection.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_Refresh")]
        public void ConnectControlViewModel_Refresh_ShouldRefreshSelectedServer()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IServer>();
            var connection = mockConnection.Object;
            mockConnection.Setup(server1 => server1.Load()).Verifiable();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer> { connection });
            var server = mockServer.Object;
            // ReSharper disable UseObjectOrCollectionInitializer
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object);
            // ReSharper restore UseObjectOrCollectionInitializer
            connectControlViewModel.SelectedConnection = connection;
            //------------Execute Test---------------------------
            connectControlViewModel.Refresh();
            //------------Assert Results-------------------------
            mockConnection.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_Refresh")]
        public void ConnectControlViewModel_Edit_ShouldEditSelectedServer()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IServer>();
            var connection = mockConnection.Object;
            mockConnection.Setup(server1 => server1.Edit()).Verifiable();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer> { connection });
            var server = mockServer.Object;
            // ReSharper disable UseObjectOrCollectionInitializer
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object);
            // ReSharper restore UseObjectOrCollectionInitializer
            connectControlViewModel.SelectedConnection = connection;
            //------------Execute Test---------------------------
            connectControlViewModel.Edit();
            //------------Assert Results-------------------------
            mockConnection.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        public void ConnectControlViewModel_Constructor_SetsUpEditCommand()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IServer>();
            var connection = mockConnection.Object;
            mockConnection.Setup(server1 => server1.Edit()).Verifiable();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer> { connection });
            var server = mockServer.Object;
            // ReSharper disable UseObjectOrCollectionInitializer
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object);
            // ReSharper restore UseObjectOrCollectionInitializer
            connectControlViewModel.SelectedConnection = connection;
            //------------Execute Test---------------------------
            connectControlViewModel.EditConnectionCommand.Execute(null);
            //------------Assert Results-------------------------
            mockConnection.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_AddNewServer")]
        public void ConnectControlViewModel_ToggleConnectionStateCommand_WhenConnected_ShouldDisconnect()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IServer>();
            mockConnection.Setup(serverConnection => serverConnection.IsConnected()).Returns(true);
            mockConnection.Setup(serverConnection => serverConnection.Disconnect()).Verifiable();
            var connection = mockConnection.Object;
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer> { connection });
            var server = mockServer.Object;
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object)
            {
                SelectedConnection = connection
            };
            //------------Execute Test---------------------------
            connectControlViewModel.ToggleConnectionStateCommand.Execute(null);
            //------------Assert Results-------------------------
            mockConnection.Verify(server1 => server1.Disconnect(),Times.Once());
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_AddNewServer")]
        public void ConnectControlViewModel_ToggleConnectionStateCommand_WhenDisconnected_ShouldConnect()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IServer>();
            mockConnection.Setup(serverConnection => serverConnection.IsConnected()).Returns(false);
            mockConnection.Setup(serverConnection => serverConnection.Connect()).Verifiable();
            var connection = mockConnection.Object;
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server1 => server1.GetServerConnections()).Returns(new List<IServer> { connection });
            var server = mockServer.Object;
            var connectControlViewModel = new ConnectControlViewModel(server, new Mock<IEventAggregator>().Object)
            {
                SelectedConnection = connection
            };
            //------------Execute Test---------------------------
            connectControlViewModel.ToggleConnectionStateCommand.Execute(null);
            //------------Assert Results-------------------------
            mockConnection.Verify(server1 => server1.Connect(),Times.Once());
        }
    }
}
