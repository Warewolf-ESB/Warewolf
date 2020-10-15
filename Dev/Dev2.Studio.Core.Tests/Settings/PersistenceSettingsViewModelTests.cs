/*
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
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Persistence;
using Dev2.Settings.Persistence;
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
            var resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var persistenceSettingsViewModel = CreatePersistenceSettingViewModel(resourceRepo);
            //------------Execute Test---------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            persistenceSettingsViewModel.SelectedPersistenceDataSource = mockResource.Object;
            persistenceSettingsViewModel.EncryptDataSource = false;

            CustomContainer.Register(new Mock<IPopupController>().Object);
            //------------Assert Results-------------------------
            persistenceSettingsViewModel.Save(new PersistenceSettingsTo());
            resourceRepo.Verify(o => o.SavePersistenceSettings(It.IsAny<IServer>(), It.IsAny<PersistenceSettingsData>()), Times.Once);
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

        static PersistenceSettingsViewModel CreatePersistenceSettingViewModel(Mock<IResourceRepository> resourceRepo = null)
        {
            var env = new Mock<IServer>();
            if (resourceRepo is null)
            {
                resourceRepo = new Mock<IResourceRepository>();
            }
            var selectedDbSourceId = Guid.NewGuid();
            var settingsData = new PersistenceSettingsData
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
            resourceRepo.Setup(res => res.GetPersistenceSettings<PersistenceSettingsData>(env.Object)).Returns(settingsData);
            resourceRepo.Setup(res => res.SavePersistenceSettings(env.Object, settingsData)).Verifiable();

            IResource mockPersistenceSource = new DbSource()
            {
                ResourceID = selectedDbSourceId,
                ResourceName = "Persistence Data Source"
            };
            var expectedList = new List<IResource>();
            expectedList.Add(mockPersistenceSource);

            resourceRepo.Setup(resourceRepository => resourceRepository.FindResourcesByType<IPersistenceSource>(env.Object)).Returns(expectedList);
            env.Setup(a => a.ResourceRepository).Returns(resourceRepo.Object);
            var vm = new PersistenceSettingsViewModel(env.Object);
            return vm;
        }
    }
}