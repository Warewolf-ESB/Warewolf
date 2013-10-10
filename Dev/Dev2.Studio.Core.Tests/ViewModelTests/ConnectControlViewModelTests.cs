using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
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
            new ConnectControlViewModel(null);
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
            var connectViewModel = new ConnectControlViewModel(env.Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(connectViewModel.ActiveEnvironment);
            Assert.AreSame(env.Object, connectViewModel.ActiveEnvironment);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_BuildConnectViewModel")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_BuildConnectViewModel_WithNullDeployResourceAndNullActiveEnvironment_DoesNotCreateConnectViewModel()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            new ConnectControlViewModelBuilder().BuildConnectControlViewModel(null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_BuildConnectViewModel")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_BuildConnectViewModel_WithDeployResourceWithNullEnvironmentAndNullActiveEnvironment_DoesNotCreateConnectViewModel()
        {
            //------------Setup for test--------------------------
            var resourceModel = new ResourceModel(null);
            //------------Execute Test---------------------------
            new ConnectControlViewModelBuilder().BuildConnectControlViewModel(resourceModel, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_BuildConnectViewModel")]
        public void ConnectControlViewModel_BuildConnectViewModel_WithDeployResourceWithNullEnvironmentAndActiveEnvironment_CreateConnectViewModelWithActiveEnvironment()
        {
            //------------Setup for test--------------------------
            var resourceModel = new ResourceModel(null);
            var mainViewModelActiveEnvironment = new Mock<IEnvironmentModel>().Object;
            //------------Execute Test---------------------------
            var controlViewModel = new ConnectControlViewModelBuilder().BuildConnectControlViewModel(resourceModel, mainViewModelActiveEnvironment);
            //------------Assert Results-------------------------
            Assert.IsNotNull(controlViewModel);
            Assert.IsNotNull(controlViewModel.ActiveEnvironment);
            Assert.AreEqual(mainViewModelActiveEnvironment, controlViewModel.ActiveEnvironment);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_BuildConnectViewModel")]
        public void ConnectControlViewModel_BuildConnectViewModel_WithDeployResourceAsResourceModelWithEnvironmentAndActiveEnvironment_CreateConnectViewModelWithResourceModelEnvironmentAsActiveEnvironment()
        {
            //------------Setup for test--------------------------
            var resourceModelEnvironmentModel = new Mock<IEnvironmentModel>().Object;
            var resourceModel = new ResourceModel(resourceModelEnvironmentModel);
            var mainViewModelActiveEnvironment = new Mock<IEnvironmentModel>().Object;
            //------------Execute Test---------------------------
            var controlViewModel = new ConnectControlViewModelBuilder().BuildConnectControlViewModel(resourceModel, mainViewModelActiveEnvironment);
            //------------Assert Results-------------------------
            Assert.IsNotNull(controlViewModel);
            Assert.IsNotNull(controlViewModel.ActiveEnvironment);
            Assert.AreEqual(resourceModelEnvironmentModel, controlViewModel.ActiveEnvironment);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_BuildConnectViewModel")]
        public void ConnectControlViewModel_BuildConnectViewModel_WithDeployResourceAsAbstractTreeViewModelWithEnvironmentAndActiveEnvironment_CreateConnectViewModelWithAbstractTreeViewModelEnvironmentAsActiveEnvironment()
        {
            //------------Setup for test--------------------------
            var treeViewModelEnvironmentModel = new Mock<IEnvironmentModel>().Object;
            var resourceModel = new EnvironmentTreeViewModel(new Mock<IEventAggregator>().Object, null, treeViewModelEnvironmentModel);
            var mainViewModelActiveEnvironment = new Mock<IEnvironmentModel>().Object;
            //------------Execute Test---------------------------
            var controlViewModel = new ConnectControlViewModelBuilder().BuildConnectControlViewModel(resourceModel, mainViewModelActiveEnvironment);
            //------------Assert Results-------------------------
            Assert.IsNotNull(controlViewModel);
            Assert.IsNotNull(controlViewModel.ActiveEnvironment);
            Assert.AreEqual(treeViewModelEnvironmentModel, controlViewModel.ActiveEnvironment);
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
            var selectedServer = connectViewModel.GetSelectedServer(new ObservableCollection<IServer>(), string.Empty);

            //------------Assert Results-------------------------
            Assert.IsNull(selectedServer);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_SourceOrEmptyInLabelTextAndAliasDoesNotMatchActiveEnvironment_LocalhostServer()
        {
            var activeServer = CreatServer("Server1", false);
            var localhostServer = CreatServer("localhost", false);

            activeServer.Alias = "Server2"; // ensure active environment does not match server alias

            VerifyGetSelectedServer("Source Server :", activeServer, localhostServer, null);
            VerifyGetSelectedServer(string.Empty, activeServer, localhostServer, null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_DestinationInLabelTextAndLocalhostIsActiveAndOneOtherIsConnected_OtherConnected()
        {
            var localhostServer = CreatServer("localhost", true);
            var connectedServer = CreatServer("connected", true);

            VerifyGetSelectedServer("Destination Server :", localhostServer, connectedServer, null);
        }


        #region Moved from ConnectViewModelTest

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_OnlyOneServer_SourceIsThatServer()
        {
            var onlyServer = CreatServer("Server", true);

            VerifyGetSelectedServer("Source Server :", onlyServer, onlyServer, null);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsLocal_SourceSetToLocal()
        {
            var localhostServer = CreatServer("localhost", true);
            var otherServer = CreatServer("Server2", true);

            VerifyGetSelectedServer("Source Server :", localhostServer, localhostServer, otherServer);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsRemote_DestinationSetToLocal()
        {
            var activeServer = CreatServer("remote", true);
            var localhostServer = CreatServer("localhost", false);

            VerifyGetSelectedServer("Destination Server :", activeServer, localhostServer, null);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsLocalServerAndThereIsOneOtherDisconnectedServer_DestinationSetToOtherServerAndItConnectsAutomatically()
        {
            var localhostServer = CreatServer("localhost", true);
            var disconnectedServer = CreatServer("disconnected", false);

            var disconnectedEnv = Mock.Get(disconnectedServer.Environment);
            disconnectedEnv.Setup(e => e.ForceLoadResources()).Verifiable();
            disconnectedEnv.Setup(env => env.Connect()).Verifiable();

            VerifyGetSelectedServer("Destination Server :", localhostServer, disconnectedServer, null);

            disconnectedEnv.Verify(env => env.Connect(), Times.Once());
            disconnectedEnv.Verify(env => env.ForceLoadResources(), Times.Once());
        }

        #endregion


        static void VerifyGetSelectedServer(string labelText, IServer activeServer, IServer expectedServer, params IServer[] otherServers)
        {
            //------------Setup for test--------------------------
            var servers = new ObservableCollection<IServer>(otherServers ?? new IServer[0]) { activeServer };
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


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_NullEnvironmentModel_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreatServer("localhost", true);
            var remoteServer = CreatServer("remote", false);
            var otherServer = CreatServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer.Environment);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer.Environment,remoteServer.Environment,otherServer.Environment);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers();
            //------------Assert Results-------------------------
            Assert.AreEqual(3,connectViewModel.Servers.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_WithEnvironmentModel_ShouldLoadServerSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreatServer("localhost", true);
            var remoteServer = CreatServer("remote", false);
            var otherServer = CreatServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer.Environment);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer.Environment,remoteServer.Environment,otherServer.Environment);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers(remoteServer.Environment);
            //------------Assert Results-------------------------
            Assert.AreEqual(3,connectViewModel.Servers.Count);
            Assert.AreEqual(remoteServer.Alias,connectViewModel.SelectedServer.Alias);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_WithEnvironmentModel_ShouldLoadServerSetSelectedServer_IsEditEnabledTrue()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreatServer("localhost", true);
            var remoteServer = CreatServer("remote", false);
            var otherServer = CreatServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer.Environment);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer.Environment,remoteServer.Environment,otherServer.Environment);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers(remoteServer.Environment);
            //------------Assert Results-------------------------
            Assert.AreEqual(3,connectViewModel.Servers.Count);
            Assert.AreEqual(remoteServer.Alias,connectViewModel.SelectedServer.Alias);
            Assert.IsTrue(connectViewModel.IsEditEnabled.GetValueOrDefault(false));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_WithEnvironmentModelNotFound_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreatServer("localhost", true);
            var remoteServer = CreatServer("remote", false);
            var otherServer = CreatServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer.Environment);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer.Environment,otherServer.Environment);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers(remoteServer.Environment);
            //------------Assert Results-------------------------
            Assert.AreEqual(2,connectViewModel.Servers.Count);
            Assert.IsNull(connectViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_NullEnvironmentModelWithMutlipleServers_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreatServer("localhost", true);
            var connectViewModel = new ConnectControlViewModel(localhostServer.Environment);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer.Environment);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers();
            //------------Assert Results-------------------------
            Assert.AreEqual(1,connectViewModel.Servers.Count);
            Assert.AreEqual(localhostServer.Alias,connectViewModel.Servers[0].Alias);
            Assert.IsFalse(connectViewModel.IsEditEnabled.GetValueOrDefault(true));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_ChangeSelected")]
        public void ConnectControlViewModel_ChangeSelected_Null_LeavesSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreatServer("localhost", true);
            var connectViewModel = new ConnectControlViewModel(localhostServer.Environment);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer.Environment);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.ChangeSelected(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, connectViewModel.Servers.Count);
            Assert.AreEqual(localhostServer.Alias, connectViewModel.Servers[0].Alias);
            Assert.IsFalse(connectViewModel.IsEditEnabled.GetValueOrDefault(true));
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_ChangeSelected")]
        public void ConnectControlViewModel_ChangeSelected_WithEnvironment_SetsSelectedServer()
        {
            SetupMef();
            var localhostServer = CreatServer("localhost", true);
            var remoteServer = CreatServer("remote", false);
            var otherServer = CreatServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer.Environment);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer.Environment, remoteServer.Environment, otherServer.Environment);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.ChangeSelected(remoteServer.Environment);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, connectViewModel.Servers.Count);
            Assert.AreEqual(remoteServer.Alias, connectViewModel.SelectedServer.Alias);
            Assert.IsTrue(connectViewModel.IsEditEnabled.GetValueOrDefault(false));
        }
        public static ImportServiceContext EnviromentRepositoryImportServiceContext;
        static void SetupMef()
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);

            var eventAggregator = new Mock<IEventAggregator>();
            var wizardEngine = new Mock<IWizardEngine>();

            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer(securityContext.Object);
            ImportService.AddExportedValueToContainer(eventAggregator.Object);
            ImportService.AddExportedValueToContainer(wizardEngine.Object);

            EnviromentRepositoryImportServiceContext = importServiceContext;
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
