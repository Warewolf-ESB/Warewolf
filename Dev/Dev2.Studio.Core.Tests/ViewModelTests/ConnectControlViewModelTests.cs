using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConnectControlViewModelViewModelTests
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_Constructor_ActiveEnvironmentIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            new ConnectControlViewModel(null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        public void ConnectControlViewModel_Constructor_ActiveEnvironmentIsNotNull_SetsProperty()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentModel>();
            //------------Execute Test---------------------------
            var connectViewModel = new ConnectControlViewModel(env.Object, new EventAggregator());

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
            var connectViewModel = new ConnectControlViewModel(env.Object, new EventAggregator());

            //------------Execute Test---------------------------
            var selectedServer = connectViewModel.GetSelectedServer("Server");

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
            var connectViewModel = new ConnectControlViewModel(env.Object, new EventAggregator());

            //------------Execute Test---------------------------
            var selectedServer = connectViewModel.GetSelectedServer("RandomLabel");

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
            var connectViewModel = new ConnectControlViewModel(env.Object, new EventAggregator());

            //------------Execute Test---------------------------
            var selectedServer = connectViewModel.GetSelectedServer(string.Empty);

            //------------Assert Results-------------------------
            Assert.IsNull(selectedServer);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_DestinationInLabelTextAndLocalhostIsActiveAndOneOtherIsConnected_OtherConnected()
        {
            var localhostServer = CreateServer("localhost", true);
            var connectedServer = CreateServer("connected", true);
            List<IEnvironmentModel> servers = new List<IEnvironmentModel> { localhostServer, connectedServer };

            VerifyGetSelectedServer("Destination Server :", localhostServer, connectedServer, servers);
        }


        #region Moved from ConnectViewModelTest

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_OnlyOneServer_SourceIsThatServer()
        {
            var onlyServer = CreateServer("Server", true);
            List<IEnvironmentModel> servers = new List<IEnvironmentModel> { onlyServer };

            VerifyGetSelectedServer("Source Server :", onlyServer, onlyServer, servers);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsLocal_SourceSetToLocal()
        {
            var localhostServer = CreateServer("localhost", true);
            var otherServer = CreateServer("Server2", true);
            List<IEnvironmentModel> servers = new List<IEnvironmentModel> { localhostServer, otherServer };


            VerifyGetSelectedServer("Source Server :", localhostServer, localhostServer, servers);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsRemote_DestinationSetToLocal()
        {
            var activeServer = CreateServer("remote", true);
            var localhostServer = CreateServer("localhost", false);
            List<IEnvironmentModel> servers = new List<IEnvironmentModel> { localhostServer, activeServer };

            VerifyGetSelectedServer("Destination Server :", activeServer, localhostServer, servers);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsLocalServerAndThereIsOneOtherDisconnectedServer_DestinationSetToOtherServerAndItConnectsAutomatically()
        {
            var localhostServer = CreateServer("localhost", true);
            var disconnectedServer = CreateServer("disconnected", false);
            List<IEnvironmentModel> servers = new List<IEnvironmentModel> { localhostServer, disconnectedServer };

            var disconnectedEnv = Mock.Get(disconnectedServer);
            disconnectedEnv.Setup(e => e.ForceLoadResources()).Verifiable();
            disconnectedEnv.Setup(env => env.Connect()).Verifiable();

            VerifyGetSelectedServer("Destination Server :", localhostServer, disconnectedServer, servers);

            disconnectedEnv.Verify(env => env.Connect(), Times.Once());
            disconnectedEnv.Verify(env => env.ForceLoadResources(), Times.Once());
        }

        #endregion


        static void VerifyGetSelectedServer(string labelText, IEnvironmentModel activeServer, IEnvironmentModel expectedServer, List<IEnvironmentModel> otherServers)
        {
            //------------Setup for test--------------------------                       

            var connectViewModel = new ConnectControlViewModel(activeServer, new EventAggregator());

            foreach(IEnvironmentModel envModel in otherServers)
            {
                connectViewModel.Servers.Add(envModel);
            }

            //------------Execute Test---------------------------
            var selectedServer = connectViewModel.GetSelectedServer(labelText);

            //------------Assert Results-------------------------
            Assert.AreSame(expectedServer, selectedServer);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_NullEnvironmentModel_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers();
            //------------Assert Results-------------------------
            Assert.AreEqual(3, connectViewModel.Servers.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_WithEnvironmentModel_ShouldLoadServerSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers(remoteServer);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, connectViewModel.Servers.Count);
            Assert.AreEqual(remoteServer.Name, connectViewModel.SelectedServer.Name);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_WithEnvironmentModel_ShouldLoadServerSetSelectedServer_IsEditEnabledTrue()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers(remoteServer);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, connectViewModel.Servers.Count);
            Assert.AreEqual(remoteServer.Name, connectViewModel.SelectedServer.Name);
            Assert.IsTrue(connectViewModel.IsEditEnabled.GetValueOrDefault(false));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_WithEnvironmentModelNotFound_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, otherServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers(remoteServer);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, connectViewModel.Servers.Count);
            Assert.IsNull(connectViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_NullEnvironmentModelWithMutlipleServers_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, connectViewModel.Servers.Count);
            Assert.AreEqual(localhostServer.Name, connectViewModel.Servers[0].Name);
            Assert.IsFalse(connectViewModel.IsEditEnabled.GetValueOrDefault(true));
        }

        public static ImportServiceContext EnviromentRepositoryImportServiceContext;
        static void SetupMef()
        {

            var eventAggregator = new Mock<IEventAggregator>();

            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer(eventAggregator.Object);

            EnviromentRepositoryImportServiceContext = importServiceContext;
        }


        static IEnvironmentModel CreateServer(string name, bool isConnected)
        {
            var isLocalhost = name == "localhost";

            var env = new Mock<IEnvironmentModel>();
            env.SetupProperty(e => e.Name, name);
            env.Setup(e => e.ID).Returns(isLocalhost ? Guid.Empty : Guid.NewGuid());
            env.Setup(e => e.IsConnected).Returns(isConnected);
            env.Setup(e => e.IsLocalHost).Returns(isLocalhost);

            return env.Object;
        }
    }
}
