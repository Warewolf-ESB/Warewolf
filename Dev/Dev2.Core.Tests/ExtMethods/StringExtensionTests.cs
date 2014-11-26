
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
