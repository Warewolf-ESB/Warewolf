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
            string expected = @"<Sentence>ThisIsTheSentenceThatMustBeConverted</Sentence><PeoplerowID=""1""><FirstName>travis</FirstName></People><PeoplerowID=""2""><FirstName>brendon</FirstName></People><PeoplerowID=""3""><FirstName>mat</FirstName></People><PeoplerowID=""4""><FirstName>sashen</FirstName></People><PeoplerowID=""5""><FirstName>trevor</FirstName></People><PeoplerowID=""6""><FirstName>barney</FirstName></People><PeoplerowID=""7""><FirstName>massimo</FirstName></People><PeoplerowID=""8""><FirstName>wallis</FirstName></People>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            expected = TestHelper.CleanUp(expected);
            ResponseData = TestHelper.CleanUp(ResponseData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void Test_CaseConvert_Recset_With_Index()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "CaseConversion_Recset_With_Index");
            string expected = @"<People rowID=""1""><FirstName>travis</FirstName></People><People rowID=""2""><FirstName>brendon</FirstName></People><People rowID=""3""><FirstName>mat</FirstName></People><People rowID=""4""><FirstName>sashen</FirstName></People><People rowID=""5""><FirstName>TREVOR</FirstName></People><People rowID=""6""><FirstName>barney</FirstName></People><People rowID=""7""><FirstName>massimo</FirstName></People><People rowID=""8""><FirstName>wallis</FirstName></People>";

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
            string expected = @"<People rowID=""1""><FirstName>travis</FirstName></People><People rowID=""2""><FirstName>brendon</FirstName></People><People rowID=""3""><FirstName>mat</FirstName></People><People rowID=""4""><FirstName>sashen</FirstName></People><People rowID=""5""><FirstName>trevor</FirstName></People><People rowID=""6""><FirstName>barney</FirstName></People><People rowID=""7""><FirstName>massimo</FirstName></People><People rowID=""8""><FirstName>wallis</FirstName></People><People rowID=""9""><FirstName>WALLIS</FirstName></People>";

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

            const string expected = "<recSet><Name>Michael</Name><Surname>CULLEN</Surname></recSet><recSet><Name>Sashen</Name><Surname>NAIDOO</Surname></recSet>";

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion CaseConvert RecordSet Tests


    }
}
