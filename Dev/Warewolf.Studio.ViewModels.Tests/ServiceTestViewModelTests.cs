using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Providers.Events;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;




namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public partial class ServiceTestViewModelTests
    {
        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFrameworkViewModel_Constructor_NullResourceModel_ShouldThrowException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new ServiceTestViewModel(default(IContextualResourceModel), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestFrameworkViewModel_MsgConstructor_NullMessage_ShouldThrowException()
        {
            //------------Setup for test--------------------------

            var resourceModel = new Mock<IContextualResourceModel>();
            //------------Execute Test---------------------------
            new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void TestFrameworkViewModel_MsgConstructor_MessageResourceModel_ShouldCreateNewtest()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var env = new Mock<IServer>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            var con = new Mock<IEnvironmentConnection>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, message, null);
            testViewModel.WebClient = new Mock<IWarewolfWebClient>().Object;
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(testViewModel.SelectedServiceTest);
        }



        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void TestFrameworkViewModel_MsgConstructor_MsgWithInputValues_ShouldAddInputvalues()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var env = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, message, null);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(testViewModel.SelectedServiceTest);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void ShoDuplicatePopup_GivenIsInvoked_ShouldShowCorrectMessage()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(controller => controller.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false)).Verifiable();
            var testVM = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testVM.ShowDuplicatePopup();
            //---------------Test Result -----------------------
            mock.Verify(controller => controller.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false), Times.Once);

        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_NotNullResourceModel_ShouldSetResourceModel()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var testVM = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testVM);
            Assert.IsNotNull(testVM.ResourceModel);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_NotNullResourceModel_ShouldSetDisplayNameIncludingResourceDisplayName()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.DisplayName).Returns("Workflow Name");
            //------------Execute Test---------------------------
            var testVM = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("My WF - Tests", testVM.DisplayName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void OnCreation_GivenIsNew_ShouldHaveRunAllTestsUrl()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("http://rsaklf/secure/My WF.tests", vm.RunAllTestsUrl);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDuplicateTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.DuplicateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DuplicateTestCommand.CanExecute(null));
        }


        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveStopTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.StopTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.StopTestCommand.CanExecute(null));
        }


        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveCreateTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.CreateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.CreateTestCommand.CanExecute(null));
        }

        IContextualResourceModel CreateResourceModel(bool isConnected = true)
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.Connection.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            moqModel.Setup(model => model.Category).Returns("My WF");
            moqModel.Setup(model => model.Environment.IsLocalHost).Returns(isConnected);
            moqModel.Setup(model => model.ResourceName).Returns("My WF");
            return moqModel.Object;
        }

        IContextualResourceModel CreateResourceModelWithMoreSave()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            moqModel.Setup(model => model.Environment.ResourceRepository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>())).Returns(new TestSaveResult() { Result = SaveResult.Success });
            return moqModel.Object;
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveModel()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.CreateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.CreateTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunAllTestsCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunAllTestsCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.RunAllTestsCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNewNotConnected_ShouldSetRunAllTestsCommandExecuteFalse()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IEventAggregator>();
            var vm = new ServiceTestViewModel(CreateResourceModel(false), new SynchronousAsyncWorker(), mock.Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunAllTestsCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunAllTestsCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunSelectedTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.RunSelectedTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNewNotConnected_ShouldSetRunSelectedTestCommandCanExecuteFalse()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IEventAggregator>();
            var vm = new ServiceTestViewModel(CreateResourceModel(false), new SynchronousAsyncWorker(), mock.Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunSelectedTestCommand.CanExecute(null));
        }



        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNewNotConnected_ShouldSetRunAllTestsInBrowserCommandCanExecuteFalse()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IEventAggregator>();
            var vm = new ServiceTestViewModel(CreateResourceModel(false), new SynchronousAsyncWorker(), mock.Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunAllTestsInBrowserCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunAllTestsInBrowserCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunSelectedTestInBrowserCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestInBrowserCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunSelectedTestInBrowserCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Sanele Mthembu")]
        public void AllNamesValid_GivenEmptyItemIn_ListOfTestName_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModelWithMoreSave(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            var privateObj = new PrivateObject(vm);
            var names = new List<string> { "", "Test2" };
            var results = privateObj.Invoke("AllNamesValid", names);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(results);
            //---------------Test Result -----------------------
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        [Timeout(250)]
        [Owner("Sanele Mthembu")]
        public void AllNamesValid_GivenListOfTestName_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModelWithMoreSave(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            var privateObj = new PrivateObject(vm);
            var names = new List<string> { "Test1", "Test2"};
            var results = privateObj.Invoke("AllNamesValid", names);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(results);
            //---------------Test Result -----------------------
            Assert.AreEqual(true, results);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Sanele Mthembu")]
        public void Save_GivenEmptyTestName_SetErrorMessage()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModelWithMoreSave(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            var DummyTest = typeof(DummyServiceTest);
            var test1 = vm.Tests.FirstOrDefault(p => p.GetType() != DummyTest);
            test1.TestName = string.Empty;
            vm.Save();
            //---------------Test Result -----------------------
            Assert.AreEqual(vm.ErrorMessage, "Cannot be null");
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void Save_GivenThrowsNoException_ShouldMarkAllTestsAsNotDirty()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModelWithMoreSave(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            vm.Save();
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.Tests.All(model => !model.IsDirty));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_SelectedServiceTest")]
        public void TestFrameworkViewModel_SelectedServiceTest_CheckIsNull()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest = null;
            //------------Assert Results-------------------------
            Assert.IsNull(testFrameworkViewModel.SelectedServiceTest);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddANewTest()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldIncrementTestNumber()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
            Assert.AreEqual("Test 1", testFrameworkViewModel.Tests[0].TestName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldDisableDuplicateCommand()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
            Assert.IsFalse(testFrameworkViewModel.DuplicateTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldShowError()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.PopupController);
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, null, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddANewTestWithDefaultName()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            var test = testFrameworkViewModel.Tests[0];
            Assert.IsNotNull(test);
            Assert.AreEqual("Test 1", test.TestName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Executed_ShouldSetSelectedTestToNewlyCreatedTest()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            Assert.IsNotNull(testFrameworkViewModel.SelectedServiceTest);
            Assert.AreEqual(1, testFrameworkViewModel.Tests.Count);
            Assert.AreEqual(testModel, testFrameworkViewModel.SelectedServiceTest);
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
            Assert.AreNotEqual(testModel, testFrameworkViewModel.SelectedServiceTest);
            Assert.AreEqual(testFrameworkViewModel.Tests[0], testFrameworkViewModel.SelectedServiceTest);
            Assert.AreEqual("http://rsaklf/secure/My WF.tests/Test 1", testFrameworkViewModel.SelectedServiceTest.RunSelectedTestUrl);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_SaveAsPublic_ShouldSetSelectedTestToPublic()
        {
            //------------Setup for test--------------------------
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            con.Setup(connection => connection.IsConnected).Returns(true);
            con.Setup(model => model.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(model => model.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>())).Returns(new TestSaveResult() { Result = SaveResult.Success }).Verifiable();
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(con.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            resourceModelMock.Setup(model => model.Category).Returns("My WF");
            resourceModelMock.Setup(model => model.ResourceName).Returns("My WF");


            var testFrameworkViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.Save();
            //------------Assert Preconditions-------------------
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
            Assert.AreEqual(testFrameworkViewModel.Tests[0], testFrameworkViewModel.SelectedServiceTest);
            Assert.AreEqual("http://rsaklf/secure/My WF.tests/Test 1", testFrameworkViewModel.SelectedServiceTest.RunSelectedTestUrl);
            //------------Execute Test---------------------------
            testFrameworkViewModel.SelectedServiceTest.AuthenticationType = AuthenticationType.Public;
            //------------Assert Results-------------------------
            Assert.AreEqual("http://rsaklf/secure/My WF.tests/Test 1", testFrameworkViewModel.SelectedServiceTest.RunSelectedTestUrl);

            testFrameworkViewModel.Save();

            Assert.AreEqual("http://rsaklf/public/My WF.tests/Test 1", testFrameworkViewModel.SelectedServiceTest.RunSelectedTestUrl);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_Tests_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            var _wasCalled = false;
            testFrameworkViewModel.PropertyChanged += (sender, args) =>
              {
                  if (args.PropertyName == "Tests")
                  {
                      _wasCalled = true;
                  }
              };
            //------------Execute Test---------------------------
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel>();
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_SelectedTest_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            var _wasCalled = false;
            testFrameworkViewModel.PropertyChanged += (sender, args) =>
              {
                  if (args.PropertyName == "SelectedServiceTest")
                  {
                      _wasCalled = true;
                  }
              };
            //------------Execute Test---------------------------
            testFrameworkViewModel.SelectedServiceTest = new ServiceTestModel(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }
        

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_ErrorMessage_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            var _wasCalled = false;
            testFrameworkViewModel.PropertyChanged += (sender, args) =>
              {
                  if (args.PropertyName == "ErrorMessage")
                  {
                      _wasCalled = true;
                  }
              };
            //------------Execute Test---------------------------
            testFrameworkViewModel.ErrorMessage = "Home";
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddInputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarInput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            var test = testFrameworkViewModel.Tests[0];
            Assert.IsNotNull(test);
            Assert.AreEqual(1, test.Inputs.Count);
            Assert.AreEqual("a", test.Inputs[0].Variable);
            Assert.AreEqual("", test.Inputs[0].Value);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddOutputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            var test = testFrameworkViewModel.Tests[0];
            Assert.IsNotNull(test);
            Assert.AreEqual(1, test.Outputs.Count);
            Assert.AreEqual("msg", test.Outputs[0].Variable);
            Assert.AreEqual("", test.Outputs[0].Value);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDeleteTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            Assert.IsNotNull(vm.DeleteTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DeleteTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenResourceModelIsConnectedAndTestIsDisabled_ShouldsetCanExecuteTrue()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            vm.CreateTestCommand.Execute(null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);

            Assert.IsTrue(vm.SelectedServiceTest != null);
            var isConnected = vm.ResourceModel.Environment.IsConnected;
            Assert.IsTrue(isConnected);
            //---------------Execute Test ----------------------

            vm.SelectedServiceTest.Enabled = false;
            //---------------Test Result -----------------------
            var canExecute = vm.DeleteTestCommand.CanExecute(vm.SelectedServiceTest);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenResourceModelIsNotConnectedAndTestIsDisabled_ShouldsetCanExecuteTrue()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(false), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            vm.CreateTestCommand.Execute(null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);

            Assert.IsTrue(vm.SelectedServiceTest != null);
            var isConnected = vm.ResourceModel.Environment.IsConnected;
            Assert.IsFalse(isConnected);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            vm.SelectedServiceTest.Enabled = false;
            //---------------Test Result -----------------------
            var canExecute = vm.DeleteTestCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void OnCreation_GivenIsDisabled_DeleteTestCommandShouldBeEnabled()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            Assert.IsNotNull(vm.DeleteTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DeleteTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void OnCreation_GivenIsEnabled_DeleteTestCommandShouldBeDisabled()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            vm.SelectedServiceTest.Enabled = true;
            Assert.IsNotNull(vm.DeleteTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DeleteTestCommand.CanExecute(null));
        }


        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Save")]
        public void ServiceTestViewModel_Save_WithTests_ShouldCallSaveTestOnResourceModel()
        {
            //------------Setup for test--------------------------
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            con.Setup(connection => connection.IsConnected).Returns(true);
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()));
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(con.Object);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            var serviceTestViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            serviceTestViewModel.CreateTestCommand.Execute(null);
            serviceTestViewModel.SelectedServiceTest.Inputs = new ObservableCollection<IServiceTestInput>
            {
                new ServiceTestInput("[[var]]","val")
            };
            serviceTestViewModel.SelectedServiceTest.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                new ServiceTestOutput("[[var]]","val", "","")
            };
            //------------Execute Test---------------------------
            Assert.IsTrue(serviceTestViewModel.CanSave);
            serviceTestViewModel.Save();
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()), Times.Once);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Save")]
        public void ServiceTestViewModel_Save_WithTestSteps_ShouldCallSaveTestOnResourceModel()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);

            var assignActivity = new DsfMultiAssignActivity();
            var uniqueId = Guid.NewGuid();
            assignActivity.UniqueID = uniqueId.ToString();
            assignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[Var1]]", "bob", 1), new ActivityDTO("[[Var2]]", "mary", 2), new ActivityDTO("[[name]]", "dora", 3) };

            var dsfSequenceActivity = new DsfSequenceActivity { Activities = new Collection<Activity> { assignActivity } };
            var dsfSeqId = Guid.NewGuid();
            dsfSequenceActivity.UniqueID = dsfSeqId.ToString();


            var sequenceActivity = new DsfSequenceActivity { Activities = new Collection<Activity> { dsfSequenceActivity } };
            var seqId = Guid.NewGuid();
            sequenceActivity.UniqueID = seqId.ToString();

            var modelItem = ModelItemUtils.CreateModelItem(sequenceActivity);
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            con.Setup(connection => connection.IsConnected).Returns(true);
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>())).Returns(new TestSaveResult { Result = SaveResult.ResourceUpdated });
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(con.Object);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.Inputs = new ObservableCollection<IServiceTestInput>
            {
                new ServiceTestInput("[[var]]","val")
            };
            testFrameworkViewModel.SelectedServiceTest.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                new ServiceTestOutput("[[var]]","val", "","")
            };
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //------------Execute Test---------------------------
            Assert.IsTrue(testFrameworkViewModel.CanSave);
            testFrameworkViewModel.Save();
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()), Times.Once);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("ServiceTestViewModel_Save")]
        public void ServiceTestViewModel_Save_DuplicateName_ShouldShowPopupError()
        {
            //------------Setup for test--------------------------
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()));
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            resourceModelMock.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            var popupController = new Mock<IPopupController>();

            //------------Execute Test---------------------------
            var serviceTestViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            serviceTestViewModel.CreateTestCommand.Execute(null);
            Assert.IsTrue(serviceTestViewModel.CanSave);
            serviceTestViewModel.Save();

            serviceTestViewModel.CreateTestCommand.Execute(null);            
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestViewModel.PopupController);
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, null, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("ServiceTestViewModel_Save")]
        public void ServiceTestViewModel_Save_Rename_ShouldRename()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var serviceTestViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            serviceTestViewModel.CreateTestCommand.Execute(null);
            Assert.IsTrue(serviceTestViewModel.IsDirty);
            Assert.IsTrue(serviceTestViewModel.CanSave);
            serviceTestViewModel.Save();

            serviceTestViewModel.SelectedServiceTest.TestName = "NewName";
            serviceTestViewModel.Save();

            //------------Assert Results-------------------------
            Assert.AreEqual("NewName", serviceTestViewModel.SelectedServiceTest.TestName);
            Assert.AreEqual(2, serviceTestViewModel.Tests.Count);
        }

        [TestMethod]
        [Timeout(1000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_IsDirty_IsFalse()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsFalse(testFrameworkViewModel.IsDirty);
        }

        [TestMethod]
        [Timeout(1000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldSetHasChangedTrue()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            var test = testFrameworkViewModel.Tests[0];
            Assert.IsNotNull(test);
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Pieter Terblanche")]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var mainViewModelMock = new Mock<IShellViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);
            const string helpText = "someText";

            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);

            //act
            testFrameworkViewModel.UpdateHelpDescriptor(helpText);

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText(helpText));
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_IsDirty")]
        public void ServiceTestViewModel_IsDirty_WhenSetTrue_ShouldUpdateDisplayNameWithStar()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            Assert.AreEqual("My WF - Tests *", testFrameworkViewModel.DisplayName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_IsDirty")]
        public void ServiceTestViewModel_IsDirty_WhenSetTrueTwice_ShouldUpdateDisplayNameWithOneStarOnly()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            Assert.AreEqual("My WF - Tests *", testFrameworkViewModel.DisplayName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Tests")]
        public void ServiceTestViewModel_Tests_GetWhenNullTests_ShouldHaveDummyTest()
        {
            //------------Setup for test--------------------------
            var resourceMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironment = new Mock<IServer>();
            var mockRepo = new Mock<IResourceRepository>();
            mockRepo.Setup(repository => repository.LoadResourceTests(It.IsAny<Guid>())).Returns((List<IServiceTestModelTO>)null);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockRepo.Object);
            resourceMock.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            var serviceTestViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------
            Assert.AreEqual(1, tests.Count);
            Assert.AreEqual("Create a new test.", tests[0].TestName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Tests")]
        public void ServiceTestViewModel_Tests_GetWhenEmptyTests_ShouldHaveDummyTest()
        {
            //------------Setup for test--------------------------
            var resourceMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironment = new Mock<IServer>();
            var mockRepo = new Mock<IResourceRepository>();
            mockRepo.Setup(repository => repository.LoadResourceTests(It.IsAny<Guid>())).Returns(new List<IServiceTestModelTO>());
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockRepo.Object);
            resourceMock.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            var serviceTestViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------
            Assert.AreEqual(1, tests.Count);
            Assert.AreEqual("Create a new test.", tests[0].TestName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Tests")]
        public void ServiceTestViewModel_Tests_GetWhenTests_ShouldHaveTestsAndDummyAtBottom()
        {
            //------------Setup for test--------------------------
            var resourceMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(model => model.Connection.IsConnected).Returns(true);
            var mockRepo = new Mock<IResourceRepository>();
            mockRepo.Setup(repository => repository.LoadResourceTests(It.IsAny<Guid>())).Returns(new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test From Server"
                }
            });
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockRepo.Object);
            resourceMock.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------
            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("Test From Server", tests[0].TestName);
            Assert.AreEqual(AuthenticationType.Public, tests[0].AuthenticationType);
            Assert.IsTrue(tests[0].Enabled);
            Assert.AreEqual("Create a new test.", tests[1].TestName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Tests")]
        public void ServiceTestViewModel_Tests_GetWhenTestsWithSteps_ShouldHaveTestsAndDummyAtBottom()
        {
            //------------Setup for test--------------------------
            var resourceMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(model => model.Connection.IsConnected).Returns(true);
            var mockRepo = new Mock<IResourceRepository>();
            var serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "Assing", new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput("[[p]]", "b", "", "") }, StepType.Mock);
            serviceTestStep.Children = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStepTO(Guid.NewGuid(),"Random",new ObservableCollection<IServiceTestOutput> {new ServiceTestOutput("[[o]]","a", "", "") },StepType.Mock)
                {
                    Parent = serviceTestStep
                }

            };

            var mockTo = new Mock<IServiceTestModelTO>();
            mockTo.SetupAllProperties();

            mockRepo.Setup(repository => repository.LoadResourceTests(It.IsAny<Guid>())).Returns(new List<IServiceTestModelTO>
            {
                mockTo.Object
            });
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockRepo.Object);
            resourceMock.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("Create a new test.", tests[1].TestName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Tests")]
        public void ServiceTestViewModel_ResourceUpdateMessage_GetWhenTestsWithSteps_ShouldHaveTestsAndDummyAtBottom()
        {
            //------------Setup for test--------------------------
            var resourceMock = CreateResourceModelWithSingleScalarOutputMock();

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(model => model.Connection.IsConnected).Returns(true);
            mockEnvironment.SetupProperty(model => model.Connection.ReceivedResourceAffectedMessage);
            var serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "Assing", new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput("[[p]]", "b", "", "") }, StepType.Mock);
            serviceTestStep.Children = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStepTO(Guid.NewGuid(),"Random",new ObservableCollection<IServiceTestOutput> {new ServiceTestOutput("[[o]]","a", "", "") },StepType.Mock)
                {
                    Parent = serviceTestStep
                }

            };

            var mockTo = new Mock<IServiceTestModelTO>();
            mockTo.Setup(to => to.TestName).Returns("N");
            mockTo.SetupAllProperties();

            mockEnvironment.Setup(model => model.ResourceRepository.LoadResourceTests(It.IsAny<Guid>())).Returns(new List<IServiceTestModelTO>
            {
               mockTo.Object
            });

            var mock = new Mock<IContextualResourceModel>();

            mockEnvironment.Setup(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(mock.Object);
            resourceMock.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            mock.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            mockEnvironment.Object.Connection.ReceivedResourceAffectedMessage.Invoke(It.IsAny<Guid>(), new CompileMessageList());
            //------------Assert Results-------------------------
            Assert.AreEqual(2, tests.Count);


        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateCommand_GivenIsDirty_ShouldSetCanExecuteFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel.DuplicateTestCommand);
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            //---------------Execute Test ----------------------
            var canDuplicate = testFrameworkViewModel.DuplicateTestCommand.CanExecute(null);
            //---------------Test Result -----------------------
            Assert.IsFalse(canDuplicate);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateCommand_GivenIsDirtyFalseAndSelectedIsNotNull_ShouldSetCanExecuteTrue()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel.DuplicateTestCommand);
            Assert.IsFalse(testFrameworkViewModel.IsDirty);
            testFrameworkViewModel.SelectedServiceTest = new ServiceTestModel(Guid.NewGuid());
            //---------------Execute Test ----------------------
            var canDuplicate = testFrameworkViewModel.DuplicateTestCommand.CanExecute(null);
            //---------------Test Result -----------------------
            Assert.IsTrue(canDuplicate);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirty_ShouldSetCanSaveFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel.DuplicateTestCommand);
            Assert.IsFalse(testFrameworkViewModel.IsDirty);
            //---------------Execute Test ----------------------
            var canSave = testFrameworkViewModel.CanSave;
            //---------------Test Result -----------------------
            Assert.IsFalse(canSave);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirtyAndValidName_ShouldSetCanSavetrue()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "name";
            //---------------Assert Precondition----------------
            Assert.IsTrue(testFrameworkViewModel.IsDirty);

            //---------------Execute Test ----------------------
            var canSave = testFrameworkViewModel.CanSave;
            //---------------Test Result -----------------------
            Assert.IsTrue(canSave);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirtyAndInvalidValidName_ShouldSetCanSaveFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "name$";
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel.DuplicateTestCommand);
            Assert.IsTrue(testFrameworkViewModel.IsDirty);

            //---------------Execute Test ----------------------
            var canSave = testFrameworkViewModel.CanSave;
            //---------------Test Result -----------------------
            Assert.IsFalse(canSave);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirtyAndEmptyName_ShouldSetCanSaveFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "";
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel.DuplicateTestCommand);
            Assert.IsTrue(testFrameworkViewModel.IsDirty);

            //---------------Execute Test ----------------------
            var canSave = testFrameworkViewModel.CanSave;
            //---------------Test Result -----------------------
            Assert.IsFalse(canSave);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void RunSelectedTestCommand_GivenSelectedTestIsNotDirty_ShouldRunTheTest()
        {
            //---------------Set up test pack-------------------
            
            var retVal = new StringBuilder();
            var mockEnvironmentModel = new Mock<IServer>();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid workspaceID) =>
            {
                retVal = o;
            });

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);
            mockEnvironmentModel.Setup(e => e.IsConnected).Returns(true);

            var mockResourceModel = CreateMockResourceModel();
            mockResourceModel.SetupGet(a => a.Environment).Returns(mockEnvironmentModel.Object);
            mockResourceModel.SetupGet(a => a.Environment.Connection).Returns(conn.Object);

            mockResourceModel.Setup(model => model.Environment.ResourceRepository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>())).Returns(new TestSaveResult() { Result = SaveResult.Success }).Verifiable();

            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved";
            testFrameworkViewModel.Save();
            //---------------Assert Precondition----------------
            Assert.IsFalse(testFrameworkViewModel.IsDirty);
            var selectedServiceTest = testFrameworkViewModel.SelectedServiceTest;
            Assert.IsNotNull(selectedServiceTest);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.RunSelectedTestCommand.Execute(null);
            //---------------Test Result -----------------------
            Assert.IsNull(testFrameworkViewModel.TestPassingResult);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void RunSelectedTestCommand_GivenSelectedTestIsDirty_ShouldSaveAndRunTheTest()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved";
            testFrameworkViewModel.Save();
            //---------------Assert Precondition----------------
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved1";
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            var selectedServiceTest = testFrameworkViewModel.SelectedServiceTest;
            Assert.IsNotNull(selectedServiceTest);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.RunSelectedTestCommand.Execute(null);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void RunSelectedTestCommand_GivenTestNameExists_ShouldShowDuplicateError()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved";
            testFrameworkViewModel.Save();
            //---------------Assert Precondition----------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved";
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            var selectedServiceTest = testFrameworkViewModel.SelectedServiceTest;
            Assert.IsNotNull(selectedServiceTest);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.RunSelectedTestCommand.Execute(null);
            //---------------Test Result -----------------------
            popupController.Verify(controller => controller.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void DeleteSelectedTestCommand_GivenTestNameExists_ShouldDeleteSelectedTest()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved";
            testFrameworkViewModel.Save();
            //---------------Assert Precondition----------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved";
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            var selectedServiceTest = testFrameworkViewModel.SelectedServiceTest;
            Assert.IsNotNull(selectedServiceTest);
            Assert.AreEqual(3, testFrameworkViewModel.Tests.Count);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.DeleteTestCommand.Execute(testFrameworkViewModel.SelectedServiceTest);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirtynameHastrailingSpaces_ShouldSetCanSaveFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "name ";
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel.DuplicateTestCommand);
            Assert.IsTrue(testFrameworkViewModel.IsDirty);

            //---------------Execute Test ----------------------
            var canSave = testFrameworkViewModel.CanSave;
            //---------------Test Result -----------------------
            Assert.IsFalse(canSave);
        }

        List<IServiceTestModel> GetTests(IServiceTestViewModel viewModel)
        {
            return viewModel.Tests.Where(model => model.GetType() != typeof(DummyServiceTest)).ToList();
        }
        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateTestCommand_GivenSelectedTesIsNotNull_ShouldAddNewTestToTestCollection()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved";
            testFrameworkViewModel.Save();
            //---------------Assert Precondition----------------
            Assert.IsFalse(testFrameworkViewModel.IsDirty);
            var testCount = GetTests(testFrameworkViewModel).Count;
            Assert.AreEqual(1, testCount);
            var selectedServiceTest = testFrameworkViewModel.SelectedServiceTest;
            Assert.IsNotNull(selectedServiceTest);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.DuplicateTestCommand.Execute(null);
            Assert.IsTrue(!testFrameworkViewModel.SelectedServiceTest.TestName.Contains("_dup"));
            //---------------Test Result -----------------------
            Assert.AreEqual(2, GetTests(testFrameworkViewModel).Count);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateTestCommand_GivenSelectedTesIsNotNull_ShouldSetSelectedTestToNewlyDuplicatedTest()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest.TestName = "NewTestSaved";
            testFrameworkViewModel.Save();
            //---------------Assert Precondition----------------
            Assert.IsFalse(testFrameworkViewModel.IsDirty);
            var testCount = GetTests(testFrameworkViewModel).Count;
            Assert.AreEqual(1, testCount);
            var selectedServiceTest = testFrameworkViewModel.SelectedServiceTest;
            Assert.IsNotNull(selectedServiceTest);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.DuplicateTestCommand.Execute(null);

            //---------------Test Result -----------------------
            Assert.IsTrue(!testFrameworkViewModel.SelectedServiceTest.TestName.Contains("_dup"));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteCommand_GivenSelectedTestIsDisabled_ShouldSetCanDeleteTrue()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, new Mock<IPopupController>().Object, null, null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.Save();
            //---------------Assert Precondition----------------
            Assert.AreEqual("Test 1", testFrameworkViewModel.SelectedServiceTest.TestName);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.SelectedServiceTest.Enabled = false;
            var caDelete = testFrameworkViewModel.DeleteTestCommand.CanExecute(testFrameworkViewModel.SelectedServiceTest);
            //---------------Test Result -----------------------
            Assert.IsTrue(caDelete);

        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenNullTest_Execute_ShouldReturnNull()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var mockServiceModel = new Mock<IServiceTestModel>();
            mockServiceModel.Setup(a => a.NameForDisplay).Returns("TestName");
            mockServiceModel.Setup(a => a.Enabled).Returns(false);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------

            testFrameworkViewModel.DeleteTestCommand.Execute(null);
            //------------Assert Results-------------------------
            popupController.Verify(controller => controller.ShowDeleteConfirmation(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenTests_ShouldShowConfirmation()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            con.Setup(connection => connection.IsConnected).Returns(true);
            con.Setup(model => model.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()));
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(con.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            resourceModelMock.Setup(model => model.Category).Returns("My WF");
            resourceModelMock.Setup(model => model.ResourceName).Returns("My WF");

            var testFrameworkViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);

            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------

            testFrameworkViewModel.DeleteTestCommand.Execute(testFrameworkViewModel.SelectedServiceTest);
            //------------Assert Results-------------------------
            popupController.Verify(controller => controller.ShowDeleteConfirmation(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenTests_ShouldUpdateTestsCollection()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IServer>();
            var con = new Mock<IEnvironmentConnection>();
            con.Setup(connection => connection.IsConnected).Returns(true);
            con.Setup(model => model.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()));
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(con.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            resourceModelMock.Setup(model => model.Category).Returns("My WF");
            resourceModelMock.Setup(model => model.ResourceName).Returns("My WF");

            var testFrameworkViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);

            var wasCalled = false;
            testFrameworkViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Tests")
                {
                    wasCalled = true;
                }
            };

            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 5", NameForDisplay = "Test 5" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //------------Assert Preconditions-------------------
            var beforeDelete = GetTests(testFrameworkViewModel).Count;
            Assert.AreEqual(1, beforeDelete);
            //------------Execute Test---------------------------
            testFrameworkViewModel.DeleteTestCommand.Execute(testFrameworkViewModel.SelectedServiceTest);
            //------------Assert Results-------------------------
            popupController.Verify(controller => controller.ShowDeleteConfirmation(It.IsAny<string>()), Times.Once);
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(beforeDelete - 1, GetTests(testFrameworkViewModel).Count);

        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenNameOne_ShouldBeValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "NameOne", NameForDisplay = "NameOne" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] {  });
            //---------------Test Result -----------------------
            Assert.AreEqual(true, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenNameOnePErcenta_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "NameOne%", NameForDisplay = "NameOne%" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] {  });
            //---------------Test Result -----------------------
            Assert.AreEqual(false, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenEmpty_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "", NameForDisplay = "" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] {});
            //---------------Test Result -----------------------
            Assert.AreEqual(false, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenNull_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid());
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { });
            //---------------Test Result -----------------------
            Assert.AreEqual(false, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivennameWithtrailingSpaces_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "name ", NameForDisplay = "name " };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { });
            //---------------Test Result -----------------------
            Assert.AreEqual(false, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void LastDateVisibility_GivenSelectedTestIsNew_ShouldHaveDateVisibilityCollapsed()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            Assert.IsNotNull(testFrameworkViewModel.SelectedServiceTest);
            //---------------Test Result -----------------------
            Assert.IsFalse(testFrameworkViewModel.SelectedServiceTest.LastRunDateVisibility);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void NeverRunStringVisibility_GivenSelectedTestIsNew_ShouldHaveDateVisibilityVisible()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            Assert.IsNotNull(testFrameworkViewModel.SelectedServiceTest);
            //---------------Test Result -----------------------
            Assert.IsTrue(testFrameworkViewModel.SelectedServiceTest.NeverRunStringVisibility);
        }

        [TestMethod]
        [Timeout(1000)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemDecision_ShouldHaveAddServiceTestStepShouldHaveArmOptions()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            const string expressionText = "{\"TheStack\":[{\"Col1\":\"[[val]]\",\"Col2\":\"5\",\"Col3\":\"\",\"Cols1\":null,\"Cols2\":null,\"Cols3\":null,\"PopulatedColumnCount\":2,\"EvaluationFn\":\"IsEqual\"}],\"TotalDecisions\":1,\"ModelName\":\"Dev2DecisionStack\",\"Mode\":\"AND\",\"TrueArmText\":\"True Path\",\"FalseArmText\":\"False Path\",\"DisplayText\":\"\"}";
            var ser = new Dev2JsonSerializer();
            var stack = ser.Deserialize<Dev2DecisionStack>(expressionText);
            dsfDecision.Conditions = stack;
            var modelItem = ModelItemUtils.CreateModelItem(dsfDecision);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(dsfDecision.GetType().Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
            var serviceTestOutput = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[0] as ServiceTestOutput;
            Assert.AreEqual(2, serviceTestOutput.OptionsForValue.Count);
            Assert.IsTrue(serviceTestOutput.HasOptionsForValue);
            Assert.AreEqual("True Path", serviceTestOutput.OptionsForValue[0]);
            Assert.AreEqual("False Path", serviceTestOutput.OptionsForValue[1]);
            Assert.AreEqual(GlobalConstants.ArmResultText, serviceTestOutput.Variable);
        }

        [TestMethod]
        [Timeout(1000)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenDebugStateDecision_ShouldHaveAddServiceTestStepShouldHaveArmOptions()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            const string expressionText = "{\"TheStack\":[{\"Col1\":\"[[val]]\",\"Col2\":\"5\",\"Col3\":\"\",\"Cols1\":null,\"Cols2\":null,\"Cols3\":null,\"PopulatedColumnCount\":2,\"EvaluationFn\":\"IsEqual\"}],\"TotalDecisions\":1,\"ModelName\":\"Dev2DecisionStack\",\"Mode\":\"AND\",\"TrueArmText\":\"True Path\",\"FalseArmText\":\"False Path\",\"DisplayText\":\"\"}";
            var ser = new Dev2JsonSerializer();
            var stack = ser.Deserialize<Dev2DecisionStack>(expressionText);
            dsfDecision.Conditions = stack;
            var mock = new Mock<IDSFDataObject>();
            mock.Setup(o => o.Environment.AddError(It.IsAny<string>()));
            var dev2Activity = dsfDecision.Execute(mock.Object, 0);
            var modelItem = ModelItemUtils.CreateModelItem(dsfDecision);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, new NewTestFromDebugMessage(), null);

            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(dsfDecision.GetType().Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
            var serviceTestOutput = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[0] as ServiceTestOutput;
            Assert.AreEqual(2, serviceTestOutput.OptionsForValue.Count);
            Assert.IsTrue(serviceTestOutput.HasOptionsForValue);
            Assert.AreEqual("True Path", serviceTestOutput.OptionsForValue[0]);
            Assert.AreEqual("False Path", serviceTestOutput.OptionsForValue[1]);
            Assert.AreEqual(GlobalConstants.ArmResultText, serviceTestOutput.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemFlowDecision_ShouldHaveAddServiceTestStepShouldHaveArmOptions()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new FlowDecision();
            var activity = new DsfFlowDecisionActivity();
            dsfDecision.Condition = activity;
            var decisionUniqueId = Guid.NewGuid();
            activity.UniqueID = decisionUniqueId.ToString();
            const string expressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:![[Name]]!,!Col2!:!!,!Col3!:!!,!PopulatedColumnCount!:1,!EvaluationFn!:!IsNotEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!Name Input!,!FalseArmText!:!Blank Input!,!DisplayText!:!If [[Name]] &lt;&gt; (Not Equal) !}\",AmbientDataList)";
            activity.ExpressionText = expressionText;
            var modelItem = ModelItemUtils.CreateModelItem(dsfDecision);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(typeof(DsfDecision).Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
            var serviceTestOutput = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[0] as ServiceTestOutput;
            Assert.AreEqual(2, serviceTestOutput.OptionsForValue.Count);
            Assert.IsTrue(serviceTestOutput.HasOptionsForValue);
            Assert.AreEqual("Name Input", serviceTestOutput.OptionsForValue[0]);
            Assert.AreEqual("Blank Input", serviceTestOutput.OptionsForValue[1]);
            Assert.AreEqual(GlobalConstants.ArmResultText, serviceTestOutput.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemSwitch_NoDefault_ShouldHaveAddServiceTestStepShouldHaveCaseOptions()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfSwitch = new DsfSwitch();
            var uniqueId = Guid.NewGuid();
            dsfSwitch.UniqueID = uniqueId.ToString();
            dsfSwitch.Switches = new Dictionary<string, IDev2Activity> { { "Case1", new Mock<IDev2Activity>().Object }, { "Case2", new Mock<IDev2Activity>().Object }, { "Case3", new Mock<IDev2Activity>().Object } };
            var modelItem = ModelItemUtils.CreateModelItem(dsfSwitch);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(dsfSwitch.GetType().Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
            var serviceTestOutput = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[0] as ServiceTestOutput;
            Assert.AreEqual(3, serviceTestOutput.OptionsForValue.Count);
            Assert.IsTrue(serviceTestOutput.HasOptionsForValue);
            Assert.AreEqual("Case1", serviceTestOutput.OptionsForValue[0]);
            Assert.AreEqual("Case2", serviceTestOutput.OptionsForValue[1]);
            Assert.AreEqual("Case3", serviceTestOutput.OptionsForValue[2]);
            Assert.AreEqual(GlobalConstants.ArmResultText, serviceTestOutput.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemSwitch_Default_ShouldHaveAddServiceTestStepShouldHaveCaseOptionsWithDefaultAtTop()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfSwitch = new DsfSwitch();
            var uniqueId = Guid.NewGuid();
            dsfSwitch.UniqueID = uniqueId.ToString();
            dsfSwitch.Switches = new Dictionary<string, IDev2Activity> { { "Case1", new Mock<IDev2Activity>().Object }, { "Case2", new Mock<IDev2Activity>().Object }, { "Case3", new Mock<IDev2Activity>().Object } };
            dsfSwitch.Default = new List<IDev2Activity> { new Mock<IDev2Activity>().Object };
            var modelItem = ModelItemUtils.CreateModelItem(dsfSwitch);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(dsfSwitch.GetType().Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
            var serviceTestOutput = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[0] as ServiceTestOutput;
            Assert.AreEqual(4, serviceTestOutput.OptionsForValue.Count);
            Assert.IsTrue(serviceTestOutput.HasOptionsForValue);
            Assert.AreEqual("Default", serviceTestOutput.OptionsForValue[0]);
            Assert.AreEqual("Case1", serviceTestOutput.OptionsForValue[1]);
            Assert.AreEqual("Case2", serviceTestOutput.OptionsForValue[2]);
            Assert.AreEqual("Case3", serviceTestOutput.OptionsForValue[3]);
            Assert.AreEqual(GlobalConstants.ArmResultText, serviceTestOutput.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemFlowSwitch_Default_ShouldHaveAddServiceTestStepShouldHaveCaseOptionsWithDefaultAtTop()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfSwitch = new FlowSwitch<string>();
            var uniqueId = Guid.NewGuid();
            var activity = new DsfFlowSwitchActivity { UniqueID = uniqueId.ToString() };
            dsfSwitch.Expression = activity;
            dsfSwitch.Cases.Add("Case1", new FlowStep());
            dsfSwitch.Cases.Add("Case2", new FlowStep());
            dsfSwitch.Cases.Add("Case3", new FlowStep());
            dsfSwitch.Default = new FlowStep();
            var modelItem = ModelItemUtils.CreateModelItem(dsfSwitch);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(typeof(DsfSwitch).Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
            var serviceTestOutput = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[0] as ServiceTestOutput;
            Assert.AreEqual(4, serviceTestOutput.OptionsForValue.Count);
            Assert.IsTrue(serviceTestOutput.HasOptionsForValue);
            Assert.AreEqual("Default", serviceTestOutput.OptionsForValue[0]);
            Assert.AreEqual("Case1", serviceTestOutput.OptionsForValue[1]);
            Assert.AreEqual("Case2", serviceTestOutput.OptionsForValue[2]);
            Assert.AreEqual("Case3", serviceTestOutput.OptionsForValue[3]);
            Assert.AreEqual(GlobalConstants.ArmResultText, serviceTestOutput.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void ItemSelected_GivenSelectedItemSelectAndApply_Default_ShouldHaveAddServiceTestStepShouldHaveCaseOptionsWithDefaultAtTop()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var uniqueId = Guid.NewGuid();
            var activity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId.ToString() };
           
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Mock, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(typeof(DsfSelectAndApplyActivity).Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
      
           
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemMultiAssign_ShouldHaveAddServiceTestStepShouldHaveOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var assignActivity = new DsfMultiAssignActivity();
            var uniqueId = Guid.NewGuid();
            assignActivity.UniqueID = uniqueId.ToString();
            assignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[Var1]]", "bob", 1), new ActivityDTO("[[Var2]]", "mary", 2), new ActivityDTO("[[name]]", "dora", 3) };
            var modelItem = ModelItemUtils.CreateModelItem(assignActivity);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            var mock = new Mock<IEnvironmentConnection>();
            mock.SetupAllProperties();
            mockResourceModel.Setup(model => model.Environment.Connection).Returns(mock.Object);
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(assignActivity.GetType().Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(3, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
            var serviceTestOutput1 = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[0] as ServiceTestOutput;
            var serviceTestOutput2 = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[1] as ServiceTestOutput;
            var serviceTestOutput3 = testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs[2] as ServiceTestOutput;
            Assert.IsFalse(serviceTestOutput1.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput2.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput3.HasOptionsForValue);
            Assert.AreEqual("[[Var1]]", serviceTestOutput1.Variable);
            Assert.AreEqual("[[Var2]]", serviceTestOutput2.Variable);
            Assert.AreEqual("[[name]]", serviceTestOutput3.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void ItemSelected_GivenSelectedItemEnhancedDotNetDll_ShouldHaveAddServiceTestStepShouldHaveOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var uniqueId = Guid.NewGuid();
            dotNetDllActivity.UniqueID = uniqueId.ToString();
            dotNetDllActivity.Constructor = new Mock<IPluginConstructor>().Object;
            dotNetDllActivity.Namespace = new Mock<INamespaceItem>().Object;
            var modelItem = ModelItemUtils.CreateModelItem(dotNetDllActivity);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            var mock = new Mock<IEnvironmentConnection>();
            mock.SetupAllProperties();
            mockResourceModel.Setup(model => model.Environment.Connection).Returns(mock.Object);
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Mock, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            Assert.AreEqual(dotNetDllActivity.GetType().Name, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].StepOutputs.Count);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemForEach_ShouldHaveAddServiceTestStepShouldHaveOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();

            var assignActivity = new DsfMultiAssignActivity();
            var uniqueId = Guid.NewGuid();
            assignActivity.UniqueID = uniqueId.ToString();
            assignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[Var1]]", "bob", 1), new ActivityDTO("[[Var2]]", "mary", 2), new ActivityDTO("[[name]]", "dora", 3) };

            var forEach = new DsfForEachActivity();
            var forEachDataFunc = new ActivityFunc<string, bool>();
            forEachDataFunc.Handler = assignActivity;
            forEach.DataFunc = forEachDataFunc;
            var forEachId = Guid.NewGuid();
            forEach.UniqueID = forEachId.ToString();

            var modelItem = ModelItemUtils.CreateModelItem(forEach);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            var serviceTestStep = testFrameworkViewModel.SelectedServiceTest.TestSteps[0];
            Assert.AreEqual(StepType.Mock, serviceTestStep.Type);
            Assert.AreEqual(forEach.GetType().Name, serviceTestStep.ActivityType);
            Assert.AreEqual(forEachId, serviceTestStep.ActivityID);

            Assert.AreEqual(1, serviceTestStep.Children.Count);
            var childItem = serviceTestStep.Children[0];
            Assert.AreEqual(StepType.Mock, childItem.Type);
            Assert.AreEqual(assignActivity.GetType().Name, childItem.ActivityType);
            Assert.AreEqual(uniqueId, childItem.ActivityID);
            Assert.AreEqual(serviceTestStep, childItem.Parent);

            Assert.AreEqual(3, childItem.StepOutputs.Count);
            var serviceTestOutput1 = childItem.StepOutputs[0] as ServiceTestOutput;
            var serviceTestOutput2 = childItem.StepOutputs[1] as ServiceTestOutput;
            var serviceTestOutput3 = childItem.StepOutputs[2] as ServiceTestOutput;
            Assert.IsFalse(serviceTestOutput1.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput2.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput3.HasOptionsForValue);
            Assert.AreEqual("[[Var1]]", serviceTestOutput1.Variable);
            Assert.AreEqual("[[Var2]]", serviceTestOutput2.Variable);
            Assert.AreEqual("[[name]]", serviceTestOutput3.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemForEachWithSequence_ShouldHaveAddServiceTestStepShouldHaveOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();

            var assignActivity = new DsfMultiAssignActivity();
            var uniqueId = Guid.NewGuid();
            assignActivity.UniqueID = uniqueId.ToString();
            assignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[Var1]]", "bob", 1), new ActivityDTO("[[Var2]]", "mary", 2), new ActivityDTO("[[name]]", "dora", 3) };

            var sequenceActivity = new DsfSequenceActivity();
            sequenceActivity.Activities = new Collection<Activity> { assignActivity };
            var seqId = Guid.NewGuid();
            sequenceActivity.UniqueID = seqId.ToString();

            var forEach = new DsfForEachActivity();
            var forEachDataFunc = new ActivityFunc<string, bool>();
            forEachDataFunc.Handler = sequenceActivity;
            forEach.DataFunc = forEachDataFunc;
            var forEachId = Guid.NewGuid();
            forEach.UniqueID = forEachId.ToString();

            var modelItem = ModelItemUtils.CreateModelItem(forEach);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            var serviceTestStep = testFrameworkViewModel.SelectedServiceTest.TestSteps[0];
            Assert.AreEqual(StepType.Mock, serviceTestStep.Type);
            Assert.AreEqual(forEach.GetType().Name, serviceTestStep.ActivityType);
            Assert.AreEqual(forEachId, serviceTestStep.ActivityID);

            Assert.AreEqual(1, serviceTestStep.Children.Count);
            var childItem = serviceTestStep.Children[0];
            Assert.AreEqual(StepType.Mock, childItem.Type);
            Assert.AreEqual(sequenceActivity.GetType().Name, childItem.ActivityType);
            Assert.AreEqual(seqId, childItem.ActivityID);
            Assert.AreEqual(serviceTestStep, childItem.Parent);

            Assert.AreEqual(1, childItem.Children.Count);
            var seqChildItem = childItem.Children[0];
            Assert.AreEqual(StepType.Mock, seqChildItem.Type);
            Assert.AreEqual(assignActivity.GetType().Name, seqChildItem.ActivityType);
            Assert.AreEqual(uniqueId, seqChildItem.ActivityID);
            Assert.AreEqual(childItem, seqChildItem.Parent);

            Assert.AreEqual(3, seqChildItem.StepOutputs.Count);
            var serviceTestOutput1 = seqChildItem.StepOutputs[0] as ServiceTestOutput;
            var serviceTestOutput2 = seqChildItem.StepOutputs[1] as ServiceTestOutput;
            var serviceTestOutput3 = seqChildItem.StepOutputs[2] as ServiceTestOutput;
            Assert.IsFalse(serviceTestOutput1.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput2.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput3.HasOptionsForValue);
            Assert.AreEqual("[[Var1]]", serviceTestOutput1.Variable);
            Assert.AreEqual("[[Var2]]", serviceTestOutput2.Variable);
            Assert.AreEqual("[[name]]", serviceTestOutput3.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemSequenceWithForEach_ShouldHaveAddServiceTestStepShouldHaveOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();

            var assignActivity = new DsfMultiAssignActivity();
            var uniqueId = Guid.NewGuid();
            assignActivity.UniqueID = uniqueId.ToString();
            assignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[Var1]]", "bob", 1), new ActivityDTO("[[Var2]]", "mary", 2), new ActivityDTO("[[name]]", "dora", 3) };

            var forEach = new DsfForEachActivity();
            var forEachDataFunc = new ActivityFunc<string, bool> { Handler = assignActivity };
            forEach.DataFunc = forEachDataFunc;
            var forEachId = Guid.NewGuid();
            forEach.UniqueID = forEachId.ToString();


            var sequenceActivity = new DsfSequenceActivity { Activities = new Collection<Activity> { forEach } };
            var seqId = Guid.NewGuid();
            sequenceActivity.UniqueID = seqId.ToString();

            var modelItem = ModelItemUtils.CreateModelItem(sequenceActivity);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            var serviceTestStep = testFrameworkViewModel.SelectedServiceTest.TestSteps[0];
            Assert.AreEqual(StepType.Mock, serviceTestStep.Type);
            Assert.AreEqual(sequenceActivity.GetType().Name, serviceTestStep.ActivityType);
            Assert.AreEqual(seqId, serviceTestStep.ActivityID);

            Assert.AreEqual(1, serviceTestStep.Children.Count);
            var childItem = serviceTestStep.Children[0];
            Assert.AreEqual(StepType.Mock, childItem.Type);
            Assert.AreEqual(forEach.GetType().Name, childItem.ActivityType);
            Assert.AreEqual(forEachId, childItem.ActivityID);
            Assert.AreEqual(serviceTestStep, childItem.Parent);

            Assert.AreEqual(1, childItem.Children.Count);
            var forEachChildItem = childItem.Children[0];
            Assert.AreEqual(assignActivity.GetType().Name, forEachChildItem.ActivityType);
            Assert.AreEqual(uniqueId, forEachChildItem.ActivityID);
            Assert.AreEqual(childItem, forEachChildItem.Parent);

            Assert.AreEqual(3, forEachChildItem.StepOutputs.Count);
            var serviceTestOutput1 = forEachChildItem.StepOutputs[0] as ServiceTestOutput;
            var serviceTestOutput2 = forEachChildItem.StepOutputs[1] as ServiceTestOutput;
            var serviceTestOutput3 = forEachChildItem.StepOutputs[2] as ServiceTestOutput;
            Assert.IsFalse(serviceTestOutput1.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput2.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput3.HasOptionsForValue);
            Assert.AreEqual("[[Var1]]", serviceTestOutput1.Variable);
            Assert.AreEqual("[[Var2]]", serviceTestOutput2.Variable);
            Assert.AreEqual("[[name]]", serviceTestOutput3.Variable);
        }


        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_DeleteTestStep_ShouldHaveAddServiceTestStepShouldHaveOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();

            var assignActivity = new DsfMultiAssignActivity();
            var uniqueId = Guid.NewGuid();
            assignActivity.UniqueID = uniqueId.ToString();
            assignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[Var1]]", "bob", 1), new ActivityDTO("[[Var2]]", "mary", 2), new ActivityDTO("[[name]]", "dora", 3) };

            var forEach = new DsfForEachActivity();
            var forEachDataFunc = new ActivityFunc<string, bool> { Handler = assignActivity };
            forEach.DataFunc = forEachDataFunc;
            var forEachId = Guid.NewGuid();
            forEach.UniqueID = forEachId.ToString();


            var sequenceActivity = new DsfSequenceActivity { Activities = new Collection<Activity> { forEach } };
            var seqId = Guid.NewGuid();
            sequenceActivity.UniqueID = seqId.ToString();

            var modelItem = ModelItemUtils.CreateModelItem(sequenceActivity);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Assert Precondition----------------      
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            var serviceTestStep = testFrameworkViewModel.SelectedServiceTest.TestSteps[0];
            Assert.AreEqual(StepType.Mock, serviceTestStep.Type);
            Assert.AreEqual(sequenceActivity.GetType().Name, serviceTestStep.ActivityType);
            Assert.AreEqual(seqId, serviceTestStep.ActivityID);

            Assert.AreEqual(1, serviceTestStep.Children.Count);
            var childItem = serviceTestStep.Children[0];
            //---------------Execute Test ----------------------
            testFrameworkViewModel.DeleteTestStepCommand.Execute(childItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(0, serviceTestStep.Children.Count);

        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemSequence_ShouldHaveAddServiceTestStepShouldHaveOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();

            var assignActivity = new DsfMultiAssignActivity();
            var uniqueId = Guid.NewGuid();
            assignActivity.UniqueID = uniqueId.ToString();
            assignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[Var1]]", "bob", 1), new ActivityDTO("[[Var2]]", "mary", 2), new ActivityDTO("[[name]]", "dora", 3) };

            var sequenceActivity = new DsfSequenceActivity();
            sequenceActivity.Activities = new Collection<Activity> { assignActivity };
            var seqId = Guid.NewGuid();
            sequenceActivity.UniqueID = seqId.ToString();

            var modelItem = ModelItemUtils.CreateModelItem(sequenceActivity);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            var serviceTestStep = testFrameworkViewModel.SelectedServiceTest.TestSteps[0];
            Assert.AreEqual(StepType.Mock, serviceTestStep.Type);
            Assert.AreEqual(sequenceActivity.GetType().Name, serviceTestStep.ActivityType);
            Assert.AreEqual(seqId, serviceTestStep.ActivityID);

            Assert.AreEqual(1, serviceTestStep.Children.Count);
            var childItem = serviceTestStep.Children[0];
            Assert.AreEqual(StepType.Mock, childItem.Type);
            Assert.AreEqual(assignActivity.GetType().Name, childItem.ActivityType);
            Assert.AreEqual(uniqueId, childItem.ActivityID);
            Assert.AreEqual(serviceTestStep, childItem.Parent);

            Assert.AreEqual(3, childItem.StepOutputs.Count);
            var serviceTestOutput1 = childItem.StepOutputs[0] as ServiceTestOutput;
            var serviceTestOutput2 = childItem.StepOutputs[1] as ServiceTestOutput;
            var serviceTestOutput3 = childItem.StepOutputs[2] as ServiceTestOutput;
            Assert.IsFalse(serviceTestOutput1.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput2.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput3.HasOptionsForValue);
            Assert.AreEqual("[[Var1]]", serviceTestOutput1.Variable);
            Assert.AreEqual("[[Var2]]", serviceTestOutput2.Variable);
            Assert.AreEqual("[[name]]", serviceTestOutput3.Variable);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        public void ItemSelected_GivenSelectedItemSequenceWithSequence_ShouldHaveAddServiceTestStepShouldHaveOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();

            var assignActivity = new DsfMultiAssignActivity();
            var uniqueId = Guid.NewGuid();
            assignActivity.UniqueID = uniqueId.ToString();
            assignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[Var1]]", "bob", 1), new ActivityDTO("[[Var2]]", "mary", 2), new ActivityDTO("[[name]]", "dora", 3) };

            var dsfSequenceActivity = new DsfSequenceActivity { Activities = new Collection<Activity> { assignActivity } };
            var dsfSeqId = Guid.NewGuid();
            dsfSequenceActivity.UniqueID = dsfSeqId.ToString();


            var sequenceActivity = new DsfSequenceActivity { Activities = new Collection<Activity> { dsfSequenceActivity } };
            var seqId = Guid.NewGuid();
            sequenceActivity.UniqueID = seqId.ToString();

            var modelItem = ModelItemUtils.CreateModelItem(sequenceActivity);
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "Test 2", NameForDisplay = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            mockWorkflowDesignerViewModel.Object.ItemSelectedAction(modelItem);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            var serviceTestStep = testFrameworkViewModel.SelectedServiceTest.TestSteps[0];
            Assert.AreEqual(StepType.Mock, serviceTestStep.Type);
            Assert.AreEqual(sequenceActivity.GetType().Name, serviceTestStep.ActivityType);
            Assert.AreEqual(seqId, serviceTestStep.ActivityID);

            Assert.AreEqual(1, serviceTestStep.Children.Count);
            var childItem = serviceTestStep.Children[0];
            Assert.AreEqual(StepType.Mock, childItem.Type);
            Assert.AreEqual(dsfSequenceActivity.GetType().Name, childItem.ActivityType);
            Assert.AreEqual(dsfSeqId, childItem.ActivityID);
            Assert.AreEqual(serviceTestStep, childItem.Parent);

            Assert.AreEqual(1, childItem.Children.Count);
            var forEachChildItem = childItem.Children[0];
            Assert.AreEqual(assignActivity.GetType().Name, forEachChildItem.ActivityType);
            Assert.AreEqual(uniqueId, forEachChildItem.ActivityID);
            Assert.AreEqual(childItem, forEachChildItem.Parent);

            Assert.AreEqual(3, forEachChildItem.StepOutputs.Count);
            var serviceTestOutput1 = forEachChildItem.StepOutputs[0] as ServiceTestOutput;
            var serviceTestOutput2 = forEachChildItem.StepOutputs[1] as ServiceTestOutput;
            var serviceTestOutput3 = forEachChildItem.StepOutputs[2] as ServiceTestOutput;
            Assert.IsFalse(serviceTestOutput1.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput2.HasOptionsForValue);
            Assert.IsFalse(serviceTestOutput3.HasOptionsForValue);
            Assert.AreEqual("[[Var1]]", serviceTestOutput1.Variable);
            Assert.AreEqual("[[Var2]]", serviceTestOutput2.Variable);
            Assert.AreEqual("[[name]]", serviceTestOutput3.Variable);
        }


        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_GivenServerIsNotLocalHost_ShouldAppendServerIntoDisplayName()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            moqModel.Setup(model => model.Environment.IsConnected).Returns(true);
            moqModel.Setup(model => model.Environment.IsLocalHost).Returns(false);
            moqModel.Setup(model => model.Environment.Connection.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            moqModel.Setup(model => model.Category).Returns("My WF");
            moqModel.Setup(model => model.ResourceName).Returns("My WF");
            const string serverName = "GenDev";
            moqModel.Setup(model => model.Environment.Name).Returns(serverName).Verifiable();

            var testFrameworkViewModel = new ServiceTestViewModel(moqModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            //---------------Assert Precondition----------------
            var fieldInfo = typeof(ServiceTestViewModel).GetField("_serverName", BindingFlags.Instance | BindingFlags.NonPublic);

            
            var server = fieldInfo.GetValue(testFrameworkViewModel).ToString();
            Assert.AreEqual(" - GenDev", server);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("My WF - Tests - GenDev", testFrameworkViewModel.DisplayName);
        }
        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_GivenServerIsLocalHost_ShouldAppendNotServerIntoDisplayName()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            moqModel.Setup(model => model.Environment.IsConnected).Returns(true);
            moqModel.Setup(model => model.Environment.IsLocalHost).Returns(true);
            moqModel.Setup(model => model.Environment.Connection.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            moqModel.Setup(model => model.Category).Returns("My WF");
            moqModel.Setup(model => model.ResourceName).Returns("My WF");
            const string serverName = "localhost";
            moqModel.Setup(model => model.Environment.Name).Returns(serverName).Verifiable();

            var testFrameworkViewModel = new ServiceTestViewModel(moqModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            //---------------Assert Precondition----------------
            var fieldInfo = typeof(ServiceTestViewModel).GetField("_serverName", BindingFlags.Instance | BindingFlags.NonPublic);

            
            var server = fieldInfo.GetValue(testFrameworkViewModel).ToString();
            Assert.AreEqual("", server);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("My WF - Tests", testFrameworkViewModel.DisplayName);
        }
        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenNoSelectecTest_ShouldBeValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid()) { TestName = "NameOne", NameForDisplay = "NameOne" };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = null;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { });
            //---------------Test Result -----------------------
            Assert.AreEqual(true, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_NullOutputs_ShouldSet_StepOutputs()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            var serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", new ObservableCollection<IServiceTestOutput>(), StepType.Mock);
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { null, serviceTestStep });

            //---------------Test Result -----------------------
            Assert.AreEqual(1, serviceTestStep.StepOutputs.Count);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_SetOutputs_ShouldReturn()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            const string datalistFragment = @"<DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recordSet Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    </recordSet>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>";

            var resourceModel = CreateResourceModel();
            resourceModel.DataList = datalistFragment;
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { null });

            //---------------Test Result -----------------------
            //NOTE: Purely for cover, meant to only return
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_SetOutputs_MoreLink_ShouldSetValue()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            const string datalistFragment = @"<DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recordSet Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    </recordSet>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>";

            var resourceModel = CreateResourceModel();
            resourceModel.DataList = datalistFragment;
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition-`---------------
            Assert.IsNotNull(methodInfo);

            var resId = Guid.NewGuid();
            var serverId = Guid.NewGuid();

            var debugItem = new DebugItem();
            var debugItemResult = new DebugItemResult
            {
                Variable = "[[Name]]",
                Value = "Bob",
                MoreLink = "MoreLink",
                GroupName = "GroupName",
                GroupIndex = 1
            };
            debugItem.ResultsList.Add(debugItemResult);
            var debugOutputs = new List<IDebugItem>();
            debugOutputs.Add(debugItem);

            var debugState = new Mock<IDebugState>();
            debugState.Setup(c => c.OriginatingResourceID).Returns(resId);
            debugState.Setup(c => c.ServerID).Returns(serverId);
            debugState.Setup(c => c.WorkspaceID).Returns(Guid.Empty);
            debugState.Setup(c => c.Outputs).Returns(debugOutputs);

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] 
            {
                debugState.Object
            });
            var outputs = testFrameworkViewModel.SelectedServiceTest.Outputs;

            //---------------Test Result -----------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("Contains", outputs[0].AssertOp);
            Assert.AreEqual("Name", outputs[0].Variable);
            Assert.AreEqual("Bob", outputs[0].Value);
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_SetOutputs_DsfEnhancedDotNetDllActivity_ShouldSetValue()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            const string datalistFragment = @"<DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recordSet Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    </recordSet>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>";

            var resourceModel = CreateResourceModel();
            resourceModel.DataList = datalistFragment;
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition-`---------------
            Assert.IsNotNull(methodInfo);

            var mockWebClient = new Mock<IWarewolfWebClient>();
            mockWebClient.Setup(web => web.DownloadString("MoreLink")).Returns("SomeAddress");
            testFrameworkViewModel.WebClient = mockWebClient.Object;

            var resId = Guid.NewGuid();
            var serverId = Guid.NewGuid();

            var debugItem = new DebugItem();
            var debugItemResult = new DebugItemResult
            {
                Variable = "[[Name]]",
                Value = "Bob",
                MoreLink = "MoreLink",
                GroupName = "GroupName",
                GroupIndex = 1
            };
            debugItem.ResultsList.Add(debugItemResult);
            var debugOutputs = new List<IDebugItem>();
            debugOutputs.Add(debugItem);

            var debugState = new Mock<IDebugState>();
            debugState.Setup(c => c.OriginatingResourceID).Returns(resId);
            debugState.Setup(c => c.ServerID).Returns(serverId);
            debugState.Setup(c => c.WorkspaceID).Returns(Guid.Empty);
            debugState.Setup(c => c.Outputs).Returns(debugOutputs);
            debugState.Setup(c => c.ActualType).Returns(typeof(DsfEnhancedDotNetDllActivity).Name);

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[]
            {
                debugState.Object
            });
            var outputs = testFrameworkViewModel.SelectedServiceTest.Outputs;

            //---------------Test Result -----------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("=", outputs[0].AssertOp);
            Assert.AreEqual("Name", outputs[0].Variable);
            Assert.AreEqual("SomeAddress", outputs[0].Value);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_AddSequence_ShouldReturn()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            var resourceModel = CreateResourceModel();
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddSequence", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition-`---------------
            Assert.IsNotNull(methodInfo);

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[]
            {
                null, null, null
            });

            //---------------Test Result -----------------------
            //NOTE: Purely for cover, meant to only return
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_AddSequence_ShouldCall_AddMissingChild()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            var resourceModel = CreateResourceModel();
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddSequence", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition-`---------------
            Assert.IsNotNull(methodInfo);

            var uniqueId = Guid.NewGuid();
            var sequence = new DsfSequenceActivity() { UniqueID = uniqueId.ToString(), DisplayName = "a" };
            var serviceTestStep = new ServiceTestStep(uniqueId, "Sequence", new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput("[[p]]", "b", "", "") }, StepType.Mock);
            var serviceTestSteps = new ObservableCollection<IServiceTestStep>();
            serviceTestSteps.Add(serviceTestStep);

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[]
            {
                sequence, serviceTestStep, serviceTestSteps
            });

            //---------------Test Result -----------------------
            //NOTE: Purely for cover, meant to only return
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_AddEnhancedDotNetDll_ShouldReturn()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            var resourceModel = CreateResourceModel();
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddEnhancedDotNetDll", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition-`---------------
            Assert.IsNotNull(methodInfo);

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[]
            {
                null, null, null
            });

            //---------------Test Result -----------------------
            //NOTE: Purely for cover, meant to only return
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_AddEnhancedDotNetDll_ShouldCall_AddEnhancedDotNetDllMethod()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            var resourceModel = CreateResourceModel();
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddEnhancedDotNetDll", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition-`---------------
            Assert.IsNotNull(methodInfo);

            var uniqueId = Guid.NewGuid();
            var pluginAction = new PluginAction
            {
                IsVoid = false
            };
            var plugins = new List<IPluginAction>
            {
                pluginAction
            };
            var sequence = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId.ToString(),
                DisplayName = "a",
                MethodsToRun = plugins,
                Constructor = new PluginConstructor { ConstructorName = "Constructor" }
            };
            var serviceTestStep = new ServiceTestStep(uniqueId, "EnhancedDotNetDllActivity", new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput("[[p]]", "b", "", "") }, StepType.Mock);
            var serviceTestSteps = new ObservableCollection<IServiceTestStep>();

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[]
            {
                sequence, serviceTestStep, serviceTestSteps
            });

            //---------------Test Result -----------------------
            //NOTE: Purely for cover, meant to only return
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_ProcessActivity_ShouldAddTestSteps()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            var resourceModel = CreateResourceModel();
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ProcessActivity", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition-`---------------
            Assert.IsNotNull(methodInfo);

            var uniqueId = Guid.NewGuid();
            var sequence = new DsfActivity { UniqueID = uniqueId.ToString(), DisplayName = "a" };
            var modelItem = ModelItemUtils.CreateModelItem(sequence);

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[]
            {
                modelItem
            });
            var testSteps = testFrameworkViewModel.SelectedServiceTest.TestSteps;

            //---------------Test Result -----------------------
            Assert.AreEqual(1, testSteps.Count);
            Assert.AreEqual("DsfActivity", testSteps[0].ActivityType);
            Assert.AreEqual("a", testSteps[0].StepDescription);
            Assert.AreEqual(1, testSteps[0].StepOutputs.Count);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        public void AddOutputs_ProcessActivity_ParentNotNull_ShouldAddTestSteps()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            var resourceModel = CreateResourceModel();
            var testFrameworkViewModel = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, popupController.Object, null, null);
            var testModel = new ServiceTestModel(Guid.NewGuid())
            {
                TestName = "NameOne",
                NameForDisplay = "NameOne"
            };
            testFrameworkViewModel.Tests = new ObservableCollection<IServiceTestModel> { testModel };
            testFrameworkViewModel.SelectedServiceTest = testModel;
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ProcessActivity", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition-`---------------
            Assert.IsNotNull(methodInfo);

            var seqUniqueId = Guid.NewGuid();
            var actUniqueId = Guid.NewGuid();
            var sequence = new DsfSequenceActivity { UniqueID = seqUniqueId.ToString(), DisplayName = "Sequence" };
            var activity = new DsfActivity { UniqueID = actUniqueId.ToString(), DisplayName = "Activity", ParentServiceID = seqUniqueId.ToString() };
            var modelItem = ModelItemUtils.CreateModelItem(sequence, activity);

            var parentTestStep = new ServiceTestStep(seqUniqueId, "Sequence", new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput("[[p]]", "b", "", "") }, StepType.Mock);
            var childTestStep = new ServiceTestStep(seqUniqueId, "Activity", new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput("[[p]]", "b", "", "") }, StepType.Mock)
            {
                Parent = parentTestStep
            };
            parentTestStep.Children.Add(childTestStep);
            testFrameworkViewModel.SelectedServiceTest.TestSteps.Add(parentTestStep);

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[]
             {
                modelItem
             });
            var testSteps = testFrameworkViewModel.SelectedServiceTest.TestSteps;

            //---------------Test Result -----------------------
            Assert.AreEqual(1, testSteps.Count);
            Assert.AreEqual("Sequence", testSteps[0].ActivityType);
            Assert.AreEqual("Sequence", testSteps[0].StepDescription);
            Assert.AreEqual(1, testSteps[0].StepOutputs.Count);
        }

        IContextualResourceModel CreateResourceModelWithSingleScalarInput()
        {
            var resourceModel = CreateResourceModel();
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.Add(new ScalarItemModel("a", enDev2ColumnArgumentDirection.Input));
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }

        IContextualResourceModel CreateResourceModelWithSingleScalarOutput()
        {
            var resourceModel = CreateResourceModel();
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }

        Mock<IContextualResourceModel> CreateResourceModelWithSingleScalarOutputMock()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return moqModel;
        }

        IContextualResourceModel CreateMockResourceModelWithSingleScalarOutput()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            moqModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            moqModel.Setup(model => model.Environment.ResourceRepository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>())).Returns(new TestSaveResult() { Result = SaveResult.Success }).Verifiable();
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true).Verifiable();
            moqModel.Setup(model => model.Environment.IsConnected).Returns(true).Verifiable();
            return moqModel.Object;
        }

        Mock<IContextualResourceModel> CreateMockResourceModel()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            var resourceModel = moqModel.Object;

            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return moqModel;
        }
    }
}
