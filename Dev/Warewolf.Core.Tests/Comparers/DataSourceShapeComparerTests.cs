/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Comparers;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Tests
{
    [TestClass]
    public class DataSourceShapeComparerTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShapeComparer))]
        public void DataSourceShapeComparer_Equals_BothNull_ReturnsTrue()
        {
            var comparer = new DataSourceShapeComparer();

            Assert.IsTrue(comparer.Equals(null, null));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShapeComparer))]
        public void DataSourceShapeComparer_Equals_OneNull_ReturnsFalse()
        {
            var comparer = new DataSourceShapeComparer();
            IDataSourceShape shape = new DataSourceShape();

            Assert.IsFalse(comparer.Equals(shape, null));
            Assert.IsFalse(comparer.Equals(null, shape));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShapeComparer))]
        public void DataSourceShapeComparer_Equals_BothNonNull_DelegatesToShapeEquals()
        {
            var comparer = new DataSourceShapeComparer();
            IDataSourceShape x = new DataSourceShape();
            IDataSourceShape y = new DataSourceShape();

            Assert.IsTrue(comparer.Equals(x, y));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShapeComparer))]
        public void DataSourceShapeComparer_GetHashCode_ReturnsObjectHashCode()
        {
            var comparer = new DataSourceShapeComparer();
            var shape = new DataSourceShape();

            Assert.AreEqual(shape.GetHashCode(), comparer.GetHashCode(shape));
        }
    }
}
