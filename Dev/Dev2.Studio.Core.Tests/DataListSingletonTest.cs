
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// A set of test cases to test the functionality of the DataListSingleton
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListSingletonTest
    {

        public static readonly object DataListSingletonTestGuard = new object();
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes

        [TestInitialize]
        public void Init()
        {
            Monitor.Enter(DataListSingletonTestGuard);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Monitor.Exit(DataListSingletonTestGuard);
        }

        #endregion

        #region SetDataList Tests

        [TestMethod]
        public void SetDataList_Expected_CurrentDataListSetInSingleton()
        {
            Mock<IDataListViewModel> mockdataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            DataListSingleton.SetDataList(mockdataListViewModel.Object);
            Assert.AreEqual(DataListSingleton.ActiveDataList, mockdataListViewModel.Object);
        }

        #endregion SetDataList Tests

        #region UpdateActiveDataList Tests

        [TestMethod]
        public void UpdateActiveDataList_Expected_NewActiveDataList()
        {
            Mock<IDataListViewModel> mockdataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            DataListSingleton.SetDataList(mockdataListViewModel.Object);
            Mock<IDataListViewModel> mock_newDataListViewModel = new Mock<IDataListViewModel>();
            mock_newDataListViewModel.Setup(dataList => dataList.Resource).Returns(Dev2MockFactory.SetupResourceModelMock().Object);
            DataListSingleton.SetDataList(mock_newDataListViewModel.Object);
            Assert.AreNotEqual(DataListSingleton.ActiveDataList, mockdataListViewModel);
        }

        #endregion UpdateActiveDataList Tests
    }
}
