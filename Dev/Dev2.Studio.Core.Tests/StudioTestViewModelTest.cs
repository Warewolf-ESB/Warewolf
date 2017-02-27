using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Threading;
using Dev2.ViewModels;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class StudioTestViewModelTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_Constructor")]
        public void WorkSurfaceContextViewModel_Constructor_ValidArguments_DebugOutputViewModelNotNull()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, new Mock<IPopupController>().Object, null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_GetView")]
        public void StudioTestViewModel_GetView_ReturnsIView_NotNull()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var view = vm.GetView();

            //------------Assert Results-------------------------
            Assert.IsNotNull(view);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_DebugOutputMessage")]
        public void StudioTestViewModel_DebugOutputMessage_Handle_NotNull()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------
            var mvm = new Mock<IMainViewModel>();
            mvm.Setup(model => model.HelpViewModel.UpdateHelpText(It.IsAny<string>()));
            CustomContainer.Register(mvm.Object);
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            vm.HelpText = string.Empty;

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;

            var debugForTest = new List<IDebugState>();
            testViewModel.SelectedServiceTest.DebugForTest = debugForTest;

            //------------Assert Results-------------------------
            vm.Handle(new DebugOutputMessage(testViewModel.SelectedServiceTest?.DebugForTest));

            Assert.AreEqual(DebugStatus.Ready, vm.DebugOutputViewModel.DebugStatus);
            mvm.Verify(model => model.HelpViewModel.UpdateHelpText(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_DoDeactivate")]
        public void StudioTestViewModel_DoDeactivate_CanSave_ExpectedFalse()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------
            var mvm = new Mock<IMainViewModel>();
            mvm.Setup(model => model.HelpViewModel.UpdateHelpText(It.IsAny<string>()));
            CustomContainer.Register(mvm.Object);
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            vm.HelpText = string.Empty;

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;

            testViewModel.SelectedServiceTest.TestName = "New Test Name";

            vm.ViewModel = testViewModel;

            var expectedValue = vm.DoDeactivate(false);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.AreEqual(string.Empty, vm.HelpText);
            Assert.IsTrue(expectedValue);
            mvm.Verify(model => model.HelpViewModel.UpdateHelpText(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_OnDispose")]
        public void StudioTestViewModel_OnDispose_ViewModel_Dispose()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            vm.Dispose();

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_PropertyChanged")]
        public void StudioTestViewModel_PropertyChanged_ViewModel_IsTrue()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var mockMainViewModel = new Mock<IMainViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(new Mock<IHelpWindowViewModel>().Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;

            var vm = new StudioTestViewModel(eventAggregator.Object, testViewModel, new Mock<IPopupController>().Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.AreEqual(" - Tests -  *", vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsTrue(vm.IsDirty);

            testViewModel.SelectedServiceTest.TestName = "New Test Name";
            vm.ViewModel = testViewModel;

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
            vm.ViewModel.RefreshCommands();
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_DoDeactivate")]
        public void StudioTestViewModel_DoDeactivate_CanSave_ExpectedTrue()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(string.Format(StringResources.ItemSource_NotSaved),
                        "Save  - Tests -  ?",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Information, "", false, false, true, false, false, false)).Verifiable();

            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;

            testViewModel.SelectedServiceTest.TestName = "New Test Name";

            vm.ViewModel = testViewModel;

            var expectedValue = vm.DoDeactivate(true);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(expectedValue);
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_DoDeactivate")]
        public void StudioTestViewModel_DoDeactivate_CanSave_MessageBoxYes()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------

            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false)).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;

            testViewModel.SelectedServiceTest.TestName = "New Test Name";

            vm.ViewModel = testViewModel;

            vm.DoDeactivate(true);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.IsNull(vm.HelpText);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_DoDeactivate")]
        public void StudioTestViewModel_DoDeactivate_CanSave_MessageBoxYesHasDuplicates()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------

            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false)).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;

            var serviceTestModel = new ServiceTestModel();
            serviceTestModel.TestName = "Test 1";
            testViewModel.Tests.Add(serviceTestModel);

            vm.ViewModel = testViewModel;

            var expectedValue = vm.DoDeactivate(true);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(expectedValue);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioTestViewModel_DoDeactivate")]
        public void StudioTestViewModel_DoDeactivate_CanSave_MessageBoxNo()
        {
            //------------Setup for test--------------------------
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(e => e.Name).Returns("My Env");
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);

            //------------Execute Test---------------------------

            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Information, "", false, false, true, false, false, false)).Returns(MessageBoxResult.No);
            CustomContainer.Register(popupController.Object);
            
            var eventAggregator = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel.Object);
            var serviceTestViewModel = new Mock<IServiceTestViewModel>();
            serviceTestViewModel.Setup(model => model.WorkflowDesignerViewModel).Returns(mockWorkSurfaceViewModel.Object);
            var vm = new StudioTestViewModel(eventAggregator.Object, serviceTestViewModel.Object, popupController.Object, new Mock<IView>().Object);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.DebugOutputViewModel);
            Assert.IsFalse(vm.HasVariables);
            Assert.IsTrue(vm.HasDebugOutput);
            Assert.IsNull(vm.DisplayName);
            Assert.AreEqual("ServiceTestsViewer", vm.ResourceType);
            Assert.IsNull(vm.HelpText);
            Assert.IsFalse(vm.IsDirty);

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            resourceModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;

            testViewModel.SelectedServiceTest.TestName = "New Test Name";

            vm.ViewModel = testViewModel;

            var expectedValue = vm.DoDeactivate(true);

            //------------Assert Results-------------------------

            Assert.IsTrue(vm.IsDirty);
            Assert.IsNull(vm.HelpText);
            Assert.IsTrue(expectedValue);
        }
    }
}
