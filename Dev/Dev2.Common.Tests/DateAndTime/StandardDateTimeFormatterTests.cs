/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.DateAndTime;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace Dev2.Common.Tests.DateAndTime
{
    [TestClass]
    public class StandardDateTimeFormatterTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_TryFormat_DateTime_Empty_SystemRegionDefaultFormat()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "",
                InputFormat = "ddmmyyyy",
                OutputFormat = @"yyyy'/'MM'/'dd",
                TimeModifierType = "Years",
                TimeModifierAmount = 23
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.IsTrue(DateTime.TryParse(result, out DateTime datetimeResult), $"Failed to parse value: {result} as a DateTime");
            Assert.AreEqual(datetimeResult.AddYears(-23).ToShortDateString(), DateTime.Now.ToShortDateString(), "Wrong date time returned from DateTime tool.");
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateOnly_TryFormat_TimeModifierType_Empty()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14101988",
                InputFormat = "ddMMyyyy",
                OutputFormat = @"yyyy/MM/dd",
                TimeModifierType = "",
                TimeModifierAmount = 23
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("1988/10/14", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateOnly_TryFormat_TimeModifierType_AddYears()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14101988 09:00:00",
                InputFormat = "ddMMyyyy hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Years",
                TimeModifierAmount = 23
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/14 09:00:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateOnly_TryFormat_TimeModifierType_SubtractYears()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14102011 09:00:00",
                InputFormat = "ddMMyyyy hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Years",
                TimeModifierAmount = -23
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("1988/10/14 09:00:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateOnly_TryFormat_TimeModifierType_AddMonths()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14102011 09:00:00",
                InputFormat = "ddMMyyyy hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Months",
                TimeModifierAmount = 4
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2012/02/14 09:00:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateOnly_TryFormat_TimeModifierType_SubtractMonths()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14102011 09:00:00",
                InputFormat = "ddMMyyyy hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Months",
                TimeModifierAmount = -11
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2010/11/14 09:00:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateOnly_TryFormat_TimeModifierType_AddDays()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14102011 09:00:00",
                InputFormat = "ddMMyyyy hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Days",
                TimeModifierAmount = 20
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/11/03 09:00:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateOnly_TryFormat_TimeModifierType_SubtractDays()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14102011 09:00:00",
                InputFormat = "ddMMyyyy hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Days",
                TimeModifierAmount = -15
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/09/29 09:00:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_AddHours()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00",
                InputFormat = @"yyyy/MM/dd hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Hours",
                TimeModifierAmount = 25
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/15 09:00:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_SubtractHours()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00",
                InputFormat = @"yyyy/MM/dd hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Hours",
                TimeModifierAmount = -25
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/13 07:00:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_AddMinutes()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00",
                InputFormat = @"yyyy/MM/dd hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Minutes",
                TimeModifierAmount = 55
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/14 08:55:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_SubtractMinutes()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00",
                InputFormat = @"yyyy/MM/dd hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Minutes",
                TimeModifierAmount = -55
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/14 07:05:00", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_AddSeconds()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00",
                InputFormat = @"yyyy/MM/dd hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Seconds",
                TimeModifierAmount = 255
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/14 08:04:15", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_SubtractSeconds()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00",
                InputFormat = @"yyyy/MM/dd hh:mm:ss",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss",
                TimeModifierType = "Seconds",
                TimeModifierAmount = -255
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/14 07:55:45", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_AddMilliSeconds()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00.000",
                InputFormat = @"yyyy/MM/dd hh:mm:ss.fff",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss.fff",
                TimeModifierType = "Milliseconds",
                TimeModifierAmount = 953
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/14 08:00:00.953", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_SubtractMilliSeconds()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00.953",
                InputFormat = @"yyyy/MM/dd hh:mm:ss.fff",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss.fff",
                TimeModifierType = "Milliseconds",
                TimeModifierAmount = -953
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/14 08:00:00.000", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_AddHours_AM_To_PM()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00.000 AM",
                InputFormat = @"yyyy/MM/dd hh:mm:ss.fff tt",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss.fff tt",
                TimeModifierType = "Hours",
                TimeModifierAmount = 30
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/15 02:00:00.000 PM", result);
            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_DateAndTime_TryFormat_TimeModifierType_AddHours_PM_To_AM()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "2011/10/14 08:00:00.000 PM",
                InputFormat = @"yyyy/MM/dd hh:mm:ss.fff tt",
                OutputFormat = @"yyyy/MM/dd hh:mm:ss.fff tt",
                TimeModifierType = "Hours",
                TimeModifierAmount = -34
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("2011/10/13 10:00:00.000 AM", result);
            Assert.AreEqual("", errorMsg);
        }
    }
}
