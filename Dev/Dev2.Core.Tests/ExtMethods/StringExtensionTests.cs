using Dev2.Common.ExtMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.ExtMethods
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class StringExtensionTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StringExtension_IsNumeric")]
        public void StringExtension_IsNumeric_StringHasASpace_False()
        {
            //------------Execute Test---------------------------
            var result = "123 142".IsNumeric();
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StringExtension_IsNumeric")]
        public void StringExtension_IsNumeric_StringIsANumericWithASpecialChar_False()
        {
            //------------Execute Test---------------------------
            var result = "123#142".IsNumeric();
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StringExtension_IsNumeric")]
        public void StringExtension_IsNumeric_StringIsNumeric_True()
        {
            //------------Execute Test---------------------------
            var result = "123142".IsNumeric();
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StringExtension_IsNumeric")]
        public void StringExtension_IsNumeric_StringIsNumericWithAPeriod_True()
        {
            //------------Execute Test---------------------------
            var result = "123.142".IsNumeric();
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StringExtension_IsNumeric")]
        public void StringExtension_IsNumeric_StringIsNegativeNumericWithAPeriod_True()
        {
            //------------Execute Test---------------------------
            decimal val;
            var result = "-123.142".IsNumeric(out val);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }
    }
}
