using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDeleteActivityWFTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfDeleteActivityWFTests
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void DeleteRecordUsingRecordsetWithNumericIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/DeleteRecordUsingRecordsetWithNumericIndex");
            const string expected = @"<DeleteResult>Success</DeleteResult><Customers index=""2""><FirstName>Wallis</FirstName><LastName>Buchan</LastName></Customers><Customers index=""3""><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></Customers><Customers index=""4""><FirstName>Travis</FirstName><LastName>Frisinger</LastName></Customers><Customers index=""5""><FirstName>Urie</FirstName><LastName>Smit</LastName></Customers><Customers index=""6""><FirstName>Brendon</FirstName><LastName>Page</LastName></Customers><Customers index=""7""><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></Customers><Customers index=""8""><FirstName>Ashley</FirstName><LastName>Lewis</LastName></Customers><Customers index=""9""><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></Customers><Customers index=""10""><FirstName>Michael</FirstName><LastName>Cullen</LastName></Customers>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void DeleteRecordUsingRecsetWithBlankIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/DeleteRecordUsingRecsetWithBlankIndex");
            const string expected = @"<DeleteResult>Success</DeleteResult><Customers index=""1""><FirstName>Barney</FirstName><LastName>Buchan</LastName></Customers><Customers index=""2""><FirstName>Wallis</FirstName><LastName>Buchan</LastName></Customers><Customers index=""3""><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></Customers><Customers index=""4""><FirstName>Travis</FirstName><LastName>Frisinger</LastName></Customers><Customers index=""5""><FirstName>Urie</FirstName><LastName>Smit</LastName></Customers><Customers index=""6""><FirstName>Brendon</FirstName><LastName>Page</LastName></Customers><Customers index=""7""><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></Customers><Customers index=""8""><FirstName>Ashley</FirstName><LastName>Lewis</LastName></Customers><Customers index=""9""><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></Customers>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DeleteRecordUsingRecordsetWithStarIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/DeleteRecordUsingRecordsetWithStarIndex");
            const string expected = @"<DeleteResult>Success</DeleteResult>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}
