using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Diagnostics;
using Dev2.Core.Tests.Environments;
using Dev2.Messages;
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Custom_Dev2_Controls
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConnectControlTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControl_SelectionChanged_WhenHasItem_NotBoundToActiveEnvironment_ShouldNotFireMessages()
        {
            //------------Setup for test--------------------------
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.Servers = new ObservableCollection<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            connectControlViewModel.BindToActiveEnvironment = false;
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServer = remoteServer;
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControl_SelectionChanged_WhenHasItem_ViewModelIsSelectionFromTreeTrueWithRemoteServer_ShouldNotFireMessages()
        {
            //------------Setup for test--------------------------
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.SelectedServer = remoteServer;
            connectControlViewModel.IsSelectionFromTree = true;
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServer = remoteServer;
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControl_SelectionChanged_WhenHasItem_ViewModelIsSelectionFromTreeTrueWithLocalhost_ShouldNotFireMessages()
        {
            //------------Setup for test--------------------------
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.SelectedServer = localhostServer;
            connectControlViewModel.IsSelectionFromTree = true;
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServer = localhostServer;
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControl_SelectionChanged_WhenHasItem_ViewModelIsSelectionFromTreeFalseBindToActiveEnvironmentTrue_ShouldFireMessages()
        {
            //------------Setup for test--------------------------
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.BindToActiveEnvironment = true;
            connectControlViewModel.IsSelectionFromTree = false;
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServer = connectControlViewModel.Servers[1];
            connectControlViewModel.SelectedServer = remoteServer;
            connectControlViewModel.SelectedServer = connectControlViewModel.Servers[1];
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Once());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Once());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_HandleUpdateSelectedServer")]
        public void ConnectControl_HandleUpdateSelectedServer_WithSourceTrueLabelTextConnect_DoesNotUpdateSelectedServer()
        {
            //------------Setup for test--------------------------
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.LabelText = "Connect";
            connectControlViewModel.IsSelectionFromTree = false;
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = connectControlViewModel.Servers[0];
            connectControlViewModel.SelectedServer = localhostServer;
            var updateSelectedServerMessage = new UpdateSelectedServer(remoteServer, true);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreEqual(localhostServer, connectControlViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_HandleUpdateSelectedServer")]
        public void ConnectControl_HandleUpdateSelectedServer_WithSourceTrueLabelTextDestination_DoesNotUpdateSelectedServer()
        {
            //------------Setup for test--------------------------
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.LabelText = "Destination Server";
            connectControlViewModel.IsSelectionFromTree = false;
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = connectControlViewModel.Servers[0];
            connectControlViewModel.SelectedServer = localhostServer;
            var updateSelectedServerMessage = new UpdateSelectedServer(remoteServer, true);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreEqual(localhostServer, connectControlViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_HandleUpdateSelectedServer")]
        public void ConnectControl_HandleUpdateSelectedServer_WithSourceTrueLabelTextSource_UpdateSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.LabelText = "Source Server";
            connectControlViewModel.IsSelectionFromTree = false;
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = connectControlViewModel.Servers[0];
            connectControlViewModel.SelectedServer = remoteServer;
            var updateSelectedServerMessage = new UpdateSelectedServer(remoteServer, true);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreNotEqual(localhostServer, connectControlViewModel.SelectedServer);
            Assert.AreEqual(remoteServer.Name, connectControlViewModel.SelectedServer.Name);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_HandleUpdateSelectedServer")]
        public void ConnectControl_HandleUpdateSelectedServer_WithSourceFalseLabelTextDestination_UpdateSelectedServer()
        {
            //------------Setup for test--------------------------
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.LabelText = "Destination Server";
            connectControlViewModel.IsSelectionFromTree = false;
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = connectControlViewModel.Servers[0];
            connectControlViewModel.SelectedServer = localhostServer;
            var updateSelectedServerMessage = new UpdateSelectedServer(remoteServer, false);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreNotEqual(localhostServer, connectControlViewModel.SelectedServer);
            Assert.AreEqual(remoteServer.Name, connectControlViewModel.SelectedServer.Name);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_HandleUpdateSelectedServer")]
        public void ConnectControl_HandleUpdateSelectedServer_WithDestinationFalseLabelTextSource_DoesNotUpdateSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            new EnvironmentRepository(mockEnvironmentRepository);
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            connectControlViewModel.LabelText = "Source Server";
            connectControlViewModel.IsSelectionFromTree = false;
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = connectControlViewModel.Servers[0];
            connectControlViewModel.SelectedServer = localhostServer;
            var updateSelectedServerMessage = new UpdateSelectedServer(remoteServer, false);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreEqual(localhostServer, connectControlViewModel.SelectedServer);
        }

        public IApp CreateMockAppForTesting(IEnvironmentModel modelToReturnForActiveEnv)
        {
            IApp app = new MockApp();
            app.MainWindow = CreateApplicationMainWindow(modelToReturnForActiveEnv);
            return app;
        }

        public Window CreateApplicationMainWindow(IEnvironmentModel modelToReturnForActiveEnv)
        {
            Window window = new Window();
            window.DataContext = CreateMockMainViewModel(modelToReturnForActiveEnv).Object;
            return window;
        }

        public Mock<IMainViewModel> CreateMockMainViewModel(IEnvironmentModel modelToReturnForActiveEnv)
        {
            Mock<IMainViewModel> mockMainViewModel = new Mock<IMainViewModel>();
            mockMainViewModel.Setup(c => c.ActiveEnvironment).Returns(modelToReturnForActiveEnv);
            return mockMainViewModel;
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
            env.Setup(e => e.Name).Returns(name);
            env.Setup(e => e.ID).Returns(isLocalhost ? Guid.Empty : Guid.NewGuid());
            env.Setup(e => e.IsConnected).Returns(isConnected);
            env.Setup(e => e.IsLocalHost).Returns(isLocalhost);

            return env.Object;
        }
    }
}
