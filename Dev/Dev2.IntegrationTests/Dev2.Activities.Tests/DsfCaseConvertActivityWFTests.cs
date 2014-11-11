
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
    // ReSharper disable InconsistentNaming
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
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/CaseConversion_With_Scalar");
            string expected = @"<Sentence>ThisIsTheSentenceThatMustBeConverted</Sentence><Peopleindex=""1""><FirstName>travis</FirstName></People><Peopleindex=""2""><FirstName>brendon</FirstName></People><Peopleindex=""3""><FirstName>mat</FirstName></People><Peopleindex=""4""><FirstName>sashen</FirstName></People><Peopleindex=""5""><FirstName>trevor</FirstName></People><Peopleindex=""6""><FirstName>barney</FirstName></People><Peopleindex=""7""><FirstName>massimo</FirstName></People><Peopleindex=""8""><FirstName>wallis</FirstName></People>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            expected = TestHelper.CleanUp(expected);
            ResponseData = TestHelper.CleanUp(ResponseData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void Test_CaseConvert_Recset_With_Index()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/CaseConversion_Recset_With_Index");
            string expected = @"<People index=""1""><FirstName>travis</FirstName></People><People index=""2""><FirstName>brendon</FirstName></People><People index=""3""><FirstName>mat</FirstName></People><People index=""4""><FirstName>sashen</FirstName></People><People index=""5""><FirstName>TREVOR</FirstName></People><People index=""6""><FirstName>barney</FirstName></People><People index=""7""><FirstName>massimo</FirstName></People><People index=""8""><FirstName>wallis</FirstName></People>";

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
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/CaseConversion_Recset_With_NoIndex");
            string expected = @"<People index=""1""><FirstName>travis</FirstName></People><People index=""2""><FirstName>brendon</FirstName></People><People index=""3""><FirstName>mat</FirstName></People><People index=""4""><FirstName>sashen</FirstName></People><People index=""5""><FirstName>trevor</FirstName></People><People index=""6""><FirstName>barney</FirstName></People><People index=""7""><FirstName>massimo</FirstName></People><People index=""8""><FirstName>wallis</FirstName></People><People index=""9""><FirstName>WALLIS</FirstName></People>";

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
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/CaseConvertRecsetWithStar");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            const string expected = "<recSet><Name>Michael</Name><Surname>CULLEN</Surname></recSet><recSet><Name>Sashen</Name><Surname>NAIDOO</Surname></recSet>";

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion CaseConvert RecordSet Tests


    }
}
