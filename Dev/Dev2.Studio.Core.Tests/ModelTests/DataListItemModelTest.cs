
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
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListItemModelTest
    {

        #region Test Fields

        private IDataListItemModel _testDataListItemModel;

        #endregion Test Fields

        #region CTOR Tests

        [TestMethod]
        public void DataListItemModelCTOR_Expected_DataListItemModelCreatedWithRespectiveFieldsPopulated()
        {
            string dataListItemDisplayName = "TestItem";
            TestDataListItemModelSet(dataListItemDisplayName);
            Assert.AreEqual(dataListItemDisplayName, _testDataListItemModel.Name);
        }

        [TestMethod]
        public void DataListItemModelCTORWithRecords_Expected_DataListItemModelCreatedWithRespectiveFieldsPopulated()
        {
            IDataListItemModel parent = CreateDataListItemModel("TestItem");
            TestDataListItemModelSet("UnitTestDataListItem", true, parent);
            Assert.IsTrue(_testDataListItemModel.IsRecordset && _testDataListItemModel.Children.Count == 10);
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

        private void TestDataListItemModelSet(string name, bool populateAllFields = false, IDataListItemModel parent = null)
        {
            if(populateAllFields)
            {
                _testDataListItemModel = new DataListItemModel(name, enDev2ColumnArgumentDirection.None
                                                             , "Test Description"
                                                             , parent
                                                             , CreateChildren(_testDataListItemModel, 10)
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

        private OptomizedObservableCollection<IDataListItemModel> CreateChildren(IDataListItemModel parent, int numberOfChildrenToCreate)
        {
            OptomizedObservableCollection<IDataListItemModel> children = new OptomizedObservableCollection<IDataListItemModel>();
            for(int i = 1; i <= numberOfChildrenToCreate; i++)
            {
                children.Add(new DataListItemModel("child" + i.ToString(), enDev2ColumnArgumentDirection.None, "", parent));
            }

            return children;
        }

        private IDataListItemModel CreateDataListItemModel(string name)
        {
            return new DataListItemModel(name);
        }


        #endregion Private Test Methods
    }
}
