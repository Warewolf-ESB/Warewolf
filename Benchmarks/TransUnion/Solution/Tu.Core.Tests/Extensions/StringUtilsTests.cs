using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Extensions;

namespace Tu.Core.Tests.Extensions
{
    [TestClass]
    public class StringUtilsTests
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringUtils_ToFields")]
        public void StringUtils_ToFields_NullString_EmptyList()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var result = StringUtils.ToFields(null, null);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringUtils_ToLines")]
        public void StringUtils_ToLines_NullString_EmptyList()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var result = StringUtils.ToLines(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringUtils_ToStringSafe")]
        public void StringUtils_ToStringSafe_Null_EmptyString()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var result = StringUtils.ToStringSafe(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringUtils_ToStringSafe")]
        public void StringUtils_ToStringSafe_DBNull_EmptyString()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var result = StringUtils.ToStringSafe(Convert.DBNull);

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, result);
        }
    }
}
