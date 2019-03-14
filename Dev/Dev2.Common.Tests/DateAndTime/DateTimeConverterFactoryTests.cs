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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.DateAndTime
{
    [TestClass]
    public class DateTimeConverterFactoryTests
    {
        [ClassInitialize]
        public static void PreConditions(TestContext testContext)
        {
            var regionName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            var regionNameUI = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

            Assert.AreEqual("en-ZA", regionName);
            Assert.AreEqual("en-ZA", regionNameUI);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DateTimeConverterFactory))]
        public void DateTimeConverterFactory_CreateFormatter()
        {
            var formatter = DateTimeConverterFactory.CreateFormatter();

            Assert.AreEqual(typeof(DateTimeFormatter), formatter.GetType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DateTimeConverterFactory))]
        public void DateTimeConverterFactory_CreateStandardFormatter()
        {
            var standardFormatter = DateTimeConverterFactory.CreateStandardFormatter();

            Assert.AreEqual(typeof(StandardDateTimeFormatter), standardFormatter.GetType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DateTimeConverterFactory))]
        public void DateTimeConverterFactory_CreateParser()
        {
            var parser = DateTimeConverterFactory.CreateParser();

            Assert.AreEqual(typeof(Dev2DateTimeParser), parser.GetType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DateTimeConverterFactory))]
        public void DateTimeConverterFactory_CreateStandardParser()
        {
            var standardParser = DateTimeConverterFactory.CreateStandardParser();

            Assert.AreEqual(typeof(StandardDateTimeParser), standardParser.GetType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DateTimeConverterFactory))]
        public void DateTimeConverterFactory_CreateComparer()
        {
            var comparer = DateTimeConverterFactory.CreateComparer();

            Assert.AreEqual(typeof(DateTimeComparer), comparer.GetType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DateTimeConverterFactory))]
        public void DateTimeConverterFactory_CreateStandardComparer()
        {
            var standardComparer = DateTimeConverterFactory.CreateStandardComparer();

            Assert.AreEqual(typeof(StandardDateTimeComparer), standardComparer.GetType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DateTimeConverterFactory))]
        public void DateTimeConverterFactory_CreateDateTimeDiffTO()
        {
            const string input1 = "input1";
            const string input2 = "input2";
            const string inputFormat = "inputFormat";
            const string outputType = "outputType";

            var dateTimeDiffTO = DateTimeConverterFactory.CreateDateTimeDiffTO(input1, input2, inputFormat, outputType);

            Assert.AreEqual(input1, dateTimeDiffTO.Input1);
            Assert.AreEqual(input2, dateTimeDiffTO.Input2);
            Assert.AreEqual(inputFormat, dateTimeDiffTO.InputFormat);
            Assert.AreEqual(outputType, dateTimeDiffTO.OutputType);
            Assert.AreEqual(typeof(DateTimeDiffTO), dateTimeDiffTO.GetType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DateTimeConverterFactory))]
        public void DateTimeConverterFactory_CreateDateTimeTO()
        {
            //string result
            const string dateTime = "input1";
            const string inputFormat = "inputFormat";
            const string outputFormat = "outputFormat";
            const string timeModifierType = "timeModifierType";
            const int timeModifierAmount = 1;
            const string result = "result";

            var dateTimeDiffTO = DateTimeConverterFactory.CreateDateTimeTO(dateTime, inputFormat, outputFormat, timeModifierType, timeModifierAmount, result);

            Assert.AreEqual(dateTime, dateTimeDiffTO.DateTime);
            Assert.AreEqual(inputFormat, dateTimeDiffTO.InputFormat);
            Assert.AreEqual(outputFormat, dateTimeDiffTO.OutputFormat);
            Assert.AreEqual(timeModifierType, dateTimeDiffTO.TimeModifierType);
            Assert.AreEqual(timeModifierAmount, dateTimeDiffTO.TimeModifierAmount);
            Assert.AreEqual(result, dateTimeDiffTO.Result);
            Assert.AreEqual(typeof(DateTimeOperationTO), dateTimeDiffTO.GetType());
        }
    }
}
