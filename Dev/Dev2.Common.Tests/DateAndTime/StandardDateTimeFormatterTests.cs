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

namespace Dev2.Common.Tests.DateAndTime
{
    [TestClass]
    public class StandardDateTimeFormatterTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_TryFormat_DateTime_Empty()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "",
                InputFormat = "ddmmyyyy",
                OutputFormat = @"yyyy'/'mm'/'dd",
                TimeModifierType = "Years",
                TimeModifierAmount = 23
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);

            var year = DateTime.Now.AddYears(23).Year.ToString();
            var resultEquals = result.StartsWith(year);
            Assert.IsTrue(resultEquals);

            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_TryFormat()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14101988",
                InputFormat = "ddmmyyyy",
                OutputFormat = @"yyyy'/'mm'/'dd",
                TimeModifierType = "Years",
                TimeModifierAmount = 23
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);

            var year = DateTime.Parse("1988/10/14").AddYears(23).Year.ToString();
            var resultEquals = result.StartsWith(year);
            Assert.IsTrue(resultEquals);

            Assert.AreEqual("", errorMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(StandardDateTimeFormatter))]
        public void StandardDateTimeFormatter_TryFormat_TimeModifierType_Empty()
        {
            IDateTimeOperationTO dateTimeOperationTO = new DateTimeOperationTO
            {
                DateTime = "14101988",
                InputFormat = "ddmmyyyy",
                OutputFormat = @"yyyy'/'mm'/'dd",
                TimeModifierType = "",
                TimeModifierAmount = 23
            };

            var standardDateTimeFormatter = new StandardDateTimeFormatter();

            var formatResult = standardDateTimeFormatter.TryFormat(dateTimeOperationTO, out string result, out string errorMsg);

            Assert.IsTrue(formatResult);
            Assert.AreEqual("1988/10/14", result);
            Assert.AreEqual("", errorMsg);
        }
    }
}
