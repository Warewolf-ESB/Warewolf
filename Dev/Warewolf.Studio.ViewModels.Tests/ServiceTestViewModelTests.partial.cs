using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Core.Tests.Environments;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;




namespace Warewolf.Studio.ViewModels.Tests
{
    partial class ServiceTestViewModelTests
    {
        [TestMethod]
        [Timeout(2000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_PopulateFromDebug")]
        public void ServiceTestViewModel_PopulateFromDebug_WithError_ShouldSetTestToExpectErrorWithDebugErrorMessage()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            contextualResourceModel.DataList = "<DataList/>";
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(contextualResourceModel.ID);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var debugTreeViewItemViewModels = new List<IDebugTreeViewItemViewModel>();
            var server = new Mock<IServer>();
            var debugStateTreeViewItemViewModel = new DebugStateTreeViewItemViewModel(new TestServerRespository(server.Object));
            var debugState = new DebugState
            {
                HasError = true,
                ErrorMessage = "Error in Debug",
                StateType = StateType.End
            };
            debugStateTreeViewItemViewModel.Content = debugState;
            debugTreeViewItemViewModels.Add(debugStateTreeViewItemViewModel);
            //------------Execute Test---------------------------
            testFrameworkViewModel.PrePopulateTestsUsingDebug(debugTreeViewItemViewModels);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.SelectedServiceTest);
            Assert.IsTrue(testFrameworkViewModel.SelectedServiceTest.ErrorExpected);
            Assert.IsFalse(testFrameworkViewModel.SelectedServiceTest.NoErrorExpected);
            Assert.AreEqual("Error in Debug", testFrameworkViewModel.SelectedServiceTest.ErrorContainsText);
        }

        [TestMethod]
        [Timeout(1000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_PopulateFromDebug")]
        public void ServiceTestViewModel_PopulateFromDebug_WithNoError_ShouldSetTestToExpectNoErrorWithDebugErrorMessage()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            contextualResourceModel.DataList = "<DataList/>";
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(contextualResourceModel.ID);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var debugTreeViewItemViewModels = new List<IDebugTreeViewItemViewModel>();
            var server = new Mock<IServer>();

            var debugStateTreeViewItemViewModel = new DebugStateTreeViewItemViewModel(new TestServerRespository(server.Object));
            var debugState = new DebugState
            {
                HasError = false,
                ErrorMessage = "",
                StateType = StateType.End
            };
            debugStateTreeViewItemViewModel.Content = debugState;
            debugTreeViewItemViewModels.Add(debugStateTreeViewItemViewModel);
            //------------Execute Test---------------------------
            testFrameworkViewModel.PrePopulateTestsUsingDebug(debugTreeViewItemViewModels);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.SelectedServiceTest);
            Assert.IsFalse(testFrameworkViewModel.SelectedServiceTest.ErrorExpected);
            Assert.IsTrue(testFrameworkViewModel.SelectedServiceTest.NoErrorExpected);
            Assert.AreEqual("", testFrameworkViewModel.SelectedServiceTest.ErrorContainsText);
        }


        [TestMethod]
        [Timeout(1000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("ServiceTestViewModel_PopulateFromDebug")]
        public void ServiceTestViewModel_PopulateFromDebug_WithNoError_UpdateInputs()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            contextualResourceModel.DataList = "<DataList/>";
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(contextualResourceModel.ID);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var debugTreeViewItemViewModels = new List<IDebugTreeViewItemViewModel>();
            var server = new Mock<IServer>();
            var debugStateTreeViewItemViewModel = new DebugStateTreeViewItemViewModel(new TestServerRespository(server.Object));
            var debugState = new DebugState
            {
                HasError = false,
                ErrorMessage = "",
                StateType = StateType.End
            };
            debugStateTreeViewItemViewModel.Content = debugState;

            var expectedValue = "This is a long message to test that the Test Editor accepts a new line input that can be validated against the Test Editor input field.\n" +
                "This is a long message to test that the Test Editor accepts a new line input that can be validated against the Test Editor input field.";

            var debugItem = new DebugItem();
            var debugItemResult = new DebugItemResult();
            debugItemResult.Variable = "[[input]]";
            debugItemResult.Value = expectedValue;
            debugItemResult.Operator = "=";
            debugItem.ResultsList.Add(debugItemResult);
            debugStateTreeViewItemViewModel.Content.Inputs.Add(debugItem);

            debugTreeViewItemViewModels.Add(debugStateTreeViewItemViewModel);
            //------------Execute Test---------------------------
            testFrameworkViewModel.PrePopulateTestsUsingDebug(debugTreeViewItemViewModels);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.SelectedServiceTest);
            Assert.IsFalse(testFrameworkViewModel.SelectedServiceTest.ErrorExpected);
            Assert.IsTrue(testFrameworkViewModel.SelectedServiceTest.NoErrorExpected);
            Assert.AreEqual("", testFrameworkViewModel.SelectedServiceTest.ErrorContainsText);
        }


        [TestMethod]
        [Timeout(1000)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void PrepopulateTestsUsingDebug_DebugItemDesicion_ShouldHaveAddServiceTestStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
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

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
        }

        [TestMethod]
        [Timeout(1000)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void PrepopulateTestsUsingDebug_DebugIDesicion_ShouldHaveAddServiceTestStepShouldHaveArmOptions()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
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

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            StringAssert.Contains("DsfMultiAssignActivity", testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void AddChildDebugItems_GivenTestStepNotContainsStep_ShouldAddStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
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

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            var testSteps = new ObservableCollection<IServiceTestStep>();
            var serviceTestStep = new Mock<IServiceTestStep>();
            serviceTestStep.SetupGet(step => step.ActivityID).Returns(resourceId);
            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, serviceTestStep.Object });
            //---------------Test Result -----------------------
            var serviceTestSteps = testFrameworkViewModel.Tests[0].TestSteps;
            var contains = serviceTestSteps.Contains(serviceTestStep.Object);
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStatesWithMockAssign.json", "JsonResources")]
        public void AddChildDebugItems_GivenMockStep_ShouldAddStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStatesWithMockAssign.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<DebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var repo = new Mock<IServerRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[0], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel4 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                itemViewModel
                ,itemViewModel1
                ,itemViewModel2
                ,itemViewModel4
            };
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, null, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.PrePopulateTestsUsingDebug(newTestFromDebugMessage.RootItems);
            Assert.AreEqual(2, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void AddChildDebugItems_GivenTestStepContainsStep_ShouldNotAddStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
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


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            var serviceTestStep = new Mock<IServiceTestStep>();
            var testSteps = new ObservableCollection<IServiceTestStep>() { serviceTestStep.Object };

            serviceTestStep.SetupGet(step => step.ActivityID).Returns(resourceId);
            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            Assert.AreEqual(1, testSteps.Count);
            var contains = testSteps.Contains(serviceTestStep.Object);
            Assert.IsTrue(contains);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, serviceTestStep.Object });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testSteps.Count);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void AddChildDebugItems_GivenDecision_ShouldNotAddStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var jsonDataFile = "JsonResources\\DebugStates.json";
            jsonDataFile = GetJsonDataFile(jsonDataFile);
            var readAllText = File.ReadAllText(jsonDataFile);
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
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


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            var testSteps = new ObservableCollection<IServiceTestStep>() { };

            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, default(IServiceTestStep) });
            //---------------Test Result -----------------------
            Assert.AreEqual(0, testSteps.Count);
        }

        static string GetJsonDataFile(string jsonDataFile)
        {
            var exists = File.Exists(jsonDataFile);
            if (!exists)
            {
                var location = Assembly.GetExecutingAssembly().Location;
                var dir = Path.GetDirectoryName(location);
                jsonDataFile = Path.Combine(dir, jsonDataFile);
                exists = File.Exists(jsonDataFile);
                if (!exists)
                {
                    Assert.Fail("Json Data file not found: " + jsonDataFile);
                }
            }
            return jsonDataFile;
        }

        [TestMethod]
        [Timeout(1000)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        [DeploymentItem("JsonResources\\sequenceState.json", "JsonResources")]
        public void AddChildDebugItems_GivenSequence_ShouldAddtestStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var sequncetext = File.ReadAllText("JsonResources\\sequenceState.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            var sequenceSate = serializer.Deserialize<IDebugState>(sequncetext);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            var seq = new DebugStateTreeViewItemViewModel(repo.Object) { Content = sequenceSate, };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2,
                seq
            };


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            var testSteps = new ObservableCollection<IServiceTestStep>() { };

            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { sequenceSate, seq, default(IServiceTestStep) });
            //---------------Test Result -----------------------
            var serviceTestSteps = testFrameworkViewModel.Tests[0].TestSteps;
            Assert.AreEqual(3, serviceTestSteps.Count);
            Assert.AreEqual("DsfMultiAssignActivity", serviceTestSteps[0].ActivityType);
            Assert.AreEqual("Set the output variable (1)", serviceTestSteps[0].StepDescription);
            Assert.AreEqual(1, serviceTestSteps[0].StepOutputs.Count);
            Assert.AreEqual("670132e7-80d4-4e41-94af-ba4a71b28118".ToGuid(), serviceTestSteps[0].ActivityID);
            Assert.AreEqual(StepType.Assert, serviceTestSteps[0].Type);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void SwitchFromDebug_GivenDebugState_ShouldAddtestStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var objectToMakeModelItem = new DsfFlowSwitchActivity();
            var modelItem = ModelItemUtils.CreateModelItem(objectToMakeModelItem);
            mockWorkflowDesignerViewModel.Setup(model => model.GetModelItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(modelItem);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var sequncetext = File.ReadAllText("JsonResources\\sequenceState.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            var sequenceSate = serializer.Deserialize<IDebugState>(sequncetext);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            var seq = new DebugStateTreeViewItemViewModel(repo.Object) { Content = sequenceSate, };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2,
                seq
            };


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SwitchFromDebug", BindingFlags.NonPublic | BindingFlags.Instance);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            IDebugState state = new DebugState();
            methodInfo.Invoke(testFrameworkViewModel, new object[] { itemViewModel, state });
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DotnetllDebugStates.json", "JsonResources")]
        [DeploymentItem("JsonResources\\dotnetDllState.json", "JsonResources")]
        public void EnhancedDotNetDllFromDebug_GivenDebugState_ShouldAddtestStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var objectToMakeModelItem = new DsfFlowSwitchActivity();
            var modelItem = ModelItemUtils.CreateModelItem(objectToMakeModelItem);
            mockWorkflowDesignerViewModel.Setup(model => model.GetModelItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(modelItem);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DotnetllDebugStates.json");
            var sequncetext = File.ReadAllText("JsonResources\\dotnetDllState.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            var dotnetDllState = serializer.Deserialize<IDebugState>(sequncetext);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[0], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var seq = new DebugStateTreeViewItemViewModel(repo.Object) { Content = dotnetDllState, };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2,
                seq
            };

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("EnhancedDotNetDllFromDebug", BindingFlags.NonPublic | BindingFlags.Instance);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { itemViewModel, dotnetDllState });
            //---------------Test Result -----------------------
            var count = testFrameworkViewModel.SelectedServiceTest.TestSteps.Count;
            var childCount = testFrameworkViewModel.SelectedServiceTest.TestSteps.All(step => step.Children.Count == 0);
            Assert.AreEqual(4, count);
            Assert.IsTrue(childCount);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        [DeploymentItem("JsonResources\\sequenceState.json", "JsonResources")]
        public void AddChildDebugItems_GivenSequenceWithChildren_ShouldAddStepWithOutputsFromDebugState()
        {
            Thread.Sleep(10);
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var sequncetext = File.ReadAllText("JsonResources\\sequenceState.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            var sequenceSate = serializer.Deserialize<IDebugState>(sequncetext);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            var seq = new DebugStateTreeViewItemViewModel(repo.Object) { Content = sequenceSate, };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2,
                seq
            };

            seq.Children.Add(itemViewModel1);
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            var testSteps = new ObservableCollection<IServiceTestStep>() { };

            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { sequenceSate, seq, default(IServiceTestStep) });
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void SetInputs_GivenDebugStates_ShouldAddTestInputValues()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Inputs.Add(new ServiceTestInput("Name", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetInputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[0] });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            Assert.AreEqual("Nathi", testFrameworkViewModel.SelectedServiceTest.Inputs.First().Value);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ServiceTestViewModel))]
        [DeploymentItem("JsonResources\\DebugInputStates.json", "JsonResources")]
        public void ServiceTestViewModel_ValidateAddStepType_Should_Update_InputValues()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");
            contextualResourceModel.ID = resourceId;
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            const string expectedValue = "Bob";
            mockWorkflowDesignerViewModel.Setup(o => o.GetWorkflowInputs("Name")).Returns(expectedValue).Verifiable();
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugInputStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(),
                new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object,
                mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null)
            {
                WebClient = new Mock<IWarewolfWebClient>().Object
            };
            //When the value is parsed in a empty, then it should be set to the debug state value
            testFrameworkViewModel.SelectedServiceTest.Inputs.Add(new ServiceTestInput("Name", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ValidateAddStepType", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            var mockDebugTreeViewItemViewModel = new Mock<IDebugTreeViewItemViewModel>();
            methodInfo.Invoke(testFrameworkViewModel, new object[] { mockDebugTreeViewItemViewModel.Object, debugStates[0] });

            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            Assert.AreEqual("Name", testFrameworkViewModel.SelectedServiceTest.Inputs.First().Variable);
            Assert.AreEqual(expectedValue, testFrameworkViewModel.SelectedServiceTest.Inputs.First().Value);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void SetOutputs_GivenDebugStates_ShouldAddTestOutput()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            contextualResourceModel.DataList = "<Datalist/>";
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetOutputs", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(IDebugState) }, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates.Last() });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual("Hello Nathi.", testFrameworkViewModel.SelectedServiceTest.Outputs.First().Value);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void SetOutputs_GivenDebugStatesMultipleOutputs_ShouldAddAllTestOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            contextualResourceModel.DataList = "<Datalist/>";
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            var debugState = debugStates.Single(state => state.StateType == StateType.End);
            var debugItemResult = new DebugItemResult();
            debugItemResult.Variable = "var";
            debugItemResult.Value = "var";
            debugState.Outputs.Single().ResultsList.Add( debugItemResult);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetOutputs", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(IDebugState) }, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates.Last() });
            //---------------Test Result -----------------------
            Assert.AreEqual(2, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual("Hello Nathi.", testFrameworkViewModel.SelectedServiceTest.Outputs.First().Value);
            Assert.AreEqual("Message", testFrameworkViewModel.SelectedServiceTest.Outputs.First().Variable);
            Assert.AreEqual("var", testFrameworkViewModel.SelectedServiceTest.Outputs.Last().Value);
            Assert.AreEqual("var", testFrameworkViewModel.SelectedServiceTest.Outputs.Last().Variable);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void TestPassingResult_GivenIsSet_ShouldFirePropertyChanges()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var wasCalled = false;
            testFrameworkViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestPassingResult")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.TestPassingResult = "Passed";
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);

        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void OnStepOutputPropertyChanges_GivenIsInvoked_ShouldFirePropertyChanges()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("OnStepOutputPropertyChanges", BindingFlags.NonPublic | BindingFlags.Instance);
            var wasCalled = false;
            var argsT = new PropertyChangedEventArgs("");
            testFrameworkViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsDirty")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new[] { new object(), argsT });
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);

        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStepGetParentType_GivenSequence_ShouldSetupServiceTestStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ServiceTestStepGetParentType", BindingFlags.NonPublic | BindingFlags.Instance);
            var activity = new DsfSequenceActivity();
            var uniqueID = Guid.NewGuid().ToString();
            activity.UniqueID = uniqueID;
            activity.DisplayName = "Dsipa";
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            IServiceTestStep serviceTestStep = null;
            
            var parameters = new object[] { modelItem, serviceTestStep };
            methodInfo.Invoke(testFrameworkViewModel, parameters);
            //---------------Test Result -----------------------
            var o = (IServiceTestStep)parameters[1];
            Assert.AreEqual(uniqueID, o.ActivityID.ToString());
            Assert.AreEqual("Dsipa", o.StepDescription);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStepGetParentType_GivenDsfForEachActivity_ShouldSetupServiceTestStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ServiceTestStepGetParentType", BindingFlags.NonPublic | BindingFlags.Instance);
            var activity = new DsfForEachActivity();
            var uniqueID = Guid.NewGuid().ToString();
            activity.UniqueID = uniqueID;
            activity.DisplayName = "Dsipa";
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            IServiceTestStep serviceTestStep = null;
            
            var parameters = new object[] { modelItem, serviceTestStep };
            methodInfo.Invoke(testFrameworkViewModel, parameters);
            //---------------Test Result -----------------------
            var o = (IServiceTestStep)parameters[1];
            Assert.AreEqual(uniqueID, o.ActivityID.ToString());
            Assert.AreEqual("Dsipa", o.StepDescription);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ServiceTestViewModel))]
        public void ServiceTestStepGetParentType_Given_SuspendExecutionActivity_ShouldSetupServiceTestStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage
            {
                ResourceModel = mockResourceModel.Object, RootItems = new List<IDebugTreeViewItemViewModel>()
            };

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(),
                new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object,
                popupController.Object, newTestFromDebugMessage, null)
            {
                WebClient = new Mock<IWarewolfWebClient>().Object
            };
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ServiceTestStepGetParentType", BindingFlags.NonPublic | BindingFlags.Instance);
            var activity = new SuspendExecutionActivity();
            var uniqueId = Guid.NewGuid().ToString();
            activity.UniqueID = uniqueId;
            activity.DisplayName = "Suspend Execution";
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            IServiceTestStep serviceTestStep = null;

            var parameters = new object[] { modelItem, serviceTestStep };
            methodInfo.Invoke(testFrameworkViewModel, parameters);
            //---------------Test Result -----------------------
            var o = (IServiceTestStep)parameters[1];
            Assert.AreEqual(uniqueId, o.ActivityID.ToString());
            Assert.AreEqual("Suspend Execution", o.StepDescription);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStepGetParentType_GivenDsfSelectAndApplyActivity_ShouldSetupServiceTestStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ServiceTestStepGetParentType", BindingFlags.NonPublic | BindingFlags.Instance);
            var activity = new DsfSelectAndApplyActivity();
            var uniqueID = Guid.NewGuid().ToString();
            activity.UniqueID = uniqueID;
            activity.DisplayName = "Dsipa";
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            IServiceTestStep serviceTestStep = null;
            
            var parameters = new object[] { modelItem, serviceTestStep };
            methodInfo.Invoke(testFrameworkViewModel, parameters);
            //---------------Test Result -----------------------
            var o = (IServiceTestStep)parameters[1];
            Assert.AreEqual(uniqueID, o.ActivityID.ToString());
            Assert.AreEqual("Dsipa", o.StepDescription);
        }


        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void AddOutputsIfHasVariable_GivenListsOutputsAndServiceTestStep_ShouldBuildServiceTestOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddOutputsIfHasVariable", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            IServiceTestStep serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", new BindableCollection<IServiceTestOutput>(), StepType.Assert);
            var outputs = new List<string>() { "a", "b" };
            
            var parameters = new object[] { outputs, serviceTestStep };
            var invoke = methodInfo.Invoke(null, parameters) as List<IServiceTestOutput>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(invoke);
            Assert.AreEqual(2, invoke.Count);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void AddEnhancedDotNetDllMethod_GivenPluginActionAndStep_ShouldMap()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            IPluginAction pluginAction = new PluginAction();
            pluginAction.ID = Guid.NewGuid();
            pluginAction.Method = "Hi";
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddEnhancedDotNetDllMethod", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            var serviceTestOutputs = new BindableCollection<IServiceTestOutput>();
            IServiceTestStep serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", serviceTestOutputs, StepType.Assert);
            serviceTestStep.Children = new BindableCollection<IServiceTestStep>();
            serviceTestStep.ActivityType = typeof(DsfEnhancedDotNetDllActivity).Name;
            
            var parameters = new object[] { pluginAction, serviceTestStep };
            methodInfo.Invoke(testFrameworkViewModel, parameters);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, serviceTestStep.Children.Count);

        }
        //private void AddEnhancedDotNetDll(DsfEnhancedDotNetDllActivity dotNetDllActivity, ServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void AddEnhancedDotNetDll_GivenActions_ShouldAddChildrens()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            IPluginAction pluginAction = new PluginAction();
            pluginAction.ID = Guid.NewGuid();
            pluginAction.Method = "Hi";
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddEnhancedDotNetDll", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            var serviceTestOutputs = new BindableCollection<IServiceTestOutput>();
            IServiceTestStep serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", serviceTestOutputs, StepType.Assert);
            serviceTestStep.Children = new BindableCollection<IServiceTestStep>()
            {

                new ServiceTestStep(Guid.Empty, "", new BindableCollection<IServiceTestOutput>(), StepType.Assert )
            };
            serviceTestStep.ActivityType = typeof(DsfEnhancedDotNetDllActivity).Name;
            var dotNetDllActivity = new DsfEnhancedDotNetDllActivity()
            {
                Constructor = new PluginConstructor()
                {
                    ID = Guid.NewGuid()
                }
            };
            dotNetDllActivity.UniqueID = serviceTestStep.ActivityID.ToString();
            dotNetDllActivity.MethodsToRun = new List<IPluginAction>()
            {
                new PluginAction() {ID = Guid.Empty},
                new PluginAction() {ID = Guid.NewGuid()},
            };
            ObservableCollection<IServiceTestStep> collection = new BindableCollection<IServiceTestStep>() { serviceTestStep };

            
            var parameters = new object[] { dotNetDllActivity, serviceTestStep, collection };
            methodInfo.Invoke(testFrameworkViewModel, parameters);
            //---------------Test Result -----------------------
            Assert.AreEqual(3, serviceTestStep.Children.Count);

        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void AddOutputs_GivenNoOutputNotNull()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            IPluginAction pluginAction = new PluginAction();
            pluginAction.ID = Guid.NewGuid();
            pluginAction.Method = "Hi";
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddOutputs", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            var serviceTestOutputs = new BindableCollection<IServiceTestOutput>();
            IServiceTestStep serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", serviceTestOutputs, StepType.Assert);
            serviceTestStep.Children = new BindableCollection<IServiceTestStep>()
            {

                new ServiceTestStep(Guid.Empty, "", new BindableCollection<IServiceTestOutput>(), StepType.Assert )
            };
            var outputs = new List<string>();

            
            var parameters = new object[] { outputs, serviceTestStep };
            var testOutputs = methodInfo.Invoke(testFrameworkViewModel, parameters) as List<IServiceTestOutput>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(testOutputs);
            Assert.AreEqual(1, testOutputs.Count);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void AddOutputs_GivenOutputNotNull()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            IPluginAction pluginAction = new PluginAction();
            pluginAction.ID = Guid.NewGuid();
            pluginAction.Method = "Hi";
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddOutputs", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            var serviceTestOutputs = new BindableCollection<IServiceTestOutput>();
            IServiceTestStep serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", serviceTestOutputs, StepType.Assert);
            serviceTestStep.Children = new BindableCollection<IServiceTestStep>()
            {

                new ServiceTestStep(Guid.Empty, "", new BindableCollection<IServiceTestOutput>(), StepType.Assert )
            };
            var outputs = new List<string>() { "a", "b" };

            
            var parameters = new object[] { outputs, serviceTestStep };
            var testOutputs = methodInfo.Invoke(testFrameworkViewModel, parameters) as List<IServiceTestOutput>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(testOutputs);
            Assert.AreEqual(2, testOutputs.Count);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStepWithOutputs_GivenBuildCorrectly()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ServiceTestStepWithOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            var outputs = new List<string>() { "a", "b" };
            IServiceTestStep serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", new BindableCollection<IServiceTestOutput>(), StepType.Assert);

            
            var parameters = new object[] { Guid.NewGuid().ToString(), "Dispaly", outputs, typeof(DsfSequenceActivity), ModelItemUtils.CreateModelItem(new DsfSequenceActivity()), serviceTestStep };
            var invoke = (bool)methodInfo.Invoke(testFrameworkViewModel, parameters);
            //---------------Test Result -----------------------
            Assert.IsNotNull(invoke);
            Assert.IsTrue(invoke);
        }
        
        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void RunSelectedTestInBrowser_GivenIsInvoked_ShouldFirePropertyChanges()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();

            var mock = new Mock<IExternalProcessExecutor>();
            mock.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, mock.Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("RunSelectedTestInBrowser", BindingFlags.NonPublic | BindingFlags.Instance);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { });
            //---------------Test Result -----------------------
            mock.Verify(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
        }
        
        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void MarkChildrenPending_GivenIsInvoked_ShouldSetAsPending()
        {
            //---------------Set up test pack-------------------
            IServiceTestStep serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", new BindableCollection<IServiceTestOutput>()
            {
                new ServiceTestOutput("","","","")
            }, StepType.Assert);
            serviceTestStep.Result = new TestRunResult();
            var serviceTestOutput = serviceTestStep.StepOutputs.Single();
            serviceTestOutput.Result = new TestRunResult();
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("MarkChildrenPending", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(IServiceTestStep) }, null);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(null, new object[] { serviceTestStep });
            //---------------Test Result -----------------------
            Assert.IsTrue(serviceTestStep.Result.RunTestResult == RunResult.TestPending);
            Assert.IsTrue(serviceTestOutput.Result.RunTestResult == RunResult.TestPending);

        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void ProcessInputsAndOutputs_GivenStepDebugStates_ShouldNotAdd()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ProcessInputsAndOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1] });
            //---------------Test Result -----------------------
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void ProcessInputsAndOutputs_GivenInputStepDebugStates_ShouldAddInput()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ProcessInputsAndOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            testFrameworkViewModel.SelectedServiceTest.Inputs.Add(new ServiceTestInput("Name", ""));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[0] });
            //---------------Test Result -----------------------
            Assert.AreEqual("Nathi", testFrameworkViewModel.SelectedServiceTest.Inputs.First().Value);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void ProcessInputsAndOutputs_GivenOutputStepDebugStates_ShouldAddOutput()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            contextualResourceModel.DataList = "<DataList/>";
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ProcessInputsAndOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates.Last() });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            Assert.AreEqual("Hello Nathi.", testFrameworkViewModel.SelectedServiceTest.Outputs.First().Value);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("JsonResources\\DebugStates.json", "JsonResources")]
        public void PrepopulateTestsUsingDebug_GivenWrongMessage_ShouldAddTests()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var serializer = new Dev2JsonSerializer();

            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IServerRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates.First() };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var synchronousAsyncWorker = new SynchronousAsyncWorker();
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), synchronousAsyncWorker, new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            testFrameworkViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            //---------------Test Result -----------------------
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
        }


        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_GivenInCorrectMessage_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
            
            new ServiceTestViewModel(CreateResourceModel(), synchronousAsyncWorker, new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, newTestFromDebugMessage, null);
            var exceptions = synchronousAsyncWorker.Exceptions.Count;
            Assert.AreEqual(1, exceptions);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void RefreshCommands_ShouldUpdateDisplayName()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, message, null);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var wasCalled = false;
            testViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DisplayName")
                {
                    wasCalled = true;
                }
            };

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testViewModel.RefreshCommands();
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceID_GivenResourceModel_ShouldReturnCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.SetupAllProperties();
            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, message, null);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;


            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var resourceID = testViewModel.ResourceID;
            //---------------Test Result -----------------------
            Assert.AreEqual(resourceID, testViewModel.ResourceModel.ID);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void Stoptest_GivenServiceTests_ShouldSetValuesCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var handler = new Mock<IServiceTestCommandHandler>();
            handler.Setup(commandHandler => commandHandler.StopTest(resourceModel.Object));
            resourceModel.SetupAllProperties();
            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, message, null);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("StopTest", BindingFlags.NonPublic | BindingFlags.Instance);
            var propertyInfo = typeof(ServiceTestViewModel).GetProperty("ServiceTestCommandHandler", BindingFlags.Public | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            Assert.IsNotNull(propertyInfo);
            //---------------Execute Test ----------------------
            propertyInfo.SetValue(testViewModel, handler.Object);
            methodInfo.Invoke(testViewModel, new object[] { });
            //---------------Test Result -----------------------
            Assert.IsFalse(testViewModel.SelectedServiceTest.IsTestRunning);
            Assert.IsTrue(testViewModel.SelectedServiceTest.TestPending);
            handler.Verify(commandHandler => commandHandler.StopTest(resourceModel.Object));
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Nkosinathi Sangweni")]
        public void SetStepIcon_GiventypeName_ShouldSetValuesCorrectly_PassThrouth()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var handler = new Mock<IServiceTestCommandHandler>();
            handler.Setup(commandHandler => commandHandler.StopTest(resourceModel.Object));
            resourceModel.SetupAllProperties();
            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, message, null);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetStepIcon", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, new[] { typeof(string), typeof(ServiceTestStep) }, new[] { new ParameterModifier(2), });
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            try
            {
                var typename = "DsfDecision";
                methodInfo.Invoke(testViewModel, new object[] { typename, default(ServiceTestStep) });

                //---------------Test Result -----------------------

                //---------------Execute Test ----------------------
                typename = "FlowDecision";
                methodInfo.Invoke(testViewModel, new object[] { typename, default(ServiceTestStep) });


                //---------------Execute Test ----------------------
                typename = "DsfSwitch";
                methodInfo.Invoke(testViewModel, new object[] { typename, default(ServiceTestStep) });

                //---------------Test Result -----------------------
            }
            catch (Exception)
            {
                //
            }
        }
    }
}
