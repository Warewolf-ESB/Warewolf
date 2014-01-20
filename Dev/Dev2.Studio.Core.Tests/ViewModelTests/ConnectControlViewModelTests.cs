using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.Navigation;
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
            var treeViewModelEnvironmentModel = new Mock<IEnvironmentModel>();
            treeViewModelEnvironmentModel.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            var resourceModel = new EnvironmentTreeViewModel(new Mock<IEventAggregator>().Object, null, treeViewModelEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var mainViewModelActiveEnvironment = new Mock<IEnvironmentModel>().Object;
            //------------Execute Test---------------------------
            var controlViewModel = new ConnectControlViewModelBuilder().BuildConnectControlViewModel(resourceModel, mainViewModelActiveEnvironment);
            //------------Assert Results-------------------------
            Assert.IsNotNull(controlViewModel);
            Assert.IsNotNull(controlViewModel.ActiveEnvironment);
            Assert.AreEqual(treeViewModelEnvironmentModel.Object, controlViewModel.ActiveEnvironment);
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
            var selectedServer = connectViewModel.GetSelectedServer(new ObservableCollection<IEnvironmentModel>(), string.Empty);

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

            VerifyGetSelectedServer("Destination Server :", localhostServer, connectedServer, null);
        }


        #region Moved from ConnectViewModelTest

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_OnlyOneServer_SourceIsThatServer()
        {
            var onlyServer = CreateServer("Server", true);

            VerifyGetSelectedServer("Source Server :", onlyServer, onlyServer, null);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsLocal_SourceSetToLocal()
        {
            var localhostServer = CreateServer("localhost", true);
            var otherServer = CreateServer("Server2", true);

            VerifyGetSelectedServer("Source Server :", localhostServer, localhostServer, otherServer);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsRemote_DestinationSetToLocal()
        {
            var activeServer = CreateServer("remote", true);
            var localhostServer = CreateServer("localhost", false);

            VerifyGetSelectedServer("Destination Server :", activeServer, localhostServer, null);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectControlViewModel_GetSelectedServer")]
        public void ConnectControlViewModel_GetSelectedServer_ActiveIsLocalServerAndThereIsOneOtherDisconnectedServer_DestinationSetToOtherServerAndItConnectsAutomatically()
        {
            var localhostServer = CreateServer("localhost", true);
            var disconnectedServer = CreateServer("disconnected", false);

            var disconnectedEnv = Mock.Get(disconnectedServer);
            disconnectedEnv.Setup(e => e.ForceLoadResources()).Verifiable();
            disconnectedEnv.Setup(env => env.Connect()).Verifiable();

            VerifyGetSelectedServer("Destination Server :", localhostServer, disconnectedServer, null);

            disconnectedEnv.Verify(env => env.Connect(), Times.Once());
            disconnectedEnv.Verify(env => env.ForceLoadResources(), Times.Once());
        }

        #endregion


        static void VerifyGetSelectedServer(string labelText, IEnvironmentModel activeServer, IEnvironmentModel expectedServer, params IEnvironmentModel[] otherServers)
        {
            //------------Setup for test--------------------------
            var servers = new ObservableCollection<IEnvironmentModel>(otherServers ?? new IEnvironmentModel[0]) { activeServer };
            if(!expectedServer.Equals(activeServer))
            {
                servers.Add(expectedServer);
            }

            var connectViewModel = new ConnectControlViewModel(activeServer);

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
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer);
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
            var connectViewModel = new ConnectControlViewModel(localhostServer);
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
            var connectViewModel = new ConnectControlViewModel(localhostServer);
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
            var connectViewModel = new ConnectControlViewModel(localhostServer);
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
            var connectViewModel = new ConnectControlViewModel(localhostServer);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.LoadServers();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, connectViewModel.Servers.Count);
            Assert.AreEqual(localhostServer.Name, connectViewModel.Servers[0].Name);
            Assert.IsFalse(connectViewModel.IsEditEnabled.GetValueOrDefault(true));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_ChangeSelected")]
        public void ConnectControlViewModel_ChangeSelected_Null_LeavesSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var connectViewModel = new ConnectControlViewModel(localhostServer);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.ChangeSelected(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, connectViewModel.Servers.Count);
            Assert.AreEqual(localhostServer.Name, connectViewModel.Servers[0].Name);
            Assert.IsFalse(connectViewModel.IsEditEnabled.GetValueOrDefault(true));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_ChangeSelected")]
        public void ConnectControlViewModel_ChangeSelected_WithEnvironment_SetsSelectedServer()
        {
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            //------------Execute Test---------------------------
            connectViewModel.ChangeSelected(remoteServer);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, connectViewModel.Servers.Count);
            Assert.AreEqual(remoteServer.Name, connectViewModel.SelectedServer.Name);
            Assert.IsTrue(connectViewModel.IsEditEnabled.GetValueOrDefault(false));
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
            env.Setup(e => e.IsLocalHost()).Returns(isLocalhost);

            return env.Object;
        }
    }
}
