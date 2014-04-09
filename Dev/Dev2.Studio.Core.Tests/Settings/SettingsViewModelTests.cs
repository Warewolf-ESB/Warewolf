using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Core.Tests.Utils;
using Dev2.Messages;
using Dev2.Providers.Events;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Security;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
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
            // ReSharper disable ObjectCreationAsStatement
            new SettingsViewModel(new Mock<IEventAggregator>().Object, null, null, null);
            // ReSharper restore ObjectCreationAsStatement

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
            // ReSharper disable ObjectCreationAsStatement
            new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, null, null);
            // ReSharper restore ObjectCreationAsStatement

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
            // ReSharper disable ObjectCreationAsStatement
            new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, null);
            // ReSharper restore ObjectCreationAsStatement

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
            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object) { ShowLogging = true };

            //------------Execute Test---------------------------

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

            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object) { ShowLogging = true };
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

            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object) { ShowLogging = true };
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
            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object) { ShowSecurity = false };

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

            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object) { ShowSecurity = true };
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

            var settingsViewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object) { ShowSecurity = true };
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
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateViewModel(CreateSettings().ToString(), "success", securityViewModel);
            viewModel.IsDirty = true;
            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, securityViewModel.SaveHitCount);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_PopReturnsYes_ResultIsSuccess_IsDirtyFalse()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.ShowSettingsCloseConfirmation()).Returns(MessageBoxResult.Yes);
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString(), "Success");
            viewModel.IsDirty = true;
            viewModel.SecurityViewModel.IsDirty = true;
#pragma warning disable 168
            var description = "Security settings have not been saved." + Environment.NewLine
#pragma warning restore 168
 + "Would you like to save the settings? " + Environment.NewLine +
                                 "-------------------------------------------------------------------" +
                                 "Yes - Save the security settings." + Environment.NewLine +
                                 "No - Discard your changes." + Environment.NewLine +
                                 "Cancel - Returns you to security settings.";
            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.SecurityViewModel.IsDirty);
            Assert.IsFalse(viewModel.IsDirty);
            Assert.IsTrue(viewModel.IsSaved);
            Assert.IsFalse(viewModel.HasErrors);
            Assert.IsFalse(viewModel.IsLoading);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_ResultIsNull_HasErrorsIsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateViewModel(CreateSettings().ToString(), null, securityViewModel);
            viewModel.IsDirty = true;


            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.SecurityViewModel.IsDirty);
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsFalse(viewModel.IsSaved);
            Assert.IsTrue(viewModel.HasErrors);
            Assert.AreEqual(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "WriteSettings"), viewModel.Errors);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_NoAuth_HasErrorsIsTrueCorrectErrorMessage()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateViewModel(CreateSettings().ToString(), null, securityViewModel);

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(true);
            Mock<IAuthorizationService> authService = new Mock<IAuthorizationService>();
            authService.Setup(c => c.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(false);
            environment.Setup(c => c.AuthorizationService).Returns(authService.Object);
            viewModel.CurrentEnvironment = environment.Object;
            viewModel.IsDirty = true;


            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------            
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsFalse(viewModel.IsSaved);
            Assert.IsTrue(viewModel.HasErrors);
            Assert.AreEqual(@"Error while saving: You don't have permission to change settings on this server.
You need Administrator permission.", viewModel.Errors);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_NotConnected_HasErrorsIsTrueCorrectErrorMessage()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateViewModel(CreateSettings().ToString(), null, securityViewModel);

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(false);
            Mock<IAuthorizationService> authService = new Mock<IAuthorizationService>();
            authService.Setup(c => c.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            environment.Setup(c => c.AuthorizationService).Returns(authService.Object);
            viewModel.CurrentEnvironment = environment.Object;
            viewModel.IsDirty = true;

            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------            
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsFalse(viewModel.IsSaved);
            Assert.IsTrue(viewModel.HasErrors);
            Assert.AreEqual("Error while saving: Server unreachable.", viewModel.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_ResultIsError_HasErrorsIsTrue()
        {
            //------------Setup for test--------------------------
            const string ErrorMessage = "A message that is not just the word Success.";

            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateViewModel(CreateSettings().ToString(), ErrorMessage, securityViewModel);
            viewModel.IsDirty = true;


            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.SecurityViewModel.IsDirty);
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsFalse(viewModel.IsSaved);
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
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentNotConnected_DoesNothing()
        {
            //------------Setup for test--------------------------
            var viewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IWin32Window>().Object);
            Assert.IsNull(viewModel.CurrentEnvironment);

            var server = new Mock<IEnvironmentModel>();

            //------------Execute Test---------------------------
            viewModel.ServerChangedCommand.Execute(server.Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.CurrentEnvironment);
            Assert.IsFalse(viewModel.CurrentEnvironment.IsConnected);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentIsNotConnected_CurrentEnvironmentSetButNotConnected()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.ShowNotConnected()).Verifiable();

            var viewModel = new SettingsViewModel(new Mock<IEventAggregator>().Object, popupController.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IWin32Window>().Object);

            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(c => c.IsConnected).Returns(false);
            mockConnection.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEventAgg = new Mock<IEventAggregator>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var server = new EnvironmentModel(mockEventAgg.Object, Guid.NewGuid(), mockConnection.Object, mockResourceRepo.Object);

            Assert.IsNull(viewModel.CurrentEnvironment);
            Assert.IsTrue(server.CanStudioExecute);

            //------------Execute Test---------------------------
            viewModel.ServerChangedCommand.Execute(server);

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.CurrentEnvironment);
            Assert.IsFalse(viewModel.CurrentEnvironment.IsConnected);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentIsConnectedAndSettingsHasErrors_ShowsLoadError()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.Show()).Verifiable();
            popupController.SetupAllProperties();

            var settings = new Data.Settings.Settings
            {
                HasError = true,
                Error = "Error occurred loading",
                Security = new SecuritySettingsTO()
            };

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(popupController.Object, settings.ToString());

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ShowErrorHitCount);
            Assert.IsNotNull(viewModel.CurrentEnvironment);
            Assert.IsNotNull(viewModel.SecurityViewModel);
            Assert.IsFalse(viewModel.IsLoading);
            Assert.IsFalse(viewModel.IsDirty);
            Assert.IsTrue(viewModel.HasErrors);
            Assert.AreEqual(settings.Error, viewModel.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentIsConnectedAndSettingsIsNull_ShowsNetworkError()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.Show()).Verifiable();
            popupController.SetupAllProperties();

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(popupController.Object, null);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ShowErrorHitCount);
            Assert.IsNotNull(viewModel.CurrentEnvironment);
            Assert.IsNull(viewModel.SecurityViewModel);
            Assert.IsFalse(viewModel.IsLoading);
            Assert.IsFalse(viewModel.IsDirty);
            Assert.IsTrue(viewModel.HasErrors);
            Assert.AreEqual(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "ReadSettings"), viewModel.Errors);
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

            var serverPerms = settings.Security.WindowsGroupPermissions.Where(p => p.IsServer).ToList();
            var resourcePerms = settings.Security.WindowsGroupPermissions.Where(p => !p.IsServer).ToList();

            // SecurityViewModel adds extra "new" permission
            Assert.AreEqual(serverPerms.Count + 1, viewModel.SecurityViewModel.ServerPermissions.Count);
            Assert.AreEqual(resourcePerms.Count + 1, viewModel.SecurityViewModel.ResourcePermissions.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ServerChangedCommand")]
        public void SettingsViewModel_ServerChangedCommand_ServerEnvironmentIsConnected_PromptsUserIfDirty()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.ShowSettingsCloseConfirmation()).Returns(MessageBoxResult.Yes);
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString(), "Success");
            viewModel.IsDirty = true;
            viewModel.SecurityViewModel.IsDirty = true;
            var description = "Security settings have not been saved." + Environment.NewLine
                                 + "Would you like to save the settings? " + Environment.NewLine +
                                 "-------------------------------------------------------------------" +
                                 "Yes - Save the security settings." + Environment.NewLine +
                                 "No - Discard your changes." + Environment.NewLine +
                                 "Cancel - Returns you to security settings.";
            //------------Execute Test---------------------------
            var env = new Mock<IEnvironmentModel>();
            env.Setup(a => a.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);
            viewModel.Handle(new SelectedServerConnectedMessage(env.Object));

            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController, "Security Settings have changed", description);

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

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsDirty_TrueSecurityNameHasStar()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateViewModel(CreateSettings().ToString());

            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsDirty);
            Assert.AreEqual("Security *", viewModel.SecurityHeader);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsDirty_FalseSecurityNameHasNoStar()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateViewModel(CreateSettings().ToString());

            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.IsDirty = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("Security", viewModel.SecurityHeader);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_OnDeactivate_DirtyFalse_ShouldShowPopup()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString());
            var description = "Security settings have not been saved." + Environment.NewLine
                                 + "Would you like to save the settings? " + Environment.NewLine +
                                 "-------------------------------------------------------------------" +
                                 "Yes - Save the security settings." + Environment.NewLine +
                                 "No - Discard your changes." + Environment.NewLine +
                                 "Cancel - Returns you to security settings.";
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.CallDeactivate();
            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController, "Security Settings have changed", description);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_OnDeactivate_DirtyFalse_PopResultYes()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.ShowSettingsCloseConfirmation()).Returns(MessageBoxResult.Yes);
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString(), "Success");
            var description = "Security settings have not been saved." + Environment.NewLine
                                 + "Would you like to save the settings? " + Environment.NewLine +
                                 "-------------------------------------------------------------------" +
                                 "Yes - Save the security settings." + Environment.NewLine +
                                 "No - Discard your changes." + Environment.NewLine +
                                 "Cancel - Returns you to security settings.";
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.CallDeactivate();
            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController, "Security Settings have changed", description);
            Assert.IsFalse(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_OnDeactivate_RequestCloseFalse_NoPopup()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.Show()).Returns(MessageBoxResult.No);
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString(), "success", securityViewModel);
            var description = "Security settings have not been saved." + Environment.NewLine
                                 + "Would you like to save the settings? " + Environment.NewLine +
                                 "-------------------------------------------------------------------" +
                                 "Yes - Save the security settings." + Environment.NewLine +
                                 "No - Discard your changes." + Environment.NewLine +
                                 "Cancel - Returns you to security settings.";
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.CallDeactivate();
            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController, "Security Settings have changed", description);
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_OnDeactivate_DirtyFalse_PopResultNo()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.Show()).Returns(MessageBoxResult.No);
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString(), "success", securityViewModel);
            var description = "Security settings have not been saved." + Environment.NewLine
                                 + "Would you like to save the settings? " + Environment.NewLine +
                                 "-------------------------------------------------------------------" +
                                 "Yes - Save the security settings." + Environment.NewLine +
                                 "No - Discard your changes." + Environment.NewLine +
                                 "Cancel - Returns you to security settings.";
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.CallDeactivate();
            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController, "Security Settings have changed", description);
            Assert.IsTrue(viewModel.IsDirty);
        }


        static void VerifySavePopup(Mock<IPopupController> popupController, string expectedHeader, string expectedDescription, bool showShown = true)
        {
            Times times = showShown ? Times.Once() : Times.Never();
            popupController.Verify(p => p.ShowSettingsCloseConfirmation(), times);

        }

        static TestSettingsViewModel CreateViewModel(string executeCommandReadResult = "", string executeCommandWriteResult = "", SecurityViewModel securityViewModel = null)
        {
            var mock = new Mock<IPopupController>();
            mock.Setup(controller => controller.Show()).Returns(MessageBoxResult.Yes);
            return CreateViewModel(mock.Object, executeCommandReadResult, executeCommandWriteResult, securityViewModel);
        }

        static TestSettingsViewModel CreateViewModel(IPopupController popupController, string executeCommandReadResult = "", string executeCommandWriteResult = "", SecurityViewModel securityViewModel = null)
        {
            var viewModel = new TestSettingsViewModel(new Mock<IEventAggregator>().Object, popupController, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IWin32Window>().Object) { TheSecurityViewModel = securityViewModel };

            var mockResourceRepo = new Mock<IResourceRepository>();

            ExecuteMessage writeMsg = null;
            if(!string.IsNullOrEmpty(executeCommandWriteResult))
            {
                writeMsg = new ExecuteMessage { HasError = executeCommandWriteResult != "Success" };
                writeMsg.SetMessage(executeCommandWriteResult);
            }

            mockResourceRepo.Setup(c => c.ReadSettings(It.IsAny<IEnvironmentModel>())).Returns(executeCommandReadResult == null ? null : new Dev2JsonSerializer().Deserialize<Data.Settings.Settings>(executeCommandReadResult));
            mockResourceRepo.Setup(c => c.WriteSettings(It.IsAny<IEnvironmentModel>(), It.IsAny<Data.Settings.Settings>())).Returns(writeMsg);

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(c => c.ResourceRepository).Returns(mockResourceRepo.Object);
            Mock<IAuthorizationService> authService = new Mock<IAuthorizationService>();
            authService.Setup(c => c.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            environment.Setup(c => c.AuthorizationService).Returns(authService.Object);

            // simulate auto-loading of ConnectControl ComboBox
            viewModel.ServerChangedCommand.Execute(environment.Object);

            return viewModel;
        }

        static Data.Settings.Settings CreateSettings()
        {
            var settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new[]
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
                {
                    CacheTimeout = GlobalConstants.DefaultTimeoutValue
                }
            };
            return settings;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsSavedSuccessVisible")]
        public void SettingsViewModel_IsSavedSuccessVisible_HasErrorsFalseAndIsDirtyFalseAndIsSavedTrue_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = CreateViewModel();
            settingsViewModel.HasErrors = false;
            settingsViewModel.IsDirty = false;
            settingsViewModel.IsSaved = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(settingsViewModel.IsSavedSuccessVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsSavedSuccessVisible")]
        public void SettingsViewModel_IsSavedSuccessVisible_HasErrorsTrueAndIsDirtyTrueAndIsSavedFalse_False()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = CreateViewModel();
            settingsViewModel.HasErrors = true;
            settingsViewModel.IsDirty = true;
            settingsViewModel.IsSaved = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.IsSavedSuccessVisible);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SettingsViewModel_DoDeactivate")]
        public void SettingsViewModel_DoDeactivate_CancelChangesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.ShowSettingsCloseConfirmation()).Returns(MessageBoxResult.Cancel).Verifiable();
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString(), "Success", securityViewModel);

            viewModel.IsDirty = true;
            //------------Execute Test---------------------------
            var result = viewModel.DoDeactivate();

            var env = viewModel.CurrentEnvironment;
            viewModel.Handle(new SelectedServerConnectedMessage(env));
            //------------Assert Results-------------------------
            mockPopupController.Verify(c => c.ShowSettingsCloseConfirmation(), Times.Once());
            Assert.IsFalse(result);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SettingsViewModel_DoDeactivate")]
        public void SettingsViewModel_DoDeactivate_YesSavesChanges()
        {
            //------------Setup for test--------------------------            

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.ShowSettingsCloseConfirmation()).Returns(MessageBoxResult.Yes);
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString(), "Success", securityViewModel);
            viewModel.IsDirty = true;
            //------------Execute Test---------------------------
            var result = viewModel.DoDeactivate();
            //------------Assert Results-------------------------

            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SettingsViewModel_DoDeactivate")]
        public void SettingsViewModel_DoDeactivate_CancelNoReturnsTrue()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.ShowSettingsCloseConfirmation()).Returns(MessageBoxResult.Cancel);
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateViewModel(mockPopupController.Object, CreateSettings().ToString(), "success", securityViewModel);

            viewModel.IsDirty = true;
            //------------Execute Test---------------------------
            var result = viewModel.DoDeactivate();
            //------------Assert Results-------------------------

            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsErrorsVisible")]
        public void SettingsViewModel_IsErrorsVisible_HasErrorsTrue_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = CreateViewModel();
            settingsViewModel.HasErrors = true;
            settingsViewModel.IsDirty = false;
            settingsViewModel.IsSaved = false;

            //------------Assert Results-------------------------
            Assert.IsTrue(settingsViewModel.IsErrorsVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsErrorsVisible")]
        public void SettingsViewModel_IsErrorsVisible_HasErrorsFalseAndAndIsDirtyTrueAndIsSavedFalse_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = CreateViewModel();
            settingsViewModel.HasErrors = false;
            settingsViewModel.IsDirty = true;
            settingsViewModel.IsSaved = false;

            //------------Assert Results-------------------------
            Assert.IsTrue(settingsViewModel.IsErrorsVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsErrorsVisible")]
        public void SettingsViewModel_IsErrorsVisible_HasErrorsFalseAndAndIsDirtyFalseAndIsSavedTrue_False()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = CreateViewModel();
            settingsViewModel.HasErrors = false;
            settingsViewModel.IsDirty = false;
            settingsViewModel.IsSaved = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.IsErrorsVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsErrorsVisible")]
        public void SettingsViewModel_IsErrorsVisible_PropertyChangedFired()
        {
            Verify_PropertyChangedFired("IsErrorsVisible", SettingsProperty.HasErrors);
            Verify_PropertyChangedFired("IsErrorsVisible", SettingsProperty.IsDirty);
            Verify_PropertyChangedFired("IsErrorsVisible", SettingsProperty.IsSaved);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsSavedSuccessVisible")]
        public void SettingsViewModel_IsSavedSuccessVisible_PropertyChangedFired()
        {
            Verify_PropertyChangedFired("IsSavedSuccessVisible", SettingsProperty.HasErrors);
            Verify_PropertyChangedFired("IsSavedSuccessVisible", SettingsProperty.IsDirty);
            Verify_PropertyChangedFired("IsSavedSuccessVisible", SettingsProperty.IsSaved);
        }

        void Verify_PropertyChangedFired(string propertyName, SettingsProperty settingsProperty)
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;
            var viewModel = new TestSettingsViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IWin32Window>().Object);
            viewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == propertyName)
                {
                    propertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            switch(settingsProperty)
            {
                case SettingsProperty.HasErrors:
                    viewModel.HasErrors = true;
                    break;
                case SettingsProperty.IsDirty:
                    viewModel.IsDirty = true;
                    break;
                case SettingsProperty.IsSaved:
                    viewModel.IsSaved = true;
                    break;
            }

            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChanged);
        }

        enum SettingsProperty
        {
            HasErrors,
            IsDirty,
            IsSaved
        }
    }
}
