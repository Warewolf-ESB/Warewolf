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

namespace Dev2.Tests
{
    [TestClass]
    public class OutputDescriptionTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(OutputDescription))]
        public void OutputDescription_DefaultConstructor_FormatUnknownAndEmptyShapes()
        {
            var description = new OutputDescription();

            Assert.AreEqual(OutputFormats.Unknown, description.Format);
            Assert.IsNotNull(description.DataSourceShapes);
            Assert.AreEqual(0, description.DataSourceShapes.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(OutputDescription))]
        public void OutputDescription_Equals_SameFormatAndShapes_ReturnsTrue()
        {
            var a = new OutputDescription { Format = OutputFormats.ShapedXML };
            var b = new OutputDescription { Format = OutputFormats.ShapedXML };

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(OutputDescription))]
        public void OutputDescription_Equals_DifferentFormat_ReturnsFalse()
        {
            var a = new OutputDescription { Format = OutputFormats.ShapedXML };
            var b = new OutputDescription { Format = OutputFormats.Unknown };

            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(OutputDescription))]
        public void OutputDescription_EqualsObject_Null_ReturnsFalse()
        {
            var description = new OutputDescription();

            Assert.IsFalse(description.Equals((object)null));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(OutputDescription))]
        public void OutputDescription_EqualsObject_SameReference_ReturnsTrue()
        {
            var description = new OutputDescription();

            Assert.IsTrue(description.Equals((object)description));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(OutputDescription))]
        public void OutputDescription_EqualsObject_DifferentType_ReturnsFalse()
        {
            var description = new OutputDescription();

            Assert.IsFalse(description.Equals("not an output description"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(OutputDescription))]
        public void OutputDescription_EqualsObject_EqualValue_ReturnsTrue()
        {
            var a = new OutputDescription { Format = OutputFormats.ShapedXML };
            object b = new OutputDescription { Format = OutputFormats.ShapedXML };

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(OutputDescription))]
        public void OutputDescription_GetHashCode_EqualForEqualDescriptions()
        {
            var a = new OutputDescription { Format = OutputFormats.ShapedXML };
            var b = new OutputDescription { Format = OutputFormats.ShapedXML, DataSourceShapes = a.DataSourceShapes };

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
