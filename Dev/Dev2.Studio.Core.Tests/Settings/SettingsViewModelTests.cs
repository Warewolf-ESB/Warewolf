/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Diagnostics.Test;
using Dev2.PerformanceCounters.Management;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class SettingsViewModelTests
    {

        [TestInitialize]
        public void SetupForTest()
        {
            var shell = new Mock<IShellViewModel>();
            var lcl = new Mock<IServer>();
            lcl.Setup(a => a.ResourceName).Returns("Localhost");
            shell.Setup(x => x.LocalhostServer).Returns(lcl.Object);
            // ReSharper disable once RedundantTypeArgumentsOfMethod
            CustomContainer.Register<IShellViewModel>(shell.Object);
            // ReSharper disable once RedundantTypeArgumentsOfMethod
            CustomContainer.Register<IEventAggregator>(new Mock<IEventAggregator>().Object);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SettingsViewModel_Constructor_NullPopupController_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, null, null, null, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object);
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
            new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, null, null, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object);
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
            new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, null, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object);
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
            var settingsViewModel = new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object);

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
            var settingsViewModel = new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object) { ShowLogging = true };

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(settingsViewModel.ShowSecurity);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowLogging")]
        public void SettingsViewModel_ShowLogging_SameValue_DoesNotRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object) { ShowLogging = true };
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowLogging = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowLogging")]
        public void SettingsViewModel_ShowLogging_DifferentValue_DoesRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object) { ShowLogging = true };
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowLogging = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowSecurity")]
        public void SettingsViewModel_ShowSecurity_True_OtherShowPropertiesAreFalse()
        {
            //------------Setup for test--------------------------
            var settingsViewModel = new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object) { ShowSecurity = false };

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

            var settingsViewModel = new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object) { ShowSecurity = true };
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

            var settingsViewModel = new SettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new Mock<IAsyncWorker>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object) { ShowSecurity = true };
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
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString(), "success", securityViewModel);
            viewModel.IsDirty = true;
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(true);
            Mock<IAuthorizationService> authService = new Mock<IAuthorizationService>();
            authService.Setup(c => c.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            environment.Setup(c => c.AuthorizationService).Returns(authService.Object);
            var repo = new Mock<IResourceRepository>();
            environment.Setup(a => a.ResourceRepository).Returns(repo.Object);
            viewModel.CurrentEnvironment = environment.Object;
            PrivateObject p = new PrivateObject(viewModel,new PrivateType( typeof(SettingsViewModel)));
            p.SetProperty("SecurityViewModel", securityViewModel);
            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, securityViewModel.SaveHitCount);
        }





        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_ResultIsNull_HasErrorsIsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString(), null, securityViewModel);
            viewModel.IsDirty = true;


            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(true);
            Mock<IAuthorizationService> authService = new Mock<IAuthorizationService>();
            authService.Setup(c => c.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            environment.Setup(c => c.AuthorizationService).Returns(authService.Object);
            var repo = new Mock<IResourceRepository>();
            environment.Setup(a => a.ResourceRepository).Returns(repo.Object);
            viewModel.CurrentEnvironment = environment.Object;


            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------

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
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString(), null, securityViewModel);

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
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_DuplicateServerPermissions_HasErrorsIsTrueCorrectErrorMessage()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            securityViewModel.ServerPermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true
            });

            securityViewModel.ServerPermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true
            });
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString(), null, securityViewModel);

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(true);
            Mock<IAuthorizationService> authService = new Mock<IAuthorizationService>();
            authService.Setup(c => c.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            environment.Setup(c => c.AuthorizationService).Returns(authService.Object);
            viewModel.CurrentEnvironment = environment.Object;
            viewModel.IsDirty = true;
            PrivateObject p = new PrivateObject(viewModel.SecurityViewModel);
            p.SetProperty("ServerPermissions", new ObservableCollection<WindowsGroupPermission>(){new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true
            },
            new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true
            }});

            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------            
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsFalse(viewModel.IsSaved);
            Assert.IsTrue(viewModel.HasErrors);
            
            var expected = StringResources.SaveSettingsDuplicateServerPermissions;


            Assert.AreEqual(expected.ToString(CultureInfo.InvariantCulture), viewModel.Errors.ToString(CultureInfo.InvariantCulture));
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_DuplicateResourcePermissions_HasErrorsIsTrueCorrectErrorMessage()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var resourceId = Guid.NewGuid();
            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false
            });

            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false,
            });
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString(), null, securityViewModel);

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(true);
            Mock<IAuthorizationService> authService = new Mock<IAuthorizationService>();
            authService.Setup(c => c.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            environment.Setup(c => c.AuthorizationService).Returns(authService.Object);
            //viewModel.CurrentEnvironment = environment.Object;
            viewModel.IsDirty = true;
            PrivateObject p = new PrivateObject(viewModel.SecurityViewModel);
            p.SetProperty("ResourcePermissions",new ObservableCollection<WindowsGroupPermission>(){new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false
            },
            new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false
            }});

            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------            
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsFalse(viewModel.IsSaved);
            Assert.IsTrue(viewModel.HasErrors);
            Assert.AreEqual(@"There are duplicate permissions for a resource, 
    i.e. one resource has permissions set twice with the same group. 
    Please clear the duplicates before saving.", viewModel.Errors);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SettingsViewModel_SaveCommand")]
        public void SettingsViewModel_SaveCommand_NotConnected_HasErrorsIsTrueCorrectErrorMessage()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString(), null, securityViewModel);

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
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString(), ErrorMessage, securityViewModel);
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.IsConnected).Returns(true);
            Mock<IAuthorizationService> authService = new Mock<IAuthorizationService>();
            authService.Setup(c => c.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            environment.Setup(c => c.AuthorizationService).Returns(authService.Object);
            var repo = new Mock<IResourceRepository>();
            environment.Setup(a => a.ResourceRepository).Returns(repo.Object);
            viewModel.CurrentEnvironment = environment.Object;
            repo.Setup(a => a.WriteSettings(It.IsAny<IEnvironmentModel>(), It.IsAny<Data.Settings.Settings>())).Returns(new ExecuteMessage() { HasError = true, Message = new StringBuilder(ErrorMessage) });
            viewModel.IsDirty = true;


            //------------Execute Test---------------------------
            viewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsFalse(viewModel.IsSaved);
            Assert.IsTrue(viewModel.HasErrors);
            Assert.AreEqual(ErrorMessage, viewModel.Errors);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsDirty_SecurityViewModelIsDirtyPropertyChanged_IsDirtyIsTrue()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());
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
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());

            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsDirty);
            Assert.AreEqual("SECURITY *", viewModel.SecurityHeader);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_WhenIsDirtySecurityModelFiresPropertyChange_SetsSettingsViewModelIsDirty()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());
            bool _wasCalled = false;
            viewModel.SecurityViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsDirty")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.IsDirty = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
            Assert.IsTrue(viewModel.IsDirty);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_WhenIsDirtyPerfCounterModelFiresPropertyChange_SetsSettingsViewModelIsDirty()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());
            bool _wasCalled = false;
            viewModel.PerfmonViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsDirty")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            viewModel.PerfmonViewModel.IsDirty = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsSecurityDirty_FalseSecurityNameHasNoStar()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());

            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.SecurityViewModel.IsDirty = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("SECURITY", viewModel.SecurityHeader);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsLoggingDirty_FalseLoggingNameHasNoStar()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());

            //------------Execute Test---------------------------
            viewModel.LogSettingsViewModel.ServerLogMaxSize = "10";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.LogSettingsViewModel.IsDirty = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("LOGGING", viewModel.LogHeader);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsLoggingDirty_TrueLoggingNameHasStar()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());

            //------------Execute Test---------------------------
            viewModel.LogSettingsViewModel.ServerLogMaxSize = "10";
            Assert.IsTrue(viewModel.IsDirty);
            //------------Assert Results-------------------------
            Assert.AreEqual("LOGGING *", viewModel.LogHeader);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsPerfCounterDirty_FalsePerfCounterNameHasNoStar()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());

            //------------Execute Test---------------------------
            viewModel.PerfmonViewModel.ResourceCounters[0].TotalErrors=true;
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.PerfmonViewModel.IsDirty = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("PERFORMANCE COUNTERS", viewModel.PerfmonHeader);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_IsPerfCounterDirty_TruePerfCounterNameHasStar()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateSettingsViewModel(CreateSettings().ToString());

            //------------Execute Test---------------------------
            viewModel.PerfmonViewModel.ResourceCounters[0].TotalErrors = true;
            Assert.IsTrue(viewModel.IsDirty);
            //------------Assert Results-------------------------
            Assert.AreEqual("PERFORMANCE COUNTERS *", viewModel.PerfmonHeader);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsViewModel_IsDirty")]
        public void SettingsViewModel_OnDeactivate_DirtyFalse_ShouldShowPopup()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            var viewModel = CreateSettingsViewModel(mockPopupController.Object, CreateSettings().ToString());
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.CallDeactivate();
            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController);
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
            var viewModel = CreateSettingsViewModel(mockPopupController.Object, CreateSettings().ToString(), "Success");
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.CallDeactivate();
            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController);
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
            var viewModel = CreateSettingsViewModel(mockPopupController.Object, CreateSettings().ToString(), "success", securityViewModel);
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.CallDeactivate();
            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController);
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
            var viewModel = CreateSettingsViewModel(mockPopupController.Object, CreateSettings().ToString(), "success", securityViewModel);
            //------------Execute Test---------------------------
            viewModel.SecurityViewModel.ResourcePermissions[0].WindowsGroup = "xxx";
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.CallDeactivate();
            //------------Assert Results-------------------------
            VerifySavePopup(mockPopupController);
            Assert.IsTrue(viewModel.IsDirty);
        }


        static void VerifySavePopup(Mock<IPopupController> popupController, bool showShown = true)
        {
            Times times = showShown ? Times.Once() : Times.Never();
            popupController.Verify(p => p.ShowSettingsCloseConfirmation(), times);

        }

        static TestSettingsViewModel CreateSettingsViewModel(string executeCommandReadResult = "", string executeCommandWriteResult = "", SecurityViewModel securityViewModel = null)
        {
            var mock = new Mock<IPopupController>();
            mock.Setup(controller => controller.Show()).Returns(MessageBoxResult.Yes);
            return CreateSettingsViewModel(mock.Object, executeCommandReadResult, executeCommandWriteResult, securityViewModel);
        }

        static TestSettingsViewModel CreateSettingsViewModel(IPopupController popupController, string executeCommandReadResult = "", string executeCommandWriteResult = "", SecurityViewModel securityViewModel = null)
        {
            var mockResourceRepo = new Mock<IResourceRepository>();
            ExecuteMessage writeMsg = null;
            if (!string.IsNullOrEmpty(executeCommandWriteResult))
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
            var viewModel = new TestSettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, popupController, new SynchronousAsyncWorker(), new Mock<IWin32Window>().Object, environment) { TheSecurityViewModel = securityViewModel };

            // simulate auto-loading of ConnectControl ComboBox
      

            return viewModel;
        }

        static Data.Settings.Settings CreateSettings()
        {
            var settings = new Data.Settings.Settings
            {
                Logging = CreateLoggingSettings(),
                Security = CreateSecuritySettings(),
                PerfCounters = CreatePerfCounterSettings()
            };
            return settings;
        }

        private static IPerformanceCounterTo CreatePerfCounterSettings()
        {
            var performanceCounterTo = new PerformanceCounterTo();
            var testCounter = new TestCounter { IsActive = true };
            performanceCounterTo.NativeCounters.Add(testCounter);
            var resourcePerformanceCounter = new TestResourceCounter { IsActive = true };
            performanceCounterTo.ResourceCounters.Add(resourcePerformanceCounter);
            return performanceCounterTo;
        }

        private static LoggingSettingsTo CreateLoggingSettings()
        {
            return new LoggingSettingsTo
            {
                FileLoggerLogLevel = "DEBUG",
                FileLoggerLogSize = 20
            };
        }

        private static SecuritySettingsTO CreateSecuritySettings()
        {
            return new SecuritySettingsTO(new[]
            {
                new WindowsGroupPermission
                {
                    IsServer = true, WindowsGroup = GlobalConstants.WarewolfGroup,
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
            };
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_IsSavedSuccessVisible")]
        public void SettingsViewModel_IsSavedSuccessVisible_HasErrorsFalseAndIsDirtyFalseAndIsSavedTrue_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = CreateSettingsViewModel();
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
            var settingsViewModel = CreateSettingsViewModel();
            settingsViewModel.HasErrors = true;
            settingsViewModel.IsDirty = true;
            settingsViewModel.IsSaved = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.IsSavedSuccessVisible);
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
            var viewModel = CreateSettingsViewModel(mockPopupController.Object, CreateSettings().ToString(), "Success", securityViewModel);
            viewModel.IsDirty = true;
            //------------Execute Test---------------------------
            var result = viewModel.DoDeactivate(true);
            //------------Assert Results-------------------------

            Assert.IsTrue(result);
            Assert.IsFalse(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SettingsViewModel_DoDeactivate")]
        public void SettingsViewModel_DoDeactivate_NoSavesChanges()
        {
            //------------Setup for test--------------------------            

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.SetupAllProperties();
            mockPopupController.Setup(controller => controller.ShowSettingsCloseConfirmation()).Returns(MessageBoxResult.No);
            var securityViewModel = new TestSecurityViewModel { IsDirty = true };
            var viewModel = CreateSettingsViewModel(mockPopupController.Object, CreateSettings().ToString(), "Success", securityViewModel);
            viewModel.IsDirty = true;
            bool propertyChanged = false;
            const string propertyName = "SecurityHeader";
            //------------Execute Test---------------------------
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    propertyChanged = true;
                }
            };

            var result = viewModel.DoDeactivate(true);
            //------------Assert Results-------------------------

            Assert.IsTrue(result);
            Assert.IsFalse(viewModel.IsDirty);
            Assert.IsTrue(propertyChanged);
            Assert.IsFalse(viewModel.SecurityViewModel.IsDirty);
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
            var viewModel = CreateSettingsViewModel(mockPopupController.Object, CreateSettings().ToString(), "success", securityViewModel);

            viewModel.IsDirty = true;
            //------------Execute Test---------------------------
            var result = viewModel.DoDeactivate(true);
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
            var settingsViewModel = CreateSettingsViewModel();
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
            var settingsViewModel = CreateSettingsViewModel();
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
            var settingsViewModel = CreateSettingsViewModel();
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
            var env = new Mock<IEnvironmentModel>();
            env.Setup(a => a.IsConnected).Returns(true);
            var viewModel = new TestSettingsViewModel(new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IPopupController>().Object, new SynchronousAsyncWorker(), new Mock<IWin32Window>().Object, env);
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    propertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            switch (settingsProperty)
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
