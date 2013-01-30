using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Converters.DateAndTime;
using Dev2.Converters.DateAndTime.Interfaces;
using System.Globalization;

namespace Unlimited.UnitTest.Framework.ConverterTests.DateTimeTests {
    /// <summary>
    /// Summary description for DateTimeParserTests
    /// </summary>
    [TestClass]
    public class DateTimeParserTests {
        static IDateTimeParser parser;
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) {
            parser = DateTimeConverterFactory.CreateParser();
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
        public void TryParseDateTime_AllArgsValid_Expected_ParserReturnsCorrectlyFormattedDateString() {
            string result;
            string inputString = "14101988";
            string formatString = "ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the date time parser can parse inputs containing literals with input format supplied
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithSimpleLiteral_Expected_InputStringParsedCorrectly() {
            string result;
            string inputString = "My birthday is : 14101988";
            string formatString = "'My birthday is : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the the parser is able to parse a date time input given the input format does not quote literals
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithInferredLiteral_Expected_InputCorrectlyParsed() {
            string result;
            string inputString = "Please Give Cake On : 14101988";
            string formatString = "Please Give Cake On : ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the Parser is able to parse an input given escaped literals
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithInferredLiteralContainingEscapedLiteralCharacter_Expected_InputParsedWithEscapedCharactersIncluded() {
            string result;
            string inputString = "Please Give ' Cake On : 14101988";
            string formatString = "Please Give \\' Cake On : ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }


        /// <summary>
        /// Test to ensure that multiple mixed escaped regions are correctly parsed
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithMixedInferredAndDelimtedLiteralRegions_Expected_InputRegionCorrectlyParsedForDateTimeValues() {
            string result;
            string inputString = "Please Give Cake On : 14101988";
            string formatString = "Please Give \'Cake On :\' ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Test to ensure that parser can parse out regions using a different escape sequence
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDoubleEscapedLiteralCharacterInInferredLiteralRegion_Expected_InputParsedCorrectly() {
            string result;
            string inputString = "Please Give ' Cake On : 14101988";
            string formatString = "Please Give ''' Cake On : ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the parser is able to parse literals within a literal region
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDoubleEscapedLiteralCharacterInLiteralRegion_Expected_InputParsedOutCorrectly() {
            string result;
            string inputString = "Please Give ' Cake On : 14101988";
            string formatString = "'Please Give ''' Cake On : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the parser is able to parse escaped literals within a literal region 
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithLiteralContainingTheLiteralEscapeCharacter_Expected_DateParsedCorrectly() {
            string result;
            string inputString = "Brendon's birthday is : 14101988";
            string formatString = "'Brendon\\'s birthday is : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the Parser is able to parse literal regions that have escape characters 
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithLiteralContainingTheLiteralEscapeCharacterAndAnEscapeCharacter_Expected_EscapedCharacter() {
            string result;
            string inputString = "Brendon\\'s birthday is : 14101988";
            string formatString = "'Brendon\\\\\\'s birthday is : 'ddmmyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that complex literal strings are handled by the datetime parser
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithComplexLiteral_Expected_InptParsedWithAllSpecifiedInputsInFormat() {
            string result;
            string inputString = "I was born on the 14th day of October in the year of 1988";
            string formatString = "'I was born on the 'dd'th day of 'MM' in the year of 'yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that Numeric input passed to the parser parser out region
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithSomeLiteralAndSomeNumeric_Expected_AllDataParsedThroughDateTimeParser() {
            string result;
            string inputString = "Tuesday,27 February 2009";
            string formatString = "DW','dd' 'MM' 'yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 27 && returnDate.Month == 02 && returnDate.Year == 2009 && returnDate.DayOfWeek == DayOfWeek.Friday) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that the parser can parse input data containing slases
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithSlashed_Expected_DateTimeParsedOut() {
            string result;
            string inputString = "14/10/1988";
            string formatString = "dd'/'mm'/'yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that dot characters are treated as literal strings 
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDotes_Expected_DateTimeParsedOUtCorrectlyWithDotExcluded() {
            string result;
            string inputString = "14.10.1988";
            string formatString = "dd'.'mm'.'yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that tbe parser is able to parse an input containing dashes and retrieves the correct date from the input string
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDashesUsingNumeric_Expected_InputDataParsedOutAccordingToInputFormat() {
            string result;
            string inputString = "14-10-1988";
            string formatString = "dd'-'mm'-'yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Test to ensure that a date format containing a dash character is not encoded into datetime input value
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDashesUsingNumericAndLiteral_Expected_DatePartsParsedOutCorrectly() {
            string result;
            string inputString = "14-October-1988";
            string formatString = "dd'-'MM'-'yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that shorthand dates are correctly parsed when parsing dates with literal input data.
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_WithDashesUsingNumericAndLiteralShorthand_Expected_DatePartsParsedOutCorrectly() {
            string result;
            string inputString = "14-Oct-1988";
            string formatString = "dd'-'M'-'yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Tests that shorthand dates are correctly parsed when parsing dates with literal input data.
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_InputFormat_WithDashesUsingNumericAndLiteralShorthand_Expected_DatePartReturnedInInputString() {
            string result;
            string inputString = "14-Oct-1988";
            string formatString = "dd'-'M'-'yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 14 && returnDate.Month == 10 && returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion Valid Parse Formats

        #region Invalid Parse Formats

        /// <summary>
        /// Tests that the DateTime parser does not throw an exception when given a null value for format but indicates that it is unable to parse a datetime 
        /// object
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_InputFormat_NULL_Expected_ParserReturnsFalseIndicatingUnableToParse() {
            bool IsParseable;
            string result;
            string inputString = "14101988";
            string formatString = null;
            IDateTimeResultTO dateTimeResult;

            IsParseable = parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.IsFalse(IsParseable);
        }

        /// <summary>
        /// Tests that the parser does not parse datetime inputs when the input format does not match the input string
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_Invalid_Expected_FalseReturnedByParserIndicatingItIsUnableToParseTheInput() {
            bool IsParseable;
            string result;
            string inputString = "baisd78qh378hd123bhd18n18378";
            string formatString = "ddmmyyyy";
            IDateTimeResultTO dateTimeResult;

            IsParseable = parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.IsFalse(IsParseable);
        }

        /// <summary>
        /// Tests that the DateTime parser does not throw an exception when given a null value for input format but indicates that it is unable to parse a datetime 
        /// object
        /// </summary>
        [TestMethod]
        public void TryParseDateTime_DateTime_NULL_Expected_FalseReturnedByParser() {
            bool IsParseable;
            string result;
            string inputString = null;
            string formatString = "ddmmyyyy";
            IDateTimeResultTO dateTimeResult;

            IsParseable = parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.IsFalse(IsParseable);
        }

        #endregion Invalid Parse Formats

        #region yy Tests

        [TestMethod]
        public void TryParseDateTime_Format_yy_Expected_LastTwoDigitsOfYearAreParsed() {
            string result;
            string inputString = "12";
            string formatString = "yy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Year == 2012) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion yy Tests

        #region Timezone Tests

        [TestMethod]
        public void TryParseDateTime_Format_Z_Expected_TimezoneCorrectlyParsed() {
            string result;
            string inputString = "GMT";
            string formatString = "Z";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.ShortName);
        }

        [TestMethod]
        public void TryParseDateTime_Format_ZZ_Expected_TimeZoneRelativeToGMTParsedOutCorrectly() {
            string result;
            string inputString = "GMT+02:00";
            string formatString = "ZZ";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.Name);
        }

        [TestMethod]
        public void TryParseDateTime_Format_ZZZ_Expected_FullTimezoneValueParsedCorrectly() {
            string result;
            string inputString = "Greenwich Mean Time";
            string formatString = "ZZZ";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.LongName);
        }

        #endregion Timezone Tests

        #region yyyy Tests

        [TestMethod]
        public void TryParseDateTime_Format_yyyy_Expected_FullYearCorrectlyParsedOut() {
            string result;
            string inputString = "1988";
            string formatString = "yyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Year == 1988) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }


        #endregion yyyy tests

        #region mm Tetsts

        [TestMethod]
        public void TryParseDateTime_Format_mm_Expected_MonthParsedAsDoubleDigit() {
            string result;
            string inputString = "01";
            string formatString = "mm";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Month == 01) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion mm Tests

        #region m Tests

        [TestMethod]
        public void TryParseDateTime_Format_m_Expected_MonthParsedWithoutPadding() {
            string result;
            string inputString = "01";
            string formatString = "m";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Month == 01) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion m Tests

        #region M Tests

        [TestMethod]
        public void TryParseDateTime_Format_M_Expected_MonthAsShorthanfTextReturned() {
            string result;
            string inputString = "Feb";
            string formatString = "M";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Month == 2) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion M Tests

        #region MM Tests

        [TestMethod]
        public void TryParseDateTime_Format_MM_Expected_FullMonthNameReturnedFromParser() {
            string result;
            string inputString = "Febuary";
            string formatString = "MM";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Month == 01) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion MM Tests

        #region d Tests

        [TestMethod]
        public void TryParseDateTime_Format_d_Expected_DayOfTheMonthReturned() {
            string result;
            string inputString = "01";
            string formatString = "d";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 01) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion d Tests

        #region dd Tests

        [TestMethod]
        public void TryParseDateTime_Format_dd_Expected_DayReturnedWithPadding() {
            string result;
            string inputString = "01";
            string formatString = "dd";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.Day == 01) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion dd Tests

        #region DW Tests

        [TestMethod]
        public void TryParseDateTime_Format_DW_Expected_DayOfTheWeekReturned() {
            string result;
            string inputString = "Tuesday";
            string formatString = "DW";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.DayOfWeek == DayOfWeek.Tuesday) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion DW Tests

        #region dW Tests

        [TestMethod]
        public void TryParseDateTime_Format_dW_Expected_DayOfTheShortHandParsed() {
            string result;
            string inputString = "Tue";
            string formatString = "dW";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.DayOfWeek == DayOfWeek.Tuesday) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion dW Tests

        #region dw Tests

        [TestMethod]
        public void TryParseDateTime_Format_dw_Expected_DayOfTheWeekParsedCorrectlyAsNumber() {
            string result;
            string inputString = "5";
            string formatString = "dw";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.DayOfWeek == DayOfWeek.Friday) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion dw Tests

        #region dyyyyy Tests

        [TestMethod]
        public void TryParseDateTime_Format_dy_Expected_MilleniumParsedOutOfDate() {
            string result;
            string inputString = "502012";
            string formatString = "dyyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.DayOfYear == 50) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion dyyyyy Tests

        #region wyyyy Tests

        [TestMethod]
        public void TryParseDateTime_Format_w_Expected_Positive() {
            string result;
            string inputString = "202012";
            string formatString = "wyyyy";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            Calendar myCal = CultureInfo.InvariantCulture.Calendar;
            int weekOfYear = myCal.GetWeekOfYear(returnDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            if(weekOfYear == 20) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion wyyyy Tests

        #region 24h Tests

        [TestMethod]
        public void TryParseDateTime_Format_24h_Expected_Positive() {
            string result;
            string inputString = "14101988 15:42";
            string formatString = "'14101988 '24h':42'";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Hours == 15) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion 24h Tests

        #region 12h Tests

        [TestMethod]
        public void TryParseDateTime_Format_12h_Expected_Positive() {
            string result;
            string inputString = "14101988 03:42";
            string formatString = "'14101988 '12h':42'";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Hours == 3) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion 12h Tests

        #region min Tests

        [TestMethod]
        public void TryParseDateTime_Format_min_Expected_Positive() {
            string result;
            string inputString = "14101988 15:42";
            string formatString = "'14101988 15:'min";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Minutes == 42) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }


        #endregion min Tests

        #region ss Tests

        [TestMethod]
        public void TryParseDateTime_Format_ss_Expected_Positive() {
            string result;
            string inputString = "14101988 15:42:32:673";
            string formatString = "'14101988 15:42:'ss':673'";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Seconds == 32) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion ss Tests

        #region sp Tests

        [TestMethod]
        public void TryParseDateTime_Format_sp_Expected_Positive() {
            string result;
            string inputString = "14101988 15:42:32:673";
            string formatString = "'14101988 15:42:32:'sp";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Milliseconds == 673) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        #endregion sp Tests

        #region am/pm Tests

        [TestMethod]
        public void TryParseDateTime_Format_am_Expected_Positive() {
            string result;
            string inputString = "14101988 1:42:32:673 am";
            string formatString = "'14101988 1:42:32:673 'am/pm";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Hours == 0) {
                string formated = returnDate.ToString();
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseDateTime_Format_pm_Expected_Positive() {
            string result;
            string inputString = "14101988 1:42:32:673 pm";
            string formatString = "'14101988 1:42:32:673 'am/pm";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Hours == 12) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseDateTime_Format_pm_Where_Hours_Is_12_Expected_Positive() {
            string result;
            string inputString = "14101988 12:42:32 PM";
            string formatString = "ddmmyyyy 12h:min:ss am/pm";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Hours == 12) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseDateTime_Format_am_Where_Hours_Is_12_Expected_Positive() {
            string result;
            string inputString = "14101988 12:42:32 am";
            string formatString = "ddmmyyyy 12h:min:ss am/pm";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Hours == 0) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }


        [TestMethod]
        public void TryParseDateTime_Format_pm_Where_Hours_Is_1_Expected_Positive() {
            string result;
            string inputString = "14101988 1:42:32 PM";
            string formatString = "ddmmyyyy 12h:min:ss am/pm";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Hours == 13) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseDateTime_Format_am_Where_Hours_Is_1_Expected_Positive() {
            string result;
            string inputString = "14101988 1:42:32 AM";
            string formatString = "ddmmyyyy 12h:min:ss am/pm";

            IDateTimeResultTO dateTimeResult;
            parser.TryParseDateTime(inputString, formatString, out dateTimeResult, out result);
            DateTime returnDate = dateTimeResult.ToDateTime();

            if(returnDate.TimeOfDay.Hours == 1) {
                Assert.IsTrue(1 == 1);
            }
            else {
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
        public void TryParseTime_AllArgs_Valid_Expected_ParsedOutCorrectly() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "yy':'mm':'dd";
            string addTime = "01:02:03";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Years == 1 && returnTO.Months == 2 && returnTO.Days == 3) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time with input format as invalid expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_InputFormat_Invalid_Expected_ErrorReturned() {
            bool isParseable;
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "yy':'asdavvaad";
            string addTime = "01:02:03";
            isParseable = parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            Assert.IsFalse(isParseable);
        }


        /// <summary>
        /// Parse time with input format NULL expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_InputFormat_NULL_Expected_ErrorReturned() {
            bool isParseable;
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = null;
            string addTime = "01:02:03";
            isParseable = parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time invalid time expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Time_Invalid_Expected_ErrorReturned() {
            bool isParseable;
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "yy':'mm':'dd";
            string addTime = "sdfsdfsdfddf";
            isParseable = parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time with input time NULL expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Time_NULL_Expected_ErrorReturned() {
            bool isParseable;
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "yy':'mm':'dd";
            string addTime = null;
            isParseable = parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time using yy input format expected error returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_yy_Expected_ErrorReturned() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "yy";
            string addTime = "01";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Years == 1) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// parse time using yyyy input format expected year returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_yyyy_Expected_YearReturned() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "yyyy";
            string addTime = "1920";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Years == 1920) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using mm input format expected months returned correctly.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_mm_Expected_MonthsReturnedCorrectly() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "mm";
            string addTime = "02";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Months == 2) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using m input format expected month returned as single digit.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_m_Expected_MonthReturnedAsSingleDigit() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "m";
            string addTime = "02";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Months == 2) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using M as input format expected month returned as single digit.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_M_Expected_MonthReturnedAsSingleDigit() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "M";
            string addTime = "2";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Months == 2) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using MM as input format expected month returned as padded digit.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_MM_Expected_MonthReturnedAsPaddedDigit() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "MM";
            string addTime = "02";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Months == 2) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using d as input format expected day returned as single digit.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_d_Expected_DayReturnedAsSingleDigit() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "d";
            string addTime = "1";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Days == 1) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using dd as input format expected day returned as padded digit.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_dd_Expected_DayReturnedAsPaddedDigit() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "dd";
            string addTime = "01";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Days == 1) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseTime_Using_DW_Expected_Positive() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "DW";
            string addTime = "4";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.DaysOfWeek == 4) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseTime_Using_dW_Expected_Positive() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "dW";
            string addTime = "4";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.DaysOfWeek == 4) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseTime_Using_dw_Expected_Positive() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "dw";
            string addTime = "4";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.DaysOfWeek == 4) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void TryParseTime_Using_dy_Expected_Positive() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "dy";
            string addTime = "123";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.DaysOfYear == 123) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using w as input format expected week returned.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_w_Expected_WeekReturned() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "w";
            string addTime = "32";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Weeks == 32) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }


        /// <summary>
        /// Parse time using 24h input format expected hours added and returned as 24h.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_24h_Expected_HoursAddedAndReturnedAs24h() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "24h";
            string addTime = "21";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Hours == 21) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time using 12h input format expected hours added according to 12h.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_12h_Expected_HoursAddedAccordingTo12h() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "12h";
            string addTime = "3";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Hours == 3) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorect object returned");
            }
        }

        /// <summary>
        /// Parse time using min as input format expected minutes added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_min_Expected_MinutesAddedToBaseTime() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "min";
            string addTime = "34";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Minutes == 34) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorect object returned");
            }
        }

        /// <summary>
        /// Parse time using ss as input format expected seconds added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_ss_Expected_SecondsAddedToBaseTime() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "ss";
            string addTime = "3";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Seconds == 3) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorect object returned");
            }
        }

        /// <summary>
        /// Parse time using sp as input format expected split seconds added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_sp_Expected_SplitSecondsAddedToBaseTime() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "sp";
            string addTime = "3";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Milliseconds == 3) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorect object returned");
            }
        }

        /// <summary>
        /// Parse time using simple literal expected date added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_SimpleLiteral_Expected_DateAddedToBaseTime() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "'The date is : 'ddmmyyyy";
            string addTime = "The date is : 02010021";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Days == 2 && returnTO.Months == 1 && returnTO.Years == 21) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorect object returned");
            }
        }

        /// <summary>
        /// Parse time using complex literal expected date added to base time.
        /// </summary>
        [TestMethod]
        public void TryParseTime_Using_ComplexLiteral_Expected_DateAddedToBaseTime() {
            IDateTimeResultTO returnTO;
            string result;
            string inputFormat = "'I was born on the 'd'th of 'm' in the year 'yy";
            string addTime = "I was born on the 2th of 1 in the year 21";
            parser.TryParseTime(addTime, inputFormat, out returnTO, out result);
            if(returnTO.Days == 2 && returnTO.Months == 1 && returnTO.Years == 21) {
                Assert.IsTrue(1 == 1);
            }
            else {
                Assert.Fail("Incorect object returned");
            }
        }

        #endregion TryParseTime Tests
    }
}
