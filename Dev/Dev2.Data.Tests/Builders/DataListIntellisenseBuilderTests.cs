/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Data.Tests.Builders
{
    [TestClass]

    public class DataListIntellisenseBuilderTest
    {
        const string trueString = "True";
        const string noneString = "None";

        [TestMethod]
        public void Generate_Given_DataList()
        {
            var intellisenseBuilder = new DataListIntellisenseBuilder
            {
                DataList = string.Format("<DataList><Person Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><Name Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></Person></DataList>", trueString, noneString)
            };
            var result = intellisenseBuilder.Generate();
            Assert.IsNotNull(result);
            Assert.AreEqual("Person", result[0].Name);
            Assert.AreEqual(1, result[0].Children.Count);
        }

        [TestMethod]
        public void Generate_Given_DataList_And_FilterType_RecordsetOnly()
        {
            var intellisenseBuilder = new DataListIntellisenseBuilder
            {
                DataList = string.Format("<DataList><Person Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><Name Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></Person></DataList>", trueString, noneString),
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
        public void Generate_Given_DataList_And_FilterType_RecordsetFields()
        {
            var intellisenseBuilder = new DataListIntellisenseBuilder
            {
                DataList = string.Format("<DataList><Person Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><Name Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></Person></DataList>", trueString, noneString),
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