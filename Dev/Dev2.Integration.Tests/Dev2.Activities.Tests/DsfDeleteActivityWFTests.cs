using System;
using System.Globalization;
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
            const string expected = @"<DeleteResult>Success</DeleteResult><Customers index=""2""><FirstName>Wallis</FirstName><LastName>Buchan</LastName></Customers><Customers index=""3""><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></Customers><Customers index=""4""><FirstName>Travis</FirstName><LastName>Frisinger</LastName></Customers><Customers index=""5""><FirstName>Urie</FirstName><LastName>Smit</LastName></Customers><Customers index=""6""><FirstName>Brendon</FirstName><LastName>Page</LastName></Customers><Customers index=""7""><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></Customers><Customers index=""8""><FirstName>Ashley</FirstName><LastName>Lewis</LastName></Customers><Customers index=""9""><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></Customers><Customers index=""10""><FirstName>Michael</FirstName><LastName>Cullen</LastName></Customers>";

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
                Assert.IsTrue(val < 2500, "Deleting tool to long it took " + val.ToString(CultureInfo.InvariantCulture));
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
            const string expected = @"<DeleteResult>Success</DeleteResult><Customers index=""1""><FirstName>Barney</FirstName><LastName>Buchan</LastName></Customers><Customers index=""2""><FirstName>Wallis</FirstName><LastName>Buchan</LastName></Customers><Customers index=""3""><FirstName>Trevor</FirstName><LastName>Williams-Ros</LastName></Customers><Customers index=""4""><FirstName>Travis</FirstName><LastName>Frisinger</LastName></Customers><Customers index=""5""><FirstName>Urie</FirstName><LastName>Smit</LastName></Customers><Customers index=""6""><FirstName>Brendon</FirstName><LastName>Page</LastName></Customers><Customers index=""7""><FirstName>Massimo</FirstName><LastName>Guerrera</LastName></Customers><Customers index=""8""><FirstName>Ashley</FirstName><LastName>Lewis</LastName></Customers><Customers index=""9""><FirstName>Sashen</FirstName><LastName>Naidoo</LastName></Customers>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DeleteRecordUsingRecordsetWithStarIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DeleteRecordUsingRecordsetWithStarIndex");
            const string expected = @"<DeleteResult>Success</DeleteResult>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}
