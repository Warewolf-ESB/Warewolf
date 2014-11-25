
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Studio.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class CalculateIntellisenseTextConverterTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CalculateIntellisenseTextConverter_Convert")]
        public void CalculateIntellisenseTextConverter_Convert_StringWithCalculationConstants_SameString()
        {
            var converter = new CalculateIntellisenseTextConverter();
            const string Expected = "sum(10,10)";
            var actual = (string)converter.Convert(Expected, typeof(bool), null, null);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CalculateIntellisenseTextConverter_Convert")]
        public void CalculateIntellisenseTextConverter_Convert_StringWithParameterAllowUserCalculateIsTrue_StringIsConverted()
        {
            var converter = new CalculateIntellisenseTextConverter();
             string inputText = string.Format("{0}sum(10,10){1}", GlobalConstants.CalculateTextConvertPrefix, GlobalConstants.CalculateTextConvertSuffix);
             var actual = (string)converter.Convert(inputText, typeof(bool), "True", null);
            Assert.AreEqual("=sum(10,10)", actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CalculateIntellisenseTextConverter_Convert")]
        public void CalculateIntellisenseTextConverter_Convert_StringWithParameterAllowUserCalculateIsFalse_StringIsNotConverted()
        {
            var converter = new CalculateIntellisenseTextConverter();
            string expected = string.Format("{0}sum(10,10){1}", GlobalConstants.CalculateTextConvertPrefix, GlobalConstants.CalculateTextConvertSuffix);
            var actual = (string)converter.Convert(expected, typeof(bool), "False", null);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CalculateIntellisenseTextConverter_Convert")]
        public void CalculateIntellisenseTextConverter_Convert_Null_Null()
        {
            var converter = new CalculateIntellisenseTextConverter();
            var actual = (string)converter.Convert(null, typeof(bool), null, null);
            Assert.AreEqual(null, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CalculateIntellisenseTextConverter_ConvertBack")]
        public void CalculateIntellisenseTextConverter_ConvertBack_StringWithCalculationConstants_SameString()
        {
            var converter = new CalculateIntellisenseTextConverter();
            const string Expected = "sum(10,10)";
            var actual = (string)converter.ConvertBack(Expected, typeof(bool), null, null);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CalculateIntellisenseTextConverter_ConvertBack")]
        public void CalculateIntellisenseTextConverter_ConvertBack_StringWithParameterAllowUserCalculateIsTrue_StringIsConverted()
        {
            var converter = new CalculateIntellisenseTextConverter();
            string expected = string.Format("{0}sum(10,10){1}", GlobalConstants.CalculateTextConvertPrefix, GlobalConstants.CalculateTextConvertSuffix);
            var actual = (string)converter.ConvertBack("=sum(10,10)", typeof(bool), "True", null);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CalculateIntellisenseTextConverter_ConvertBack")]
        public void CalculateIntellisenseTextConverter_ConvertBack_StringWithParameterAllowUserCalculateIsFalse_StringIsNotConverted()
        {
            var converter = new CalculateIntellisenseTextConverter();
            string expected = string.Format("{0}sum(10,10){1}", GlobalConstants.CalculateTextConvertPrefix, GlobalConstants.CalculateTextConvertSuffix);
            var actual = (string)converter.ConvertBack(expected, typeof(bool), "False", null);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CalculateIntellisenseTextConverter_ConvertBack")]
        public void CalculateIntellisenseTextConverter_ConvertBack_Null_Null()
        {
            var converter = new CalculateIntellisenseTextConverter();
            var actual = (string)converter.ConvertBack(null, typeof(bool), null, null);
            Assert.AreEqual(null, actual);
        }
    }
}
    
