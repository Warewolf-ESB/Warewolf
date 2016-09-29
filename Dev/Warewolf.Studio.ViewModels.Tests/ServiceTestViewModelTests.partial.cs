using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Dev2;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    partial class ServiceTestViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_DebugItemDesicion_ShouldHaveAddServiceTestStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(contextualResourceModel.ID);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDecision());
            mockWorkflowDesignerViewModel.Setup(model => model.GetModelItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(modelItem);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModeInput = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[0], };
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModeInput
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2

            };
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_DebugIDesicion_ShouldHaveAddServiceTestStepShouldHaveArmOptions()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            StringAssert.Contains("DsfMultiAssignActivity", testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenTestStepNotContainsStep_ShouldAddStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>();
            var serviceTestStep = new Mock<IServiceTestStep>();
            serviceTestStep.SetupGet(step => step.UniqueId).Returns(resourceId);
            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, testSteps, serviceTestStep.Object });
            //---------------Test Result -----------------------
            var contains = testSteps.Contains(serviceTestStep.Object);
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenTestStepContainsStep_ShouldNotAddStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };
          

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            var serviceTestStep = new Mock<IServiceTestStep>();
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>() {serviceTestStep.Object};
            
            serviceTestStep.SetupGet(step => step.UniqueId).Returns(resourceId);
            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            Assert.AreEqual(1, testSteps.Count);
            var contains = testSteps.Contains(serviceTestStep.Object);
            Assert.IsTrue(contains);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, testSteps, serviceTestStep.Object });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testSteps.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenDecision_ShouldNotAddStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };
          

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>() {};
            
            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, testSteps, default(IServiceTestStep) });
            //---------------Test Result -----------------------
            Assert.AreEqual(0, testSteps.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenMultiAssign_ShouldAddStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };
          

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>() {};
            
            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[2], itemViewModel1, testSteps, default(IServiceTestStep) });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testSteps.Count);
            Assert.AreEqual(1, testSteps[0].ActivityType);
            Assert.AreEqual(1, testSteps[0].StepDescription);
            Assert.AreEqual(1, testSteps[0].StepOutputs.Count);
            Assert.AreEqual(1, testSteps[0].UniqueId);
            Assert.AreEqual(StepType.Assert, testSteps[0].Type);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_GivenWrongMessage_ShouldAddTests()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates.First() };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var synchronousAsyncWorker = new SynchronousAsyncWorker();
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), synchronousAsyncWorker, new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_GivenInCorrectMessage_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new ExecuteResourceMessage(mockResourceModel.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var synchronousAsyncWorker = new SynchronousAsyncWorker();
            // ReSharper disable once ObjectCreationAsStatement
            new ServiceTestViewModel(CreateResourceModel(), synchronousAsyncWorker, new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var exceptions = synchronousAsyncWorker.Exceptions.Count;
            Assert.AreEqual(1, exceptions);

            //---------------Test Result -----------------------
        }


    }
}
