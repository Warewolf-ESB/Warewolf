using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDeleteActivityWFTests
    /// </summary>
    [TestClass]
    public class DsfDeleteActivityWFTests
    {
        string WebserverURI = ServerSettings.WebserverURI;
        public DsfDeleteActivityWFTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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

        [TestMethod]
        public void DeleteRecordUsingRecordsetWithNumericIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DeleteRecordUsingRecordsetWithNumericIndex");
            string expected = @"<DeleteResult>Success</DeleteResult><Customers><FirstName></FirstName><LastName></LastName></Customers><Customers><FirstName>Wallis</FirstName><LastName>Buchan</LastName></Customers><Customers><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></Customers><Customers><FirstName>Travis</FirstName><LastName>Frisinger</LastName></Customers><Customers><FirstName>Urie</FirstName><LastName>Smit</LastName></Customers><Customers><FirstName>Brendon</FirstName><LastName>Page</LastName></Customers><Customers><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></Customers><Customers><FirstName>Ashley</FirstName><LastName>Lewis</LastName></Customers><Customers><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></Customers><Customers><FirstName>Michael</FirstName><LastName>Cullen</LastName></Customers>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void DeleteRecordUsingRecsetWithBlankIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DeleteRecordUsingRecsetWithBlankIndex");
            string expected = @"<DeleteResult>Success</DeleteResult><Customers><FirstName>Barney</FirstName><LastName>Buchan</LastName></Customers><Customers><FirstName>Wallis</FirstName><LastName>Buchan</LastName></Customers><Customers><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></Customers><Customers><FirstName>Travis</FirstName><LastName>Frisinger</LastName></Customers><Customers><FirstName>Urie</FirstName><LastName>Smit</LastName></Customers><Customers><FirstName>Brendon</FirstName><LastName>Page</LastName></Customers><Customers><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></Customers><Customers><FirstName>Ashley</FirstName><LastName>Lewis</LastName></Customers><Customers><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></Customers>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DeleteRecordUsingRecordsetWithStarIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DeleteRecordUsingRecordsetWithStarIndex");
            string expected = @"<DeleteResult>Success</DeleteResult>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}
