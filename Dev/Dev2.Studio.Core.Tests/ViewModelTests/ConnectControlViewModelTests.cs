using System;
using System.Collections.Generic;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    public class ConnectControlViewModelViewModelTests
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_Constructo")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_Constructor_ActiveEnvironmentIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var connectViewModel = new ConnectControlViewModel(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_Constructo")]
        public void ConnectControlViewModel_Constructor_ActiveEnvironmentIsNotNull_SetsProperty()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentModel>();
            //------------Execute Test---------------------------
            var connectViewModel = new ConnectControlViewModel(env.Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(connectViewModel.ActiveEnvironment);
            Assert.AreSame(env.Object, connectViewModel.ActiveEnvironment);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_NoServers_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentModel>();
            var connectViewModel = new ConnectControlViewModel(env.Object);

            //------------Execute Test---------------------------
            var selectedServer = connectViewModel.GetSelectedServer(null, string.Empty);

            //------------Assert Results-------------------------
            Assert.IsNull(selectedServer);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_LabelDoesNotContainDestination_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentModel>();
            var connectViewModel = new ConnectControlViewModel(env.Object);

            //------------Execute Test---------------------------
            var selectedServer = connectViewModel.GetSelectedServer(null, "RandomLabel");

            //------------Assert Results-------------------------
            Assert.IsNull(selectedServer);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ServerCountIsZero_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentModel>();
            var connectViewModel = new ConnectControlViewModel(env.Object);

            //------------Execute Test---------------------------
            var selectedServer = connectViewModel.GetSelectedServer(new List<IServer>(), string.Empty);

            //------------Assert Results-------------------------
            Assert.IsNull(selectedServer);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_SourceOrEmptyInLabelTextAndAliasDoesNotMatchActiveEnvironment_LocalhostServer()
        {
            var activeServer = CreatServer(name: "Server1", isConnected: false);
            var localhostServer = CreatServer(name: "localhost", isConnected: false);

            activeServer.Alias = "Server2"; // ensure active environment does not match server alias

            VerifyGetSelectedServer(labelText: "Source Server :", activeServer: activeServer, expectedServer: localhostServer, otherServers: null);
            VerifyGetSelectedServer(labelText: string.Empty, activeServer: activeServer, expectedServer: localhostServer, otherServers: null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_DestinationInLabelTextAndLocalhostIsActiveAndOneOtherIsConnected_OtherConnected()
        {
            var localhostServer = CreatServer(name: "localhost", isConnected: true);
            var connectedServer = CreatServer(name: "connected", isConnected: true);

            VerifyGetSelectedServer(labelText: "Destination Server :", activeServer: localhostServer, expectedServer: connectedServer, otherServers: null);
        }


        #region Moved from ConnectViewModelTest

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_OnlyOneServer_SourceIsThatServer()
        {
            var onlyServer = CreatServer(name: "Server", isConnected: true);

            VerifyGetSelectedServer(labelText: "Source Server :", activeServer: onlyServer, expectedServer: onlyServer, otherServers: null);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsLocal_SourceSetToLocal()
        {
            var localhostServer = CreatServer(name: "localhost", isConnected: true);
            var otherServer = CreatServer(name: "Server2", isConnected: true);

            VerifyGetSelectedServer(labelText: "Source Server :", activeServer: localhostServer, expectedServer: localhostServer, otherServers: otherServer);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsRemote_DestinationSetToLocal()
        {
            var activeServer = CreatServer(name: "remote", isConnected: true);
            var localhostServer = CreatServer(name: "localhost", isConnected: false);

            VerifyGetSelectedServer(labelText: "Destination Server :", activeServer: activeServer, expectedServer: localhostServer, otherServers: null);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsLocalServerAndThereIsOneOtherDisconnectedServer_DestinationSetToOtherServerAndItConnectsAutomatically()
        {
            var localhostServer = CreatServer(name: "localhost", isConnected: true);
            var disconnectedServer = CreatServer(name: "disconnected", isConnected: false);

            var disconnectedEnv = Mock.Get(disconnectedServer.Environment);
            disconnectedEnv.Setup(e => e.ForceLoadResources()).Verifiable();
            disconnectedEnv.Setup(env => env.Connect()).Verifiable();

            VerifyGetSelectedServer(labelText: "Destination Server :", activeServer: localhostServer, expectedServer: disconnectedServer, otherServers: null);

            disconnectedEnv.Verify(env => env.Connect(), Times.Once());
            disconnectedEnv.Verify(env => env.ForceLoadResources(), Times.Once());
        }

        #endregion


        static void VerifyGetSelectedServer(string labelText, IServer activeServer, IServer expectedServer, params IServer[] otherServers)
        {
            //------------Setup for test--------------------------
            var servers = new List<IServer>(otherServers ?? new IServer[0]) { activeServer };
            if(!expectedServer.Equals(activeServer))
            {
                servers.Add(expectedServer);
            }
            
            var connectViewModel = new ConnectControlViewModel(activeServer.Environment);

            //------------Execute Test---------------------------
            var selectedServer = connectViewModel.GetSelectedServer(servers, labelText);

            //------------Assert Results-------------------------
            Assert.AreSame(expectedServer, selectedServer);

        }

        static ServerDTO CreatServer(string name, bool isConnected)
        {
            var isLocalhost = name == "localhost";

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Name).Returns(name);
            env.Setup(e => e.ID).Returns(isLocalhost ? Guid.Empty : Guid.NewGuid());
            env.Setup(e => e.IsConnected).Returns(isConnected);
            env.Setup(e => e.IsLocalHost()).Returns(isLocalhost);

            return new ServerDTO(env.Object);
        }
    }
}
