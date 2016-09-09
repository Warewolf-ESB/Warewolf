using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2;
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
            new ServiceTestViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_NotNullResourceModel_ShouldSetResourceModel()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var testVM = new ServiceTestViewModel(new Mock<IContextualResourceModel>().Object);
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
            var testVM = new ServiceTestViewModel(mockResourceModel.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual("Workflow Name - Tests", testVM.DisplayName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void OnCreation_GivenIsNew_ShouldHaveRunAllTestsUrl()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel());
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
            var vm = new ServiceTestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.DuplicateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.DuplicateTestCommand.CanExecute(null));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveStopTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel());
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
            var vm = new ServiceTestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.CreateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.CreateTestCommand.CanExecute(null));
        }

        private IContextualResourceModel CreateResourceModel()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(true);
            moqModel.Setup(model => model.Environment.IsConnected).Returns(true);
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
            moqModel.Setup(model => model.Environment.ResourceRepository.SaveTests(It.IsAny<Guid>(), It.IsAny<List<IServiceTestModel>>()));
            return moqModel.Object;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveModel()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel());
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
            var vm = new ServiceTestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunAllTestsCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.RunAllTestsCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunSelectedTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.RunSelectedTestCommand.CanExecute(null));
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunAllTestsInBrowserCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunAllTestsInBrowserCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.RunAllTestsInBrowserCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunSelectedTestInBrowserCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel());
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
            var vm = new ServiceTestViewModel(CreateResourceModelWithMoreSave());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel());
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
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_Tests_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel());
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
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddInputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarInput());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput());
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
            var vm = new ServiceTestViewModel(CreateResourceModel());
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
        public void OnCreation_GivenIsDisabled_DeleteTestCommandShouldBeEnabled()
        {
            //---------------Set up test pack-------------------
            var vm = new ServiceTestViewModel(CreateResourceModel());
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
            var vm = new ServiceTestViewModel(CreateResourceModel());
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
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<Guid>(), It.IsAny<List<IServiceTestModel>>()));
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            var serviceTestViewModel = new ServiceTestViewModel(resourceModelMock.Object);
            serviceTestViewModel.CreateTestCommand.Execute(null);
            //------------Execute Test---------------------------
            Assert.IsTrue(serviceTestViewModel.CanSave);
            serviceTestViewModel.Save();
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.SaveTests(It.IsAny<Guid>(), It.IsAny<List<IServiceTestModel>>()), Times.Once);
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
            mockResourceRepo.Setup(repository => repository.SaveTests(It.IsAny<Guid>(), It.IsAny<List<IServiceTestModel>>()));
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            resourceModelMock.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);

            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            //------------Execute Test---------------------------
            var serviceTestViewModel = new ServiceTestViewModel(resourceModelMock.Object);
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
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_IsDirty_IsFalse()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput());
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
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var mainViewModelMock = new Mock<IMainViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);
            var helpText = "someText";

            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput());

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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput());
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
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModelWithSingleScalarOutput());
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
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------
            Assert.AreEqual(1,tests.Count);
            Assert.AreEqual("Create a new test.",tests[0].TestName);
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
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------
            Assert.AreEqual(1,tests.Count);
            Assert.AreEqual("Create a new test.",tests[0].TestName);
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
            var serviceTestViewModel = new ServiceTestViewModel(resourceMock.Object);

            //------------Execute Test---------------------------
            var tests = serviceTestViewModel.Tests;
            //------------Assert Results-------------------------
            Assert.AreEqual(2,tests.Count);
            Assert.AreEqual("Test From Server", tests[0].TestName);
            Assert.AreEqual(AuthenticationType.Public, tests[0].AuthenticationType);
            Assert.IsTrue(tests[0].Enabled);
            Assert.AreEqual("Create a new test.",tests[1].TestName);
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
    }
}
