using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.ViewModels.Explorer;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for ConnectViewModelTest
    /// </summary>
    [TestClass]
    public class ConnectViewModelTest
    {
        static ImportServiceContext _importServiceContext;
        ConnectViewModel _connectViewmodel;

        #region MyTestInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = _importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());

            var mockSecurityContext = new Mock<IFrameworkSecurityContext>();
            ImportService.AddExportedValueToContainer(mockSecurityContext.Object);

            var wizardEngine = new Mock<IWizardEngine>();
            ImportService.AddExportedValueToContainer(wizardEngine.Object);
            }

        [TestInitialize]
        public void MyTestInitialize()
        {
            ImportService.CurrentContext = _importServiceContext;

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("xxxx");

            var targetEnvironment = new Mock<IEnvironmentModel>();
            targetEnvironment.Setup(e => e.Connection).Returns(connection.Object);

            _connectViewmodel = new ConnectViewModel(new Mock<IEventAggregator>().Object, new TestEnvironmentRespository(targetEnvironment.Object, new[] { targetEnvironment.Object }));
        }

        #endregion

        #region Okay Command

        [TestMethod]
        public void ConnectOkayCommandWithValidIPAddressExpectedSuccess()
        {
            _connectViewmodel.WebServerPort = "1234";
            _connectViewmodel.Name = "TestServer";
            _connectViewmodel.DsfAddress = "http://127.0.0.1:77/dsf";

            _connectViewmodel.OkayCommand.Execute("abc");

            Assert.AreEqual("http://127.0.0.1:77/dsf", _connectViewmodel.Server.AppAddress);
        }

        [TestMethod]
        public void ConnectOkayCommandWithValidDnsAddressExpectedResolvesSuccessfully()
        {
            _connectViewmodel.WebServerPort = "1234";
            _connectViewmodel.Name = "TestServer";
            _connectViewmodel.DsfAddress = "http://LoCaLhoSt:77/dsf";

            _connectViewmodel.OkayCommand.Execute("abc");

            Assert.AreEqual("http://127.0.0.1:77/dsf", _connectViewmodel.Server.AppAddress);
        }

        [TestMethod]
        public void ConnectOkayCommandWithInValidAddressExpectedFailure()
        {
            _connectViewmodel.OkayCommand.Execute("abc");
            Assert.AreEqual(null, _connectViewmodel.Server.AppAddress);
        }
        
        #endregion

        #region Cancel Command

        [TestMethod]
        public void ConnectCancelCommandWithInValidAddressExpectedDoesNotAdd()
        {
            _connectViewmodel.CancelCommand.Execute("abc");

            Assert.AreEqual(0, _connectViewmodel.Server.Servers.Count);
        }
        
        #endregion

        #region OnLoaded

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectViewModel_OnLoaded")]
        public void ConnectControl_OnLoaded_OnlyOneServer_SourceIsThatServer()
        {
            //New Mocks
            var control = new Mock<IConnectControl>();
            var expectedServer = new Mock<IServer>();

            //Setup Mock Control
            control.Setup(c => c.LabelText).Returns("Source Server :");
            control.Setup(c => c.Servers).Returns(new List<IServer> { expectedServer.Object });
            control.SetupSet(c => c.SelectedServer = expectedServer.Object);

            //Setup ViewModel
            var connectViewModel = new ConnectViewModel();
            var sourceServer = new Mock<IEnvironmentModel>();
            sourceServer.Setup(svr => svr.Name).Returns(control.Object.Servers[0].Alias);
            connectViewModel.ActiveEnvironment = sourceServer.Object;

            //------------Execute Test---------------------------
            connectViewModel.OnLoaded(control.Object, new RoutedEventArgs());

            // Assert item selected
            control.VerifySet(c => c.SelectedServer = expectedServer.Object, Times.Once(), "No server selected by default");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectViewModel_OnLoaded")]
        public void ConnectControl_OnLoaded_ActiveIsLocal_SourceSetToLocal()
        {
            //New Mocks
            var control = new Mock<IConnectControl>();
            var expectedServer = new Mock<IServer>();
            var expectedEnvironment = new Mock<IEnvironmentModel>();

            //Setup Mocks
            expectedEnvironment.Setup(env => env.Name).Returns("This is the Active Server");
            expectedServer.Setup(svr => svr.Alias).Returns("This is the Active Server");
            expectedServer.Setup(svr => svr.IsLocalHost).Returns(true);
            expectedServer.Setup(svr => svr.Environment).Returns(expectedEnvironment.Object);
            control.Setup(c => c.LabelText).Returns("Source Server :");
            control.Setup(c => c.Servers).Returns(new List<IServer> { new Mock<IServer>().Object, expectedServer.Object });
            control.SetupSet(c => c.SelectedServer = expectedServer.Object);

            //Setup ViewModel
            var connectViewModel = new ConnectViewModel { ActiveEnvironment = expectedServer.Object.Environment };

            //------------Execute Test---------------------------
            connectViewModel.OnLoaded(control.Object, new RoutedEventArgs());

            // Assert item selected
            control.VerifySet(c => c.SelectedServer = expectedServer.Object, Times.Once(), "No server selected by default");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectViewModel_OnLoaded")]
        public void ConnectControl_OnLoaded_ActiveIsRemote_DestinationSetToLocal()
        {
            //New Mocks
            var control = new Mock<IConnectControl>();
            var activeServer = new Mock<IServer>();
            var localServer = new Mock<IServer>();
            var expectedEnvironment = new Mock<IEnvironmentModel>();

            //Setup Mocks
            expectedEnvironment.Setup(env => env.Name).Returns("This is the Active Server");
            activeServer.Setup(svr => svr.Alias).Returns("This is the Active Server");
            activeServer.Setup(svr => svr.IsLocalHost).Returns(false);
            activeServer.Setup(svr => svr.Environment).Returns(expectedEnvironment.Object);
            localServer.Setup(svr => svr.IsLocalHost).Returns(true);
            control.Setup(c => c.LabelText).Returns("Destination Server :");
            control.Setup(c => c.Servers).Returns(new List<IServer> { localServer.Object, activeServer.Object });
            control.SetupSet(c => c.SelectedServer = activeServer.Object);

            //Setup ViewModel
            var connectViewModel = new ConnectViewModel { ActiveEnvironment = activeServer.Object.Environment };

            //------------Execute Test---------------------------
            connectViewModel.OnLoaded(control.Object, new RoutedEventArgs());

            // Assert item selected
            control.VerifySet(c => c.SelectedServer = localServer.Object, Times.Once(), "No server selected by default");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ConnectViewModel_OnLoaded")]
        public void ConnectControl_OnLoaded_ActiveIsLocalServerAndThereIsOneOtherDisconnectedServer_DestinationSetToOtherServerAndItConnectsAutomatically()
        {
            //New Mocks
            var control = new Mock<IConnectControl>();
            var activeServer = new Mock<IServer>();
            var remoteServer = new Mock<IServer>();
            var expectedEnvironment = new Mock<IEnvironmentModel>();
            var remoteEnvironment = new Mock<IEnvironmentModel>();

            //Setup Mocks
            expectedEnvironment.Setup(env => env.Name).Returns("This is the Active Server");
            expectedEnvironment.Setup(env => env.IsLocalHost()).Returns(true);
            remoteEnvironment.Setup(env => env.Connect()).Verifiable();
            remoteEnvironment.Setup(env => env.ForceLoadResources()).Verifiable();
            activeServer.Setup(svr => svr.Alias).Returns("This is the Active Server");
            activeServer.Setup(svr => svr.Environment).Returns(expectedEnvironment.Object);
            activeServer.Setup(svr => svr.IsLocalHost).Returns(true);
            remoteServer.Setup(svr => svr.Environment).Returns(remoteEnvironment.Object);
            remoteServer.Setup(svr => svr.IsLocalHost).Returns(false);
            control.Setup(c => c.LabelText).Returns("Destination Server :");
            control.Setup(c => c.Servers).Returns(new List<IServer> { remoteServer.Object, activeServer.Object });
            control.SetupSet(c => c.SelectedServer = activeServer.Object);

            //Setup ViewModel
            var connectViewModel = new ConnectViewModel { ActiveEnvironment = activeServer.Object.Environment };

            //------------Execute Test---------------------------
            connectViewModel.OnLoaded(control.Object, new RoutedEventArgs());

            // Assert item selected
            control.VerifySet(c => c.SelectedServer = remoteServer.Object, Times.Once(), "No server selected by default");
            remoteEnvironment.Verify(env => env.Connect(), Times.Once(), "Destination server did not autoconnect");
            remoteEnvironment.Verify(env => env.ForceLoadResources(), Times.Once(), "Destination server resources not loaded");
        }
        
        #endregion
    }
}
