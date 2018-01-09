using System;
using System.Globalization;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.CustomControls.Tests.Converters
{
    [TestClass]
    public class IsValidDateTimeConverterTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Convert_GivenNotDateTime_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var dateTimeConverter = new IsValidDateTimeConverter();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var convert = dateTimeConverter.Convert(Person.StringTime, typeof(Person), null, CultureInfo.CurrentCulture);
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(convert.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Convert_GivenDateTime_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var dateTimeConverter = new IsValidDateTimeConverter();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var convert = dateTimeConverter.Convert(DateTime.Now, typeof(Person), null, CultureInfo.CurrentCulture);
            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(convert.ToString()));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConvertBack_GivenAnyArgs_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var dateTimeConverter = new IsValidDateTimeConverter();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                var convert = dateTimeConverter.ConvertBack(DateTime.Now, typeof(Person), null, CultureInfo.CurrentCulture);
                //---------------Test Result -----------------------
            }
            catch(Exception e )
            {
                var b = e is NotImplementedException;
                Assert.IsTrue(b);
            }
        }


    }
}
