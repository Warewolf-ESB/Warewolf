/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Builders
{
    [TestClass]
    public class DataListIntellisenseBuilderTests
    {
        const string True = "True";
        const string None = "None";

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_NoDataList()
        {
            var builder = new DataListIntellisenseBuilder();


            var result = builder.Generate();

            Assert.IsNotNull(builder.FilterTO);

            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(false, result.IsReadOnly);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_InvalidData_DoesNotThrow()
        {
            var builder = new DataListIntellisenseBuilder();
            builder.DataList = "<asdf";
            var result = builder.Generate();

            Assert.IsNotNull(builder.FilterTO);

            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(false, result.IsReadOnly);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_WithDataList()
        {
            var dataList = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";

            var builder = new DataListIntellisenseBuilder();
            builder.DataList = dataList;

            var result = builder.Generate();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("", result[0].Description);
            Assert.AreEqual("scalar1", result[0].Name);
            Assert.IsNull(result[0].Children);

            Assert.AreEqual("", result[1].Description);
            Assert.AreEqual("rs", result[1].Name);
            Assert.AreEqual(2, result[1].Children.Count);

            Assert.AreEqual("", result[1].Children[0].Description);
            Assert.AreEqual("f1", result[1].Children[0].Name);
            Assert.IsNull(result[1].Children[0].Children);

            Assert.AreEqual("", result[1].Children[1].Description);
            Assert.AreEqual("f2", result[1].Children[1].Name);
            Assert.IsNull(result[1].Children[1].Children);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_WithDataList_FilterOnlyScalars()
        {
            var filterTo = new IntellisenseFilterOpsTO()
            {
                FilterType = Common.Interfaces.enIntellisensePartType.ScalarsOnly
            };
            var dataList = "<DataList><scalar1 Description=\"scalar desc\">s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";

            var builder = new DataListIntellisenseBuilder();
            builder.DataList = dataList;
            builder.FilterTO = filterTo;

            var result = builder.Generate();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("scalar desc", result[0].Description);
            Assert.AreEqual("scalar1", result[0].Name);
            Assert.IsNull(result[0].Children);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_WithDataList_FilterOnlyRecordsets()
        {
            var filterTo = new IntellisenseFilterOpsTO()
            {
                FilterType = Common.Interfaces.enIntellisensePartType.RecordsetsOnly
            };
            var dataList = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";

            var builder = new DataListIntellisenseBuilder();
            builder.DataList = dataList;
            builder.FilterTO = filterTo;

            var result = builder.Generate();

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("", result[0].Description);
            Assert.AreEqual("rs()", result[0].Name);
            Assert.IsNull(result[0].Children);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_WithDataList_FilterOnlyFields()
        {
            var filterTo = new IntellisenseFilterOpsTO()
            {
                FilterType = Common.Interfaces.enIntellisensePartType.RecordsetFields
            };
            var dataList = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";

            var builder = new DataListIntellisenseBuilder();
            builder.DataList = dataList;
            builder.FilterTO = filterTo;

            var result = builder.Generate();

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("", result[0].Description);
            Assert.AreEqual("rs", result[0].Name);
            Assert.AreEqual(2, result[0].Children.Count);

            Assert.AreEqual("", result[0].Children[0].Description);
            Assert.AreEqual("f1", result[0].Children[0].Name);
            Assert.IsNull(result[0].Children[0].Children);

            Assert.AreEqual("", result[0].Children[1].Description);
            Assert.AreEqual("f2", result[0].Children[1].Name);
            Assert.IsNull(result[0].Children[1].Children);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_Given_DataList()
        {
            var intellisenseBuilder = new DataListIntellisenseBuilder
            {
                DataList = string.Format("<DataList><Person Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><Name Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></Person></DataList>", True, None)
            };
            var result = intellisenseBuilder.Generate();
            Assert.IsNotNull(result);
            Assert.AreEqual("Person", result[0].Name);
            Assert.AreEqual(1, result[0].Children.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_Given_DataList_And_FilterType_RecordsetOnly()
        {
            var intellisenseBuilder = new DataListIntellisenseBuilder
            {
                DataList = string.Format("<DataList><Person Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><Name Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></Person></DataList>", True, None),
                FilterTO = new IntellisenseFilterOpsTO
                {
                    FilterType = Common.Interfaces.enIntellisensePartType.RecordsetsOnly
                }
            };
            var result = intellisenseBuilder.Generate();
            Assert.IsNotNull(result);
            Assert.AreEqual("Person()", result[0].Name);
            Assert.IsNull(result[0].Children);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataListIntellisenseBuilder")]
        public void DataListIntellisenseBuilder_Generate_Given_DataList_And_FilterType_RecordsetFields()
        {
            var intellisenseBuilder = new DataListIntellisenseBuilder
            {
                DataList = string.Format("<DataList><Person Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><Name Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></Person></DataList>", True, None),
                FilterTO = new IntellisenseFilterOpsTO
                {
                    FilterType = Common.Interfaces.enIntellisensePartType.RecordsetFields
                }
            };
            var result = intellisenseBuilder.Generate();
            Assert.IsNotNull(result);
            Assert.AreEqual("Person", result[0].Name);
            Assert.AreEqual(1, result[0].Children.Count);
        }
    }
}
