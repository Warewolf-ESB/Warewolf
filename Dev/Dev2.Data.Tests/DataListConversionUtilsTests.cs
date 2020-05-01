/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.Data.Tests
{
    [TestClass]
    public class DataListConversionUtilsTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataListConversionUtils))]
        public void DataListConversionUtils_CreateListToBindTo_GetInputs()
        {
            var scalarListOne = new List<IScalar>
            {
                new Scalar { Name = "[[a]]", Value = "1", IODirection = enDev2ColumnArgumentDirection.Input },
                new Scalar { Name = "[[b]]", Value = "2", IODirection = enDev2ColumnArgumentDirection.Both }
            };

            var scalarListTwo = new List<IScalar>
            {
                new Scalar { Name = "[[a]]", Value = "1", IODirection = enDev2ColumnArgumentDirection.Input },
                new Scalar { Name = "[[b]]", Value = "2", IODirection = enDev2ColumnArgumentDirection.Both }
            };

            var recSetColOne = new Dictionary<int, List<IScalar>> { { 1, scalarListOne } };
            var recSetColTwo = new Dictionary<int, List<IScalar>> { { 2, scalarListTwo } };

            var recordSetList = new List<IRecordSet>
            {
                new RecordSet { Name = "[[rec().a]]", Columns = recSetColOne, Value = "1", IODirection = enDev2ColumnArgumentDirection.Input },
                new RecordSet { Name = "[[rec().b]]", Columns = recSetColTwo, Value = "2", IODirection = enDev2ColumnArgumentDirection.Both }
            };

            var complexObjectsList = new List<IComplexObject>
            {
                new ComplexObject { Name = "@item", Value = "1", IODirection = enDev2ColumnArgumentDirection.Input },
                new ComplexObject { Name = "@newItem", Value = "2", IODirection = enDev2ColumnArgumentDirection.Both }
            };

            var mockDataListModel = new Mock<IDataListModel>();
            mockDataListModel.Setup(dataListModel => dataListModel.Scalars).Returns(scalarListOne);
            mockDataListModel.Setup(dataListModel => dataListModel.RecordSets).Returns(recordSetList);
            mockDataListModel.Setup(dataListModel => dataListModel.ComplexObjects).Returns(complexObjectsList);

            var dataListConversionUtils = new DataListConversionUtils();

            var listItems = dataListConversionUtils.CreateListToBindTo(mockDataListModel.Object);

            Assert.AreEqual(8, listItems.Count);

            Assert.AreEqual("[[a]]", listItems[0].DisplayValue);
            Assert.AreEqual("[[a]]", listItems[0].Field);
            Assert.AreEqual("1", listItems[0].Value);
            Assert.IsNull(listItems[0].Index);
            Assert.IsFalse(listItems[0].IsObject);
            Assert.IsNull(listItems[0].Recordset);

            Assert.AreEqual("[[b]]", listItems[1].DisplayValue);
            Assert.AreEqual("[[b]]", listItems[1].Field);
            Assert.AreEqual("2", listItems[1].Value);
            Assert.IsNull(listItems[1].Index);
            Assert.IsFalse(listItems[1].IsObject);
            Assert.IsNull(listItems[1].Recordset);

            Assert.AreEqual("[[rec().a]](1).[[a]]", listItems[2].DisplayValue);
            Assert.AreEqual("[[a]]", listItems[2].Field);
            Assert.AreEqual("1", listItems[2].Value);
            Assert.AreEqual("1", listItems[2].Index);
            Assert.IsFalse(listItems[2].IsObject);
            Assert.AreEqual("[[rec().a]]", listItems[2].Recordset);

            Assert.AreEqual("[[rec().a]](1).[[b]]", listItems[3].DisplayValue);
            Assert.AreEqual("[[b]]", listItems[3].Field);
            Assert.AreEqual("2", listItems[3].Value);
            Assert.AreEqual("1", listItems[3].Index);
            Assert.IsFalse(listItems[3].IsObject);
            Assert.AreEqual("[[rec().a]]", listItems[3].Recordset);

            Assert.AreEqual("[[rec().b]](2).[[a]]", listItems[4].DisplayValue);
            Assert.AreEqual("[[a]]", listItems[4].Field);
            Assert.AreEqual("1", listItems[4].Value);
            Assert.AreEqual("2", listItems[4].Index);
            Assert.IsFalse(listItems[4].IsObject);
            Assert.AreEqual("[[rec().b]]", listItems[4].Recordset);

            Assert.AreEqual("[[rec().b]](2).[[b]]", listItems[5].DisplayValue);
            Assert.AreEqual("[[b]]", listItems[5].Field);
            Assert.AreEqual("2", listItems[5].Value);
            Assert.AreEqual("2", listItems[5].Index);
            Assert.IsFalse(listItems[5].IsObject);
            Assert.AreEqual("[[rec().b]]", listItems[5].Recordset);

            Assert.AreEqual("@item", listItems[6].DisplayValue);
            Assert.AreEqual("item", listItems[6].Field);
            Assert.AreEqual("1", listItems[6].Value);
            Assert.IsNull(listItems[6].Index);
            Assert.IsTrue(listItems[6].IsObject);
            Assert.IsNull(listItems[6].Recordset);

            Assert.AreEqual("@newItem", listItems[7].DisplayValue);
            Assert.AreEqual("newItem", listItems[7].Field);
            Assert.AreEqual("2", listItems[7].Value);
            Assert.IsNull(listItems[7].Index);
            Assert.IsTrue(listItems[7].IsObject);
            Assert.IsNull(listItems[7].Recordset);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataListConversionUtils))]
        public void DataListConversionUtils_CreateListToBindTo_GetOutputs()
        {
            var scalarListOne = new List<IScalar>
            {
                new Scalar { Name = "[[a]]", Value = "1", IODirection = enDev2ColumnArgumentDirection.Output },
                new Scalar { Name = "[[b]]", Value = null, IODirection = enDev2ColumnArgumentDirection.Both }
            };

            var scalarListTwo = new List<IScalar>
            {
                new Scalar { Name = "[[a]]", Value = "1", IODirection = enDev2ColumnArgumentDirection.Output },
                new Scalar { Name = "[[b]]", Value = "2", IODirection = enDev2ColumnArgumentDirection.Both }
            };

            var recSetColOne = new Dictionary<int, List<IScalar>> { { 1, scalarListOne } };
            var recSetColTwo = new Dictionary<int, List<IScalar>> { { 2, scalarListTwo } };

            var recordSetList = new List<IRecordSet>
            {
                new RecordSet { Name = "[[rec().a]]", Columns = recSetColOne, Value = "1", IODirection = enDev2ColumnArgumentDirection.Output },
                new RecordSet { Name = "[[rec().b]]", Columns = recSetColTwo, Value = "2", IODirection = enDev2ColumnArgumentDirection.Both }
            };

            var complexObjectsList = new List<IComplexObject>
            {
                new ComplexObject { Name = "@item", Value = "1", IODirection = enDev2ColumnArgumentDirection.Output },
                new ComplexObject { Name = "@newItem", Value = "2", IODirection = enDev2ColumnArgumentDirection.Both }
            };

            var mockDataListModel = new Mock<IDataListModel>();
            mockDataListModel.Setup(dataListModel => dataListModel.Scalars).Returns(scalarListOne);
            mockDataListModel.Setup(dataListModel => dataListModel.RecordSets).Returns(recordSetList);
            mockDataListModel.Setup(dataListModel => dataListModel.ComplexObjects).Returns(complexObjectsList);

            var dataListConversionUtils = new DataListConversionUtils();

            var listItems = dataListConversionUtils.GetOutputs(mockDataListModel.Object);

            Assert.AreEqual(8, listItems.Count);

            Assert.AreEqual("[[a]]", listItems[0].DisplayValue);
            Assert.AreEqual("[[a]]", listItems[0].Field);
            Assert.AreEqual("1", listItems[0].Value);
            Assert.IsNull(listItems[0].Index);
            Assert.IsFalse(listItems[0].IsObject);
            Assert.IsNull(listItems[0].Recordset);

            Assert.AreEqual("[[b]]", listItems[1].DisplayValue);
            Assert.AreEqual("[[b]]", listItems[1].Field);
            Assert.IsNull(listItems[1].Value);
            Assert.IsNull(listItems[1].Index);
            Assert.IsFalse(listItems[1].IsObject);
            Assert.IsNull(listItems[1].Recordset);

            Assert.AreEqual("[[rec().a]](1).[[a]]", listItems[2].DisplayValue);
            Assert.AreEqual("[[a]]", listItems[2].Field);
            Assert.AreEqual("1", listItems[2].Value);
            Assert.AreEqual("1", listItems[2].Index);
            Assert.IsFalse(listItems[2].IsObject);
            Assert.AreEqual("[[rec().a]]", listItems[2].Recordset);

            Assert.AreEqual("[[rec().a]](1).[[b]]", listItems[3].DisplayValue);
            Assert.AreEqual("[[b]]", listItems[3].Field);
            Assert.IsNull(listItems[3].Value);
            Assert.AreEqual("1", listItems[3].Index);
            Assert.IsFalse(listItems[3].IsObject);
            Assert.AreEqual("[[rec().a]]", listItems[3].Recordset);

            Assert.AreEqual("[[rec().b]](2).[[a]]", listItems[4].DisplayValue);
            Assert.AreEqual("[[a]]", listItems[4].Field);
            Assert.AreEqual("1", listItems[4].Value);
            Assert.AreEqual("2", listItems[4].Index);
            Assert.IsFalse(listItems[4].IsObject);
            Assert.AreEqual("[[rec().b]]", listItems[4].Recordset);

            Assert.AreEqual("[[rec().b]](2).[[b]]", listItems[5].DisplayValue);
            Assert.AreEqual("[[b]]", listItems[5].Field);
            Assert.AreEqual("2", listItems[5].Value);
            Assert.AreEqual("2", listItems[5].Index);
            Assert.IsFalse(listItems[5].IsObject);
            Assert.AreEqual("[[rec().b]]", listItems[5].Recordset);

            Assert.AreEqual("@item", listItems[6].DisplayValue);
            Assert.AreEqual("item", listItems[6].Field);
            Assert.AreEqual("1", listItems[6].Value);
            Assert.IsNull(listItems[6].Index);
            Assert.IsTrue(listItems[6].IsObject);
            Assert.IsNull(listItems[6].Recordset);

            Assert.AreEqual("@newItem", listItems[7].DisplayValue);
            Assert.AreEqual("newItem", listItems[7].Field);
            Assert.AreEqual("2", listItems[7].Value);
            Assert.IsNull(listItems[7].Index);
            Assert.IsTrue(listItems[7].IsObject);
            Assert.IsNull(listItems[7].Recordset);
        }
    }
}
