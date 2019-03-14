/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Studio.Core.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
	[TestCategory("Studio Resources Core")]
    public class DateTimeToStringConverterTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DateTimeToStringConverter_Convert")]
        public void DateTimeToStringConverter_Convert_ValueNotDateTime_NoStringReturned()
        {
            //------------Setup for test--------------------------
            var dateTimeToStringConverter = new DateTimeToStringConverter();
            //------------Execute Test---------------------------
            var stringObj=dateTimeToStringConverter.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(stringObj));
            var convertedValue = dateTimeToStringConverter.Convert("some data", null, null, null);
            //------------Assert Results-------------------------
            Assert.IsNotInstanceOfType(convertedValue, typeof(string));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DateTimeToStringConverter_Convert")]
        public void DateTimeToStringConverter_Convert_HasDateTimeValue_StringWithSplitSeconds()
        {
            //------------Setup for test--------------------------
            var dateTimeToStringConverter = new DateTimeToStringConverter();
            var dateTimeToConvert = new DateTime(2014, 01, 02, 10, 15, 52, 52);
            //------------Execute Test---------------------------
            var convertedValue = dateTimeToStringConverter.Convert(dateTimeToConvert, null, null, null).ToString();
            //------------Assert Results-------------------------
            Assert.IsTrue("2014/01/02 10:15:52.0520 AM" == convertedValue || "02/01/2014 10:15:52.0520 AM" == convertedValue, $"{convertedValue} string does not contain split seconds.");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DateTimeToStringConverter_Convert")]
        public void DateTimeToStringConverter_Convert_HasFormat_StringUsingProvidedFormat()
        {
            //------------Setup for test--------------------------
            var dateTimeToStringConverter = new DateTimeToStringConverter();
            dateTimeToStringConverter.Format = "dd/MMM/yyyy";
            var dateTimeToConvert = new DateTime(2014, 01, 02, 10, 15, 52, 52);
            //------------Execute Test---------------------------
            var convertedValue = dateTimeToStringConverter.Convert(dateTimeToConvert, null, null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("02/Jan/2014", convertedValue);
        }
    }
}
