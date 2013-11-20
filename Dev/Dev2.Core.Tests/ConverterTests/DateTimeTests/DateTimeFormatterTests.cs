using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Dev2.Converters.DateAndTime;
using Dev2.Converters.DateAndTime.Interfaces;

namespace Unlimited.UnitTest.Framework.ConverterTests.DateTimeTests
{
    /// <summary>
    /// Summary description for DateTimeFormatterTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DateTimeFormatterTests
    {
        static IDateTimeFormatter formatter;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            formatter = DateTimeConverterFactory.CreateFormatter();
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

        #region Format Tests

        /// <summary>
        /// Tests that the formatter can correctly read input format and return a correctly formatted output
        /// </summary>
        [TestMethod]
        public void FormatAllArgsValid_Expected_ResultFormattedAccordingToOutputFormat()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "14101988";
            dateTimeTO.InputFormat = "ddmmyyyy";
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd";
            dateTimeTO.TimeModifierType = "Years";
            dateTimeTO.TimeModifierAmount = 23;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            string expected = "2011/10/14";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that the formatter can accept and return formats with Timezone values
        /// </summary>
        [TestMethod]
        public void FormatAllArgsValid_WithTimeZone_Expected_ResultContainsFullTimezoneName()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "14101988GMT";
            dateTimeTO.InputFormat = "ddmmyyyyZ";
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd' 'ZZZ";
            dateTimeTO.TimeModifierType = "Years";
            dateTimeTO.TimeModifierAmount = 23;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            string expected = "2011/10/14 Greenwich Mean Time";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that the formatter correctly fails to retrieve the correct input given an invalid input format
        /// </summary>
        [TestMethod]
        public void FormatInputFormatInvalid_Expected_UnableToParseInvalidInputFormat()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "14101988";
            dateTimeTO.InputFormat = "dwakkmslyyabsdh'asdx'";
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd";
            dateTimeTO.TimeModifierType = "Years";
            dateTimeTO.TimeModifierAmount = 23;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            if (isFormatCorrect)
            {
                Assert.Fail("Incorrect ouput format should not work correctly.");
            }
            else
            {
                Assert.IsTrue(!string.IsNullOrEmpty(errorMsg));
            }
        }

        //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
        /// <summary>
        /// Tests that invalid modifiers are not used to modify date
        /// </summary>
        [TestMethod]
        public void FormatTimeModifierTypeInvalid_Expected_DateNotModified()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "14101988";
            dateTimeTO.InputFormat = "ddmmyyyy";
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd";
            dateTimeTO.TimeModifierType = "WrongType";
            dateTimeTO.TimeModifierAmount = 23;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);

            Assert.IsTrue(result == "1988/10/14");

        }

        //2013.02.12: Ashley Lewis - Bug 8725, Task 8840
        [TestMethod]
        public void FormatWithTrailingZerosInOutputExpectedTrailingZerosNotRemoved()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "2013/02/07 08:38:56.953 PM";
            dateTimeTO.InputFormat = "yyyy/mm/dd 12h:min:ss.sp am/pm";
            dateTimeTO.OutputFormat = "sp";
            dateTimeTO.TimeModifierType = "Split Secs";
            dateTimeTO.TimeModifierAmount = -53;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);

            Assert.AreEqual("900", result);
        }
        [TestMethod]
        public void FormatWithTrailingSpacesInInputExpectedOutputDateNotBlank()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "14101988  ";
            dateTimeTO.InputFormat = "ddmmyyyy  ";
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd";
            dateTimeTO.TimeModifierType = "Years";
            dateTimeTO.TimeModifierAmount = 23;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);

            Assert.AreEqual("2011/10/14", result);
        }

        /// <summary>
        /// Tests that non-matching input to format does not return any date time format
        /// </summary>
        [TestMethod]
        public void FormatDateTimeInvalid_Expected_ErrorMessageReturnedByFormatter()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "WrongFormat";
            dateTimeTO.InputFormat = "ddmmyyyy";
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd";
            dateTimeTO.TimeModifierType = "Years";
            dateTimeTO.TimeModifierAmount = 23;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            if (isFormatCorrect)
            {
                Assert.Fail("Incorrect ouput format should not work correctly.");
            }
            else
            {
                Assert.IsTrue(!string.IsNullOrEmpty(errorMsg));
            }
        }

        /// <summary>
        /// Tests that null datetime values are correctly handled by the Formatter
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void FormatDateTimeNULLorEmpty_Expected_ErrorMessageReturnedByFormatter()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = null;
            dateTimeTO.InputFormat = "ddmmyyyy";
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd";
            dateTimeTO.TimeModifierType = "Years";
            dateTimeTO.TimeModifierAmount = 23;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            if (isFormatCorrect)
            {
                Assert.Fail("Incorrect ouput format should not work correctly.");
            }
            else
            {
                Assert.IsTrue(!string.IsNullOrEmpty(errorMsg));
            }
        }

        /// <summary>
        /// Tests that modifier that is empty does not modify date in any way
        /// </summary>
        [TestMethod]
        public void FormatTimeModifierTypeNULLorEmpty_Expected_SameDateReturned()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "14101988";
            dateTimeTO.InputFormat = "ddmmyyyy";
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd";
            dateTimeTO.TimeModifierType = "";
            dateTimeTO.TimeModifierAmount = 0;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            string expected = "1988/10/14";

            Assert.AreEqual(expected, result);
        }

        //06.05.2013: Ashley Lewis - Bug 9300
        /// <summary>
        /// Tests that if the input format is null, the formatter assumes current culture default date time formatting and tries invariant culture
        /// </summary>
        [TestMethod]
        public void FormatInputFormatNULLorEmptyExpectedDefaultFormatUsed()
        {
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "05/06/2013 10:29:50 AM";
            dateTimeTO.InputFormat = null;
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd' '12h':'min':'ss' 'am/pm";
            formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            string expected = "2013/05/06 10:29:50 AM";

            Assert.AreEqual(expected, result, "Date time did not default input format to en-US");
        }
        /// <summary>
        /// Tests that if the input format is null, the formatter assumes invariant culture default date time formatting and tries invariant culture
        /// </summary>
        [TestMethod]
        public void FormatInputFormatNULLorEmptyExpectedInvariantFormatUsed()
        {
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "05/06/2013 10:29:50";
            dateTimeTO.InputFormat = null;
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd' '12h':'min':'ss' 'am/pm";
            formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            string expected = "2013/05/06 10:29:50 AM";

            Assert.AreEqual(expected, result, "Date time did not default input format to invariant culture");
        }
        /// <summary>
        /// Tests that if the input format is null, the formatter assumes FullDateTimeFormat formatting and tries invariant culture
        /// </summary>
        [TestMethod]
        public void FormatInputFormatNULLorEmptyExpectedFullDateTimeFormatUsed()
        {
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "Monday, May 06, 2013 10:29:50 AM";
            dateTimeTO.InputFormat = null;
            dateTimeTO.OutputFormat = @"yyyy'/'mm'/'dd' '12h':'min':'ss' 'am/pm";
            formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            string expected = "2013/05/06 10:29:50 AM";

            Assert.AreEqual(expected, result, "Date time did not default input format to full date time");
        }


        /// <summary>
        /// Tests that if the output format is empty, the formatter returns an error regarding this
        /// </summary>
        [TestMethod]
        public void FormatOutputFormatNULLorEmpty_Expected_NoOutputFormattingPerformed()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "14101988";
            dateTimeTO.InputFormat = "ddmmyyyy";
            dateTimeTO.OutputFormat = "";
            dateTimeTO.TimeModifierType = "Years";
            dateTimeTO.TimeModifierAmount = 23;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            string expected = "14102011";

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that if there is an empty output format specified, the input format is used
        /// </summary>
        [TestMethod]
        public void FormatInputFormatNULLorEmpty_Expected_DateValueAssumesInputFormat()
        {
            bool isFormatCorrect;
            string result = string.Empty;
            string errorMsg = string.Empty;
            IDateTimeOperationTO dateTimeTO = new DateTimeOperationTO();
            dateTimeTO.DateTime = "2012/8/20";
            dateTimeTO.InputFormat = @"yyyy'/'m'/'d";
            dateTimeTO.OutputFormat = "";
            dateTimeTO.TimeModifierType = "";
            dateTimeTO.TimeModifierAmount = 0;
            isFormatCorrect = formatter.TryFormat(dateTimeTO, out result, out errorMsg);
            string expected = "2012/8/20";

            Assert.AreEqual(expected, result);
        }

        //27.09.2012: massimo.guerrera - Added after bug was found
        /// <summary>
        /// Tests that the formatter is able to apply a week when it is not given one
        /// </summary>
        [TestMethod]
        public void TryFormat_Converting_Date_To_Week_Expected_WeekofTheSpecifiedDataReturned()
        {
            string result;
            string error;
            string inputString = "06-01-2013";
            string formatString = "dd-mm-yyyy";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "w", "", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2");
        }

        //27.09.2012: massimo.guerrera - Added after bug was found
        /// <summary>
        /// Tests that the formatter is able to apply a week when it is not given one
        /// </summary>
        [TestMethod]
        public void TryFormat_Converting_Date_To_ww_Expected_WeekReturnedInDoubleDigitFormat()
        {
            string result;
            string error;
            string inputString = "06-01-2013";
            string formatString = "dd-mm-yyyy";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "ww", "", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "02");
        }

        #endregion Format Tests

        #region Time Modifier Tests

        #region Blank Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Blank_Positive_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2012/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "", 25, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2012/02/15 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Blank_Negative_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "", -25, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Blank_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Blank Tests

        #region Years Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Years_Positive_Expected_Correct_Addition()
        {
            string result;
            string error;
            string inputString = "2012/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Years", 25, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2037/02/15 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Years_Negative_Expected_Correct_Subtraction()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Years", -25, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2000/02/15 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Years_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Years", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Years Tests

        #region Months Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Months_Positive_Expected_Correct_Addition()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Months", 12, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2026/02/15 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Months_Negative_Expected_Correct_Subtraction()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Months", -12, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2024/02/15 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Months_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Months", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Months Tests

        #region Days Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Days_Positive_Expected_Correct_Addition()
        {
            string result;
            string error;
            string inputString = "2025/06/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Days", 30, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/07/15 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Days_Negative_Expected_Correct_Subtraction()
        {
            string result;
            string error;
            string inputString = "2025/06/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Days", -30, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/05/16 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Days_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Days", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Days Tests

        #region Week Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Weeks_Positive_Expected_Correct_Addition()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Weeks", 2, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/03/01 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Weeks_Negative_Expected_Correct_Subtraction()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Weeks", -2, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/01 11:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Weeks_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Weeks", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Week Tests

        #region Hours Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Hours_Positive_Expected_Correct_Addition()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Hours", 2, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 01:21:51 PM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Hours_Negative_Expected_Correct_Subtraction()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Hours", -2, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 09:21:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Hours_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Hours", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Hours Tests

        #region Minutes Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Minutes_Positive_Expected_Correct_Addition()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Minutes", 2, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:23:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Minutes_Negative_Expected_Correct_Subtraction()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Minutes", -2, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:19:51 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Minutes_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Minutes", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Minutes Tests

        #region Seconds Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Seconds_Positive_Expected_Correct_Addition()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Seconds", 2, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:53 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Seconds_Negative_Expected_Correct_Subtraction()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Seconds", -2, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:49 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Seconds_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Seconds", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Seconds Tests

        #region Split Secs Tests

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Split_Secs_Positive_Expected_Correct_Addition()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Split Secs", 1000, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:52 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Split_Secs_Negative_Expected_Correct_Subtraction()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Split Secs", -1000, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:50 AM");
        }

        //28.09.2012: massimo.guerrera - Added after bug was found
        [TestMethod]
        public void TimeModifiers_Split_Secs_Zero_Expected_No_Change()
        {
            string result;
            string error;
            string inputString = "2025/02/15 11:21:51 AM";
            string formatString = "yyyy/mm/dd 12h:min:ss am/pm";

            IDateTimeFormatter formatter = DateTimeConverterFactory.CreateFormatter();
            IDateTimeOperationTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeTO(inputString, formatString, "yyyy/mm/dd 12h:min:ss am/pm", "Split Secs", 0, "");
            formatter.TryFormat(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "2025/02/15 11:21:51 AM");
        }

        #endregion Split Secs Tests

        #endregion Time Modifier Tests
    }
}
