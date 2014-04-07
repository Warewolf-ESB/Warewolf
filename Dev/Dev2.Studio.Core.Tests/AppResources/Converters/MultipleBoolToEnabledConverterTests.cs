using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    public class MultipleBoolToEnabledConverterTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("MultipleBoolToEnabledConverter_Convert")]
        public void MultipleBoolToEnabledConverter_Convert_WithTrueTrueFalse_ReturnsFalse()
        {
            MultipleBoolToEnabledConverter multipleBoolToEnabledConverter = new MultipleBoolToEnabledConverter();
            object[] values = { true, true, false };
            var actual = multipleBoolToEnabledConverter.Convert(values, null, null, null);

            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("MultipleBoolToEnabledConverter_Convert")]
        public void MultipleBoolToEnabledConverter_Convert_WithTrueTrueTrue_ReturnsTrue()
        {
            MultipleBoolToEnabledConverter multipleBoolToEnabledConverter = new MultipleBoolToEnabledConverter();
            object[] values = { true, true, true };
            var actual = multipleBoolToEnabledConverter.Convert(values, null, null, null);

            Assert.AreEqual(true, actual);
        }
    }
}
