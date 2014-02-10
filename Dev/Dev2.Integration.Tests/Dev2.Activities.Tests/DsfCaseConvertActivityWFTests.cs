using System;
using System.Text.RegularExpressions;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfCaseConvertActivityWFTests
    /// </summary>
    [TestClass]
    public class DsfCaseConvertActivityWFTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region CaseConvert RecordSet Tests

        [TestMethod]
        public void Test_CaseConvert_With_Scalar()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "CaseConversion_With_Scalar");
            string expected = @"<Sentence>ThisIsTheSentenceThatMustBeConverted</Sentence><People><FirstName>travis</FirstName></People><People><FirstName>brendon</FirstName></People><People><FirstName>mat</FirstName></People><People><FirstName>sashen</FirstName></People><People><FirstName>trevor</FirstName></People><People><FirstName>barney</FirstName></People><People><FirstName>massimo</FirstName></People><People><FirstName>wallis</FirstName></People>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            expected = TestHelper.CleanUp(expected);
            ResponseData = TestHelper.CleanUp(ResponseData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void Test_CaseConvert_Recset_With_Index()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "CaseConversion_Recset_With_Index");
            string expected = @"<People>      <FirstName>travis</FirstName>    </People>    <People>      <FirstName>brendon</FirstName>    </People>    <People>      <FirstName>mat</FirstName>    </People>    <People>      <FirstName>sashen</FirstName>    </People>    <People>      <FirstName>TREVOR</FirstName>    </People>    <People>      <FirstName>barney</FirstName>    </People>    <People>      <FirstName>massimo</FirstName>    </People>    <People>      <FirstName>wallis</FirstName>    </People>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void Test_CaseConvert_Recset_With_NoIndex()
        {
            // Since the UnitTest allow for two individual fields, this shall go ahead as it stands.
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "CaseConversion_Recset_With_NoIndex");
            string expected = @"<People><FirstName>travis</FirstName></People><People><FirstName>brendon</FirstName></People><People><FirstName>mat</FirstName></People><People><FirstName>sashen</FirstName></People><People><FirstName>trevor</FirstName></People><People><FirstName>barney</FirstName></People><People><FirstName>massimo</FirstName></People><People><FirstName>wallis</FirstName></People><People><FirstName>WALLIS</FirstName></People>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            ResponseData = TestHelper.RemoveWhiteSpaceBetweenTags(ResponseData);

            StringAssert.Contains(ResponseData, expected);
        }


        // Author : Massimo Guerrera
        // Refers to Bug 7819
        // Initial issue was the case convert was replacing all values in a record set with the last change.
        public void CaseConvert_Given_RecsetWithStar_Expected_AllRecordsUpdatedToUpperCase()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "CaseConvertRecsetWithStar");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            string expected = "<recSet><Name>Michael</Name><Surname>CULLEN</Surname></recSet><recSet><Name>Sashen</Name><Surname>NAIDOO</Surname></recSet>";

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion CaseConvert RecordSet Tests


    }
}
