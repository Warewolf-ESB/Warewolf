using System;
using System.Globalization;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.CustomControls.Tests.Converters
{
    [TestClass]
    public class FilterStringToBoolConverterTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Convert_GivenEmptyValue_ShouldReturnCorrectly()
        {
            //---------------Set up test pack-------------------
            var boolConverter = new FilterStringToBoolConverter();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var convert = boolConverter.Convert(Person.EmptyVal, typeof(string), Person.StringTime, CultureInfo.CurrentCulture);
                Assert.IsFalse(bool.Parse(convert.ToString()));
            }
            catch(Exception ex)
            {
                //---------------Test Result -----------------------
              Assert.Fail(ex.Message);
            }
          
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Convert_GivenValue_ShouldReturnCorrectly()
        {
            //---------------Set up test pack-------------------
            var boolConverter = new FilterStringToBoolConverter();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var convert = boolConverter.Convert(Person.StringTime, typeof(string), Person.StringTime, CultureInfo.CurrentCulture);
                Assert.IsTrue(bool.Parse(convert.ToString()));
            }
            catch(Exception ex)
            {
                //---------------Test Result -----------------------
              Assert.Fail(ex.Message);
            }
          
        }
    }
}
