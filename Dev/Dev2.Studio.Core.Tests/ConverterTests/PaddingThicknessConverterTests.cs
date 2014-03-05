using Dev2.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class PaddingThicknessConverterTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("PaddingThicknessConverter_Convert")]
        public void PaddingThicknessConverter_Convert_WhenInputIsEmpty_ReturnsAZeroThicknessValue()
        {
            //------------Setup for test--------------------------
            var converter = new PaddingThicknessConverter();

            //------------Execute Test---------------------------
            var result = converter.Convert("", typeof(string), null, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(new Thickness(0, 0, 0, 0), result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("PaddingThicknessConverter_Convert")]
        public void PaddingThicknessConverter_Convert_WhenInputHasData_ReturnsAValidThicknessValue()
        {
            //------------Setup for test--------------------------
            var converter = new PaddingThicknessConverter();

            //------------Execute Test---------------------------
            var result = converter.Convert("D", typeof(string), null, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(new Thickness(2, 0, 0, 0), result);
        }
    }
}
