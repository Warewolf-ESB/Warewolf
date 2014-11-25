
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
using System.Windows;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MathFunctionToDoubleConverterTests
    {
        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("MathFunctionToDoubleConverter")]
        [Description("When the mathfunction is max a thickness needs to be returned with the thicknes.left equal to the maximum")]
        public void MathFunctionConverter_UnitTest_MultipleInputsWithMax_ExpectsMaximum()
        {
            var converter = new MathFunctionDoubleToThicknessConverter();
            converter.Function = MathFunction.Max;
            converter.Offset = 0;
            var actual = converter.Convert(new object[] { 50, 100 }, typeof(Thickness), null, null);
            var expected = new Thickness(100, 0, 0, 0);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("MathFunctionToDoubleConverter")]
        [Description("When the mathfunction is min a thickness needs to be returned with the thicknes.left equal to the minimum")]
        public void MathFunctionConverter_UnitTest_MultipleInputsWithMin_ExpectsMinimum()
        {
            var converter = new MathFunctionDoubleToThicknessConverter();
            converter.Function = MathFunction.Min;
            converter.Offset = 0;
            var actual = converter.Convert(new object[] { 50, 100 }, typeof(Thickness), null, null);
            var expected = new Thickness(50, 0, 0, 0);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("MathFunctionToDoubleConverter")]
        [Description("When the mathfunction is max, and an offset is applied, a thickness needs to be returned with the thicknes.left equal to the maximum minus the offset")]
        public void MathFunctionConverter_UnitTest_MultipleInputsWithMaxAndOffset_ExpectsMaximumWithOffset()
        {
            var converter = new MathFunctionDoubleToThicknessConverter();
            converter.Function = MathFunction.Max;
            converter.Offset = 10;
            var actual = converter.Convert(new object[] { 50, 100 }, typeof(Thickness), null, null);
            var expected = new Thickness(90, 0, 0, 0);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("MathFunctionToDoubleConverter")]
        [Description("When the mathfunction is minimum, and an offset is applied, a thickness needs to be returned with the thicknes.left equal to the minimum minus the offset")]
        public void MathFunctionConverter_UnitTest_MultipleInputsWithMinAndOffset_ExpectsinimumWithOffset()
        {
            var converter = new MathFunctionDoubleToThicknessConverter();
            converter.Function = MathFunction.Min;
            converter.Offset = 10;
            var actual = converter.Convert(new object[] { 50, 100 }, typeof(Thickness), null, null);
            var expected = new Thickness(40, 0, 0, 0);
            Assert.AreEqual(actual, expected);
        }
    }
}
