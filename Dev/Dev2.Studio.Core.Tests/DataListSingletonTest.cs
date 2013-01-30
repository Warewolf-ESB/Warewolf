using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core;
using Moq;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Core.Tests {
    /// <summary>
    /// A set of test cases to test the functionality of the DataListSingleton
    /// </summary>
    [TestClass]
    public class DataListSingletonTest {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region SetDataList Tests

        [TestMethod]
        public void SetDataList_Expected_CurrentDataListSetInSingleton() {
            Mock<IDataListViewModel> mockdataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            DataListSingleton.SetDataList(mockdataListViewModel.Object);
            Assert.AreEqual(DataListSingleton.ActiveDataList, mockdataListViewModel.Object);
        }

        #endregion SetDataList Tests

        #region UpdateActiveDataList Tests

        [TestMethod]
        public void UpdateActiveDataList_Expected_NewActiveDataList() {
            Mock<IDataListViewModel> mockdataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            DataListSingleton.SetDataList(mockdataListViewModel.Object);
            Mock<IDataListViewModel> mock_newDataListViewModel = new Mock<IDataListViewModel>();
            mock_newDataListViewModel.Setup(dataList => dataList.Resource).Returns(Dev2MockFactory.SetupResourceModelMock().Object);
            DataListSingleton.UpdateDataList(mock_newDataListViewModel.Object);
            Assert.AreNotEqual(DataListSingleton.ActiveDataList, mockdataListViewModel);
        }

        #endregion UpdateActiveDataList Tests
    }
}
