using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data.Binary_Objects;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
// ReSharper disable UseObjectOrCollectionInitializer

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServiceTestCommandHandlerModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_ExecuteNoInputs_ShouldCreateTestModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput(), 1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.IsNotNull(testModel.Inputs);
            Assert.AreEqual(0, testModel.Inputs.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_ShouldSetTestName()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput(), 1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.AreEqual("Test 1", testModel.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_ShouldSetDefaultNoError()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput(), 1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.IsTrue(testModel.NoErrorExpected);
            Assert.IsFalse(testModel.ErrorExpected);
            Assert.AreEqual(string.Empty, testModel.ErrorContainsText);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_ShouldSetTestPending()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput(), 1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.IsTrue(testModel.TestPending);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_ShouldSetEnabled()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput(), 1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.IsTrue(testModel.Enabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTest_GivenTestNumber2_ShouldSetTestNameToTest_2()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarInput(), 2);
            //---------------Test Result -----------------------
            Assert.AreEqual("Test 2", testModel.TestName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTest_GivenTestNumber0_ShouldSetTestNameToTest_1()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarInput(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual("Test 1", testModel.TestName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_Execute_ShouldAddInputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarInput(), 1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.AreEqual(1, testModel.Inputs.Count);
            Assert.AreEqual("a", testModel.Inputs[0].Variable);
            Assert.AreEqual("", testModel.Inputs[0].Value);

            var testModel1 = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarInput(), 1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel1);
            Assert.AreEqual(1, testModel1.Inputs.Count);
            Assert.AreEqual("a", testModel1.Inputs[0].Variable);
            Assert.AreEqual("", testModel1.Inputs[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_Execute_ShouldAddOutputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarOutput(), 1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.AreEqual(1, testModel.Outputs.Count);
            Assert.AreEqual("res", testModel.Outputs[0].Variable);
            Assert.AreEqual("", testModel.Outputs[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_Execute_ShouldHaveActionToAddRowForRecordset()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarInputAndRecordSetInput(), 1);
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(testModel);
            Assert.AreEqual(2, testModel.Inputs.Count);
            Assert.AreEqual("a", testModel.Inputs[0].Variable);
            Assert.AreEqual("", testModel.Inputs[0].Value);
            Assert.AreEqual("rec(1).field", testModel.Inputs[1].Variable);
            Assert.AreEqual("", testModel.Inputs[1].Value);
            //------------Execute Test---------------------------
            testModel.Inputs[1].Value = "value1";
            //------------Assert Results-------------------------
            Assert.AreEqual("value1", testModel.Inputs[1].Value);
            Assert.AreEqual("rec(2).field", testModel.Inputs[2].Variable);
            Assert.AreEqual("", testModel.Inputs[2].Value);

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestCommandHandlerModelTests_RunAllTests")]
        public void TestCommandHandlerModelTests_RunAllTests_Execute_ShouldThrowError()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);
            testFrameworkViewModel.RunAllTestsCommand(true, new ObservableCollection<IServiceTestModel>(), new Mock<IContextualResourceModel>().Object, new SynchronousAsyncWorker());
            //------------Assert Results-------------------------
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, null, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestCommandHandler_RunSelectedTestCommand")]
        public void ServiceTestCommandHandler_RunSelectedTestCommand_ResourceModelTest_ShouldUpdateTestWithPassResult()
        {
            //------------Setup for test--------------------------
            var serviceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var selectedServiceTest = new ServiceTestModel(Guid.NewGuid());

            var testRunResult = new TestRunResult();
            testRunResult.DebugForTest = new List<IDebugState>();
            testRunResult.RunTestResult = RunResult.TestPassed;
            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            mockSelectedServiceTestModelTO.Setup(a => a.Result).Returns(testRunResult);
            mockSelectedServiceTestModelTO.Setup(a => a.TestSteps).Returns(new List<IServiceTestStep>());

            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();

            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            selectedServiceTest.Enabled = true;
            //------------Execute Test---------------------------
            serviceTestCommandHandler.RunSelectedTest(selectedServiceTest, mockResourceModel.Object, new SynchronousAsyncWorker());
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ExecuteTest(It.IsAny<IContextualResourceModel>(), It.IsAny<string>()));
            Assert.IsTrue(selectedServiceTest.TestPassed);
            Assert.IsFalse(selectedServiceTest.TestFailing);
            Assert.IsFalse(selectedServiceTest.TestPending);
            Assert.IsFalse(selectedServiceTest.TestInvalid);
            Assert.IsNotNull(selectedServiceTest.DebugForTest);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestCommandHandler_RunSelectedTestCommand")]
        public void ServiceTestCommandHandler_RunSelectedTestCommand_ResourceModelTest_ShouldUpdateTestWithFailResult()
        {
            //------------Setup for test--------------------------
            var serviceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var selectedServiceTest = new ServiceTestModel(Guid.NewGuid());

            var testRunResult = new TestRunResult();
            testRunResult.DebugForTest = new List<IDebugState>();
            testRunResult.RunTestResult = RunResult.TestFailed;
            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            mockSelectedServiceTestModelTO.Setup(a => a.Result).Returns(testRunResult);
            mockSelectedServiceTestModelTO.Setup(a => a.TestSteps).Returns(new List<IServiceTestStep>());
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();

            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            selectedServiceTest.Enabled = true;
            //------------Execute Test---------------------------
            serviceTestCommandHandler.RunSelectedTest(selectedServiceTest, mockResourceModel.Object, new SynchronousAsyncWorker());
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ExecuteTest(It.IsAny<IContextualResourceModel>(), It.IsAny<string>()));
            Assert.IsFalse(selectedServiceTest.TestPassed);
            Assert.IsTrue(selectedServiceTest.TestFailing);
            Assert.IsFalse(selectedServiceTest.TestPending);
            Assert.IsFalse(selectedServiceTest.TestInvalid);
            Assert.IsNotNull(selectedServiceTest.DebugForTest);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestCommandHandler_RunSelectedTestCommand")]
        public void ServiceTestCommandHandler_RunSelectedTestCommand_ResourceModelTest_ShouldUpdateTestWithInvalidResult()
        {
            //------------Setup for test--------------------------
            var serviceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var selectedServiceTest = new ServiceTestModel(Guid.NewGuid());

            var testRunResult = new TestRunResult();
            testRunResult.DebugForTest = new List<IDebugState>();
            testRunResult.RunTestResult = RunResult.TestInvalid;
            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            mockSelectedServiceTestModelTO.Setup(a => a.Result).Returns(testRunResult);
            mockSelectedServiceTestModelTO.Setup(a => a.TestSteps).Returns(new List<IServiceTestStep>());
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();

            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            selectedServiceTest.Enabled = true;
            //------------Execute Test---------------------------
            serviceTestCommandHandler.RunSelectedTest(selectedServiceTest, mockResourceModel.Object, new SynchronousAsyncWorker());
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ExecuteTest(It.IsAny<IContextualResourceModel>(), It.IsAny<string>()));
            Assert.IsFalse(selectedServiceTest.TestPassed);
            Assert.IsFalse(selectedServiceTest.TestFailing);
            Assert.IsFalse(selectedServiceTest.TestPending);
            Assert.IsTrue(selectedServiceTest.TestInvalid);
            Assert.IsNotNull(selectedServiceTest.DebugForTest);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ServiceTestCommandHandler_RunSelectedTestCommand")]
        public void ServiceTestCommandHandler_RunSelectedTestCommand_ResourceModelTest_ShouldUpdateTestWithPendingResult()
        {
            //------------Setup for test--------------------------
            var serviceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var selectedServiceTest = new ServiceTestModel(Guid.NewGuid());

            var testRunResult = new TestRunResult();
            testRunResult.DebugForTest = new List<IDebugState>();
            testRunResult.RunTestResult = RunResult.TestPending;
            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            mockSelectedServiceTestModelTO.Setup(a => a.Result).Returns(testRunResult);
            mockSelectedServiceTestModelTO.Setup(a => a.TestSteps).Returns(new List<IServiceTestStep>());
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();

            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            selectedServiceTest.Enabled = true;
            //------------Execute Test---------------------------
            serviceTestCommandHandler.RunSelectedTest(selectedServiceTest, mockResourceModel.Object, new SynchronousAsyncWorker());
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ExecuteTest(It.IsAny<IContextualResourceModel>(), It.IsAny<string>()));
            Assert.IsFalse(selectedServiceTest.TestPassed);
            Assert.IsFalse(selectedServiceTest.TestFailing);
            Assert.IsTrue(selectedServiceTest.TestPending);
            Assert.IsFalse(selectedServiceTest.TestInvalid);
            Assert.IsNotNull(selectedServiceTest.DebugForTest);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestCommandHandler_RunSelectedTestCommand")]
        public void ServiceTestCommandHandler_RunSelectedTestCommand_ResourceModelTest_ShouldUpdateTestWithNullResult()
        {
            //------------Setup for test--------------------------
            var serviceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var selectedServiceTest = new ServiceTestModel(Guid.NewGuid());
            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            selectedServiceTest.Enabled = true;
            //------------Execute Test---------------------------
            serviceTestCommandHandler.RunSelectedTest(selectedServiceTest, mockResourceModel.Object, new SynchronousAsyncWorker());
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ExecuteTest(It.IsAny<IContextualResourceModel>(), It.IsAny<string>()));
            Assert.IsFalse(selectedServiceTest.TestPassed);
            Assert.IsFalse(selectedServiceTest.TestFailing);
            Assert.IsFalse(selectedServiceTest.TestPending);
            Assert.IsTrue(selectedServiceTest.TestInvalid);
            Assert.IsNull(selectedServiceTest.DebugForTest);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestCommandHandler_RunSelectedTestCommand")]
        public void ServiceTestCommandHandler_RunAllSelectedTestCommand_ResourceModelTest_ShouldCallExecuteForEachTest()
        {
            //------------Setup for test--------------------------
            var serviceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var selectedServiceTest = new ServiceTestModel(Guid.NewGuid());
            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var testRunResult = new TestRunResult();
            testRunResult.DebugForTest = new List<IDebugState>();
            testRunResult.RunTestResult = RunResult.TestInvalid;
            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            selectedServiceTest.Enabled = true;
            //------------Execute Test---------------------------
            serviceTestCommandHandler.RunAllTestsCommand(false, new List<IServiceTestModel> { selectedServiceTest, new ServiceTestModel(Guid.NewGuid()) }, mockResourceModel.Object, new SynchronousAsyncWorker());
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ExecuteTest(It.IsAny<IContextualResourceModel>(), It.IsAny<string>()), Times.Exactly(2));
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestCommandHandler_RunSelectedTestCommand")]
        public void ServiceTestCommandHandler_RunSelectedTestCommand_ResourceModelTest_ShouldUpdateTestWithResourceDeletedResult()
        {
            //------------Setup for test--------------------------
            var serviceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var selectedServiceTest = new ServiceTestModel(Guid.NewGuid());

            var testRunResult = new TestRunResult();
            testRunResult.DebugForTest = new List<IDebugState>();
            testRunResult.RunTestResult = RunResult.TestResourceDeleted;
            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            mockSelectedServiceTestModelTO.Setup(a => a.Result).Returns(testRunResult);
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false)).Verifiable();
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.CloseResourceTestView(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            CustomContainer.Register(mockPopupController.Object);
            CustomContainer.Register(mockShellViewModel.Object);

            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            selectedServiceTest.Enabled = true;
            //------------Execute Test---------------------------
            serviceTestCommandHandler.RunSelectedTest(selectedServiceTest, mockResourceModel.Object, new SynchronousAsyncWorker());
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ExecuteTest(It.IsAny<IContextualResourceModel>(), It.IsAny<string>()));
            mockPopupController.Verify(controller => controller.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false));
            mockShellViewModel.Verify(model => model.CloseResourceTestView(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestCommandHandlerModelTests_RunAllTests")]
        public void TestCommandHandlerModelTests_RunAllTestsInBrowser_Execute_ShouldThrowError()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);
            testFrameworkViewModel.RunAllTestsInBrowser(true, "Url", new Mock<IExternalProcessExecutor>().Object);
            //------------Assert Results-------------------------
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, null, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHanderModel_RunAllTestsInBrowser")]
        public void TestCommandHanderModel_RunAllTestsInBrowser_WhenNotDirty_ShouldFireOpenInBrowser()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var mockExternalExecutor = new Mock<IExternalProcessExecutor>();
            mockExternalExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            //------------Execute Test---------------------------
            testFrameworkViewModel.RunAllTestsInBrowser(false, "http://localhost:3142/secure/Hello World.tests", mockExternalExecutor.Object);
            //------------Assert Results-------------------------
            mockExternalExecutor.Verify(executor => executor.OpenInBrowser(It.IsAny<Uri>()), Times.Once());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateTest_GivenValidArgs_ShouldAddNew_dupTest()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var serviceTestModels = new ObservableCollection<IServiceTestModel>
            {
                new ServiceTestModel(Guid.NewGuid())
            };
            var resourceId = Guid.NewGuid();
            var serviceTestModel = new ServiceTestModel(resourceId)
            {
                TestName = "Test",
                NameForDisplay = "Test",
                Inputs = new ObservableCollection<IServiceTestInput>(),
                Outputs = new ObservableCollection<IServiceTestOutput>(),
                UserName = "userName",
                Password = "Pppp",
                TestPending = true,
                AuthenticationType = AuthenticationType.Windows,
                Enabled = true,
                IsNewTest = false,
                NewTest = true,
                NoErrorExpected = true

            };
            //---------------Assert Precondition----------------
            Assert.AreEqual(1, serviceTestModels.Count);
            Assert.IsNotNull(serviceTestModel.TestName);
            //---------------Execute Test ----------------------

            var dupTest = testFrameworkViewModel.DuplicateTest(serviceTestModel, 1);
            //---------------Test Result -----------------------
            Assert.IsTrue(dupTest.IsDirty);
            Assert.AreEqual("Test 1", dupTest.TestName);
            Assert.AreEqual("Test 1 *", dupTest.NameForDisplay);
            Assert.AreEqual(resourceId, dupTest.ParentId);
            Assert.AreEqual(serviceTestModel.Inputs.Count, dupTest.Inputs.Count);
            Assert.AreEqual(serviceTestModel.Outputs.Count, dupTest.Outputs.Count);
            Assert.AreEqual(true, dupTest.Enabled);
            Assert.AreEqual(serviceTestModel.AuthenticationType, dupTest.AuthenticationType);
            Assert.AreEqual(serviceTestModel.UserName, dupTest.UserName);
            Assert.AreEqual(serviceTestModel.Password, dupTest.Password);
            Assert.AreEqual(true, dupTest.TestPending);
            Assert.AreEqual(true, dupTest.NewTest);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunSelectedTestInBrowser_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var processor = new Mock<IExternalProcessExecutor>();
            var uri = new Uri("https://www.google.com");
            processor.Setup(executor => executor.OpenInBrowser(uri));
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testFrameworkViewModel.RunSelectedTestInBrowser(uri.ToString(), processor.Object);
            //---------------Test Result -----------------------
            processor.Verify(executor => executor.OpenInBrowser(uri));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateServiceTestOutputFromResult_GivenNoResultStep_ShouldReturnEmpty()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var methodInfo = typeof(ServiceTestCommandHandlerModel).GetMethod("CreateServiceTestOutputFromResult", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { new ObservableCollection<IServiceTestOutput>(), new ServiceTestStep(Guid.NewGuid(), "", new ObservableCollection<IServiceTestOutput>(), StepType.Mock) });

            //---------------Test Result -----------------------
            var enumerable = invoke as ICollection;
            Assert.IsNotNull(enumerable);
            Assert.AreEqual(0, enumerable.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateServiceTestOutputFromResult_GivenResultStep_ShouldPopulateOutputs()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var methodInfo = typeof(ServiceTestCommandHandlerModel).GetMethod("CreateServiceTestOutputFromResult", BindingFlags.NonPublic | BindingFlags.Instance);
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var testOutput = new ServiceTestOutput("a", "Micky", "", "")
            {
                Result = new TestRunResult { Message = "Hi" },
                OptionsForValue = new List<string> { "Hi" },
                HasOptionsForValue = true,
                AssertOp = "=",
                AddStepOutputRow = delegate (string s) { Debug.WriteLine(s); }
            };
            serviceTestOutputs.Add(testOutput);
            const StepType stepType = StepType.Mock;
            var serviceTestStep = new ServiceTestStep(Guid.NewGuid(), "", new ObservableCollection<IServiceTestOutput>(), stepType);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(testFrameworkViewModel, new object[] { serviceTestOutputs, serviceTestStep });
            //---------------Test Result -----------------------
            var observableCollection = invoke as ObservableCollection<IServiceTestOutput>;
            Assert.IsNotNull(observableCollection);
            Assert.AreEqual(1, observableCollection.Count);
            var serviceTestOutput = observableCollection.Single();
            Assert.AreEqual("Hi", serviceTestOutput.Result.Message);
            Assert.AreEqual("Hi", serviceTestOutput.OptionsForValue.Single());
            Assert.AreEqual(true, serviceTestOutput.HasOptionsForValue);
            Assert.AreEqual("=", serviceTestOutput.AssertOp);
            Assert.IsNotNull(((ServiceTestOutput)serviceTestOutput).AddStepOutputRow);
            Assert.AreEqual(testOutput.Variable, serviceTestOutput.Variable);
            Assert.AreEqual(testOutput.Value, serviceTestOutput.Value);
            Assert.AreEqual(testOutput.From, serviceTestOutput.From);
            Assert.AreEqual(testOutput.To, serviceTestOutput.To);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetChildrenTestResult_GivenValidArgs_ShouldPassThough_NullParams()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var methodInfo = typeof(ServiceTestCommandHandlerModel).GetMethod("SetChildrenTestResult", BindingFlags.NonPublic | BindingFlags.Instance);
            var serviceTestOutputs = new List<IServiceTestOutput>();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            try
            {
                methodInfo.Invoke(testFrameworkViewModel, new object[] { default(IEnumerable<IServiceTestStep>), default(ObservableCollection<IServiceTestStep>) });
            }
            catch(TargetInvocationException ex)
            {
                // ReSharper disable once PossibleNullReferenceException
                var b = ex.InnerException.GetType() == typeof(NullReferenceException);
                //---------------Test Result -----------------------
                Assert.IsTrue(b);
            }

            //---------------Test Result -----------------------
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetChildrenTestResult_GivenValidArgs_ShouldPassThough()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var methodInfo = typeof(ServiceTestCommandHandlerModel).GetMethod("SetChildrenTestResult", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var serviceTestSteps = new ObservableCollection<IServiceTestStep>();
            var uniqueId = Guid.NewGuid();
            serviceTestSteps.Add(
                new ServiceTestStep(uniqueId, "", new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
                {
                    Result = new TestRunResult() { Message = "Hi" }
                    ,
                    Children = new ObservableCollection<IServiceTestStep>()
                    {
                        new ServiceTestStep(uniqueId,"", new ObservableCollection<IServiceTestOutput>(), StepType.Mock )
                    }
                });

            var observableCollection = new ObservableCollection<IServiceTestStep>();
            observableCollection.Add(new ServiceTestStep(uniqueId, "", new ObservableCollection<IServiceTestOutput>(), StepType.Mock));

            methodInfo.Invoke(testFrameworkViewModel, new object[] { serviceTestSteps, observableCollection });
            //---------------Test Result -----------------------
            Assert.AreEqual("Hi", observableCollection.Single().Result.Message);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunSelectedTest_GivenNullArgs_ShouldReturn()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            testFrameworkViewModel.RunSelectedTest(serviceTestModel, default(IContextualResourceModel), default(IAsyncWorker));
            //---------------Test Result -----------------------
            Assert.IsFalse(serviceTestModel.IsTestRunning);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunSelectedTest_GivenResultWithOutputs_ShouldPopulateSelectedTesWithResultOutputs()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var selectedServiceTest = new ServiceTestModel(Guid.NewGuid());

            var testRunResult = new TestRunResult();
            testRunResult.DebugForTest = new List<IDebugState>();
            testRunResult.RunTestResult = RunResult.TestPassed;

            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            mockSelectedServiceTestModelTO.Setup(a => a.Result).Returns(testRunResult);
            mockSelectedServiceTestModelTO.Setup(a => a.Outputs).Returns(new List<IServiceTestOutput>()
            {
                new ServiceTestOutput("a","a","a","a") {AssertOp = "=",Result = new TestRunResult() {Message = "Hi"}}
            });

            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false)).Verifiable();
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.CloseResourceTestView(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            CustomContainer.Register(mockPopupController.Object);
            CustomContainer.Register(mockShellViewModel.Object);

            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            Assert.IsFalse(selectedServiceTest.IsTestRunning);
            Assert.IsNull(selectedServiceTest.Outputs);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.RunSelectedTest(selectedServiceTest, mockResourceModel.Object, new SynchronousAsyncWorker());
            //---------------Test Result -----------------------
            Assert.IsNotNull(selectedServiceTest.Outputs);
            Assert.AreEqual(1, selectedServiceTest.Outputs.Count);
            Assert.AreEqual("=", selectedServiceTest.Outputs.Single().AssertOp);
            Assert.AreEqual("Hi", selectedServiceTest.Outputs.Single().Result.Message);
            Assert.AreEqual("a", selectedServiceTest.Outputs.Single().Variable);
            Assert.AreEqual("a", selectedServiceTest.Outputs.Single().From);
            Assert.AreEqual("a", selectedServiceTest.Outputs.Single().To);
            Assert.AreEqual("a", selectedServiceTest.Outputs.Single().Value);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunSelectedTest_GivenResultWithOutputWithTestSteps_ShouldPopulateSelectedTesWithResultOutputsStep()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var resourceId = Guid.NewGuid();
            var selectedServiceTest = new ServiceTestModel(resourceId);

            var testRunResult = new TestRunResult();
            testRunResult.DebugForTest = new List<IDebugState>();
            testRunResult.RunTestResult = RunResult.TestPassed;
            var uniqueId = Guid.NewGuid();
            selectedServiceTest.TestSteps = new ObservableCollection<IServiceTestStep>()
            {
                new ServiceTestStep(uniqueId,"act", new ObservableCollection<IServiceTestOutput>(), StepType.Mock )
            };
            var mockSelectedServiceTestModelTO = new Mock<IServiceTestModelTO>();
            mockSelectedServiceTestModelTO.Setup(a => a.Result).Returns(testRunResult);
            mockSelectedServiceTestModelTO.Setup(a => a.Outputs).Returns(new List<IServiceTestOutput>()
            {
                new ServiceTestOutput("a","a","a","a") {AssertOp = "=",Result = new TestRunResult() {Message = "Hi"}}
            });
            mockSelectedServiceTestModelTO.Setup(to => to.TestSteps).Returns(new List<IServiceTestStep>()
            {
                new ServiceTestStep(uniqueId,"act", new ObservableCollection<IServiceTestOutput>(), StepType.Mock )
                {
                    Result = new TestRunResult() {Message = "StepMsg", DebugForTest = new List<IDebugState>(), RunTestResult = RunResult.TestPassed}
                }
            });

            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false)).Verifiable();
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.CloseResourceTestView(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            CustomContainer.Register(mockPopupController.Object);
            CustomContainer.Register(mockShellViewModel.Object);

            mockResourceRepo.Setup(repository => repository.ExecuteTest(mockResourceModel.Object, It.IsAny<string>())).Returns(mockSelectedServiceTestModelTO.Object);
            mockEnvironment.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            Assert.IsFalse(selectedServiceTest.IsTestRunning);
            Assert.IsNull(selectedServiceTest.Outputs);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.RunSelectedTest(selectedServiceTest, mockResourceModel.Object, new SynchronousAsyncWorker());
            Assert.IsNotNull(selectedServiceTest.Outputs);
            Assert.AreEqual(1, selectedServiceTest.Outputs.Count);
            Assert.AreEqual("=", selectedServiceTest.Outputs.Single().AssertOp);
            Assert.AreEqual("Hi", selectedServiceTest.Outputs.Single().Result.Message);
            Assert.AreEqual("a", selectedServiceTest.Outputs.Single().Variable);
            Assert.AreEqual("a", selectedServiceTest.Outputs.Single().From);
            Assert.AreEqual("a", selectedServiceTest.Outputs.Single().To);
            Assert.AreEqual("a", selectedServiceTest.Outputs.Single().Value);
            Assert.AreNotEqual(default(DateTime), selectedServiceTest.LastRunDate);
            Assert.AreEqual(true, selectedServiceTest.LastRunDateVisibility);

            Assert.AreEqual(false, selectedServiceTest.TestPending);
            Assert.AreEqual(true, selectedServiceTest.TestPassed);
            Assert.AreEqual(false, selectedServiceTest.TestFailing);
            Assert.AreEqual(false, selectedServiceTest.TestInvalid);
            Assert.AreEqual("StepMsg", selectedServiceTest.TestSteps.Single().Result.Message);
            Assert.AreEqual(RunResult.TestPassed, selectedServiceTest.TestSteps.Single().Result.RunTestResult);
            Assert.IsNotNull(selectedServiceTest.TestSteps.Single().Result.DebugForTest);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void StopTest_PassThrough()
        {
            //---------------Set up test pack-------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var mock = new Mock<IContextualResourceModel>();
            var repo = new Mock<IResourceRepository>();
            var environmentalModel = new Mock<IEnvironmentModel>();
            environmentalModel.SetupGet(model => model.ResourceRepository).Returns(repo.Object);
            mock.SetupGet(model => model.Environment).Returns(environmentalModel.Object);
            mock.Setup(model => model.Environment.ResourceRepository.StopExecution(It.IsAny<IContextualResourceModel>()));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            testFrameworkViewModel.StopTest(mock.Object);
            //---------------Test Result -----------------------
            mock.Verify(model => model.Environment.ResourceRepository.StopExecution(It.IsAny<IContextualResourceModel>()), Times.Once);
        }

        private IResourceModel CreateResourceModelWithSingleScalarInput()
        {
            var moqModel = new Mock<IResourceModel>();
            moqModel.SetupAllProperties();
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("a", enDev2ColumnArgumentDirection.Input));
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }

        private IResourceModel CreateResourceModelWithSingleScalarInputAndRecordSetInput()
        {
            var moqModel = new Mock<IResourceModel>();
            moqModel.SetupAllProperties();
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("a", enDev2ColumnArgumentDirection.Input));
            var recordSetItemModel = new RecordSetItemModel("rec", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel> { new RecordSetFieldItemModel("field", recordSetItemModel, enDev2ColumnArgumentDirection.Input) };
            recordSetItemModel.Children = recordSetFieldItemModels;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel);
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }

        private IResourceModel CreateResourceModelWithSingleScalarOutput()
        {
            var moqModel = new Mock<IResourceModel>();
            moqModel.SetupAllProperties();
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("res", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }

        private IResourceModel CreateResourceModelWithNoInput()
        {
            var moqModel = new Mock<IResourceModel>();
            moqModel.SetupAllProperties();
            return moqModel.Object;
        }
    }
}