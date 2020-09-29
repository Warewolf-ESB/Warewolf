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
using Warewolf.Resource.Errors;

namespace Dev2.Tests.ConverterTests.DateTimeTests
{
    [TestClass]
    public class DateTimeParserTests
    {
        static IDateTimeParser _parser;
        
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void PreConditions()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-ZA");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-ZA");

            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
        }

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            _parser = DateTimeConverterFactory.CreateParser();
        }

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_AllArgsValid_Expected_ParserReturnsCorrectlyFormattedDateString()
        {
            const string inputString = "14101988";
            const string formatString = "ddmmyyyy";

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

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithSimpleLiteral_Expected_InputStringParsedCorrectly()
        {
            const string inputString = "My birthday is : 14101988";
            const string formatString = "'My birthday is : 'ddmmyyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithInferredLiteral_Expected_InputCorrectlyParsed()
        {
            const string inputString = "Please Give Cake On : 14101988";
            const string formatString = "Please Give Cake On : ddmmyyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithInferredLiteralContainingEscapedLiteralCharacter_Expected_InputParsedWithEscapedCharactersIncluded()
        {
            const string inputString = "Please Give ' Cake On : 14101988";
            const string formatString = "Please Give \\' Cake On : ddmmyyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithMixedInferredAndDelimtedLiteralRegions_Expected_InputRegionCorrectlyParsedForDateTimeValues()
        {
            const string inputString = "Please Give Cake On : 14101988";
            const string formatString = "Please Give \'Cake On :\' ddmmyyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithDoubleEscapedLiteralCharacterInInferredLiteralRegion_Expected_InputParsedCorrectly()
        {
            const string inputString = "Please Give ' Cake On : 14101988";
            const string formatString = "'Please Give '' Cake On : 'ddmmyyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithDoubleEscapedLiteralCharacterInLiteralRegion_Expected_InputParsedOutCorrectly()
        {
            const string inputString = "Please Give ' Cake On : 14101988";
            const string formatString = "'Please Give '' Cake On : 'ddmmyyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithLiteralContainingTheLiteralEscapeCharacter_Expected_DateParsedCorrectly()
        {
            const string inputString = "Brendon's birthday is : 14101988";
            const string formatString = "'Brendon\\'s birthday is : 'ddmmyyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithLiteralContainingTheLiteralEscapeCharacterAndAnEscapeCharacter_Expected_EscapedCharacter()
        {
            const string inputString = "Brendon\\'s birthday is : 14101988";
            const string formatString = "'Brendon\\\\\\'s birthday is : 'ddmmyyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithComplexLiteral_Expected_InptParsedWithAllSpecifiedInputsInFormat()
        {
            const string inputString = "I was born on the 14th day of October in the year of 1988";
            const string formatString = "'I was born on the 'dd'th day of 'MM' in the year of 'yyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithSomeLiteralAndSomeNumeric_Expected_AllDataParsedThroughDateTimeParser()
        {
            const string inputString = "Tuesday,27 February 2009";
            const string formatString = "DW','dd' 'MM' 'yyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Day == 27 && returnDate.Month == 02 && returnDate.Year == 2009 && returnDate.DayOfWeek == DayOfWeek.Friday)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithSlashed_Expected_DateTimeParsedOut()
        {
            const string inputString = "14/10/1988";
            const string formatString = "dd'/'mm'/'yyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithDotes_Expected_DateTimeParsedOUtCorrectlyWithDotExcluded()
        {
            const string inputString = "14.10.1988";
            const string formatString = "dd'.'mm'.'yyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithDashesUsingNumeric_Expected_InputDataParsedOutAccordingToInputFormat()
        {
            const string inputString = "14-10-1988";
            const string formatString = "dd'-'mm'-'yyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithDashesUsingNumericAndLiteral_Expected_DatePartsParsedOutCorrectly()
        {
            const string inputString = "14-October-1988";
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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_WithDashesUsingNumericAndLiteralShorthand_Expected_DatePartsParsedOutCorrectly()
        {
            const string inputString = "14-Oct-1988";
            const string formatString = "dd'-'M'-'yyyy";

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

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_InputFormat_WithDashesUsingNumericAndLiteralShorthand_Expected_DatePartReturnedInInputString()
        {
            const string inputString = "14-Oct-1988";
            const string formatString = "dd'-'M'-'yyyy";

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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTimeWithInputFormatNULLExpectedDefaultFormatUsed()
        {
            var inputString = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            var defaultFormat = GlobalConstants.PreviousDev2DotNetDefaultDateTimeFormat;
            var translatedFormat = _parser.TranslateDotNetToDev2Format(defaultFormat, out string tmpError);
            var IsParseable = _parser.TryParseDateTime(inputString, null, out IDateTimeResultTO dateTimeResult, out string result);

            const string s = "Default format: {0}\nTranslated format: {1}\nDateTime.Now= {2}\nResult: {3}";

            Assert.IsTrue(IsParseable, string.Format(s, defaultFormat, translatedFormat, inputString, result));
        }

        [TestMethod]
        [TestCategory("DateTimeParserUnitTest")]
        [Description("Test for blank DateTimeParser input time defaults to system time")]
        [Owner("Ashley Lewis")]
        public void DateTimeParser_DateTimeParserUnitTest_ParseWithBlankInput_DateTimeNowIsUsed()
        {
            var inputString = string.Empty;
            var formatString = string.Empty;

            var defaultFormat = GlobalConstants.PreviousDev2DotNetDefaultDateTimeFormat;
            var translatedFormat = _parser.TranslateDotNetToDev2Format(defaultFormat, out string tmpError);
            var IsParseable = _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);

            const string s = "Default format: {0}\nTranslated format: {1}\nDateTime.Now= {2}\nResult: {3}";

            Assert.IsTrue(IsParseable, string.Format(s, defaultFormat, translatedFormat, inputString, result));
        }
        
        [TestMethod]
        [TestCategory("DateTimeParserUnitTest")]
        [Description("Test for complex DateTimeParser input datetime without trailing spaces")]
        [Owner("Ashley Lewis")]
        public void DateTimeParser_TryParseDateTime_ComplexArgsWithoutTrailingSpaces_Expected_ParserReturnsCorrectlyFormattedDateString()
        {
            const string InputString = "Year 44 week 43 yearweak (UTC+02:00) Harare, Pretoria | South Africa Standard Time | South Africa Standard Time | October | Oct | 10 | 290 | Sunday | Sun | 7 |16 | 22 | 2044/10/16 10:25:36.953 PM A.D.";
            const string FormatString = "'Year' yy 'week' ww 'yearweak' ZZZ | ZZ | Z | MM | M | m | dy | DW | dW | dw |d | 24h | yyyy/mm/dd 12h:min:ss.sp am/pm Era";

            Assert.IsTrue(_parser.TryParseDateTime(InputString, FormatString, out IDateTimeResultTO dateTimeResult, out string result), "Cannot parse valid date time");
            var returnDate = dateTimeResult.ToDateTime();

            Assert.IsTrue(returnDate.Day == 16 && returnDate.Month == 10 && returnDate.Year == 2044, "Incorrect object returned");
        }
        
        [TestMethod]
        [TestCategory("DateTimeParserUnitTest")]
        [Description("Test for complex DateTimeParser input datetime with trailing spaces")]
        [Owner("Ashley Lewis")]
        public void DateTimeParser_TryParseDateTime_ComplexArgsWithTrailingSpaces_Expected_ParserReturnsCorrectlyFormattedDateString()
        {
            const string InputString = "Year 44 week 43 yearweak (UTC+02:00) Harare, Pretoria | South Africa Standard Time | South Africa Standard Time | October | Oct | 10 | 290 | Sunday | Sun | 7 |16 | 22 | 2044/10/16 10:25:36.953 PM A.D. ";
            const string FormatString = "'Year' yy 'week' ww 'yearweak' ZZZ | ZZ | Z | MM | M | m | dy | DW | dW | dw |d | 24h | yyyy/mm/dd 12h:min:ss.sp am/pm Era ";

            var resultBool = _parser.TryParseDateTime(InputString, FormatString, out IDateTimeResultTO dateTimeResult, out string result);

            Assert.IsTrue(result == string.Empty, "Error returned: " + result);
            Assert.IsTrue(resultBool, "Cannot parse valid date time.");
            Assert.IsTrue(dateTimeResult.Days == 16 && dateTimeResult.Months == 10 && dateTimeResult.Years == 2044, "Incorrect object returned");
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_Invalid_Expected_FalseReturnedByParserIndicatingItIsUnableToParseTheInput()
        {
            const string inputString = "baisd78qh378hd123bhd18n18378";
            const string formatString = "ddmmyyyy";

            var IsParseable = _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);

            Assert.IsFalse(IsParseable);
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_NULL_Expected_FalseReturnedByParser()
        {
            const string formatString = "ddmmyyyy";

            var IsParseable = _parser.TryParseDateTime(null, formatString, out IDateTimeResultTO dateTimeResult, out string result);

            Assert.IsFalse(IsParseable);
        }

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_DateTime_UnexpectedBackslash_BackSlashFormatErrorReturnedByParser()
        {
            const string inputString = "\\\"30041988\\\\\"";
            const string formatString = "\\\"ddmmyyyy\\\\\"";

            var IsParseable = _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);

            Assert.IsFalse(IsParseable);
            Assert.AreEqual(@"A \'\\\' character must be followed by a \' or preceded by a \\.", result);
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_yy_Expected_LastTwoDigitsOfYearAreParsed()
        {
            const string inputString = "12";
            const string formatString = "yy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Year == 2012)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_Z_Expected_TimezoneCorrectlyParsed()
        {
            const string inputString = "GMT";
            const string formatString = "Z";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.ShortName);
        }

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_ZZ_Expected_TimeZoneRelativeToGMTParsedOutCorrectly()
        {
            const string inputString = "GMT+02:00";
            const string formatString = "ZZ";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.Name);
        }

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_ZZZ_Expected_FullTimezoneValueParsedCorrectly()
        {
            const string inputString = "Greenwich Mean Time";
            const string formatString = "ZZZ";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);

            Assert.AreEqual(inputString, dateTimeResult.TimeZone.LongName);
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_yyyy_Expected_FullYearCorrectlyParsedOut()
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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_mm_Expected_MonthParsedAsDoubleDigit()
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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_m_Expected_MonthParsedWithoutPadding()
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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_M_Expected_MonthAsShorthanfTextReturned()
        {
            const string inputString = "Feb";
            const string formatString = "M";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.Month == 2)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_MM_Expected_FullMonthNameReturnedFromParser()
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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_d_Expected_DayOfTheMonthReturned()
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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_dd_Expected_DayReturnedWithPadding()
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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_DW_Expected_DayOfTheWeekReturned()
        {
            const string inputString = "Tuesday";
            const string formatString = "DW";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_dW_Expected_DayOfTheShortHandParsed()
        {
            const string inputString = "Tue";
            const string formatString = "dW";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_dw_Expected_DayOfTheWeekParsedCorrectlyAsNumber()
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
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_dy_Expected_MilleniumParsedOutOfDate()
        {
            const string inputString = "502012";
            const string formatString = "dyyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.DayOfYear == 50)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_w_Expected_Positive()
        {
            const string inputString = "202012";
            const string formatString = "wyyyy";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            var myCal = CultureInfo.InvariantCulture.Calendar;
            var weekOfYear = myCal.GetWeekOfYear(returnDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            if (weekOfYear == 20)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_24h_Expected_Positive()
        {
            const string inputString = "14101988 15:42";
            const string formatString = "'14101988 '24h':42'";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 15)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_12h_Expected_Positive()
        {
            const string inputString = "14101988 03:42";
            const string formatString = "'14101988 '12h':42'";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 3)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_min_Expected_Positive()
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

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_ss_Expected_Positive()
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

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_sp_Expected_Positive()
        {
            const string inputString = "14101988 15:42:32:673";
            const string formatString = "'14101988 15:42:32:'sp";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Milliseconds == 673)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_am_Expected_Positive()
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
        public void DateTimeParser_TryParseDateTime_Format_pm_Expected_Positive()
        {
            const string inputString = "14101988 1:42:32:673 pm";
            const string formatString = "'14101988 1:42:32:673 'am/pm";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

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
        public void DateTimeParser_TryParseDateTime_Format_pm_Where_Hours_Is_12_Expected_Positive()
        {
            const string inputString = "14101988 12:42:32 PM";
            const string formatString = "ddmmyyyy 12h:min:ss am/pm";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

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
        public void DateTimeParser_TryParseDateTime_Format_am_Where_Hours_Is_12_Expected_Positive()
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


        [TestMethod]
        public void DateTimeParser_TryParseDateTime_Format_pm_Where_Hours_Is_1_Expected_Positive()
        {
            const string inputString = "14101988 1:42:32 PM";
            const string formatString = "ddmmyyyy 12h:min:ss am/pm";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

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
        public void DateTimeParser_TryParseDateTime_Format_am_Where_Hours_Is_1_Expected_Positive()
        {
            const string inputString = "14101988 1:42:32 AM";
            const string formatString = "ddmmyyyy 12h:min:ss am/pm";

            _parser.TryParseDateTime(inputString, formatString, out IDateTimeResultTO dateTimeResult, out string result);
            var returnDate = dateTimeResult.ToDateTime();

            if (returnDate.TimeOfDay.Hours == 1)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Incorrect object returned");
            }
        }

        /// <summary>
        /// Parse time All Arguments valid expected Parsed Correctly
        /// </summary>
        [TestMethod]
        public void DateTimeParser_TryParseTime_AllArgs_Valid_Expected_ParsedOutCorrectly()
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
        public void DateTimeParser_TryParseTime_InputFormat_Invalid_Expected_ErrorReturned()
        {
            const string inputFormat = "yy':'asdavvaad";
            const string addTime = "01:02:03";
            var isParseable = _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
            Assert.IsFalse(isParseable);
        }


        /// <summary>
        /// Parse time with input format NULL expected error returned.
        /// </summary>
        [TestMethod]
        public void DateTimeParser_TryParseTime_InputFormat_NULL_Expected_ErrorReturned()
        {
            const string addTime = "01:02:03";
            var isParseable = _parser.TryParseTime(addTime, null, out IDateTimeResultTO returnTO, out string result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time invalid time expected error returned.
        /// </summary>
        [TestMethod]
        public void DateTimeParser_TryParseTime_Time_Invalid_Expected_ErrorReturned()
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
        public void DateTimeParser_TryParseTimeTimeNullExpectedErrorReturned()
        {
            const string inputFormat = "yy':'mm':'dd";
            var isParseable = _parser.TryParseTime(null, inputFormat, out IDateTimeResultTO returnTO, out string result);
            Assert.IsFalse(isParseable);
        }

        /// <summary>
        /// Parse time using yy input format expected error returned.
        /// </summary>
        [TestMethod]
        public void DateTimeParser_TryParseTimeUsingYyExpectedErrorReturned()
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
        public void DateTimeParser_TryParseTimeUsingYyyyExpectedYearReturned()
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
        public void DateTimeParser_TryParseTimeUsingMmExpectedMonthsReturnedCorrectly()
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
        public void DateTimeParser_TryParseTime_Using_m_Expected_MonthReturnedAsSingleDigit()
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
        public void DateTimeParser_TryParseTimeUsingMExpectedMonthReturnedAsSingleDigit()
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
        public void DateTimeParser_TryParseTimeUsingMmExpectedMonthReturnedAsPaddedDigit()
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
        public void DateTimeParser_TryParseTimeUsingDExpectedDayReturnedAsSingleDigit()
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
        public void DateTimeParser_TryParseTimeUsingDdExpectedDayReturnedAsPaddedDigit()
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

        [TestMethod]
        public void DateTimeParser_TryParseTime_Using_DW_Expected_Positive()
        {
            const string inputFormat = "DW";
            const string addTime = "4";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
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
        public void DateTimeParser_TryParseTimeUsing_DWExpectedPositive()
        {
            const string inputFormat = "dW";
            const string addTime = "4";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
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
        public void DateTimeParser_TryParseTimeUsingDwExpectedPositive()
        {
            const string inputFormat = "dw";
            const string addTime = "4";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
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
        public void DateTimeParser_TryParseTimeUsingDyExpectedPositive()
        {
            const string inputFormat = "dy";
            const string addTime = "123";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
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
        public void DateTimeParser_TryParseTimeUsingWExpectedWeekReturned()
        {
            const string inputFormat = "w";
            const string addTime = "32";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
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
        public void DateTimeParser_TryParseTimeUsing24HExpectedHoursAddedAndReturnedAs24H()
        {
            const string inputFormat = "24h";
            const string addTime = "21";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
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
        public void DateTimeParser_TryParseTimeUsing12HExpectedHoursAddedAccordingTo12H()
        {
            const string inputFormat = "12h";
            const string addTime = "3";
            _parser.TryParseTime(addTime, inputFormat, out IDateTimeResultTO returnTO, out string result);
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
        public void DateTimeParser_TryParseTimeUsingMinExpectedMinutesAddedToBaseTime()
        {
            const string InputFormat = "min";
            const string AddTime = "34";
            _parser.TryParseTime(AddTime, InputFormat, out IDateTimeResultTO returnTo, out string result);
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
        public void DateTimeParser_TryParseTimeUsingSsExpectedSecondsAddedToBaseTime()
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
        /// Parse time using sp as input format expected split seconds added to base time.
        /// </summary>
        [TestMethod]
        public void DateTimeParser_TryParseTimeUsingSpExpectedSplitSecondsAddedToBaseTime()
        {
            const string InputFormat = "sp";
            const string AddTime = "3";
            _parser.TryParseTime(AddTime, InputFormat, out IDateTimeResultTO returnTo, out string result);
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
        public void DateTimeParser_TryParseTimeUsingSimpleLiteralExpectedDateAddedToBaseTime()
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

        /// <summary>
        /// Parse time using complex literal expected date added to base time.
        /// </summary>
        [TestMethod]
        public void DateTimeParser_TryParseTimeUsingComplexLiteralExpectedDateAddedToBaseTime()
        {
            const string InputFormat = "'I was born on the 'd'th of 'm' in the year 'yy";
            const string AddTime = "I was born on the 2th of 1 in the year 21";
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
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatOfDayoftheMonthExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "d, dd, ddd, dddd";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "d', 'dd', 'dW', 'DW");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatOfMonthExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "MMMM, MMM, MM, M";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "MM', 'M', 'mm', 'm");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForYearsExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "yyyyy, yyyy, yy, y, yyy";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "yyyyy', 'yyyy', 'yy', 'y', 'yyy");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForLongDateExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "dddd, dd MMMM yyyy";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "DW', 'dd' 'MM' 'yyyy");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForShortDateExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "MM/dd/yyyy";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "mm'/'dd'/'yyyy");
        }
       
        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForHoursExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "h, hh, H, HH";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "12h', '12h', '24h', '24h");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForMinutesExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "m, mm";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "min', 'min");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForSecondsExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "s, ss";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "s', 'ss");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForSplitSecondsExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "f, ff, fff, ffff, ffffff, fffffff,";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "f', 'ff', 'sp', 'ffff', 'ffffff', 'fffffff','");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForNonZeroSplitSecondsExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "F- FF- FFF- FFFF- FFFFFF- FFFFFFF-";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "F'- 'FF'- 'FFF'- 'FFFF'- 'FFFFFF'- 'FFFFFFF'-'");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForShortTimeExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "HH:mm";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "24h':'min");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForLongTimeExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "HH:mm:ss.fffffff";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "24h':'min':'ss'.'fffffff");
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForTimeZoneExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "K";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "K");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForTimeZonesExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "'Hours offset from UTC: 'z', Hours offset from UTC, with a leading zero: 'zz', Offset with minutes: 'zzz";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "'Hours offset from UTC: 'z', Hours offset from UTC, with a leading zero: 'zz', Offset with minutes: 'zzz");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForRoundTripExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "yyyy'-'mm'-'dd'T'24h':'min':'ss'.'fffffffK");
        }
        
        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForAmpmDesignatorExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "t, tt";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "t', 'am/pm");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForEraExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "g, gg";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "g', 'gg");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForFullDateShortTimeExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "dddd, dd MMMM yyyy HH:mm";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "DW', 'dd' 'MM' 'yyyy' '24h':'min");
        }

        [TestMethod]
        public void DateTimeParser_TryParseTime_TranslateFormatFromDotNetWhereFormatForFullLongTimeExpectedInDev2Format()
        {
            //initialize
            IDateTimeParser translatingParser = new Dev2DateTimeParser();
            var inputFormat = "dddd', 'dd MMMM yyyy HH:mm:ss";

            //execute
            inputFormat = translatingParser.TranslateDotNetToDev2Format(inputFormat, out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.AreEqual(inputFormat, "DW', 'dd' 'MM' 'yyyy' '24h':'min':'ss");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DateTimeParserHelper_IsNumberWeekOfYear_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
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
        public void DateTimeParserHelper_IsNumberSeconds_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
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
        public void DateTimeParserHelper_IsNumberMinutes_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
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
        public void DateTimeParserHelper_IsNumberDayOfWeek_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
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
        public void DateTimeParserHelper_IsNumber24H_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
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
        public void DateTimeParserHelper_IsNumberMilliseconds_GivenA_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
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
        public void DateTimeParserHelper_GetDayOfWeekInt_GivenSunday_ShouldReturn7()
        {
            //---------------Set up test pack-------------------
            var dateTimeParser = new DateTimeParserHelper();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DateTimeParserHelper).GetMethod("GetDayOfWeekInt", BindingFlags.Static | BindingFlags.NonPublic);
            var result = methodInfo.Invoke(dateTimeParser, new object[] { DayOfWeek.Sunday });
            //---------------Test Result -----------------------
            Assert.AreEqual(7 , int.Parse(result.ToString()));
        }

        [TestMethod]
        public void DateTimeLiteralProcessor_ProcessEscapedDateTimeRegion_WhileNotOnEscapeCharacter_ExpectBackslashFormatError()
        {
            //---------------Set up test pack-------------------
            string error = "";
            string currentValue = "";
            bool nothingDied = false;
            //---------------Execute Test ----------------------
            DateTimeLiteralProcessor.ProcessInsideEscapedLiteral(ref error, 'a', DateTimeParser.LiteralRegionStates.InsideInferredLiteralRegion, ref currentValue, ref nothingDied);
            //---------------Test Result -----------------------
            Assert.AreEqual(ErrorResource.BackSlashFormatError, error, "Processing an unescaped DateTime region did not throw backslash format error.");
        }

        [TestMethod]
        public void DateTimeLiteralProcessor_ProcessInferredEscapedDateTimeRegion_WhileNotOnEscapeCharacter_ExpectBackslashFormatError()
        {
            //---------------Set up test pack-------------------
            string error = "";
            string currentValue = "";
            bool nothingDied = false;
            //---------------Execute Test ----------------------
            DateTimeLiteralProcessor.ProcessInsideInferredEscapedLiteral(ref error, 'a', DateTimeParser.LiteralRegionStates.InsideInferredLiteralRegion, ref currentValue, ref nothingDied);
            //---------------Test Result -----------------------
            Assert.AreEqual(ErrorResource.BackSlashFormatError, error, "Processing an unescaped inferred DateTime region did not throw backslash format error.");
        }
    }
}
