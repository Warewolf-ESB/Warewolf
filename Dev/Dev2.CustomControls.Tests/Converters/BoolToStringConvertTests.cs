using System;
using System.Globalization;
using System.Windows.Data;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;



namespace Dev2.CustomControls.Tests.Converters
{
    [TestClass]
    public class BoolToStringConvertTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConvertBack_GivenAnyArg_ShouldReturnDoNothing()
        {
            //---------------Set up test pack-------------------
            var convert = new BoolToStringConvert();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var convertBack = convert.ConvertBack(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<CultureInfo>());
            //---------------Test Result -----------------------
            Assert.AreEqual(Binding.DoNothing, convertBack);
        }
    }
}
