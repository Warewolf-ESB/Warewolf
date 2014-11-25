
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Security;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Dialogs;
using Dev2.Explorer;
using Dev2.Models;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Dev2.Core.Tests.Dialogs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class ResourcePickerDialogTests
    {
        readonly Action<System.Action, DispatcherPriority> _Invoke = (a, b) => { };
        [TestInitialize]
        public void Initialize()
        {
            AppSettings.LocalHost = "http://localhost:3142";
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_NullEnvironmentRepository_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ResourcePickerDialog(enDsfActivityType.All, null, null, null, false, StudioResourceRepository.Instance, new Mock<IConnectControlSingleton>().Object);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_ConstructorWith2Args_NullEnvironmentRepository_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ResourcePickerDialog(enDsfActivityType.All, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_NullEventAggregator_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ResourcePickerDialog(enDsfActivityType.All, new Mock<IEnvironmentRepository>().Object, null, null, false, StudioResourceRepository.Instance, new Mock<IConnectControlSingleton>().Object);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_NullAsyncWorker_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ResourcePickerDialog(enDsfActivityType.All, new Mock<IEnvironmentRepository>().Object, new Mock<IEventAggregator>().Object, null, false, StudioResourceRepository.Instance, new Mock<IConnectControlSingleton>().Object);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ResourcePickerDialog(enDsfActivityType.All, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        public void ResourcePickerDialog_Constructor_EnvironmentModelIsNotNull_EnvironmentRepositoryIsNew()
        {
            //------------Setup for test-------------------------
            SetupMef();
            var dialogWindow = new Mock<IDialog>();

            //------------Execute Test---------------------------
            var environmentModel = new Mock<IEnvironmentModel>().Object;
            var dialog = new TestResourcePickerDialog(enDsfActivityType.All, environmentModel) { CreateDialogResult = dialogWindow.Object };
            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreNotSame(EnvironmentRepository.Instance, dialog.CreateDialogDataContext.NavigationViewModel.EnvironmentRepository);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        public void ResourcePickerDialog_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------
            SetupMef();
            var dialogWindow = new Mock<IDialog>();
            const enDsfActivityType ActivityType = enDsfActivityType.All;
            const bool IsFromActivityDrop = true;
            var repository = new StudioResourceRepository(GetTestData(), Guid.Empty, _Invoke);
            var envRepo = new TestLoadEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            //var envRepo = EnvironmentRepository.Create(new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            var dialog = new TestResourcePickerDialog(ActivityType, envRepo, new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, IsFromActivityDrop, repository)
            {
                CreateDialogResult = dialogWindow.Object
            };
            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreEqual(ActivityType, dialog.CreateDialogDataContext.ActivityType);
            Assert.AreEqual(IsFromActivityDrop, dialog.CreateDialogDataContext.NavigationViewModel.IsFromActivityDrop);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourcePickerDialog_ShowDialog")]
        public void ResourcePickerDialog_SelectedResourceNotNull_NavigationViewModelActivityDropViewModelItemSelected()
        {
            //------------Setup for test-------------------------
            SetupMef();
            var dialogWindow = new Mock<IDialog>();
            const enDsfActivityType ActivityType = enDsfActivityType.All;
            const bool IsFromActivityDrop = true;
            var repository = new StudioResourceRepository(GetTestData(), Guid.Empty, _Invoke);

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.CanStudioExecute).Returns(true);
            mockEnvironmentModel.Setup(model => model.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(connection => connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockEnvironmentConnection.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            var mockResourceRepository = CreateMockResourceRepository(mockEnvironmentModel);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            var envRepo = new TestLoadEnvironmentRespository(mockEnvironmentModel.Object);
            var selectedResource = mockResourceRepository.Object.All().ToList()[1];
            var dialog = new TestResourcePickerDialog(ActivityType, envRepo, mockEventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, IsFromActivityDrop, repository)
            {
                CreateDialogResult = dialogWindow.Object,
                SelectedResource = selectedResource
            };
            //------------Execute Test---------------------------

            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreEqual(selectedResource, dialog.CreateDialogDataContext.SelectedResourceModel);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourcePickerDialog_ShowDialog")]
        public void ResourcePickerDialog_SelectedResourceNotContextualResourceModel_NavigationViewModelActivityDropViewModelItemNotSelected()
        {
            //------------Setup for test-------------------------
            SetupMef();
            var dialogWindow = new Mock<IDialog>();
            const enDsfActivityType ActivityType = enDsfActivityType.All;
            const bool IsFromActivityDrop = true;

            var repository = new StudioResourceRepository(GetTestData(), Guid.Empty, _Invoke);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.CanStudioExecute).Returns(true);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(connection => connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockEnvironmentConnection.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            var mockResourceRepository = CreateMockResourceRepository(mockEnvironmentModel);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            var envRepo = new TestLoadEnvironmentRespository(mockEnvironmentModel.Object);
            var selectedResource = mockResourceRepository.Object.All().ToList()[1];
            var dialog = new TestResourcePickerDialog(ActivityType, envRepo, mockEventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, IsFromActivityDrop, repository)
            {
                CreateDialogResult = dialogWindow.Object,
                SelectedResource = new Mock<IResourceModel>().Object
            };
            //------------Execute Test---------------------------

            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreNotEqual(selectedResource, dialog.CreateDialogDataContext.SelectedResourceModel);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourcePickerDialog_DetermineDropActivityType")]
        public void ResourcePickerDialog_DetermineDropActivityType_WhenWorkflowExactMatch_ExpectWorkflow()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var typeOf = ResourcePickerDialog.DetermineDropActivityType("DsfWorkflowActivity");

            //------------Assert Results-------------------------
            Assert.AreEqual(enDsfActivityType.Workflow, typeOf);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourcePickerDialog_ShowDialog")]
        public void ResourcePickerDialog_WithEnvironmentModel_NavigationViewModelFiltered()
        {
            //------------Setup for test-------------------------
            SetupMef();
            var dialogWindow = new Mock<IDialog>();
            const enDsfActivityType ActivityType = enDsfActivityType.All;
            const bool IsFromActivityDrop = true;

            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.CanStudioExecute).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            var environment2Id = Guid.NewGuid();

            var mockEnvironmentModel2 = new Mock<IEnvironmentModel>();
            mockEnvironmentModel2.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel2.Setup(model => model.CanStudioExecute).Returns(true);
            mockEnvironmentModel2.Setup(model => model.ID).Returns(environment2Id);

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(connection => connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockEnvironmentConnection.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);

            var mockEnvironmentConnection2 = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection2.Setup(connection => connection.AppServerUri).Returns(new Uri("http://tests:3142"));
            mockEnvironmentConnection2.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            mockEnvironmentModel2.Setup(model => model.Connection).Returns(mockEnvironmentConnection2.Object);


            var mockResourceRepository = CreateMockResourceRepository(mockEnvironmentModel);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            var envRepo = new TestLoadEnvironmentRespository(mockEnvironmentModel.Object, mockEnvironmentModel2.Object);
            var selectedResource = mockResourceRepository.Object.All().ToList()[1];

            var mockExplorerResourceRepository = new Mock<IClientExplorerResourceRepository>();

            var repository = new StudioResourceRepository(GetTestData(), Guid.Empty, _Invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            ExplorerItemModel server2Item = new ExplorerItemModel { EnvironmentId = environment2Id, DisplayName = "Server 2", ResourceType = Common.Interfaces.Data.ResourceType.Server };

            ExplorerItemModel resourceItemServer2 = new ExplorerItemModel { EnvironmentId = environment2Id, DisplayName = "Resource Server 2", ResourceType = Common.Interfaces.Data.ResourceType.WorkflowService };
            server2Item.Children.Add(resourceItemServer2);
            repository.ExplorerItemModels.Add(server2Item);

            var dialog = new TestResourcePickerDialog(ActivityType, envRepo, mockEventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, IsFromActivityDrop, repository)
            {
                CreateDialogResult = dialogWindow.Object,
                SelectedResource = new Mock<IResourceModel>().Object
            };
            //------------Execute Test---------------------------

            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog(mockEnvironmentModel2.Object);

            //------------Assert Results-------------------------
            Assert.AreNotEqual(selectedResource, dialog.CreateDialogDataContext.SelectedResourceModel);
        }

        private IExplorerItem GetTestData(string workFlowId = "DF279411-F678-4FCC-BE88-A1B613EE51E3",
                                         string dbServiceId = "DF279411-F678-4FCC-BE88-A1B613EE51E3", Guid? folderID = null)
        {
            var workflow1 = new ServerExplorerItem
            {
                ResourceType = Common.Interfaces.Data.ResourceType.WorkflowService,
                DisplayName = "workflow1",
                ResourceId = string.IsNullOrEmpty(workFlowId) ? Guid.NewGuid() : Guid.Parse(workFlowId),
                Permissions = Permissions.Administrator
            };

            var dbService1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.DbService, DisplayName = "dbService1", ResourceId = string.IsNullOrEmpty(dbServiceId) ? Guid.NewGuid() : Guid.Parse(dbServiceId), Permissions = Permissions.Contribute };
            var webService1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.WebService, DisplayName = "webService1", ResourceId = Guid.NewGuid(), Permissions = Permissions.View };
            var pluginService1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.PluginService, DisplayName = "pluginService1", ResourceId = Guid.NewGuid(), Permissions = Permissions.View };
            var dbSource1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.DbSource, DisplayName = "dbSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var webSource1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.WebSource, DisplayName = "webSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var pluginSource1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.PluginSource, DisplayName = "pluginSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var emailSource1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.EmailSource, DisplayName = "emailSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var serverSource1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.ServerSource, DisplayName = "serverSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var folder1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.Folder, DisplayName = "folder1", ResourceId = folderID ?? Guid.NewGuid(), Permissions = Permissions.Administrator };
            var folder2 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.Folder, DisplayName = "folder2", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var subfolder1 = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.Folder, DisplayName = "subfolder1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var localhost = new ServerExplorerItem { ResourceType = Common.Interfaces.Data.ResourceType.Server, DisplayName = "localhost", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };

            dbService1.Parent = webService1.Parent = pluginService1.Parent = subfolder1.Parent = folder1;
            dbSource1.Parent = webSource1.Parent = pluginSource1.Parent = emailSource1.Parent = serverSource1.Parent = folder2;

            folder2.Children = new List<IExplorerItem>
                {
                    dbSource1,
                    webSource1,
                    pluginSource1,
                    emailSource1,
                    serverSource1
                };


            folder1.Children = new List<IExplorerItem>
                {
                    dbService1, 
                    webService1,
                    pluginService1, 
                    subfolder1
                };

            localhost.Children = new List<IExplorerItem> { folder1, workflow1 };
            workflow1.Parent = localhost;
            folder1.Parent = localhost;

            return localhost;
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourcePickerDialog_DetermineDropActivityType")]
        public void ResourcePickerDialog_DetermineDropActivityType_WhenWorkflowFuzzyMatch_ExpectWorkflow()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var typeOf = ResourcePickerDialog.DetermineDropActivityType("DsfWorkflowActivityFoobar");

            //------------Assert Results-------------------------
            Assert.AreEqual(enDsfActivityType.Workflow, typeOf);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourcePickerDialog_DetermineDropActivityType")]
        public void ResourcePickerDialog_DetermineDropActivityType_WhenServiceExactMatch_ExpectWorkflow()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var typeOf = ResourcePickerDialog.DetermineDropActivityType("DsfServiceActivity");

            //------------Assert Results-------------------------
            Assert.AreEqual(enDsfActivityType.Service, typeOf);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourcePickerDialog_DetermineDropActivityType")]
        public void ResourcePickerDialog_DetermineDropActivityType_WhenServiceFuzzyMatch_ExpectWorkflow()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var typeOf = ResourcePickerDialog.DetermineDropActivityType("DsfServiceActivityFoobar");

            //------------Assert Results-------------------------
            Assert.AreEqual(enDsfActivityType.Service, typeOf);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourcePickerDialog_DetermineDropActivityType")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_DetermineDropActivityType_WhenServiceNull_ExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            ResourcePickerDialog.DetermineDropActivityType(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourcePickerDialog_DetermineDropActivityType")]
        public void ResourcePickerDialog_DetermineDropActivityType_WhenServiceRubishName_ExpectAll()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var typeOf = ResourcePickerDialog.DetermineDropActivityType("Foobar");

            //------------Assert Results-------------------------
            Assert.AreEqual(enDsfActivityType.All, typeOf);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_ShowDialog")]
        public void ResourcePickerDialog_ShowDialog_DialogResultIsOkay_SelectedResourceIsNotNull()
        {
            //------------Setup for test--------------------------
            var picker = new TestResourcePickerDialogOkay();

            //------------Execute Test---------------------------
            var result = picker.ShowDialog();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            Assert.IsNotNull(picker.SelectedResource);
            Assert.AreSame(picker.CreateDialogDataContext.SelectedResourceModel, picker.SelectedResource);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_ShowDropDialog")]
        public void ResourcePickerDialog_ShowDropDialog_ActivityTypeIsAll_DoesNothing()
        {
            //------------Setup for test--------------------------
            ResourcePickerDialog dialog = null;
            DsfActivityDropViewModel dropViewModel;

            //------------Execute Test---------------------------
            var result = ResourcePickerDialog.ShowDropDialog(ref dialog, "xxxx", out dropViewModel);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            Assert.IsNull(dropViewModel);
            Assert.IsNull(dialog);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_ShowDropDialog")]
        public void ResourcePickerDialog_ShowDropDialog_ActivityTypeIsNotAllAndPickerIsNull_CreatesPickerAndInvokesShowDialog()
        {
            //------------Setup for test--------------------------
            TestResourcePickerDialog dialog = null;
            DsfActivityDropViewModel dropViewModel;

            //------------Execute Test---------------------------
            var result = ResourcePickerDialog.ShowDropDialog(ref dialog, GlobalConstants.ResourcePickerServiceString, out dropViewModel);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            Assert.IsNotNull(dropViewModel);
            Assert.IsNotNull(dialog);
        }

        static Mock<IResourceRepository> CreateMockResourceRepository(Mock<IEnvironmentModel> mockEnvironmentModel)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(r => r.Category).Returns("Testing");
            mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");
            mockResourceModel.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            var mockResourceModel1 = new Mock<IContextualResourceModel>();
            mockResourceModel1.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel1.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock1");
            mockResourceModel1.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);


            var mockResourceModel2 = new Mock<IContextualResourceModel>();
            mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");
            mockResourceModel2.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            Collection<IResourceModel> mockResources = new Collection<IResourceModel>
            {
                mockResourceModel.Object,
                mockResourceModel1.Object,
                mockResourceModel2.Object
            };

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(r => r.All()).Returns(mockResources);
            return mockResourceRepository;
        }

        static void SetupMef()
        {
        }

    }
}
