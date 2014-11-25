
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Data;
using System.Linq;
using Dev2.Converters.Graph.DataTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.ConverterTests.GraphTests.DataTable_Test
{
    [TestClass]
    public class DataTableMapperTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataTableMapper_Map")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DataTableMapper_Map_WhenNull_ExpectNull()
        {
            //------------Setup for test--------------------------
            var dataTableMapper = new DataTableMapper();
            //------------Execute Test---------------------------
            dataTableMapper.Map(null).ToList();

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataTableMapper_Map")]
        public void DataTableMapper_Map_WhenValidDataTable_ExpectValidPaths()
        {
            //------------Setup for test--------------------------
            var dataTableMapper = new DataTableMapper();
            DataTable obj = new DataTable("Foo");
            obj.Columns.Add("Col1");
            obj.Columns.Add("Col2");

            obj.Rows.Add(new object[] { "a", "b" });
            obj.Rows.Add(new object[] { "c", "d" });
            obj.Rows.Add(new object[] { "e", "f" });

            //------------Execute Test---------------------------
            var result = dataTableMapper.Map(obj).ToList();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Foo().Col1", result[0].ActualPath);
            Assert.AreEqual("Foo().Col1", result[0].DisplayPath);
            Assert.AreEqual("a__COMMA__c__COMMA__e", result[0].SampleData);

            Assert.AreEqual("Foo().Col2", result[1].ActualPath);
            Assert.AreEqual("Foo().Col2", result[1].DisplayPath);
            Assert.AreEqual("b__COMMA__d__COMMA__f", result[1].SampleData);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataTableMapper_Map")]
        public void DataTableMapper_Map_WhenValidDataTableWithHTMLData_ExpectValidPaths()
        {
            //------------Setup for test--------------------------


            var htmlFragment = @"<html xmlns=""http://www.w3.org/1999/xhtml"">
<head><title>
            All Build Definitions - Microsoft Team Foundation Server
</title>
</head>
</html>";
            var dataTableMapper = new DataTableMapper();
            DataTable obj = new DataTable("Foo");
            obj.Columns.Add("Col1");
            obj.Columns.Add("Col2");

            obj.Rows.Add(new object[] { "a", "b" });
            obj.Rows.Add(new object[] { "c", htmlFragment });

            //------------Execute Test---------------------------
            var result = dataTableMapper.Map(obj).ToList();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Foo().Col1", result[0].ActualPath);
            Assert.AreEqual("Foo().Col1", result[0].DisplayPath);
            Assert.AreEqual("a__COMMA__c", result[0].SampleData);

            Assert.AreEqual("Foo().Col2", result[1].ActualPath);
            Assert.AreEqual("Foo().Col2", result[1].DisplayPath);
            Assert.AreEqual("b__COMMA__" + htmlFragment, result[1].SampleData);
        }
    }
}
