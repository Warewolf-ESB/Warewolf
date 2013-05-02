using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Network.Messaging.Messages;
using Dev2.Runtime.Configuration;
using Dev2.Studio.Core.Configuration;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Xml.Linq;

namespace Dev2.Core.Tests.ViewModelTests.Configuration
{
    [TestClass]
    public class RuntimeConfigurationViewModelTests
    {
        #region Load Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadWhereEnvironmentIsNullExpectedException()
        {
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            Mock<IPopupController> popup = new Mock<IPopupController>();
            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);

            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(null);
            runtimeConfigurationViewModel.Load(null);
        }

        [TestMethod]
        public void LoadWhereExcpetionOccursDuringMessageSendingWhenFetchingRuntimeConfigurationFromServerExpectedErrorDialogShown()
        {
            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(new Exception());
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            Mock<IPopupController> popup = new Mock<IPopupController>();

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Load(environment.Object);

            windowManager.Verify(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
        }

        [TestMethod]
        public void LoadWhereErrorMessageReturnedWhenFetchingRuntimeConfigurationFromServerExpectedErrorDialogShown()
        {
            ErrorMessage resultMessage = new ErrorMessage();

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            Mock<IPopupController> popup = new Mock<IPopupController>();

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Load(environment.Object);

            windowManager.Verify(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
        }

        [TestMethod]
        public void LoadWhereUnknownMessageReturnedWhenFetchingRuntimeConfigurationFromServerExpectedErrorDialogShown()
        {
            TestMessage resultMessage = new TestMessage();

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<TestMessage>(resultMessage);
            Mock<IPopupController> popup = new Mock<IPopupController>();
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Load(environment.Object);

            windowManager.Verify(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
        }

        [TestMethod]
        public void LoadWhereSettingsMessageReturnedWhenFetchingRuntimeConfigurationFromServerExpectedAssemblyAddedToRespository()
        {
            SettingsMessage resultMessage = new SettingsMessage
            {
                ConfigurationXml = new XElement("NoData"),
                AssemblyHashCode = "ABC",
                Assembly = new byte[1],
            };

            string actualAssemblyHashCode = null;
            byte[] actualAssembly = null;

            string expectedAssemblyHashCode = resultMessage.AssemblyHashCode;
            byte[] expectedAssembly = resultMessage.Assembly;

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IPopupController> popup = new Mock<IPopupController>();

            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            assemblyRepository.Setup(r => r.Add(It.IsAny<string>(), It.IsAny<byte[]>())).Callback((string s, byte[] b) =>
            {
                actualAssemblyHashCode = s;
                actualAssembly = b;
            }).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Load(environment.Object);

            assemblyRepository.Verify(r => r.Add(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
            Assert.AreEqual(actualAssemblyHashCode, expectedAssemblyHashCode, "The assembly hash information returned from the server isn't added to the repository.");
            Assert.AreEqual(expectedAssembly, actualAssembly, "The assembly information returned from the server isn't added to the repository.");
        }

        [TestMethod]
        public void LoadWhereNullAssemblyReturnedWhenLoadingUserControlExpectedErrorDialogShown()
        {
            SettingsMessage resultMessage = new SettingsMessage
            {
                ConfigurationXml = new XElement("NoData"),
                AssemblyHashCode = "ABC",
                Assembly = new byte[1],
            };

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IPopupController> popup = new Mock<IPopupController>();
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Load(environment.Object);

            windowManager.Verify(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
        }

        [TestMethod]
        public void LoadWhereAssemblyReturnedWhenLoadingUserControlDoesntContainTheExpectedTypeExpectedErrorDialogShown()
        {
            SettingsMessage resultMessage = new SettingsMessage
            {
                ConfigurationXml = new XElement("NoData"),
                AssemblyHashCode = "ABC",
                Assembly = new byte[1]
            };

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IPopupController> popup = new Mock<IPopupController>();

            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            assemblyRepository.Setup(r => r.Load(It.IsAny<string>())).Returns(typeof(RuntimeConfigurationViewModelTests).Assembly);

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Load(environment.Object);

            windowManager.Verify(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
        }

        [TestMethod]
        public void LoadWhereAssemblyReturnedWhenLoadingUserControlIsValid()
        {
            SettingsMessage resultMessage = new SettingsMessage
            {
                ConfigurationXml = new XElement("NoData"),
                AssemblyHashCode = "ABC",
                Assembly = new byte[1]
            };

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IPopupController> popup = new Mock<IPopupController>();

            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            assemblyRepository.Setup(r => r.Load(It.IsAny<string>())).Returns(typeof(IConfigurationAssemblyMarker).Assembly);

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Load(environment.Object);

            Assert.IsNotNull(runtimeConfigurationViewModel.RuntimeConfigurationUserControl, "The usercontrol wasn't populated from the configuration assembly.");
        }

        #endregion

        #region Save Tests

        [TestMethod]
        public void SaveWhereExcpetionOccursDuringMessageSendingToTheServerExpectedErrorDialogShown()
        {
            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(new Exception());
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            Mock<IPopupController> popup = new Mock<IPopupController>();

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Save(new XElement("NoData"));

            windowManager.Verify(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
        }

        [TestMethod]
        public void SaveWhereErrorMessageReturnedFromServerExpectedErrorDialogShown()
        {
            ErrorMessage resultMessage = new ErrorMessage();

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            Mock<IPopupController> popup = new Mock<IPopupController>();

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Save(new XElement("NoData"));

            windowManager.Verify(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
        }

        [TestMethod]
        public void SaveWhereUnknownMessageReturnedFromServerExpectedErrorDialogShown()
        {
            TestMessage resultMessage = new TestMessage();

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<TestMessage>(resultMessage);
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();

            Mock<IPopupController> popup = new Mock<IPopupController>();
            popup.Setup(p => p.Show()).Verifiable();

            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Save(new XElement("NoData"));

            popup.Verify(m => m.Show(), Times.Once(), "An error dialog was meant to be shown but it wasn't.");
        }

        [TestMethod]
        public void SaveWhereSuccessReturnedFromServerExpectedNoExceptionAndNoDialogs()
        {
            SettingsMessage resultMessage = new SettingsMessage
            {
                Result = NetworkMessageResult.Success
            };

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();

            Mock<IPopupController> popup = new Mock<IPopupController>();
            popup.Setup(p => p.Show()).Verifiable();

            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();
            windowManager.Setup(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Save(new XElement("NoData"));

            popup.Verify(p => p.Show(), Times.Never(), "A pop was shown on success, this means there was an unexpected error.");
            windowManager.Verify(m => m.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null), Times.Never(), "A error dialog was shown on success, this means there was an unexpected error.");
        }

        [TestMethod]
        public void SaveWhereUnknownReturnedFromServerExpectedPopupShown()
        {
            SettingsMessage resultMessage = new SettingsMessage
            {
                Result = NetworkMessageResult.Unknown
            };

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();

            Mock<IPopupController> popup = new Mock<IPopupController>();
            popup.Setup(p => p.Show()).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Save(new XElement("NoData"));

            popup.Verify(p => p.Show(), Times.Once(), "A pop was shown on success, this means there was an unexpected error.");
        }

        [TestMethod]
        public void SaveWhereVersionConflictReturnedFromServerExpectedNoExceptionPopup()
        {
            SettingsMessage resultMessage = new SettingsMessage
            {
                Result = NetworkMessageResult.VersionConflict
            };

            Mock<IEnvironmentModel> environment = Dev2MockFactory.SetupEnvironmentModel<SettingsMessage>(resultMessage);
            Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository = new Mock<IRuntimeConfigurationAssemblyRepository>();
            Mock<IWindowManager> windowManager = new Mock<IWindowManager>();

            Mock<IPopupController> popup = new Mock<IPopupController>();
            popup.Setup(p => p.Show()).Verifiable();

            ImportService.CurrentContext = CompositionInitializer.InitializeForSettingsViewModel(assemblyRepository, windowManager, popup);
            RuntimeConfigurationViewModel runtimeConfigurationViewModel = new RuntimeConfigurationViewModel(environment.Object);
            runtimeConfigurationViewModel.Save(new XElement("NoData"));

            popup.Verify(p => p.Show(), Times.Once(), "An popup was meant to be shown but it wasn't.");
        }

        #endregion

    }
}
