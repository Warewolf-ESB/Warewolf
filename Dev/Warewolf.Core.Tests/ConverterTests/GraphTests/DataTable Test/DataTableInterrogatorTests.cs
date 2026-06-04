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
    public class DataTableInterrogatorTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataTableInterrogator))]
        public void DataTableInterrogator_CreateMapper_ReturnsDataTableMapper()
        {
            var interrogator = new DataTableInterrogator();

            var mapper = interrogator.CreateMapper(new System.Data.DataTable());

            Assert.IsInstanceOfType(mapper, typeof(DataTableMapper));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataTableInterrogator))]
        [ExpectedException(typeof(NotImplementedException))]
        public void DataTableInterrogator_CreateNavigator_ThrowsNotImplemented()
        {
            var interrogator = new DataTableInterrogator();

            interrogator.CreateNavigator(new System.Data.DataTable(), typeof(DataTablePath));
        }
    }
}
