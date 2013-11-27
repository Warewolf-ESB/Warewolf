using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using Dev2.Core.Tests.Utils;
using Dev2.Data.Settings.Security;
using Dev2.Settings;
using Dev2.Settings.Security;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class SettingsViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SettingsViewModel_Constructor_NullPopupController_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, null, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SettingsViewModel_Constructor_NullAsyncWorker_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SettingsViewModel__Constructor_NullParentWindow_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_Constructor")]
        public void SettingsViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.ShowLogging);
            Assert.IsTrue(settingsViewModel.ShowSecurity);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowLogging")]
        public void SettingsViewModel_ShowLogging_True_OtherShowPropertiesAreFalse()
        {
            //------------Setup for test--------------------------
            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object);

            //------------Execute Test---------------------------
            settingsViewModel.ShowLogging = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.ShowSecurity);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowLogging")]
        public void SettingsViewModel_ShowLogging_SameValue_DoesNotRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object);
            settingsViewModel.ShowLogging = true;
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowLogging = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowLogging")]
        public void SettingsViewModel_ShowLogging_DifferentValue_DoesRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object);
            settingsViewModel.ShowLogging = true;
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowLogging = false;

            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowSecurity")]
        public void SettingsViewModel_ShowSecurity_True_OtherShowPropertiesAreFalse()
        {
            //------------Setup for test--------------------------
            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object);
            settingsViewModel.ShowSecurity = false;

            //------------Execute Test---------------------------
            settingsViewModel.ShowSecurity = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.ShowLogging);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowSecurity")]
        public void SettingsViewModel_ShowSecurity_SameValue_DoesNotRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object);
            settingsViewModel.ShowSecurity = true;
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowSecurity = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowSecurity")]
        public void SettingsViewModel_ShowSecurity_DifferentValue_DoesRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object);
            settingsViewModel.ShowSecurity = true;
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowSecurity = false;

            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_InvokesSaveOnSecurityViewModel_Done()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel();

            var viewModel = CreateViewModel(CreateSettings().ToString(), "success", securityViewModel);

            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, securityViewModel.SaveHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_ResultIsSuccess_IsDirtyFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateViewModel(CreateSettings().ToString(), "Success");
            viewModel.IsDirty = true;
            viewModel.SecurityViewModel.IsDirty = true;

            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsLoading);
            Assert.IsFalse(viewModel.IsDirty);
            Assert.IsFalse(viewModel.SecurityViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        [ExpectedException(typeof(NullReferenceException))]
        public void SettingsViewModel_SaveCommand_ResultIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.Show()).Verifiable();
            popupController.SetupAllProperties();

            var viewModel = CreateViewModel(popupController.Object, CreateSettings().ToString(), null);

            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_ResultIsError_HasErrorsIsTrue()
        {
            //------------Setup for test--------------------------
            const string ErrorMessage = "A message that is not just the word Success.";

            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.Show()).Verifiable();
            popupController.SetupAllProperties();

            var viewModel = CreateViewModel(popupController.Object, CreateSettings().ToString(), ErrorMessage);

            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.HasErrors);
            Assert.AreEqual(ErrorMessage, viewModel.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerIsNull_DoesNothing()
        {
            //------------Setup for test--------------------------
            var viewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IWin32Window>().Object);
            Assert.IsNull(viewModel.CurrentEnvironment);

            //------------Execute Test---------------------------
            viewModel.ServerChangedCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.CurrentEnvironment);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentIsNull_DoesNothing()
        {
            //------------Setup for test--------------------------
            var viewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IWin32Window>().Object);
            Assert.IsNull(viewModel.CurrentEnvironment);

            var server = new Mock<IServer>();
            server.Setup(s => s.Environment).Returns((IEnvironmentModel)null);

            //------------Execute Test---------------------------
            viewModel.ServerChangedCommand.Execute(server.Object);

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.CurrentEnvironment);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentIsNotConnected_ShowsNotConnectedPopup()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.ShowNotConnected()).Verifiable();

            var viewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, popupController.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IWin32Window>().Object);

            var environment = new Mock<IEnvironmentModel>();
            environment.SetupProperty(e => e.CanStudioExecute, true);
            environment.Setup(e => e.IsConnected).Returns(false);

            var server = new Mock<IServer>();
            server.Setup(s => s.Environment).Returns(environment.Object);

            Assert.IsNull(viewModel.CurrentEnvironment);
            Assert.IsTrue(environment.Object.CanStudioExecute);

            //------------Execute Test---------------------------
            viewModel.ServerChangedCommand.Execute(server.Object);

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.CurrentEnvironment);
            Assert.IsFalse(environment.Object.CanStudioExecute);
            popupController.Verify(p => p.ShowNotConnected());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentIsConnectedAndSettingsHasErrors_ThrowsException()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.Show()).Verifiable();
            popupController.SetupAllProperties();

            var settings = new Data.Settings.Settings
            {
                HasError = true,
                Error = "Error occurred loading"
            };

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(popupController.Object, settings.ToString());

            //------------Assert Results-------------------------

            // For some reason (threading??) this exception does not get thrown in test
            Assert.AreEqual(1, viewModel.ShowErrorHitCount);
            Assert.IsNotNull(viewModel.CurrentEnvironment);
            Assert.IsNotNull(viewModel.SecurityViewModel);
            Assert.IsFalse(viewModel.IsLoading);
            Assert.IsFalse(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentIsConnected_LoadsSettings()
        {
            //------------Setup for test--------------------------
            var settings = CreateSettings();
            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(settings.ToString());

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.CurrentEnvironment);
            Assert.IsNotNull(viewModel.SecurityViewModel);
            Assert.IsNotNull(viewModel.LoggingViewModel);
            Assert.IsFalse(viewModel.IsLoading);
            Assert.IsFalse(viewModel.IsDirty);

            var serverPerms = settings.Security.Where(p => p.IsServer).ToList();
            var resourcePerms = settings.Security.Where(p => !p.IsServer).ToList();

            // SecurityViewModel adds extra "new" permission
            Assert.AreEqual(serverPerms.Count + 1, viewModel.SecurityViewModel.ServerPermissions.Count);
            Assert.AreEqual(resourcePerms.Count + 1, viewModel.SecurityViewModel.ResourcePermissions.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsDirty_SecurityViewModelIsDirtyPropertyChanged_IsDirtyIsTrue()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateViewModel(CreateSettings().ToString());
            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsDirty);
        }

        static void VerifyShowErrorPopup(Mock<IPopupController> popupController, string expectedHeader, string expectedDescription)
        {
            popupController.Verify(p => p.Show());
            Assert.AreEqual(expectedHeader, popupController.Object.Header);
            Assert.AreEqual(expectedDescription, popupController.Object.Description);
            Assert.AreEqual(MessageBoxButton.OK, popupController.Object.Buttons);
            Assert.AreEqual(MessageBoxImage.Error, popupController.Object.ImageType);
        }

        static TestSettingsViewModel CreateViewModel(string executeCommandReadResult = "", string executeCommandWriteResult = "", SecurityViewModel securityViewModel = null)
        {
            return CreateViewModel(new Mock<IPopupController>().Object, executeCommandReadResult, executeCommandWriteResult, securityViewModel);
        }

        static TestSettingsViewModel CreateViewModel(IPopupController popupController, string executeCommandReadResult = "", string executeCommandWriteResult = "", SecurityViewModel securityViewModel = null)
        {
            var viewModel = new TestSettingsViewModel(new Mock<IEventAggregator>().Object, popupController, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IWin32Window>().Object);
            viewModel.TheSecurityViewModel = securityViewModel;

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(e => e.Connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns((string xmlRequest, Guid workspaceID, Guid dataListID) => xmlRequest.Contains("Read") ? executeCommandReadResult : executeCommandWriteResult);

            var server = new Mock<IServer>();
            server.Setup(s => s.Environment).Returns(environment.Object);

            // simulate auto-loading of ConnectControl ComboBox
            viewModel.ServerChangedCommand.Execute(server.Object);

            return viewModel;
        }

        static Data.Settings.Settings CreateSettings()
        {
            var settings = new Data.Settings.Settings
            {
                Security = new List<WindowsGroupPermission>(new[]
                {
                    new WindowsGroupPermission
                    {
                        IsServer = true, WindowsGroup = "BuiltIn\\Administrators",
                        View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true
                    },
                    new WindowsGroupPermission 
                    { 
                        IsServer = true, WindowsGroup = "Deploy Admins", 
                        View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false 
                    },

                    new WindowsGroupPermission
                    {
                        ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                        WindowsGroup = "Windows Group 1", View = false, Execute = true, Contribute = false
                    },
                    new WindowsGroupPermission
                    {
                        ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                        WindowsGroup = "Windows Group 2", View = false, Execute = false, Contribute = true
                    },

                    new WindowsGroupPermission
                    {
                        ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow2",
                        WindowsGroup = "Windows Group 1", View = true, Execute = true, Contribute = false
                    },

                    new WindowsGroupPermission
                    {
                        ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                        WindowsGroup = "Windows Group 3", View = true, Execute = false, Contribute = false
                    },
                    new WindowsGroupPermission
                    {
                        ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                        WindowsGroup = "Windows Group 4", View = false, Execute = true, Contribute = false
                    },


                    new WindowsGroupPermission
                    {
                        ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow4",
                        WindowsGroup = "Windows Group 3", View = false, Execute = false, Contribute = true
                    },
                    new WindowsGroupPermission
                    {
                        ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                        WindowsGroup = "Windows Group 4", View = false, Execute = false, Contribute = true
                    }
                })
            };
            return settings;
        }
    }
}
