/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String.Json;

namespace Dev2.Tests
{
    [TestClass]
    public class DataSourceShapeTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_DefaultConstructor_PathsIsEmptyList()
        {
            var shape = new DataSourceShape();

            Assert.IsNotNull(shape.Paths);
            Assert.AreEqual(0, shape.Paths.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_TypedEquals_NullOther_ReturnsFalse()
        {
            var shape = new DataSourceShape();

            Assert.IsFalse(shape.Equals((IDataSourceShape)null));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_TypedEquals_SameReference_ReturnsTrue()
        {
            var shape = new DataSourceShape();

            Assert.IsTrue(shape.Equals((IDataSourceShape)shape));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_TypedEquals_EmptyPaths_ReturnsTrue()
        {
            var a = new DataSourceShape();
            var b = new DataSourceShape();

            Assert.IsTrue(a.Equals((IDataSourceShape)b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_TypedEquals_MatchingPaths_ReturnsTrue()
        {
            var a = new DataSourceShape();
            var b = new DataSourceShape();
            a.Paths.Add(new PocoPath("Name", "Name"));
            b.Paths.Add(new PocoPath("Name", "Name"));

            Assert.IsTrue(a.Equals((IDataSourceShape)b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_TypedEquals_DifferingActualPath_ReturnsFalse()
        {
            var a = new DataSourceShape();
            var b = new DataSourceShape();
            a.Paths.Add(new PocoPath("Name", "Name"));
            b.Paths.Add(new PocoPath("Other", "Name"));

            Assert.IsFalse(a.Equals((IDataSourceShape)b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_TypedEquals_DifferentPathTypes_ReturnsFalse()
        {
            // Same string content but different IPath implementations — exercises the
            // equalTypes check inside DataSourceShape.EqualsMethod.
            var a = new DataSourceShape();
            var b = new DataSourceShape();
            a.Paths.Add(new PocoPath("Name", "Name"));
            b.Paths.Add(new JsonPath("Name", "Name"));

            Assert.IsFalse(a.Equals((IDataSourceShape)b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_ObjectEquals_Null_ReturnsFalse()
        {
            var shape = new DataSourceShape();

            Assert.IsFalse(shape.Equals((object)null));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_ObjectEquals_SameReference_ReturnsTrue()
        {
            var shape = new DataSourceShape();

            Assert.IsTrue(shape.Equals((object)shape));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_ObjectEquals_DifferentType_ReturnsFalse()
        {
            var shape = new DataSourceShape();

            Assert.IsFalse(shape.Equals("not a data source shape"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_ObjectEquals_EqualValue_ReturnsTrue()
        {
            var a = new DataSourceShape();
            object b = new DataSourceShape();

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_GetHashCode_NonNullPaths_ReturnsListHash()
        {
            var shape = new DataSourceShape();

            Assert.AreEqual(shape.Paths.GetHashCode(), shape.GetHashCode());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DataSourceShape))]
        public void DataSourceShape_GetHashCode_NullPaths_ReturnsZero()
        {
            var shape = new DataSourceShape { Paths = null };

            Assert.AreEqual(0, shape.GetHashCode());
        }
    }
}
