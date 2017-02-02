/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ModelTests
{
    [TestClass]
    public class DataListItemModelTest
    {
/*
        #region Test Fields

        private IDataListItemModel _testDataListItemModel;

        #endregion Test Fields

        #region CTOR Tests

        [TestMethod]
        public void DataListItemModelCTOR_Expected_DataListItemModelCreatedWithRespectiveFieldsPopulated()
        {
            string dataListItemDisplayName = "TestItem";
            TestDataListItemModelSet(dataListItemDisplayName);
            Assert.AreEqual(dataListItemDisplayName, _testDataListItemModel.DisplayName);
        }

        [TestMethod]
        public void DataListItemModelCTORWithRecords_Expected_DataListItemModelCreatedWithRespectiveFieldsPopulated()
        {
            IDataListItemModel parent = CreateDataListItemModel("TestItem");
            TestDataListItemModelSet("UnitTestDataListItem", true);
            Assert.IsNotNull(_testDataListItemModel);
            Assert.AreEqual(_testDataListItemModel.DisplayName, "TestItem");
        }

        #endregion CTOR Tests

        #region Name Validation

        /// <summary>
        /// Checks that the name validation does not incorrectly validate names
        /// </summary>
        [TestMethod]
        public void SetName_ValidName_Expected_NoValidationErrorMessageOnNameSet()
        {
            IDataListItemModel dataListItemModel = new DataListItemModel("MyDisplayName");
            dataListItemModel.DisplayName = "UnitTestDisplayName";
            Assert.IsTrue(string.IsNullOrEmpty(dataListItemModel.ErrorMessage));
        }


        /// <summary>
        /// Checks that the name validation identifies invalid names
        /// </summary>
        [TestMethod]
        public void SetName_InvalidName_Expected_ValidationErrorMessageOnNameSet()
        {
            IDataListItemModel dataListItemModel = new DataListItemModel("MyDisplayName");
            dataListItemModel.DisplayName = "UnitTestWith&amp;&lt;&gt;&quot;&apos;";
            Assert.IsTrue(!string.IsNullOrEmpty(dataListItemModel.ErrorMessage));
        }

        /// <summary>
        /// Checks that the name validation identifies escaped characters names
        /// </summary>
        [TestMethod]
        public void SetName_XmlEscapeCharactersInName_Expected_ValidationErrorMessageOnNameSet()
        {
            IDataListItemModel dataListItemModel = new DataListItemModel("MyDisplayName");
            dataListItemModel.DisplayName = "UnitTestWith<>";
            Assert.IsTrue(!string.IsNullOrEmpty(dataListItemModel.ErrorMessage));
        }

        #endregion Name Validation

        #region Private Test Methods

        private void TestDataListItemModelSet(string name, bool populateAllFields = false)
        {
            if (populateAllFields)
            {
                _testDataListItemModel = new DataListItemModel(name, enDev2ColumnArgumentDirection.None
                                                             , "Test Description"
                                                             , false
                                                             , ""
                                                             , true
                                                             , true
                                                             , false
                                                             , false);
            }
            else
            {
                _testDataListItemModel = new DataListItemModel(name);
            }

        }

        private IDataListItemModel CreateDataListItemModel(string name)
        {
            return new DataListItemModel(name);
        }


        #endregion Private Test Methods
 * */
    }
}
