using System;
using System.Globalization;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.CustomControls.Tests.Converters
{
    [TestClass]
    public class StringToTimespanConverterTests
    {
        
     

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Convert_GivenTimeSpanValue_ShouldConvertCorrectly()
        {
            //---------------Set up test pack-------------------
            var timespanConverter = new StringToTimespanConverter();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var convert = timespanConverter.Convert(Person.Time, typeof(int), null, CultureInfo.CurrentCulture);
            //---------------Test Result -----------------------
            Assert.AreEqual(2.ToString(), convert);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConvertBack_GivenStringValue_ShouldConvertCorrectly()
        {
            //---------------Set up test pack-------------------
            var timespanConverter = new StringToTimespanConverter();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var convert = timespanConverter.ConvertBack(Person.StringTime, typeof(TimeSpan), null, CultureInfo.CurrentCulture);
            //---------------Test Result -----------------------
            var b = convert is TimeSpan;
            Assert.IsTrue(b);
        }
    }
}
