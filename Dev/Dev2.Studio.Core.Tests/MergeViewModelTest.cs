using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
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
        [TestInitialize]
        public void Init()
        {
            var newServerRepo = new Mock<IServerRepository>();
            CustomContainer.Register(newServerRepo.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkSurfaceContextViewModel_Constructor")]
        public void WorkSurfaceContextViewModel_Constructor_ValidArguments_DebugOutputViewModelNotNull()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(environmentModel);
            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_GetView")]
        public void MergeViewModel_GetView_ReturnsIView_NotNull()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(environmentModel);

            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
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
        public void MergeViewModel_DoDeactivate_CanSave_ExpectedFalse()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(environmentModel);

            //------------Execute Test---------------------------
            var mvm = new Mock<IShellViewModel>();
            mvm.Setup(model => model.HelpViewModel.UpdateHelpText(It.IsAny<string>()));
            CustomContainer.Register(mvm.Object);
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            vm.HelpText = string.Empty;

            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(resourceModel.Object, resourceModel.Object, false);

            vm.ViewModel = mergeWorkflowViewModel;

            var expectedValue = vm.DoDeactivate(false);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.AreEqual(string.Empty, vm.HelpText);
            Assert.IsTrue(expectedValue);
            mvm.Verify(model => model.HelpViewModel.UpdateHelpText(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_OnDispose")]
        public void MergeViewModel_OnDispose_ViewModel_Dispose()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(environmentModel);

            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(eventAggregator.Object, viewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
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
        public void MergeViewModel_PropertyChanged_ViewModel_IsTrue()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(environmentModel);

            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var mockMainViewModel = new Mock<IShellViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(new Mock<IHelpWindowViewModel>().Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);

            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(resourceModel.Object, resourceModel.Object, false);

            var vm = new MergeViewModel(eventAggregator.Object, mergeWorkflowViewModel, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.AreEqual("Merge *", vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsTrue(vm.IsDirty);

            vm.ViewModel = mergeWorkflowViewModel;

            bool wasCalled = false;
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DisplayName")
                {
                    wasCalled = true;
                }
            };

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_DoDeactivate")]
        public void MergeViewModel_DoDeactivate_CanSave_ExpectedTrue()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(environmentModel);

            //------------Execute Test---------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(string.Format(StringResources.ItemSource_NotSaved),
                        "Save Merge ?",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Information, "", false, false, true, false, false, false)).Verifiable();

            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(eventAggregator.Object, viewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(resourceModel.Object, resourceModel.Object, false);

            vm.ViewModel = mergeWorkflowViewModel;

            var expectedValue = vm.DoDeactivate(true);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(expectedValue);
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MergeViewModel_DoDeactivate")]
        public void MergeViewModel_DoDeactivate_CanSave_MessageBoxYes()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(environmentModel);

            //------------Execute Test---------------------------

            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false)).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);

            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(eventAggregator.Object, viewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(resourceModel.Object, resourceModel.Object, false);

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
        public void MergeViewModel_DoDeactivate_CanSave_MessageBoxNo()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.Server).Returns(environmentModel);

            //------------Execute Test---------------------------

            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false)).Returns(MessageBoxResult.No);
            CustomContainer.Register(popupController.Object);

            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var viewModel = new Mock<IMergeWorkflowViewModel>();
            viewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new MergeViewModel(eventAggregator.Object, viewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("MergeConflicts", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(resourceModel.Object, resourceModel.Object, false);

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
