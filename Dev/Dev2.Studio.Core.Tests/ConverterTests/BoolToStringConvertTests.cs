
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class BoolToStringConvertTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("BoolToStringConvert_Convert")]
        public void BoolToStringConvert_Convert_WhenBooleanIsTrue_ReturnsSuccess()
        {
            //------------Setup for test--------------------------
            var converter = new BoolToStringConvert();
            //------------Execute Test---------------------------
            var result = converter.Convert(true, typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("BoolToStringConvert_Convert")]
        public void BoolToStringConvert_Convert_WhenBooleanIsFalse_ReturnsFailure()
        {
            //------------Setup for test--------------------------
            var converter = new BoolToStringConvert();
            //------------Execute Test---------------------------
            var result = converter.Convert(false, typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual("Failure", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("BoolToStringConvert_Convert")]
        public void BoolToStringConvert_Convert_WhenIsNotBoolean_DefaultsToFailure()
        {
            //------------Setup for test--------------------------
            var converter = new BoolToStringConvert();
            //------------Execute Test---------------------------
            var result = converter.Convert("", typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual("Failure", result);
        }
    }
}
