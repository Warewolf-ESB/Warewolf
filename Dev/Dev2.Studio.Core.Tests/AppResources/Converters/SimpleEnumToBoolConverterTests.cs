using System.Globalization;
using System.Windows.Data;
using Dev2.AppResources.Converters;
using Dev2.Common.Interfaces.Infrastructure.Logging;
using Dev2.Settings.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    public class SimpleEnumToBoolConverterTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SimpleEnumToBoolConverter_Convert")]
        public void SimpleEnumToBoolConverter_Convert_GivenMatchingEnumValue_True()
        {
            //------------Setup for test--------------------------
            var simpleEnumToBoolConvert = new SimpleEnumToBoolConverter();
            
            //------------Execute Test---------------------------
            var convert = (bool)simpleEnumToBoolConvert.Convert(LogLevel.DEBUG, typeof(bool), LogLevel.DEBUG, CultureInfo.InvariantCulture);
            //------------Assert Results-------------------------
            Assert.IsTrue(convert);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SimpleEnumToBoolConverter_Convert")]
        public void SimpleEnumToBoolConverter_Convert_GivenNotMatchingEnumValue_False()
        {
            //------------Setup for test--------------------------
            var simpleEnumToBoolConvert = new SimpleEnumToBoolConverter();
            
            //------------Execute Test---------------------------
            var convert = (bool)simpleEnumToBoolConvert.Convert(LogLevel.FATAL, typeof(bool), LogLevel.DEBUG, CultureInfo.InvariantCulture);
            //------------Assert Results-------------------------
            Assert.IsFalse(convert);
        }        
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SimpleEnumToBoolConverter_Convert")]
        public void SimpleEnumToBoolConverter_ConvertBack_GivenMatchingEnumValue_ReturnEnum()
        {
            //------------Setup for test--------------------------
            var simpleEnumToBoolConvert = new SimpleEnumToBoolConverter();
            
            //------------Execute Test---------------------------
            var convert = (LogLevel)simpleEnumToBoolConvert.ConvertBack(true, typeof(bool), LogLevel.DEBUG, CultureInfo.InvariantCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.DEBUG,convert);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SimpleEnumToBoolConverter_Convert")]
        public void SimpleEnumToBoolConverter_ConvertBack_GivenNotMatchingEnumValue_ReturnBindingNothing()
        {
            //------------Setup for test--------------------------
            var simpleEnumToBoolConvert = new SimpleEnumToBoolConverter();
            
            //------------Execute Test---------------------------
            var convert = simpleEnumToBoolConvert.ConvertBack(false, typeof(bool), LogLevel.DEBUG, CultureInfo.InvariantCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual(Binding.DoNothing,convert);
        }
    }
}
