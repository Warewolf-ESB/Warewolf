using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDeleteActivityWFTests
    /// </summary>
    [TestClass]
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
            string PostData = String.Format("{0}{1}", WebserverURI, "DeleteRecordUsingRecordsetWithNumericIndex");
            string expected = @"<DeleteResult>Success</DeleteResult><Customers><FirstName>Wallis</FirstName><LastName>Buchan</LastName></Customers><Customers><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></Customers><Customers><FirstName>Travis</FirstName><LastName>Frisinger</LastName></Customers><Customers><FirstName>Urie</FirstName><LastName>Smit</LastName></Customers><Customers><FirstName>Brendon</FirstName><LastName>Page</LastName></Customers><Customers><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></Customers><Customers><FirstName>Ashley</FirstName><LastName>Lewis</LastName></Customers><Customers><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></Customers><Customers><FirstName>Michael</FirstName><LastName>Cullen</LastName></Customers>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DeleteRecordsActivity_Delete")]
        public void DeleteRecordsActivity_Delete_LargePayload_TakesLessThenTwoAndAHalfSecond()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DeleteTestFlow");

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            int startIndex = ResponseData.IndexOf(@"<yeardiff>", StringComparison.Ordinal) + 10;
            int endIndex = ResponseData.IndexOf(@"</yeardiff>", StringComparison.Ordinal);
            string substring = ResponseData.Substring(startIndex, endIndex - startIndex);
            int val;
            if(int.TryParse(substring, out val))
            {
                Assert.IsTrue(val < 2500, "Deleting tool to long");
            }
            else
            {
                Assert.Fail("Could get the time");
            }
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
