/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Converters.Graph.DataTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.ConverterTests.GraphTests.DataTable_Test
{
    [TestClass]
    public class DataTablePathTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataTablePath))]
        public void DataTablePath_DefaultConstructor_ProducesEmptyPath()
        {
            var path = new DataTablePath();

            Assert.AreEqual("", path.ActualPath);
            Assert.AreEqual("", path.DisplayPath);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataTablePath))]
        public void DataTablePath_WithTableName_PrefixesColumnWithTable()
        {
            var path = new DataTablePath("Foo", "Col1");

            Assert.AreEqual("Foo().Col1", path.ActualPath);
            Assert.AreEqual("Foo().Col1", path.DisplayPath);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataTablePath))]
        public void DataTablePath_WithoutTableName_UsesColumnOnly()
        {
            var path = new DataTablePath("", "Col1");

            Assert.AreEqual("Col1", path.ActualPath);
            Assert.AreEqual("Col1", path.DisplayPath);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataTablePath))]
        [ExpectedException(typeof(NotImplementedException))]
        public void DataTablePath_GetSegements_ThrowsNotImplemented()
        {
            new DataTablePath().GetSegements();
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataTablePath))]
        [ExpectedException(typeof(NotImplementedException))]
        public void DataTablePath_CreatePathSegment_ThrowsNotImplemented()
        {
            new DataTablePath().CreatePathSegment("anything");
        }
    }
}
