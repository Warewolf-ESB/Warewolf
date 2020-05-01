#pragma warning disable
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.ViewModels;
using Dev2.ViewModels.Merge;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class MergeViewModelTest
    {
        private const string ResourceName = "MergeResource";
        private const string DisplayName = "Merge";
        private const string ServiceDefinition = "<x/>";
        private static Mock<IServer> _environmentModel;
        private static readonly Guid ServerId = Guid.NewGuid();
        private static readonly Guid FirstResourceId = Guid.NewGuid();
        private static Mock<IContextualResourceModel> _firstResource;
        private static Mock<IResourceRepository> _resourceRepo;
        private static IServerRepository _serverRepo;
        private static Mock<IPopupController> _popupController;
        private static Mock<IShellViewModel> _shellViewModel;
        private static Mock<IEventAggregator> _eventAggregator;
        private static Mock<IServiceDifferenceParser> _mockParseServiceForDifferences;

        private static Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();

            result.Setup(c => c.ResourceName).Returns(ResourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(DisplayName);
            result.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(ServiceDefinition));
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(_environmentModel.Object);
            result.Setup(c => c.ServerID).Returns(ServerId);
            result.Setup(c => c.ID).Returns(FirstResourceId);

            return result;
        }

        static void CreateEnvironmentModel()
        {
            _environmentModel = CreateMockEnvironment();
            _resourceRepo = new Mock<IResourceRepository>();

            _firstResource = CreateResource(ResourceType.WorkflowService);
            var coll = new Collection<IResourceModel> { _firstResource.Object };
            _resourceRepo.Setup(c => c.All()).Returns(coll);


            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);
        }

        private static IServerRepository GetEnvironmentRepository()
        {
            var models = new List<IServer> { _environmentModel.Object };
            var mock = new Mock<IServerRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.IsLoaded).Returns(true);
            mock.Setup(repository => repository.Source).Returns(_environmentModel.Object);
            mock.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(_environmentModel.Object);
            _serverRepo = mock.Object;
            return _serverRepo;
        }

        private static Mock<IServer> CreateMockEnvironment(params string[] sources)
        {
            var rand = new Random();
            var connection = CreateMockConnection(rand, sources);

            var env = new Mock<IServer>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.EnvironmentID).Returns(Guid.NewGuid());

            env.Setup(e => e.Name).Returns($"Server_{rand.Next(1, 100)}");

            return env;
        }

        private static Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(new Guid());
            connection.Setup(c => c.AppServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}/dsf"));
            connection.Setup(c => c.WebServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}"));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder($"<XmlData>{string.Join("\n", sources)}</XmlData>"));
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            return connection;
        }

        [TestInitialize]
        public void Init()
        {
            var newServerRepo = new Mock<IServerRepository>();

            CreateEnvironmentModel();
            _serverRepo = GetEnvironmentRepository();
            _popupController = new Mock<IPopupController>();
            _eventAggregator = new Mock<IEventAggregator>();

            _mockParseServiceForDifferences = new Mock<IServiceDifferenceParser>();

            _shellViewModel = new Mock<IShellViewModel>();
            _shellViewModel.Setup(model => model.HelpViewModel.UpdateHelpText(It.IsAny<string>()));

            CustomContainer.Register(newServerRepo.Object);
            CustomContainer.Register(_shellViewModel.Object);
            CustomContainer.Register(_popupController.Object);
            CustomContainer.Register(_mockParseServiceForDifferences.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_GetView")]
        public void MergeViewModel_GetView_ReturnsIView_NotNull()
        {
            //------------Setup for test--------------------------
            _firstResource = CreateResource(ResourceType.WorkflowService);

            var mockWorkSurfaceViewModel = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(_environmentModel.Object);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(_firstResource.Object);

            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();

            _mockParseServiceForDifferences.Setup(parser =>
                parser.GetDifferences(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(),
                    false)).Returns((currentTree, diffTree));

            //------------Execute Test---------------------------

            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.MergePreviewWorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(_eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsFalse(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var view = vm.GetView();

            //------------Assert Results-------------------------
            Assert.IsNotNull(view);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_DoDeactivate")]
        [DoNotParallelize]
        public void MergeViewModel_DoDeactivate_CanSave_ExpectedFalse()
        {
            //------------Setup for test--------------------------
            _firstResource = CreateResource(ResourceType.WorkflowService);

            var mockWorkSurfaceViewModel = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(_environmentModel.Object);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(_firstResource.Object);

            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();

            _mockParseServiceForDifferences.Setup(parser =>
                parser.GetDifferences(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(),
                    false)).Returns((currentTree, diffTree));

            //------------Execute Test---------------------------

            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.MergePreviewWorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(_eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsFalse(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            vm.HelpText = string.Empty;

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(_firstResource.Object, _firstResource.Object, false);

            vm.ViewModel = mergeWorkflowViewModel;

            var expectedValue = vm.DoDeactivate(false);

            //------------Assert Results-------------------------

            Assert.IsFalse(vm.ViewModel.IsDirty);
            Assert.IsTrue(vm.IsDirty);
            Assert.AreEqual(string.Empty, vm.HelpText);
            Assert.IsTrue(expectedValue);
            _shellViewModel.Verify(model => model.HelpViewModel.UpdateHelpText(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_OnDispose")]
        public void MergeViewModel_OnDispose_ViewModel_Dispose()
        {
            //------------Setup for test--------------------------
            _firstResource = CreateResource(ResourceType.WorkflowService);

            var mockWorkSurfaceViewModel = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(_environmentModel.Object);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(_firstResource.Object);

            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();

            _mockParseServiceForDifferences.Setup(parser =>
                parser.GetDifferences(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(),
                    false)).Returns((currentTree, diffTree));

            //------------Execute Test---------------------------

            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.MergePreviewWorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(_eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsFalse(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            vm.Dispose();

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_PropertyChanged")]
        [DoNotParallelize]
        public void MergeViewModel_PropertyChanged_DisplayName_IsTrue()
        {
            //------------Setup for test--------------------------
            _firstResource = CreateResource(ResourceType.WorkflowService);

            var mockWorkSurfaceViewModel = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(_environmentModel.Object);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(_firstResource.Object);

            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();

            _mockParseServiceForDifferences.Setup(parser =>
                parser.GetDifferences(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(),
                    false)).Returns((currentTree, diffTree));

            //------------Execute Test---------------------------

            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.MergePreviewWorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(_eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(_firstResource.Object, _firstResource.Object, false);
            mergeWorkflowViewModel.HasMergeStarted = true;

            vm.ViewModel = mergeWorkflowViewModel;

            bool wasCalled = false;
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DisplayName")
                {
                    wasCalled = true;
                }
            };

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsFalse(vm.HasDebugOutput);
            Assert.AreEqual(mergeWorkflowViewModel.DisplayName, vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsTrue(vm.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_PropertyChanged")]
        [DoNotParallelize]
        public void MergeViewModel_PropertyChanged_DataListViewModel_IsTrue()
        {
            //------------Setup for test--------------------------
            _firstResource = CreateResource(ResourceType.WorkflowService);

            var mockWorkSurfaceViewModel = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(_environmentModel.Object);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(_firstResource.Object);

            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();

            _mockParseServiceForDifferences.Setup(parser =>
                parser.GetDifferences(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(),
                    false)).Returns((currentTree, diffTree));

            //------------Execute Test---------------------------

            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.MergePreviewWorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(_eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(_firstResource.Object, _firstResource.Object, false);
            mergeWorkflowViewModel.HasMergeStarted = true;

            vm.ViewModel = mergeWorkflowViewModel;

            bool wasCalled = false;
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DataListViewModel")
                {
                    wasCalled = true;
                }
            };

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsFalse(vm.HasDebugOutput);
            Assert.AreEqual(mergeWorkflowViewModel.DisplayName, vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsTrue(vm.IsDirty);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_DoDeactivate")]
        [DoNotParallelize]
        public void MergeViewModel_DoDeactivate_CanSave_ExpectedTrue()
        {
            //------------Setup for test--------------------------
            _firstResource = CreateResource(ResourceType.WorkflowService);

            var mockWorkSurfaceViewModel = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(_environmentModel.Object);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(_firstResource.Object);

            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();

            _mockParseServiceForDifferences.Setup(parser =>
                parser.GetDifferences(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(),
                    false)).Returns((currentTree, diffTree));

            //------------Execute Test---------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(string.Format(StringResources.ItemSource_NotSaved),
                        "Save Merge ?",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Information, "", false, false, true, false, false, false)).Verifiable();

            var mockConflictRow = new Mock<IToolConflictRow>();
            mockConflictRow.Setup(cr => cr.HasConflict).Returns(true);
            var conflicts = new List<IToolConflictRow>();
            conflicts.Add(mockConflictRow.Object);

            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.MergePreviewWorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            viewModel.Setup(model => model.Conflicts).Returns(conflicts);
            var vm = new MergeViewModel(_eventAggregator.Object, viewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsFalse(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(_firstResource.Object, _firstResource.Object, false);
            mergeWorkflowViewModel.Conflicts = conflicts;
            mergeWorkflowViewModel.HasMergeStarted = true;
            
            vm.ViewModel = mergeWorkflowViewModel;

            var expectedValue = vm.DoDeactivate(true);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.IsTrue(vm.ViewModel.IsDirty);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(expectedValue);
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_DoDeactivate")]
        [DoNotParallelize]
        public void MergeViewModel_DoDeactivate_CanSave_MessageBoxYes()
        {
            //------------Setup for test--------------------------
            _firstResource = CreateResource(ResourceType.WorkflowService);

            var mockWorkSurfaceViewModel = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(_environmentModel.Object);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(_firstResource.Object);

            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();

            _mockParseServiceForDifferences.Setup(parser =>
                parser.GetDifferences(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(),
                    false)).Returns((currentTree, diffTree));

            //------------Execute Test---------------------------

            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false)).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);

            var mockConflictRow = new Mock<IToolConflictRow>();
            mockConflictRow.Setup(cr => cr.HasConflict).Returns(true);
            var conflicts = new List<IToolConflictRow>();
            conflicts.Add(mockConflictRow.Object);

            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.MergePreviewWorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(_eventAggregator.Object, viewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsFalse(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(_firstResource.Object, _firstResource.Object, false);
            mergeWorkflowViewModel.Conflicts = conflicts;
            mergeWorkflowViewModel.HasMergeStarted = true;
            mergeWorkflowViewModel.DisplayName = "Merge";

            vm.ViewModel = mergeWorkflowViewModel;

            vm.DoDeactivate(true);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.IsNull(vm.HelpText);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_DoDeactivate")]
        [DoNotParallelize]
        public void MergeViewModel_DoDeactivate_CanSave_MessageBoxNo()
        {
            //------------Setup for test--------------------------
            _firstResource = CreateResource(ResourceType.WorkflowService);

            var mockWorkSurfaceViewModel = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(_environmentModel.Object);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(_firstResource.Object);

            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();

            _mockParseServiceForDifferences.Setup(parser =>
                parser.GetDifferences(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(),
                    false)).Returns((currentTree, diffTree));

            //------------Execute Test---------------------------

            _popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false)).Returns(MessageBoxResult.No);
            CustomContainer.Register(_popupController.Object);

            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.MergePreviewWorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(_eventAggregator.Object, viewModel.Object, _popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsFalse(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(_firstResource.Object, _firstResource.Object, false);
            mergeWorkflowViewModel.HasMergeStarted = true;
            mergeWorkflowViewModel.DisplayName = "Merge";

            vm.ViewModel = mergeWorkflowViewModel;

            var expectedValue = vm.DoDeactivate(true);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.IsNull(vm.HelpText);
            Assert.IsTrue(expectedValue);
        }
    }
}
