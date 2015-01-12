using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
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
            new ConnectControlViewModel(null);
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
            var connectControlViewModel = new ConnectControlViewModel(server);
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
            var connectControlViewModel = new ConnectControlViewModel(server);
            // ReSharper restore UseObjectOrCollectionInitializer
            
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedConnection = connection;
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlViewModel.SelectedConnection);
        }
    }
}
