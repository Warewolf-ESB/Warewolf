using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Warewolf.ComponentModel;

namespace Dev2.Sql.Tests
{
    [TestClass]    
    public class ExtensiosnTests
    {
        [TestMethod]
        public void ToStringSafeWithNullExpectedReturnsEmptyString()
        {
            var result = Extensions.ToStringSafe(null);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ToStringSafeWithStringExpectedReturnsString()
        {
            const string Expected = "hello";
            var result = Extensions.ToStringSafe(Expected);
            Assert.AreEqual(Expected, result);
        }

        [TestMethod]
        public void ToStringSafeWithDbNullExpectedReturnsEmptyString()
        {
            var result = Extensions.ToStringSafe(Convert.DBNull);
            Assert.AreEqual(string.Empty, result);
        }

    }
}
