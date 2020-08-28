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
using System.Globalization;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.ConverterTests.DateTimeTests
{
    /// <summary>
    /// Summary description for DateTimeParserTests
    /// </summary>
    [TestClass]
    public class StandardDateTimeParserTests
    {
        [TestInitialize]
        public void PreConditions()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-ZA");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-ZA");

            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
        }

        static IDateTimeParser _parser;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            _parser = DateTimeConverterFactory.CreateStandardParser();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region TryParseDateTime Tests


        #region Valid Parse Formats

        /// <summary>
        /// Tests that the parser returns the date time object with correctly evaluated date parts
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_AllArgsValid_Expected_ParserReturnsCorrectlyFormattedDateString()
        {
            const string inputString = "14101988";
            const string formatString = "ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the date time parser can parse inputs containing literals with input format supplied
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithSimpleLiteral_Expected_InputStringParsedCorrectly()
        {
            const string inputString = "My birthday is : 14101988";
            const string formatString = "'My birthday is : 'ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the the parser is able to parse a date time input given the input format does not quote literals
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithInferredLiteral_Expected_InputCorrectlyParsed()
        {
            const string inputString = "Please Give Cake On : 14101988";
            const string formatString = "Please Give Cake On : ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the Parser is able to parse an input given escaped literals
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithInferredLiteralContainingEscapedLiteralCharacter_Expected_InputParsedWithEscapedCharactersIncluded()
        {
            const string inputString = "Please Give ' Cake On : 14101988";
            const string formatString = "Please Give \\' Cake On : ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }


        /// <summary>
        /// Test to ensure that multiple mixed escaped regions are correctly parsed
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithMixedInferredAndDelimtedLiteralRegions_Expected_InputRegionCorrectlyParsedForDateTimeValues()
        {
            const string inputString = "Please Give Cake On : 14101988";
            const string formatString = "Please Give \'Cake On :\' ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Test to ensure that parser can parse out regions using a different escape sequence
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDoubleEscapedLiteralCharacterInInferredLiteralRegion_Expected_InputParsedCorrectly()
        {
            const string inputString = "Please Give ' Cake On : 14101988";
            const string formatString = "'Please Give '' Cake On : 'ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the parser is able to parse literals within a literal region
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDoubleEscapedLiteralCharacterInLiteralRegion_Expected_InputParsedOutCorrectly()
        {
            const string inputString = "Please Give ' Cake On : 14101988";
            const string formatString = "'Please Give '' Cake On : 'ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the parser is able to parse escaped literals within a literal region 
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithLiteralContainingTheLiteralEscapeCharacter_Expected_DateParsedCorrectly()
        {
            const string inputString = "Brendon's birthday is : 14101988";
            const string formatString = "'Brendon\\'s birthday is : 'ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the Parser is able to parse literal regions that have escape characters 
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithLiteralContainingTheLiteralEscapeCharacterAndAnEscapeCharacter_Expected_EscapedCharacter()
        {
            const string inputString = "Brendon\\'s birthday is : 14101988";
            const string formatString = "'Brendon\\\\\\'s birthday is : 'ddMMyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }





        /// <summary>
        /// Tests that the parser can parse input data containing slases
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithSlashed_Expected_DateTimeParsedOut()
        {
            const string inputString = "14/10/1988";
            const string formatString = "dd'/'MM'/'yyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that dot characters are treated as literal strings 
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDotes_Expected_DateTimeParsedOUtCorrectlyWithDotExcluded()
        {
            const string inputString = "14.10.1988";
            const string formatString = "dd'.'MM'.'yyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that tbe parser is able to parse an input containing dashes and retrieves the correct date from the input string
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDashesUsingNumeric_Expected_InputDataParsedOutAccordingToInputFormat()
        {
            const string inputString = "14-10-1988";
            const string formatString = "dd'-'MM'-'yyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }




        #region yyyy Tests

        [TestMethod]
        public void TryParseDateTime_Format_yyyy_Expected_FullYearCorrectlyParsedOut()
        {
            const string inputString = "1988";
            const string formatString = "yyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }


        #endregion yyyy tests

        #region mm Tetsts

        [TestMethod]
        public void TryParseDateTime_Format_mm_Expected_MonthParsedAsDoubleDigit()
        {
            const string inputString = "01";
            const string formatString = "mm";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Month == 01)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion mm Tests

        #region m Tests

        [TestMethod]
        public void TryParseDateTime_Format_m_Expected_MonthParsedWithoutPadding()
        {
            const string inputString = "01";
            const string formatString = "m";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Month == 01)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion m Tests

        

        #region MM Tests

        [TestMethod]
        public void TryParseDateTime_Format_MM_Expected_FullMonthNameReturnedFromParser()
        {
            const string inputString = "Febuary";
            const string formatString = "MM";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Month == 01)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion MM Tests

        #region d Tests

        [TestMethod]
        public void TryParseDateTime_Format_d_Expected_DayOfTheMonthReturned()
        {
            const string inputString = "01";
            const string formatString = "d";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 01)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion d Tests

        #region dd Tests

        [TestMethod]
        public void TryParseDateTime_Format_dd_Expected_DayReturnedWithPadding()
        {
            const string inputString = "01";
            const string formatString = "dd";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 01)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion dd Tests
                

        

        #region dw Tests

        [TestMethod]
        public void TryParseDateTime_Format_dw_Expected_DayOfTheWeekParsedCorrectlyAsNumber()
        {
            const string inputString = "5";
            const string formatString = "dw";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.DayOfWeek == DayOfWeek.Friday)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion dw Tests                         
              

        #region min Tests

        [TestMethod]
        public void TryParseDateTime_Format_min_Expected_Positive()
        {
            const string inputString = "14101988 15:42";
            const string formatString = "'14101988 15:'min";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Minutes == 42)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }


        #endregion min Tests

        #region ss Tests

        [TestMethod]
        public void TryParseDateTime_Format_ss_Expected_Positive()
        {
            const string inputString = "14101988 15:42:32:673";
            const string formatString = "'14101988 15:42:'ss':673'";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Seconds == 32)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion ss Tests


        #region am/pm Tests

        [TestMethod]
        public void TryParseDateTime_Format_am_Expected_Positive()
        {
            const string inputString = "14101988 1:42:32:673 am";
            const string formatString = "'14101988 1:42:32:673 'am/pm";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 0)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }



        [TestMethod]
        public void TryParseDateTime_Format_am_Where_Hours_Is_12_Expected_Positive()
        {
            const string inputString = "14101988 12:42:32 am";
            const string formatString = "ddmmyyyy 12h:min:ss am/pm";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 0)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }





        #endregion am/pm Tests

        #endregion TryParseDateTime Tests

        #region TryParseTime Tests

        /// <summary>
        /// Parse time All Arguments valid expected Parsed Correctly
        /// </summary>
        [TestMethod]
        public void TryParseTime_AllArgs_Valid_Expected_ParsedOutCorrectly()
        {
            const string inputFormat = "yy':'mm':'dd";
            const string addTime = "01:02:03";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Years == 1 && returnTO.Months == 2 && returnTO.Days == 3)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time with input format as invalid expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_InputFormat_Invalid_Expected_ErrorReturned()
        {
            const string inputFormat = "yy':'asdavvaad";
            const string addTime = "01:02:03";
            var isParseable = _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time invalid time expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Time_Invalid_Expected_ErrorReturned()
        {
            const string inputFormat = "yy':'mm':'dd";
            const string addTime = "sdfsdfsdfddf";
            var isParseable = _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time with input time NULL expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTimeTimeNullExpectedErrorReturned()
        {
            const string inputFormat = "yy':'mm':'dd";
            var isParseable = _parser.TryParseTime(null, inputFormat, out IDateTimeResultTO returnTO, out string result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time using yy input format expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingYyExpectedErrorReturned()
        {
            const string inputFormat = "yy";
            const string addTime = "01";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Years == 1)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// parse time using yyyy input format expected year returned.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingYyyyExpectedYearReturned()
        {
            const string inputFormat = "yyyy";
            const string addTime = "1920";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Years == 1920)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using mm input format expected months returned correctly.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingMmExpectedMonthsReturnedCorrectly()
        {
            const string inputFormat = "mm";
            const string addTime = "02";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Months == 2)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using m input format expected month returned as single digit.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_m_Expected_MonthReturnedAsSingleDigit()
        {
            const string inputFormat = "m";
            const string addTime = "02";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Months == 2)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using M as input format expected month returned as single digit.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingMExpectedMonthReturnedAsSingleDigit()
        {
            const string inputFormat = "M";
            const string addTime = "2";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Months == 2)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using MM as input format expected month returned as padded digit.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingMmExpectedMonthReturnedAsPaddedDigit()
        {
            const string inputFormat = "MM";
            const string addTime = "02";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Months == 2)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using d as input format expected day returned as single digit.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingDExpectedDayReturnedAsSingleDigit()
        {
            const string inputFormat = "d";
            const string addTime = "1";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Days == 1)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using dd as input format expected day returned as padded digit.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingDdExpectedDayReturnedAsPaddedDigit()
        {
            const string inputFormat = "dd";
            const string addTime = "01";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            if (returnTO.Days == 1)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }





        /// <summary>
        /// Parse time using ss as input format expected seconds added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingSsExpectedSecondsAddedToBaseTime()
        {
            const string InputFormat = "ss";
            const string AddTime = "3";
            _parser.TryParseTime(AddTime, InputFormat, out IDateTimeResultTO returnTo, out string result);
            if (returnTo.Seconds == 3)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorect object returned");
            }
        }



        /// <summary>
        /// Parse time using simple literal expected date added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingSimpleLiteralExpectedDateAddedToBaseTime()
        {
            const string InputFormat = "'The date is : 'ddmmyyyy";
            const string AddTime = "The date is : 02010021";
            _parser.TryParseTime(AddTime, InputFormat, out IDateTimeResultTO returnTo, out string result);
            if (returnTo.Days == 2 && returnTo.Months == 1 && returnTo.Years == 21)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorect object returned");
            }
        }





        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNumberWeekOfYear_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            //IsNumberWeekOfYear(string data, bool treatAsTime)
            var dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("IsNumberWeekOfYear");
            var result = methodInfo.Invoke(dateTimeParser, new object[] { "A", true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(result.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNumberSeconds_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            //IsNumberWeekOfYear(string data, bool treatAsTime)
            var dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("IsNumberSeconds");
            var result = methodInfo.Invoke(dateTimeParser, new object[] { "A", true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(result.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNumberMinutes_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            //IsNumberWeekOfYear(string data, bool treatAsTime)
            var dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("IsNumberMinutes");
            var result = methodInfo.Invoke(dateTimeParser, new object[] { "A", true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(result.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNumberDayOfWeek_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            //IsNumberWeekOfYear(string data, bool treatAsTime)
            var dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("IsNumberDayOfWeek");
            var result = methodInfo.Invoke(dateTimeParser, new object[] { "A", true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(result.ToString()));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNumber24H_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            //IsNumberWeekOfYear(string data, bool treatAsTime)
            var dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("IsNumber24H");
            var result = methodInfo.Invoke(dateTimeParser, new object[] { "A", true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(result.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNumberMilliseconds_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            //IsNumberWeekOfYear(string data, bool treatAsTime)
            var dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("IsNumberMilliseconds");
            var result = methodInfo.Invoke(dateTimeParser, new object[] { "A", true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(result.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDayOfWeekInt_GivenSunday_ShouldReturn7()
        {
            //---------------Set up test pack-------------------
            var dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("GetDayOfWeekInt", BindingFlags.Static | BindingFlags.NonPublic);
            var result = methodInfo.Invoke(dateTimeParser, new object[] { DayOfWeek.Sunday });
            //---------------Test Result -----------------------
            Assert.AreEqual(7, int.Parse(result.ToString()));
        }
    }
    #endregion
    #endregion
}
