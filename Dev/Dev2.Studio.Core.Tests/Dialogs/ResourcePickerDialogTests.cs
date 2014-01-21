using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Dialogs;
using Dev2.Providers.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Dialogs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResourcePickerDialogTests
    {
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
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, null, null, null, false);

            //------------Assert Results-------------------------

        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_ConstructorWith2Args_NullEnvironmentRepository_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_NullEventAggregator_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, new Mock<IEnvironmentRepository>().Object, null, null, false);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_NullAsyncWorker_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, new Mock<IEnvironmentRepository>().Object, new Mock<IEventAggregator>().Object, null, false);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourcePickerDialog_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourcePickerDialog_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var dialog = new ResourcePickerDialog(enDsfActivityType.All, null);

            //------------Assert Results-------------------------
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
            var dialog = new TestResourcePickerDialog(enDsfActivityType.All, new Mock<IEnvironmentModel>().Object) { CreateDialogResult = dialogWindow.Object };
            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreNotSame(EnvironmentRepository.Instance, dialog.CreateDialogDataContext.ExplorerViewModel.EnvironmentRepository);
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

            var envRepo = new TestLoadEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            //var envRepo = EnvironmentRepository.Create(new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            var dialog = new TestResourcePickerDialog(ActivityType, envRepo, new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, IsFromActivityDrop)
            {
                CreateDialogResult = dialogWindow.Object
            };
            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreEqual(ActivityType, dialog.CreateDialogDataContext.ActivityType);
            Assert.AreEqual(IsFromActivityDrop, dialog.CreateDialogDataContext.ExplorerViewModel.NavigationViewModel.IsFromActivityDrop);
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
            var dialog = new TestResourcePickerDialog(ActivityType, envRepo, mockEventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, IsFromActivityDrop)
            {
                CreateDialogResult = dialogWindow.Object,
                SelectedResource = selectedResource
            };
            //------------Execute Test---------------------------

            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreEqual(selectedResource, dialog.CreateDialogDataContext.SelectedResourceModel);
            var selectedTreeNode = dialog.CreateDialogDataContext.ExplorerViewModel.NavigationViewModel.Root.FindChild(selectedResource as IContextualResourceModel);
            Assert.IsNotNull(selectedTreeNode);
            Assert.IsTrue(selectedTreeNode.IsSelected);
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
            var dialog = new TestResourcePickerDialog(ActivityType, envRepo, mockEventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, IsFromActivityDrop)
            {
                CreateDialogResult = dialogWindow.Object,
                SelectedResource = new Mock<IResourceModel>().Object
            };
            //------------Execute Test---------------------------

            // Need to invoke ShowDialog in order to get the DsfActivityDropViewModel
            dialog.ShowDialog();

            //------------Assert Results-------------------------
            Assert.AreNotEqual(selectedResource, dialog.CreateDialogDataContext.SelectedResourceModel);
            var selectedTreeNode = dialog.CreateDialogDataContext.ExplorerViewModel.NavigationViewModel.Root.FindChild(selectedResource as IContextualResourceModel);
            Assert.IsNotNull(selectedTreeNode);
            Assert.IsFalse(selectedTreeNode.IsSelected);
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
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());
        }

    }
}
