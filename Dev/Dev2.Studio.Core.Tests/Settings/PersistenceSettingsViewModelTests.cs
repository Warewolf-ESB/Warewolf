﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Persistence;
using Dev2.Settings.Persistence;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;
using Warewolf.Data;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    [TestCategory("Studio Settings Core")]
    public class PersistenceSettingsViewModelTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_Constructor_Equals_Null_ReturnFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = CreatePersistenceSettingViewModel();
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            persistenceSettingsViewModel.SetItem(viewModel);

            //------------Execute Test---------------------------
            var expected = viewModel.Equals(persistenceSettingsViewModel);
            //------------Assert Results-------------------------
            Assert.IsFalse(expected);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_Constructor_Equals_NotNull_ReturnTrue()
        {
            //------------Setup for test--------------------------
            var viewModel = CreatePersistenceSettingViewModel();
            var viewModel2 = CreatePersistenceSettingViewModel();
            viewModel2.SetItem(viewModel);
            viewModel2.ResourceSourceId = viewModel.ResourceSourceId;

            //------------Execute Test---------------------------
            var expected = viewModel.Equals(viewModel2);
            //------------Assert Results-------------------------
            Assert.IsTrue(expected);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_EncryptDataSource_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "EncryptDataSource")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.EncryptDataSource = false;
            //------------Assert Results-------------------------
            Assert.AreEqual(false, persistenceSettingsViewModel.EncryptDataSource);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_Enable_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Enable")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.Enable = false;
            //------------Assert Results-------------------------
            Assert.AreEqual(false, persistenceSettingsViewModel.Enable);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_PrepareSchemaIfNecessary_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "PrepareSchemaIfNecessary")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.PrepareSchemaIfNecessary = false;
            //------------Assert Results-------------------------
            Assert.AreEqual(false, persistenceSettingsViewModel.PrepareSchemaIfNecessary);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_ServerName_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ServerName")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.ServerName = "ServerName";
            //------------Assert Results-------------------------
            Assert.AreEqual( "ServerName", persistenceSettingsViewModel.ServerName);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_DashboardName_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DashboardName")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.DashboardName = "DashboardName";
            //------------Assert Results-------------------------
            Assert.AreEqual("DashboardName", persistenceSettingsViewModel.DashboardName);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_DashboardHostname_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DashboardHostname")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.DashboardHostname = "DashboardHostname";
            //------------Assert Results-------------------------
            Assert.AreEqual("DashboardHostname", persistenceSettingsViewModel.DashboardHostname);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_DashboardPort_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DashboardPort")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.DashboardPort = "DashboardPort";
            //------------Assert Results-------------------------
            Assert.AreEqual("DashboardPort", persistenceSettingsViewModel.DashboardPort);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_HangfireDashboardUrl_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "HangfireDashboardUrl")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.DashboardHostname = "NewHostName";
            //------------Assert Results-------------------------
            Assert.AreEqual("NewHostName:5001/Dashboardname", persistenceSettingsViewModel.HangfireDashboardUrl);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);

            hasPropertyChanged = false;
            persistenceSettingsViewModel.DashboardPort = "4444";
            Assert.AreEqual("NewHostName:4444/Dashboardname", persistenceSettingsViewModel.HangfireDashboardUrl);
            Assert.IsTrue(hasPropertyChanged);

            hasPropertyChanged = false;
            persistenceSettingsViewModel.DashboardName = "NewName";
            Assert.AreEqual("NewHostName:4444/NewName", persistenceSettingsViewModel.HangfireDashboardUrl);
            Assert.IsTrue(hasPropertyChanged);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_PersistenceScheduler_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "PersistenceScheduler")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            persistenceSettingsViewModel.PersistenceScheduler = "Hangfire";
            //------------Assert Results-------------------------
            Assert.AreEqual("Hangfire", persistenceSettingsViewModel.PersistenceScheduler);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_ResourceSourceId_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel();
            var hasPropertyChanged = false;
            persistenceSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ResourceSourceId")
                {
                    hasPropertyChanged = true;
                }
            };
            Assert.IsFalse(persistenceSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            var newResourceId= Guid.NewGuid();
            persistenceSettingsViewModel.ResourceSourceId = newResourceId;
            //------------Assert Results-------------------------
            Assert.AreEqual(newResourceId, persistenceSettingsViewModel.ResourceSourceId);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(persistenceSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_Save()
        {
            CustomContainer.Register(new Mock<IPopupController>().Object);

            var resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel(resourceRepo);
            //------------Execute Test---------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            persistenceSettingsViewModel.SelectedPersistenceDataSource = mockResource.Object;
            persistenceSettingsViewModel.EncryptDataSource = false;

            //------------Assert Results-------------------------
            persistenceSettingsViewModel.Save(new PersistenceSettingsTo());
            resourceRepo.Verify(o => o.SavePersistenceSettings(It.IsAny<IServer>(), It.IsAny<PersistenceSettingsData>()), Times.Once);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_Save_ConfirmEnableIsTrue()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel(resourceRepo);
            //------------Execute Test---------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            persistenceSettingsViewModel.SelectedPersistenceDataSource = mockResource.Object;
            persistenceSettingsViewModel.Enable = false;

            CustomContainer.Register(new Mock<IPopupController>().Object);
            //------------Assert Results-------------------------
            persistenceSettingsViewModel.Save(new PersistenceSettingsTo());
            resourceRepo.Verify(o => o.SavePersistenceSettings(It.IsAny<IServer>(), It.IsAny<PersistenceSettingsData>()), Times.Once);
            Assert.AreEqual(true, persistenceSettingsViewModel.Enable);
            Assert.AreEqual("Default", persistenceSettingsViewModel.SelectedPersistenceDataSource.ResourceName);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var viewModel = CreatePersistenceSettingViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
            viewModel.CloseHelpCommand.Execute(null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_HangfireDashboardBrowser_OpenInBrowser()
        {
            const string expectedUrl = "http://localhost:5001/hangfire";
            var uri = new Uri(expectedUrl);
            var mockExternalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExternalProcessExecutor.Setup(o => o.OpenInBrowser(uri)).Verifiable();

            var viewModel = CreateViewModelWithExternalProcess(mockExternalProcessExecutor);
            viewModel.DashboardHostname = "http://localhost";
            viewModel.DashboardPort = "5001";
            viewModel.DashboardName = "hangfire";
            viewModel.HangfireDashboardBrowserCommand.Execute(null);

            mockExternalProcessExecutor.Verify(o => o.OpenInBrowser(uri), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_IsPageComplete_SchedulerErrorMsg()
        {
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(o => o.ShowSaveErrorDialog(StringResources.SaveSettingsPermissionsSchedulerErrorMsg))
                .Returns(MessageBoxResult.OK).Verifiable();
            CustomContainer.Register(mockPopupController.Object);

            var resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel(resourceRepo, false);
            persistenceSettingsViewModel.PersistenceScheduler = null;
            //------------Execute Test---------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            persistenceSettingsViewModel.SelectedPersistenceDataSource = mockResource.Object;
            persistenceSettingsViewModel.EncryptDataSource = false;

            //------------Assert Results-------------------------
            persistenceSettingsViewModel.Save(new PersistenceSettingsTo());
            mockPopupController.Verify(o => o.ShowSaveErrorDialog(StringResources.SaveSettingsPermissionsSchedulerErrorMsg), Times.Once);
            resourceRepo.Verify(o => o.SavePersistenceSettings(It.IsAny<IServer>(), It.IsAny<PersistenceSettingsData>()), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_IsPageComplete_ServerErrorMsg()
        {
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(o => o.ShowSaveErrorDialog(StringResources.SaveSettingsPermissionsServerErrorMsg))
                .Returns(MessageBoxResult.OK).Verifiable();
            CustomContainer.Register(mockPopupController.Object);

            var resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel(resourceRepo, false);
            persistenceSettingsViewModel.SelectedPersistenceScheduler = "Hangfire";
            //------------Execute Test---------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            persistenceSettingsViewModel.SelectedPersistenceDataSource = mockResource.Object;
            persistenceSettingsViewModel.EncryptDataSource = false;

            //------------Assert Results-------------------------
            persistenceSettingsViewModel.Save(new PersistenceSettingsTo());
            mockPopupController.Verify(o => o.ShowSaveErrorDialog(StringResources.SaveSettingsPermissionsServerErrorMsg), Times.Once);
            resourceRepo.Verify(o => o.SavePersistenceSettings(It.IsAny<IServer>(), It.IsAny<PersistenceSettingsData>()), Times.Never);
            Assert.AreEqual("Hangfire", persistenceSettingsViewModel.SelectedPersistenceScheduler);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_IsPageComplete_DataSourceErrorMsg()
        {
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(o => o.ShowSaveErrorDialog(StringResources.SaveSettingsPermissionsDataSourceErrorMsg))
                .Returns(MessageBoxResult.OK).Verifiable();
            CustomContainer.Register(mockPopupController.Object);

            var resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel(resourceRepo, false);
            persistenceSettingsViewModel.SelectedPersistenceScheduler = "Hangfire";
            persistenceSettingsViewModel.ServerName = "servername";
            //------------Execute Test---------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            persistenceSettingsViewModel.EncryptDataSource = false;

            //------------Assert Results-------------------------
            persistenceSettingsViewModel.Save(new PersistenceSettingsTo());
            mockPopupController.Verify(o => o.ShowSaveErrorDialog(StringResources.SaveSettingsPermissionsDataSourceErrorMsg), Times.Once);
            resourceRepo.Verify(o => o.SavePersistenceSettings(It.IsAny<IServer>(), It.IsAny<PersistenceSettingsData>()), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsViewModel))]
        public void PersistenceSettingsViewModel_IsPageComplete_SettingsChanged()
        {
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(o => o.ShowPersistenceSettingsChanged())
                .Returns(MessageBoxResult.No).Verifiable();
            CustomContainer.Register(mockPopupController.Object);

            var resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel(resourceRepo, false);
            persistenceSettingsViewModel.SelectedPersistenceScheduler = "Hangfire";
            persistenceSettingsViewModel.ServerName = "servername";
            //------------Execute Test---------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            persistenceSettingsViewModel.SelectedPersistenceDataSource = mockResource.Object;
            persistenceSettingsViewModel.EncryptDataSource = false;

            //------------Assert Results-------------------------
            persistenceSettingsViewModel.Save(new PersistenceSettingsTo());
            mockPopupController.Verify(o => o.ShowPersistenceSettingsChanged(), Times.Once);
            resourceRepo.Verify(o => o.SavePersistenceSettings(It.IsAny<IServer>(), It.IsAny<PersistenceSettingsData>()), Times.Never);
        }

        private static Mock<IServer> CreateMockServer(Mock<IResourceRepository> resourceRepo = null, bool addSettings = true)
        {
            var mockServer = new Mock<IServer>();
            if (resourceRepo is null)
            {
                resourceRepo = new Mock<IResourceRepository>();
            }
            var selectedDbSourceId = Guid.NewGuid();
            var settingsData = new PersistenceSettingsData();
            if (addSettings)
            {
                settingsData = new PersistenceSettingsData
                {
                    PersistenceScheduler = "Hangfire",
                    Enable = true,
                    PersistenceDataSource = new NamedGuidWithEncryptedPayload
                    {
                        Name = "Data Source",
                        Value = selectedDbSourceId,
                        Payload = "foo"
                    },
                    EncryptDataSource = true,
                    DashboardHostname = "DashboardHostname",
                    DashboardName = "Dashboardname",
                    DashboardPort = "5001",
                    PrepareSchemaIfNecessary = true,
                    ServerName = "servername"
                };
            }

            resourceRepo.Setup(res => res.GetPersistenceSettings<PersistenceSettingsData>(mockServer.Object)).Returns(settingsData);
            resourceRepo.Setup(res => res.SavePersistenceSettings(mockServer.Object, settingsData)).Verifiable();

            IResource mockPersistenceSource = new DbSource()
            {
                ResourceID = selectedDbSourceId,
                ResourceName = "Persistence Data Source"
            };
            var expectedList = new List<IResource> {mockPersistenceSource};

            resourceRepo.Setup(resourceRepository => resourceRepository.FindResourcesByType<IPersistenceSource>(mockServer.Object)).Returns(expectedList);
            mockServer.Setup(a => a.ResourceRepository).Returns(resourceRepo.Object);
            return mockServer;
        }

        private static PersistenceSettingsViewModel CreatePersistenceSettingViewModel(Mock<IResourceRepository> resourceRepo = null, bool addSettings = true)
        {
            var env = CreateMockServer(resourceRepo, addSettings);
            var vm = new PersistenceSettingsViewModel(env.Object);
            return vm;
        }

        private static PersistenceSettingsViewModel CreateViewModelWithExternalProcess(IMock<IExternalProcessExecutor> externalProcessMock, Mock<IResourceRepository> resourceRepo = null)
        {
            var env = CreateMockServer(resourceRepo);
            var vm = new PersistenceSettingsViewModel(env.Object, externalProcessMock.Object);
            return vm;
        }
    }
}