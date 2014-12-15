
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Warewolf.ComponentModel;

namespace Dev2.Sql.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
