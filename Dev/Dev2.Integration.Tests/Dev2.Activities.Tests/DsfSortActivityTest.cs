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
        private readonly string WebserverURI = ServerSettings.WebserverURI;
        const string _sortBackwardsWorkflow = "SortBackwardsTest";
        const string _sortForwardsWorkflow = "SortForwardsTest";
        const string _sortDateTimeWorkflow = "SortActivity_DateSort";
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
        public void Sort_Text_Forwards_Expected_Records_To_Be_In_Reverse_Alphabetic_Order()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, _sortForwardsWorkflow);
            string expected = @"<People><FirstName>Ashley</FirstName><LastName>Lewis</LastName></People><People><FirstName>Barney</FirstName><LastName>Buchan</LastName></People><People><FirstName>Brendon</FirstName><LastName>Page</LastName></People><People><FirstName>Jurie</FirstName><LastName>Smit</LastName></People><People><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></People><People><FirstName>Michael</FirstName><LastName>Cullen</LastName></People><People><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></People><People><FirstName>Travis</FirstName><LastName>Frisinger</LastName></People><People><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></People><People><FirstName>Wallis</FirstName><LastName>Buchan</LastName></People>";

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
            string PostData = String.Format("{0}{1}", WebserverURI, _sortBackwardsWorkflow);
            string expected = @"<People><FirstName>Wallis</FirstName><LastName>Buchan</LastName></People><People><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></People><People><FirstName>Travis</FirstName><LastName>Frisinger</LastName></People><People><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></People><People><FirstName>Michael</FirstName><LastName>Cullen</LastName></People><People><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></People><People><FirstName>Jurie</FirstName><LastName>Smit</LastName></People><People><FirstName>Brendon</FirstName><LastName>Page</LastName></People><People><FirstName>Barney</FirstName><LastName>Buchan</LastName></People><People><FirstName>Ashley</FirstName><LastName>Lewis</LastName></People>";

            string actual = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(actual, expected);
        }

        /// <summary>
        /// This method will sort a date record set according to date in ascending order
        /// </summary>
        [TestMethod]
        public void SortRecordOnDateTime_Expected_RecordSetSortedAccordingToDateTime()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, _sortDateTimeWorkflow);
            string expected = @"<DateRecordSet><Date>Monday, November 17, 2008 05:11:59 PM</Date></DateRecordSet><DateRecordSet><Date>Tuesday, July 24, 2012 04:00:00 PM</Date></DateRecordSet><DateRecordSet><Date>Wednesday, July 25, 2012 05:11:59 PM</Date></DateRecordSet><DateRecordSet><Date>Thursday, July 26, 2012 05:11:59 PM</Date></DateRecordSet>";

            string actual = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(actual, expected);
        }

        #endregion Sort Ascending Tests
    }
}
