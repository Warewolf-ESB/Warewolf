/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Core;
using Dev2.Studio.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// A set of test cases to test the functionality of the DataListSingleton
    /// </summary>
    [TestClass]
    public class DataListSingletonTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataListSingleton))]
        public void DataListSingleton_SetDataList_Expected_CurrentDataListSetInSingleton()
        {
            var mockdataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            DataListSingleton.SetDataList(mockdataListViewModel.Object);
            Assert.AreEqual(DataListSingleton.ActiveDataList, mockdataListViewModel.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataListSingleton))]
        public void DataListSingleton_UpdateActiveDataList_Expected_NewActiveDataList()
        {
            var mockdataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            DataListSingleton.SetDataList(mockdataListViewModel.Object);
            var mock_newDataListViewModel = new Mock<IDataListViewModel>();
            mock_newDataListViewModel.Setup(dataList => dataList.Resource).Returns(Dev2MockFactory.SetupResourceModelMock().Object);
            DataListSingleton.SetDataList(mock_newDataListViewModel.Object);
            Assert.AreNotEqual(DataListSingleton.ActiveDataList, mockdataListViewModel);
        }
    }
}
