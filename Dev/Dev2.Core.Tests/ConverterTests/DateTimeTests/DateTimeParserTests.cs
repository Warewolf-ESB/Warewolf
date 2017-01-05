/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.ConverterTests.DateTimeTests
{
    /// <summary>
    /// Summary description for DateTimeParserTests
    /// </summary>
    [TestClass]
    public class DateTimeParserTests
    {
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
            _parser = DateTimeConverterFactory.CreateParser();
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
            string result;
            const string inputString = "14101988";
            const string formatString = "ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "My birthday is : 14101988";
            const string formatString = "'My birthday is : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "Please Give Cake On : 14101988";
            const string formatString = "Please Give Cake On : ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "Please Give ' Cake On : 14101988";
            const string formatString = "Please Give \\' Cake On : ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "Please Give Cake On : 14101988";
            const string formatString = "Please Give \'Cake On :\' ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "Please Give ' Cake On : 14101988";
            //2013.06.03: Ashley Lewis for bug 9601 - double escape not triple escape
            const string formatString = "'Please Give '' Cake On : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "Please Give ' Cake On : 14101988";
            //2013.06.03: Ashley Lewis for bug 9601 - double escape not triple escape
            const string formatString = "'Please Give '' Cake On : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "Brendon's birthday is : 14101988";
            const string formatString = "'Brendon\\'s birthday is : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "Brendon\\'s birthday is : 14101988";
            const string formatString = "'Brendon\\\\\\'s birthday is : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
        /// Tests that complex literal strings are handled by the datetime parser
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithComplexLiteral_Expected_InptParsedWithAllSpecifiedInputsInFormat()
        {
            string result;
            const string inputString = "I was born on the 14th day of October in the year of 1988";
            const string formatString = "'I was born on the 'dd'th day of 'MM' in the year of 'yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
        /// Tests that Numeric input passed to the parser parser out region
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithSomeLiteralAndSomeNumeric_Expected_AllDataParsedThroughDateTimeParser()
        {
            string result;
            const string inputString = "Tuesday,27 February 2009";
            const string formatString = "DW','dd' 'MM' 'yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 27 && returnDate.Month == 02 && returnDate.Year == 2009 && returnDate.DayOfWeek == DayOfWeek.Friday)
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
            string result;
            const string inputString = "14/10/1988";
            const string formatString = "dd'/'mm'/'yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "14.10.1988";
            const string formatString = "dd'.'mm'.'yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "14-10-1988";
            const string formatString = "dd'-'mm'-'yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
        /// Test to ensure that a date format containing a dash character is not encoded into datetime input value
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDashesUsingNumericAndLiteral_Expected_DatePartsParsedOutCorrectly()
        {
            string result;
            const string inputString = "14-October-1988";
            const string formatString = "dd'-'MM'-'yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
        /// Tests that shorthand dates are correctly parsed when parsing dates with literal input data.
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDashesUsingNumericAndLiteralShorthand_Expected_DatePartsParsedOutCorrectly()
        {
            string result;
            const string inputString = "14-Oct-1988";
            const string formatString = "dd'-'M'-'yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
        /// Tests that shorthand dates are correctly parsed when parsing dates with literal input data.
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_InputFormat_WithDashesUsingNumericAndLiteralShorthand_Expected_DatePartReturnedInInputString()
        {
            string result;
            const string inputString = "14-Oct-1988";
            const string formatString = "dd'-'M'-'yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        //07.03.2013: Ashley Lewis - PBI 9167: Null input format is now valid
        [TestMethod]
        public void TryParseDateTimeWithInputFormatNULLExpectedDefaultFormatUsed()
        {
            string result;
            string inputString = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            IDateTimeResultTO dateTimeResult;

            string defaultFormat = GlobalConstants.Dev2DotNetDefaultDateTimeFormat;
            string tmpError;
            string translatedFormat = _parser.TranslateDotNetToDev2Format(defaultFormat, out tmpError);
            var IsParseable = _parser.TryParseDateTime(inputString, null, out dateTimeResult, out result);

            const string s = "Default format: {0}\nTranslated format: {1}\nDateTime.Now= {2}\nResult: {3}";

            Assert.IsTrue(IsParseable, string.Format(s, defaultFormat, translatedFormat, inputString, result));
        }

        [TestMethod]
        [TestCategory("DateTimeParserUnitTest")]
        [Description("Test for blank DateTimeParser input time defaults to system time")]
        [Owner("Ashley Lewis")]
        public void DateTimeParser_DateTimeParserUnitTest_ParseWithBlankInput_DateTimeNowIsUsed()
        {
            string result;
            string inputString = string.Empty;
            string formatString = string.Empty;
            IDateTimeResultTO dateTimeResult;

            string defaultFormat = GlobalConstants.Dev2DotNetDefaultDateTimeFormat;
            string tmpError;
            string translatedFormat = _parser.TranslateDotNetToDev2Format(defaultFormat, out tmpError);
            var IsParseable = _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            const string s = "Default format: {0}\nTranslated format: {1}\nDateTime.Now= {2}\nResult: {3}";

            Assert.IsTrue(IsParseable, string.Format(s, defaultFormat, translatedFormat, inputString, result));
        }

        /// <summary>
        /// Tests that the parser returns the date time object with correctly evaluated date parts
        /// </summary>
        [TestMethod]
        [TestCategory("DateTimeParserUnitTest")]
        [Description("Test for complex DateTimeParser input datetime without trailing spaces")]
        [Owner("Ashley Lewis")]
        public void TryParseDateTime_ComplexArgsWithoutTrailingSpaces_Expected_ParserReturnsCorrectlyFormattedDateString()
        {
            string result;
            const string InputString = "Year 44 week 43 yearweak (UTC+02:00) Harare, Pretoria | South Africa Standard Time | South Africa Standard Time | October | Oct | 10 | 290 | Sunday | Sun | 7 |16 | 22 | 2044/10/16 10:25:36.953 PM A.D.";
            const string FormatString = "'Year' yy 'week' ww 'yearweak' ZZZ | ZZ | Z | MM | M | m | dy | DW | dW | dw |d | 24h | yyyy/mm/dd 12h:min:ss.sp am/pm Era";

            IDateTimeResultTO dateTimeResult;
            Assert.IsTrue(_parser.TryParseDateTime(InputString, FormatString, out dateTimeResult, out result), "Cannot parse valid date time");
            var returnDate = dateTimeResult.ToDateTime();

            Assert.IsTrue(returnDate.Day == 16 && returnDate.Month == 10 && returnDate.Year == 2044, "Incorrect object returned");
        }

        /// <summary>
        /// Tests that the parser returns the date time object with correctly evaluated date parts
        /// </summary>
        [TestMethod]
        [TestCategory("DateTimeParserUnitTest")]
        [Description("Test for complex DateTimeParser input datetime with trailing spaces")]
        [Owner("Ashley Lewis")]
        public void TryParseDateTime_ComplexArgsWithTrailingSpaces_Expected_ParserReturnsCorrectlyFormattedDateString()
        {
            string result;
            const string InputString = "Year 44 week 43 yearweak (UTC+02:00) Harare, Pretoria | South Africa Standard Time | South Africa Standard Time | October | Oct | 10 | 290 | Sunday | Sun | 7 |16 | 22 | 2044/10/16 10:25:36.953 PM A.D. ";
            const string FormatString = "'Year' yy 'week' ww 'yearweak' ZZZ | ZZ | Z | MM | M | m | dy | DW | dW | dw |d | 24h | yyyy/mm/dd 12h:min:ss.sp am/pm Era ";
            IDateTimeResultTO dateTimeResult;

            var resultBool = _parser.TryParseDateTime(InputString, FormatString, out dateTimeResult, out result);

            Assert.IsTrue(result == string.Empty, "Error returned: " + result);
            Assert.IsTrue(resultBool, "Cannot parse valid date time.");
            Assert.IsTrue(dateTimeResult.Days == 16 && dateTimeResult.Months == 10 && dateTimeResult.Years == 2044, "Incorrect object returned");
        }

        #endregion Valid Parse Formats

        #region Invalid Parse Formats

        /// <summary>
        /// Tests that the parser does not parse datetime inputs when the input format does not match the input string
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_Invalid_Expected_FalseReturnedByParserIndicatingItIsUnableToParseTheInput()
        {
            string result;
            const string inputString = "baisd78qh378hd123bhd18n18378";
            const string formatString = "ddmmyyyy";
            IDateTimeResultTO dateTimeResult;

            var IsParseable = _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.IsFalse(IsParseable);
        }

        /// <summary>
        /// Tests that the DateTime parser does not throw an exception when given a null value for input format but indicates that it is unable to parse a datetime 
        /// object
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_NULL_Expected_FalseReturnedByParser()
        {
            string result;
            const string formatString = "ddmmyyyy";
            IDateTimeResultTO dateTimeResult;

            var IsParseable = _parser.TryParseDateTime(null, formatString, out dateTimeResult, out result);

            Assert.IsFalse(IsParseable);
        }

        #endregion Invalid Parse Formats

        #region yy Tests

        [TestMethod]
        public void TryParseDateTime_Format_yy_Expected_LastTwoDigitsOfYearAreParsed()
        {
            string result;
            const string inputString = "12";
            const string formatString = "yy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Year == 2012)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion yy Tests

        #region Timezone Tests

        [TestMethod]
        public void TryParseDateTime_Format_Z_Expected_TimezoneCorrectlyParsed()
        {
            string result;
            const string inputString = "GMT";
            const string formatString = "Z";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.ShortName);
        }

        [TestMethod]
        public void TryParseDateTime_Format_ZZ_Expected_TimeZoneRelativeToGMTParsedOutCorrectly()
        {
            string result;
            const string inputString = "GMT+02:00";
            const string formatString = "ZZ";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.Name);
        }

        [TestMethod]
        public void TryParseDateTime_Format_ZZZ_Expected_FullTimezoneValueParsedCorrectly()
        {
            string result;
            const string inputString = "Greenwich Mean Time";
            const string formatString = "ZZZ";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.LongName);
        }

        #endregion Timezone Tests

        #region yyyy Tests

        [TestMethod]
        public void TryParseDateTime_Format_yyyy_Expected_FullYearCorrectlyParsedOut()
        {
            string result;
            const string inputString = "1988";
            const string formatString = "yyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "01";
            const string formatString = "mm";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "01";
            const string formatString = "m";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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

        #region M Tests

        [TestMethod]
        public void TryParseDateTime_Format_M_Expected_MonthAsShorthanfTextReturned()
        {
            string result;
            const string inputString = "Feb";
            const string formatString = "M";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Month == 2)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion M Tests

        #region MM Tests

        [TestMethod]
        public void TryParseDateTime_Format_MM_Expected_FullMonthNameReturnedFromParser()
        {
            string result;
            const string inputString = "Febuary";
            const string formatString = "MM";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "01";
            const string formatString = "d";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "01";
            const string formatString = "dd";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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

        #region DW Tests

        [TestMethod]
        public void TryParseDateTime_Format_DW_Expected_DayOfTheWeekReturned()
        {
            string result;
            const string inputString = "Tuesday";
            const string formatString = "DW";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion DW Tests

        #region dW Tests

        [TestMethod]
        public void TryParseDateTime_Format_dW_Expected_DayOfTheShortHandParsed()
        {
            string result;
            const string inputString = "Tue";
            const string formatString = "dW";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion dW Tests

        #region dw Tests

        [TestMethod]
        public void TryParseDateTime_Format_dw_Expected_DayOfTheWeekParsedCorrectlyAsNumber()
        {
            string result;
            const string inputString = "5";
            const string formatString = "dw";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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

        #region dyyyyy Tests

        [TestMethod]
        public void TryParseDateTime_Format_dy_Expected_MilleniumParsedOutOfDate()
        {
            string result;
            const string inputString = "502012";
            const string formatString = "dyyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.DayOfYear == 50)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion dyyyyy Tests

        #region wyyyy Tests

        [TestMethod]
        public void TryParseDateTime_Format_w_Expected_Positive()
        {
            string result;
            const string inputString = "202012";
            const string formatString = "wyyyy";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            Calendar myCal = CultureInfo.InvariantCulture.Calendar;
            int weekOfYear = myCal.GetWeekOfYear(returnDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            if (weekOfYear == 20)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion wyyyy Tests

        #region 24h Tests

        [TestMethod]
        public void TryParseDateTime_Format_24h_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 15:42";
            const string formatString = "'14101988 '24h':42'";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 15)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion 24h Tests

        #region 12h Tests

        [TestMethod]
        public void TryParseDateTime_Format_12h_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 03:42";
            const string formatString = "'14101988 '12h':42'";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 3)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion 12h Tests

        #region min Tests

        [TestMethod]
        public void TryParseDateTime_Format_min_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 15:42";
            const string formatString = "'14101988 15:'min";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
            string result;
            const string inputString = "14101988 15:42:32:673";
            const string formatString = "'14101988 15:42:'ss':673'";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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

        #region sp Tests

        [TestMethod]
        public void TryParseDateTime_Format_sp_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 15:42:32:673";
            const string formatString = "'14101988 15:42:32:'sp";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Milliseconds == 673)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion sp Tests

        #region am/pm Tests

        [TestMethod]
        public void TryParseDateTime_Format_am_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 1:42:32:673 am";
            const string formatString = "'14101988 1:42:32:673 'am/pm";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
        public void TryParseDateTime_Format_pm_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 1:42:32:673 pm";
            const string formatString = "'14101988 1:42:32:673 'am/pm";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 12)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseDateTime_Format_pm_Where_Hours_Is_12_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 12:42:32 PM";
            const string formatString = "ddmmyyyy 12h:min:ss am/pm";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 12)
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
            string result;
            const string inputString = "14101988 12:42:32 am";
            const string formatString = "ddmmyyyy 12h:min:ss am/pm";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

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
        public void TryParseDateTime_Format_pm_Where_Hours_Is_1_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 1:42:32 PM";
            const string formatString = "ddmmyyyy 12h:min:ss am/pm";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 13)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseDateTime_Format_am_Where_Hours_Is_1_Expected_Positive()
        {
            string result;
            const string inputString = "14101988 1:42:32 AM";
            const string formatString = "ddmmyyyy 12h:min:ss am/pm";

            IDateTimeResultTO dateTimeResult;
            _parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 1)
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "yy':'mm':'dd";
            const string addTime = "01:02:03";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "yy':'asdavvaad";
            const string addTime = "01:02:03";
            var isParseable = _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            Assert.IsFalse(isParseable);
        }


        /// <summary>
        /// Parse time with input format NULL expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_InputFormat_NULL_Expected_ErrorReturned()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string addTime = "01:02:03";
            var isParseable = _parser.TryParseTime(addTime, null, out returnTO, out result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time invalid time expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Time_Invalid_Expected_ErrorReturned()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "yy':'mm':'dd";
            const string addTime = "sdfsdfsdfddf";
            var isParseable = _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time with input time NULL expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTimeTimeNullExpectedErrorReturned()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "yy':'mm':'dd";
            var isParseable = _parser.TryParseTime(null, inputFormat, out returnTO, out result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time using yy input format expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingYyExpectedErrorReturned()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "yy";
            const string addTime = "01";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "yyyy";
            const string addTime = "1920";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "mm";
            const string addTime = "02";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "m";
            const string addTime = "02";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "M";
            const string addTime = "2";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "MM";
            const string addTime = "02";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "d";
            const string addTime = "1";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
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
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "dd";
            const string addTime = "01";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if (returnTO.Days == 1)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseTime_Using_DW_Expected_Positive()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "DW";
            const string addTime = "4";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if (returnTO.DaysOfWeek == 4)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseTimeUsing_DWExpectedPositive()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "dW";
            const string addTime = "4";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if (returnTO.DaysOfWeek == 4)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseTimeUsingDwExpectedPositive()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "dw";
            const string addTime = "4";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if (returnTO.DaysOfWeek == 4)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseTimeUsingDyExpectedPositive()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "dy";
            const string addTime = "123";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if (returnTO.DaysOfYear == 123)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using w as input format expected week returned.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingWExpectedWeekReturned()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "w";
            const string addTime = "32";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if (returnTO.Weeks == 32)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }


        /// <summary>
        /// Parse time using 24h input format expected hours added and returned as 24h.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsing24HExpectedHoursAddedAndReturnedAs24H()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "24h";
            const string addTime = "21";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if (returnTO.Hours == 21)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using 12h input format expected hours added according to 12h.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsing12HExpectedHoursAddedAccordingTo12H()
        {
            IDateTimeResultTO returnTO;
            string result;
            const string inputFormat = "12h";
            const string addTime = "3";
            _parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if (returnTO.Hours == 3)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorect object returned");
            }
        }

        /// <summary>
        /// Parse time using min as input format expected minutes added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingMinExpectedMinutesAddedToBaseTime()
        {
            IDateTimeResultTO returnTo;
            string result;
            const string InputFormat = "min";
            const string AddTime = "34";
            _parser.TryParseTime(AddTime, InputFormat, out returnTo, out result);
            if (returnTo.Minutes == 34)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorect object returned");
            }
        }

        /// <summary>
        /// Parse time using ss as input format expected seconds added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingSsExpectedSecondsAddedToBaseTime()
        {
            IDateTimeResultTO returnTo;
            string result;
            const string InputFormat = "ss";
            const string AddTime = "3";
            _parser.TryParseTime(AddTime, InputFormat, out returnTo, out result);
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
        /// Parse time using sp as input format expected split seconds added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingSpExpectedSplitSecondsAddedToBaseTime()
        {
            IDateTimeResultTO returnTo;
            string result;
            const string InputFormat = "sp";
            const string AddTime = "3";
            _parser.TryParseTime(AddTime, InputFormat, out returnTo, out result);
            if (returnTo.Milliseconds == 3)
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
            IDateTimeResultTO returnTo;
            string result;
            const string InputFormat = "'The date is : 'ddmmyyyy";
            const string AddTime = "The date is : 02010021";
            _parser.TryParseTime(AddTime, InputFormat, out returnTo, out result);
            if (returnTo.Days == 2 && returnTo.Months == 1 && returnTo.Years == 21)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorect object returned");
            }
        }

        /// <summary>
        /// Parse time using complex literal expected date added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTimeUsingComplexLiteralExpectedDateAddedToBaseTime()
        {
            IDateTimeResultTO returnTo;
            string result;
            const string InputFormat = "'I was born on the 'd'th of 'm' in the year 'yy";
            const string AddTime = "I was born on the 2th of 1 in the year 21";
            _parser.TryParseTime(AddTime, InputFormat, out returnTo, out result);
            if (returnTo.Days == 2 && returnTo.Months == 1 && returnTo.Years == 21)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorect object returned");
            }
        }

        #endregion TryParseTime Tests

        #region Dot Net Translator

        #region Dates

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatOfDayoftheMonthExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "d, dd, ddd, dddd";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "d', 'dd', 'dW', 'DW");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatOfMonthExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "MMMM, MMM, MM, M";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "MM', 'M', 'mm', 'm");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForYearsExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "yyyyy, yyyy, yy, y, yyy";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "yyyyy', 'yyyy', 'yy', 'y', 'yyy");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForLongDateExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "dddd, dd MMMM yyyy";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "DW', 'dd' 'MM' 'yyyy");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForShortDateExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "MM/dd/yyyy";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "mm'/'dd'/'yyyy");
        }

        #endregion

        #region Times

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForHoursExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "h, hh, H, HH";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "12h', '12h', '24h', '24h");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForMinutesExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "m, mm";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "min', 'min");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForSecondsExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "s, ss";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "s', 'ss");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForSplitSecondsExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "f, ff, fff, ffff, ffffff, fffffff,";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "f', 'ff', 'sp', 'ffff', 'ffffff', 'fffffff','");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForNonZeroSplitSecondsExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "F- FF- FFF- FFFF- FFFFFF- FFFFFFF-";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "F'- 'FF'- 'FFF'- 'FFFF'- 'FFFFFF'- 'FFFFFFF'-'");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForShortTimeExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "HH:mm";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "24h':'min");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForLongTimeExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "HH:mm:ss.fffffff";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "24h':'min':'ss'.'fffffff");
        }

        #endregion

        #region Time Zones

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForTimeZoneExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "K";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "K");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForTimeZonesExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "'Hours offset from UTC: 'z', Hours offset from UTC, with a leading zero: 'zz', Offset with minutes: 'zzz";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "'Hours offset from UTC: 'z', Hours offset from UTC, with a leading zero: 'zz', Offset with minutes: 'zzz");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForRoundTripExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "yyyy'-'mm'-'dd'T'24h':'min':'ss'.'fffffffK");
        }

        #endregion

        #region Special

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForAmpmDesignatorExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "t, tt";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "t', 'am/pm");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForEraExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "g, gg";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "g', 'gg");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForFullDateShortTimeExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "dddd, dd MMMM yyyy HH:mm";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "DW', 'dd' 'MM' 'yyyy' '24h':'min");
        }

        [TestMethod]
        public void TranslateFormatFromDotNetWhereFormatForFullLongTimeExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new DateTimeParser();
            string inputFormat = "dddd', 'dd MMMM yyyy HH:mm:ss";
            string error;

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "DW', 'dd' 'MM' 'yyyy' '24h':'min':'ss");
        }

        #endregion

        #endregion

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNumberWeekOfYear_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            //IsNumberWeekOfYear(string data, bool treatAsTime)
            DateTimeParserHelper dateTimeParser = new DateTimeParserHelper();
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
            DateTimeParserHelper dateTimeParser = new DateTimeParserHelper();
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
            DateTimeParserHelper dateTimeParser = new DateTimeParserHelper();
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
            DateTimeParserHelper dateTimeParser = new DateTimeParserHelper();
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
            DateTimeParserHelper dateTimeParser = new DateTimeParserHelper();
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
            DateTimeParserHelper dateTimeParser = new DateTimeParserHelper();
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
            DateTimeParserHelper dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("GetDayOfWeekInt", BindingFlags.Static | BindingFlags.NonPublic);
            var result = methodInfo.Invoke(dateTimeParser, new object[] { DayOfWeek.Sunday });
            //---------------Test Result -----------------------
            Assert.AreEqual(7 , int.Parse(result.ToString()));
        }
    }
}
