using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Dev2.Studio.Core.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]    
    public class IntEnsureMinConverterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntEnsureMinConverter_Convert")]
        public void IntEnsureMinConverter_Convert_AnyValue_Value()
        {
            //------------Setup for test--------------------------
            var converter = new IntEnsureMinConverter();

            //------------Execute Test---------------------------
            var result = converter.Convert(1, typeof(int), null, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntEnsureMinConverter_ConvertBack")]
        public void IntEnsureMinConverter_ConvertBack_NonInteger_MinValue()
        {
            //------------Setup for test--------------------------
            var converter = new IntEnsureMinConverter();

            const int MinValue = 1;

            //------------Execute Test---------------------------
            var result = converter.ConvertBack("a", typeof(int), MinValue, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(MinValue, result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntEnsureMinConverter_ConvertBack")]
        public void IntEnsureMinConverter_ConvertBack_IntegerGreaterThanOrEqualToMin_Integer()
        {
            //------------Setup for test--------------------------
            var converter = new IntEnsureMinConverter();

            const int MinValue = 1;
            const int IntValue = 3;

            //------------Execute Test---------------------------
            var result = converter.ConvertBack(IntValue, typeof(int), MinValue, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(IntValue, result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntEnsureMinConverter_ConvertBack")]
        public void IntEnsureMinConverter_ConvertBack_IntegerLessThanMin_Min()
        {
            //------------Setup for test--------------------------
            var converter = new IntEnsureMinConverter();

            const int MinValue = 3;
            const int IntValue = 1;

            //------------Execute Test---------------------------
            var result = converter.ConvertBack(IntValue, typeof(int), MinValue, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(MinValue, result);
        }

    }
}
