using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfSortActivity
    /// </summary>
    [TestClass]
    public class DsfSortActivityTest
    {
        private readonly string _webserverUri = ServerSettings.WebserverURI;
        const string SortBackwardsWorkflow = "INTEGRATION TEST SERVICES/SortBackwardsTest";
        const string SortForwardsWorkflow = "INTEGRATION TEST SERVICES/SortForwardsTest";
        const string SortDateTimeWorkflow = "INTEGRATION TEST SERVICES/SortActivity_DateSort";
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Sort Descending Tests

        /// <summary>
        /// This method will sort a people recordset by their first name alphabetically from a to z
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Sort_Text_Forwards_Expected_Records_To_Be_In_Reverse_Alphabetic_Order()
        {
            string PostData = String.Format("{0}{1}", _webserverUri, SortForwardsWorkflow);
            const string expected = @"<People index=""1""><FirstName>Ashley</FirstName><LastName>Lewis</LastName></People><People index=""2""><FirstName>Barney</FirstName><LastName>Buchan</LastName></People><People index=""3""><FirstName>Brendon</FirstName><LastName>Page</LastName></People><People index=""4""><FirstName>Jurie</FirstName><LastName>Smit</LastName></People><People index=""5""><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></People><People index=""6""><FirstName>Michael</FirstName><LastName>Cullen</LastName></People><People index=""7""><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></People><People index=""8""><FirstName>Travis</FirstName><LastName>Frisinger</LastName></People><People index=""9""><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></People><People index=""10""><FirstName>Wallis</FirstName><LastName>Buchan</LastName></People>";

            string actual = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(actual, expected);
        }

        #endregion Sort Descending Tests

        #region Sort Ascending Tests

        /// <summary>
        /// This method will sort a people recordset by their first name alphabetically from z to a
        /// </summary>
        [TestMethod]
        public void Sort_Text_Backwards_Expected_Records_To_Be_In_Reverse_Alphabetic_Order()
        {
            string PostData = String.Format("{0}{1}", _webserverUri, SortBackwardsWorkflow);
            const string expected = @"<People index=""1""><FirstName>Wallis</FirstName><LastName>Buchan</LastName></People><People index=""2""><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></People><People index=""3""><FirstName>Travis</FirstName><LastName>Frisinger</LastName></People><People index=""4""><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></People><People index=""5""><FirstName>Michael</FirstName><LastName>Cullen</LastName></People><People index=""6""><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></People><People index=""7""><FirstName>Jurie</FirstName><LastName>Smit</LastName></People><People index=""8""><FirstName>Brendon</FirstName><LastName>Page</LastName></People><People index=""9""><FirstName>Barney</FirstName><LastName>Buchan</LastName></People><People index=""10""><FirstName>Ashley</FirstName><LastName>Lewis</LastName></People>";

            string actual = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(actual, expected);
        }

        /// <summary>
        /// This method will sort a date record set according to date in ascending order
        /// </summary>
        [TestMethod]
        public void SortRecordOnDateTime_Expected_RecordSetSortedAccordingToDateTime()
        {
            string PostData = String.Format("{0}{1}", _webserverUri, SortDateTimeWorkflow);
            const string expected = @"<DateRecordSet index=""1""><Date>Monday, November 17, 2008 05:11:59 PM</Date></DateRecordSet><DateRecordSet index=""2""><Date>Tuesday, July 24, 2012 04:00:00 PM</Date></DateRecordSet><DateRecordSet index=""3""><Date>Wednesday, July 25, 2012 05:11:59 PM</Date></DateRecordSet><DateRecordSet index=""4""><Date>Thursday, July 26, 2012 05:11:59 PM</Date></DateRecordSet>";

            string actual = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(actual, expected);
        }

        #endregion Sort Ascending Tests
    }
}
