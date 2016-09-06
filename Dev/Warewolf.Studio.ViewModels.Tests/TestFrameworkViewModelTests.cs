using System;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Data.Binary_Objects;
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
    public class TestFrameworkViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFrameworkViewModel_Constructor_NullResourceModel_ShouldThrowException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new TestViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Constructor")]
        public void TestFrameworkViewModel_Constructor_NotNullResourceModel_ShouldSetResourceModel()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var testVM = new TestViewModel(new Mock<IResourceModel>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testVM);
            Assert.IsNotNull(testVM.ResourceModel);
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveSaveCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.SaveCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.SaveCommand.CanExecute(null));
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDeleteTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.DeleteTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DeleteTestCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDuplicateTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestViewModel(CreateResourceModel());
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
            var vm = new TestViewModel(CreateResourceModel());
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
            var vm = new TestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.CreateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsTrue(vm.CreateTestCommand.CanExecute(null));
        }

        private IResourceModel CreateResourceModel()
        {
            var moqModel = new Mock<IResourceModel>();
            moqModel.SetupAllProperties();
            return moqModel.Object;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveModel()
        {
            //---------------Set up test pack-------------------
            var vm = new TestViewModel(CreateResourceModel());
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
            var vm = new TestViewModel(CreateResourceModel());
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
            var vm = new TestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunSelectedTestCommand.CanExecute(null));
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunAllTestsInBrowserCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestViewModel(CreateResourceModel());
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
            var vm = new TestViewModel(CreateResourceModel());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunSelectedTestInBrowserCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunSelectedTestInBrowserCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddANewTest()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new TestViewModel(CreateResourceModel());
            //------------Assert Preconditions-------------------
            Assert.IsNull(testFrameworkViewModel.Tests);
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, testFrameworkViewModel.Tests.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddANewTestWithDefaultName()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new TestViewModel(CreateResourceModel());
            //------------Assert Preconditions-------------------
            Assert.IsNull(testFrameworkViewModel.Tests);
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
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
            var testFrameworkViewModel = new TestViewModel(CreateResourceModel());
            var testModel = new TestModel { TestName = "Test 2" };
            testFrameworkViewModel.Tests = new ObservableCollection<ITestModel> { testModel };
            testFrameworkViewModel.SelectedTest = testModel;
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            Assert.IsNotNull(testFrameworkViewModel.SelectedTest);
            Assert.AreEqual(1, testFrameworkViewModel.Tests.Count);
            Assert.AreEqual(testModel, testFrameworkViewModel.SelectedTest);
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
            Assert.AreNotEqual(testModel, testFrameworkViewModel.SelectedTest);
            Assert.AreEqual(testFrameworkViewModel.Tests[1], testFrameworkViewModel.SelectedTest);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_Tests_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new TestViewModel(CreateResourceModel());
            var _wasCalled = false;
            testFrameworkViewModel.PropertyChanged += (sender, args) =>
              {
                  if (args.PropertyName == "Tests")
                  {
                      _wasCalled = true;
                  }
              };
            //------------Execute Test---------------------------
            testFrameworkViewModel.Tests = new ObservableCollection<ITestModel>();
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_Tests")]
        public void TestFrameworkViewModel_SelectedTest_SetProperty_ShouldFireOnPropertyChanged()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new TestViewModel(CreateResourceModel());
            var _wasCalled = false;
            testFrameworkViewModel.PropertyChanged += (sender, args) =>
              {
                  if (args.PropertyName == "SelectedTest")
                  {
                      _wasCalled = true;
                  }
              };
            //------------Execute Test---------------------------
            testFrameworkViewModel.SelectedTest = new TestModel();
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddInputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new TestViewModel(CreateResourceModelWithSingleScalarInput());
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            var test = testFrameworkViewModel.Tests[0];
            Assert.IsNotNull(test);
            Assert.AreEqual(1, test.Inputs.Count);
            Assert.AreEqual("a", test.Inputs[0].Variable);
            Assert.AreEqual(null, test.Inputs[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestFrameworkViewModel_CreateTestCommand")]
        public void TestFrameworkViewModel_CreateTestCommand_Execute_ShouldAddOutputsFromResourceModel()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new TestViewModel(CreateResourceModelWithSingleScalarOutput());
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            testFrameworkViewModel.CreateTestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(testFrameworkViewModel.Tests);
            var test = testFrameworkViewModel.Tests[0];
            Assert.IsNotNull(test);
            Assert.AreEqual(1, test.OutPuts.Count);
            Assert.AreEqual("msg", test.OutPuts[0].Variable);
            Assert.AreEqual(null, test.OutPuts[0].Value);
        }

        private IResourceModel CreateResourceModelWithSingleScalarInput()
        {
            var resourceModel = CreateResourceModel();
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("a", enDev2ColumnArgumentDirection.Input));
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }

        private IResourceModel CreateResourceModelWithSingleScalarOutput()
        {
            var resourceModel = CreateResourceModel();
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return resourceModel;
        }
    }
}
