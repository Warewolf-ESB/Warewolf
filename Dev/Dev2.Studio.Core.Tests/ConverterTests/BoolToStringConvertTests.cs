using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]    
    // ReSharper disable InconsistentNaming
    public class BoolToStringConvertTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("BoolToStringConvert_Convert")]
        public void BoolToStringConvert_Convert_WhenBooleanIsTrue_ReturnsSuccess()
        {
            //------------Setup for test--------------------------
            var converter = new BoolToStringConvert();
            //------------Execute Test---------------------------
            var result = converter.Convert(true, typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("BoolToStringConvert_Convert")]
        public void BoolToStringConvert_Convert_WhenBooleanIsFalse_ReturnsFailure()
        {
            //------------Setup for test--------------------------
            var converter = new BoolToStringConvert();
            //------------Execute Test---------------------------
            var result = converter.Convert(false, typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual("Failure", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("BoolToStringConvert_Convert")]
        public void BoolToStringConvert_Convert_WhenIsNotBoolean_DefaultsToFailure()
        {
            //------------Setup for test--------------------------
            var converter = new BoolToStringConvert();
            //------------Execute Test---------------------------
            var result = converter.Convert("", typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual("Failure", result);
        }
    }
}
