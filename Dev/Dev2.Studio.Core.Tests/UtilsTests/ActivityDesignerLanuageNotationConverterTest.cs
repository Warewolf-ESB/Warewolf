
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
using Dev2.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.UtilsTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ActivityDesignerLanuageNotationConverterTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation")]
        public void ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation_WhenStringEmpty_EmptyString()
        {
            //------------Execute Test---------------------------

            var result = ActivityDesignerLanuageNotationConverter.ConvertToTopLevelRSNotation(string.Empty);

            //------------Assert Results-------------------------

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation")]
        public void ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation_WhenStringScalar_AppendNotationRecordset()
        {
            //------------Execute Test---------------------------

            var result = ActivityDesignerLanuageNotationConverter.ConvertToTopLevelRSNotation("abc");

            //------------Assert Results-------------------------

            Assert.AreEqual("[[abc()]]", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation")]
        public void ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation_WhenStringRecordsetWithStar_StarNotationRecordset()
        {
            //------------Execute Test---------------------------

            var result = ActivityDesignerLanuageNotationConverter.ConvertToTopLevelRSNotation("abc(*)");

            //------------Assert Results-------------------------

            Assert.AreEqual("[[abc(*)]]", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation")]
        public void ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation_WhenStringRecordsetWithStarAndField_StarNotationRecordset()
        {
            //------------Execute Test---------------------------

            var result = ActivityDesignerLanuageNotationConverter.ConvertToTopLevelRSNotation("abc(*).va");

            //------------Assert Results-------------------------

            Assert.AreEqual("[[abc(*)]]", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation")]
        public void ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation_WhenStringRecordsetWithSingleOpenRegion_AppendNotationRecordset()
        {
            //------------Execute Test---------------------------

            var result = ActivityDesignerLanuageNotationConverter.ConvertToTopLevelRSNotation("abc(");

            //------------Assert Results-------------------------

            Assert.AreEqual("[[abc()]]", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation")]
        public void ActivityDesignerLanuageNotationConverter_ConverToTopLevelRSNotation_WhenStringRecordsetWithSingleOpenRegionAndStar_StarNotationRecordset()
        {
            //------------Execute Test---------------------------

            var result = ActivityDesignerLanuageNotationConverter.ConvertToTopLevelRSNotation("abc(*");

            //------------Assert Results-------------------------

            Assert.AreEqual("[[abc(*)]]", result);
        }

    }
}
