using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable ObjectCreationAsStatement

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServiceTestViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFrameworkViewModel_Constructor_NullResourceModel_ShouldThrowException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new ServiceTestViewModel(null, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShoDuplicatePopup_GivenIsInvoked_ShouldShowCorrectMessage()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(controller => controller.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false)).Verifiable();
            CustomContainer.Register(mock.Object);
            var testVM = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testVM.ShowDuplicatePopup();
            //---------------Test Result -----------------------
            mock.Verify(controller => controller.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false), Times.Once);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_NotNullResourceModel_ShouldSetResourceModel()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var testVM = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testVM);
            Assert.IsNotNull(testVM.ResourceModel);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_NotNullResourceModel_ShouldSetDisplayNameIncludingResourceDisplayName()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.DisplayName).Returns("Workflow Name");
            //------------Execute Test---------------------------
            var testVM = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.AreEqual("My WF - Tests", testVM.DisplayName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void OnCreation_GivenIsNew_ShouldHaveRunAllTestsUrl()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("http://rsaklf/secure/My WF.tests", vm.RunAllTestsUrl);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDuplicateTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.DuplicateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DuplicateTestCommand.CanExecute(null));
        }
        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveStopTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.StopTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.StopTestCommand.CanExecute(null));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveCreateTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.CreateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.CreateTestCommand.CanExecute(null));
        }

        private IContextualResourceModel CreateResourceModel(bool isConnected = true)
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.Connection.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            moqModel.Setup(model => model.Category).Returns("My WF");
            moqModel.Setup(model => model.ResourceName).Returns("My WF");
            return moqModel.Object;
        }

        private IContextualResourceModel CreateResourceModelWithMoreSave()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            moqModel.Setup(model => model.Environment.ResourceRepository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>())).Returns(new TestSaveResult() {Result = SaveResult.Success});
            return moqModel.Object;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveModel()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.CreateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.CreateTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunAllTestsCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunAllTestsCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.RunAllTestsCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNewNotConnected_ShouldSetRunAllTestsCommandExecuteFalse()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IEventAggregator>();
            var vm = new ServiceTestViewModel(CreateResourceModel(false), new SynchronousAsyncWorker(), mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunAllTestsCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunAllTestsCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunSelectedTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.RunSelectedTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNewNotConnected_ShouldSetRunSelectedTestCommandCanExecuteFalse()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IEventAggregator>();
            var vm = new ServiceTestViewModel(CreateResourceModel(false), new SynchronousAsyncWorker(), mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunSelectedTestCommand.CanExecute(null));
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNewNotConnected_ShouldSetRunAllTestsInBrowserCommandCanExecuteFalse()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IEventAggregator>();
            var vm = new ServiceTestViewModel(CreateResourceModel(false), new SynchronousAsyncWorker(), mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunAllTestsInBrowserCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunAllTestsInBrowserCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunSelectedTestInBrowserCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestInBrowserCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunSelectedTestInBrowserCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Save_GivenThrowsNoException_ShouldMarkAllTestsAsNotDirty()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModelWithMoreSave(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            vm.Save();
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.Tests.All(model => !model.IsDirty));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_SelectedServiceTest")]
        public void TestFrameworkViewModel_SelectedServiceTest_CheckIsNull()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.SelectedServiceTest = null;
            //------------Assert Results-------------------------
            Assert.IsNull(testFrameworkViewModel.SelectedServiceTest);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddANewTest()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldShowError()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.PopupController);
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, null, false, true, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddANewTestWithDefaultName()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Executed_ShouldSetSelectedTestToNewlyCreatedTest()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
            Assert.AreEqual("http://rsaklf/secure/My WF.tests/Test 2", testFrameworkViewModel.SelectedServiceTest.RunSelectedTestUrl);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_SaveAsPublic_ShouldSetSelectedTestToPublic()
        {
            //------------Setup for test--------------------------
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
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

            var testFrameworkViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_Tests_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_SelectedTest_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_ErrorMessage_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddInputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarInput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddOutputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDeleteTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            Assert.IsNotNull(vm.DeleteTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DeleteTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenResourceModelIsConnectedAndTestIsDisabled_ShouldsetCanExecuteTrue()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenResourceModelIsNotConnectedAndTestIsDisabled_ShouldsetCanExecuteTrue()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(false), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Pieter Terblanche")]
        public void OnCreation_GivenIsDisabled_DeleteTestCommandShouldBeEnabled()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            vm.CreateTestCommand.Execute(null);
            Assert.IsNotNull(vm.DeleteTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DeleteTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void OnCreation_GivenIsEnabled_DeleteTestCommandShouldBeDisabled()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Save")]
        public void ServiceTestViewModel_Save_WithTests_ShouldCallSaveTestOnResourceModel()
        {
            //------------Setup for test--------------------------
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            con.Setup(connection => connection.IsConnected).Returns(true);
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()));
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(con.Object);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            var serviceTestViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            serviceTestViewModel.CreateTestCommand.Execute(null);
            //------------Execute Test---------------------------
            Assert.IsTrue(serviceTestViewModel.CanSave);
            serviceTestViewModel.Save();
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ServiceTestViewModel_Save")]
        public void ServiceTestViewModel_Save_DuplicateName_ShouldShowPopupError()
        {
            //------------Setup for test--------------------------
            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<IResourceModel>(), It.IsAny<List<IServiceTestModelTO>>()));
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            resourceModelMock.Setup(model => model.Environment.Connection.IsConnected).Returns(true);

            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            //------------Execute Test---------------------------
            var serviceTestViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            serviceTestViewModel.CreateTestCommand.Execute(null);
            Assert.IsTrue(serviceTestViewModel.CanSave);
            serviceTestViewModel.Save();

            serviceTestViewModel.CreateTestCommand.Execute(null);
            Assert.IsTrue(serviceTestViewModel.CanSave);
            serviceTestViewModel.Save();

            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestViewModel.PopupController);
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, null, false, true, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ServiceTestViewModel_Save")]
        public void ServiceTestViewModel_Save_Rename_ShouldRename()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var serviceTestViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Pieter Terblanche")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_IsDirty_IsFalse()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsFalse(testFrameworkViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldSetHasChangedTrue()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Pieter Terblanche")]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var mainViewModelMock = new Mock<IMainViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);
            const string helpText = "someText";

            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);

            //act
            testFrameworkViewModel.UpdateHelpDescriptor(helpText);

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText(helpText));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_IsDirty")]
        public void ServiceTestViewModel_IsDirty_WhenSetTrue_ShouldUpdateDisplayNameWithStar()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            Assert.AreEqual("My WF - Tests *", testFrameworkViewModel.DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_IsDirty")]
        public void ServiceTestViewModel_IsDirty_WhenSetTrueTwice_ShouldUpdateDisplayNameWithOneStarOnly()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            Assert.AreEqual("My WF - Tests *", testFrameworkViewModel.DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Tests")]
        public void ServiceTestViewModel_Tests_GetWhenNullTests_ShouldHaveDummyTest()
        {
            //------------Setup for test--------------------------
            var resourceMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockRepo = new Mock<IResourceRepository>();
            mockRepo.Setup(repository => repository.LoadResourceTests(It.IsAny<Guid>())).Returns((List<IServiceTestModelTO>)null);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockRepo.Object);
            resourceMock.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------
            Assert.AreEqual(1, tests.Count);
            Assert.AreEqual("Create a new test.", tests[0].TestName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Tests")]
        public void ServiceTestViewModel_Tests_GetWhenEmptyTests_ShouldHaveDummyTest()
        {
            //------------Setup for test--------------------------
            var resourceMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockRepo = new Mock<IResourceRepository>();
            mockRepo.Setup(repository => repository.LoadResourceTests(It.IsAny<Guid>())).Returns(new List<IServiceTestModelTO>());
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockRepo.Object);
            resourceMock.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------
            Assert.AreEqual(1, tests.Count);
            Assert.AreEqual("Create a new test.", tests[0].TestName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestViewModel_Tests")]
        public void ServiceTestViewModel_Tests_GetWhenTests_ShouldHaveTestsAndDummyAtBottom()
        {
            //------------Setup for test--------------------------
            var resourceMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironment = new Mock<IEnvironmentModel>();
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
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);

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
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateCommand_GivenIsDirty_ShouldSetCanExecuteTrue()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel.DuplicateTestCommand);
            Assert.IsTrue(testFrameworkViewModel.IsDirty);
            //---------------Execute Test ----------------------
            var canDuplicate = testFrameworkViewModel.DuplicateTestCommand.CanExecute(null);
            //---------------Test Result -----------------------
            Assert.IsTrue(canDuplicate);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateCommand_GivenIsDirtyFalseAndSelectedIsNotNull_ShouldSetCanExecuteTrue()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirty_ShouldSetCanSaveFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel.DuplicateTestCommand);
            Assert.IsFalse(testFrameworkViewModel.IsDirty);
            //---------------Execute Test ----------------------
            var canSave = testFrameworkViewModel.CanSave;
            //---------------Test Result -----------------------
            Assert.IsFalse(canSave);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirtyAndValidName_ShouldSetCanSavetrue()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirtyAndInvalidValidName_ShouldSetCanSaveFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirtyAndEmptyName_ShouldSetCanSaveFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void CanSave_GivenIsDirtynameHastrailingSpaces_ShouldSetCanSaveFalse()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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

        private List<IServiceTestModel> GetTests([NotNull]IServiceTestViewModel viewModel)
        {
            return viewModel.Tests.Where(model => model.GetType() != typeof(DummyServiceTest)).ToList();
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateTestCommand_GivenSelectedTesIsNotNull_ShouldAddNewTestToTestCollection()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateTestCommand_GivenSelectedTesIsNotNull_ShouldSetSelectedTestToNewlyDuplicatedTest()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void DeleteCommand_GivenSelectedTestIsDisabled_ShouldSetCanDeleteTrue()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenNullTest_Execute_ShouldReturnNull()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);
            var testFrameworkViewModel = new ServiceTestViewModel(CreateMockResourceModelWithSingleScalarOutput(), new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
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
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenTests_ShouldShowConfirmation()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
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

            var testFrameworkViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);

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
        [Owner("Nkosinathi Sangweni")]
        public void DeleteTestCommand_GivenTests_ShouldUpdateTestsCollection()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);

            var resourceModelMock = CreateResourceModelWithSingleScalarOutputMock();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
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

            var testFrameworkViewModel = new ServiceTestViewModel(resourceModelMock.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);

            bool wasCalled = false;
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
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenNameOne_ShouldBeValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { "NameOne" });
            //---------------Test Result -----------------------
            Assert.AreEqual(true, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenNameOnePErcenta_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { "NameOne%" });
            //---------------Test Result -----------------------
            Assert.AreEqual(false, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenEmpty_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { "" });
            //---------------Test Result -----------------------
            Assert.AreEqual(false, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivenNull_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { null });
            //---------------Test Result -----------------------
            Assert.AreEqual(false, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsValid_GivennameWithtrailingSpaces_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("IsValidName", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { "name " });
            //---------------Test Result -----------------------
            Assert.AreEqual(false, bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LastDateVisibility_GivenSelectedTestIsNew_ShouldHaveDateVisibilityCollapsed()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            Assert.IsNotNull(testFrameworkViewModel.SelectedServiceTest);
            //---------------Test Result -----------------------
            Assert.AreEqual(Visibility.Collapsed, testFrameworkViewModel.SelectedServiceTest.LastRunDateVisibility);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NeverRunStringVisibility_GivenSelectedTestIsNew_ShouldHaveDateVisibilityVisible()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var testFrameworkViewModel = new ServiceTestViewModel(mockResourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            Assert.IsNotNull(testFrameworkViewModel.SelectedServiceTest);
            //---------------Test Result -----------------------
            Assert.AreEqual(Visibility.Visible, testFrameworkViewModel.SelectedServiceTest.NeverRunStringVisibility);
        }

        private IContextualResourceModel CreateResourceModelWithSingleScalarInput()
        {
            var resourceModel = CreateResourceModel();
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("a", enDev2ColumnArgumentDirection.Input));
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }

        private IContextualResourceModel CreateResourceModelWithSingleScalarOutput()
        {
            var resourceModel = CreateResourceModel();
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }

        private Mock<IContextualResourceModel> CreateResourceModelWithSingleScalarOutputMock()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return moqModel;
        }

        private IContextualResourceModel CreateMockResourceModelWithSingleScalarOutput()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            moqModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            return moqModel.Object;
        }

        private Mock<IContextualResourceModel> CreateMockResourceModel()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return moqModel;
        }
    }
}
