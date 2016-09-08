using System.Collections.ObjectModel;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

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
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput());
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.IsNull(testModel.Inputs);
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
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput());
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.AreEqual("Test 1", testModel.TestName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCommandHandlerModelTests_CreateTest")]
        public void TestCommandHandlerModelTests_CreateTest_ShouldSetIsDirtyTrue()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput());
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.IsTrue(testModel.IsDirty);
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
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput());
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
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithNoInput());
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.IsTrue(testModel.Enabled);
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
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarInput());
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel);
            Assert.AreEqual(1, testModel.Inputs.Count);
            Assert.AreEqual("a", testModel.Inputs[0].Variable);
            Assert.AreEqual("", testModel.Inputs[0].Value);
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
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarOutput());
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
            var testModel = testFrameworkViewModel.CreateTest(CreateResourceModelWithSingleScalarInputAndRecordSetInput());
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
        [TestCategory("TestCommandHandlerModelTests_DeleteTest")]
        public void TestCommandHandlerModelTests_DeleteTest_Execute_ShouldThrowError()
        {
            //------------Setup for test--------------------------
            var testFrameworkViewModel = new ServiceTestCommandHandlerModel();
            var mockServiceModel = new Mock<IServiceTestModel>();
            mockServiceModel.Setup(a => a.NameForDisplay).Returns("TestName");
            mockServiceModel.Setup(a => a.Enabled).Returns(false);
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            var popupController = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            CustomContainer.Register(popupController.Object);
            testFrameworkViewModel.DeleteTest(mockServiceModel.Object);
            //------------Assert Results-------------------------
            popupController.Verify(controller => controller.ShowDeleteConfirmation(It.IsAny<string>()), Times.Once);
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
            var popupController = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            CustomContainer.Register(popupController.Object);
            testFrameworkViewModel.RunAllTestsCommand(true);
            //------------Assert Results-------------------------
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, null, false, true, false, false), Times.Once);
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
            var popupController = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            CustomContainer.Register(popupController.Object);
            testFrameworkViewModel.RunAllTestsInBrowser(true);
            //------------Assert Results-------------------------
            popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, null, false, true, false, false), Times.Once);
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
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel>();
            recordSetFieldItemModels.Add(new RecordSetFieldItemModel("field",recordSetItemModel,enDev2ColumnArgumentDirection.Input));
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