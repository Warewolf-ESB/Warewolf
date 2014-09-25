using System;
using System.Text.RegularExpressions;
using ActivityUnitTests;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{

    /// <summary>
    /// Summary description for DsfForEachActivityWFTests
    /// </summary>
    [TestClass]
    public class DsfForEachActivityWFTests : BaseActivityUnitTest
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region ForEach Behaviour Tests

        // Blocked by Bug 7926

        [TestMethod]
        public void ForEachNestedWorkFlow()
        {

            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/NewForEachNestedForEachTest");
            const string expected = @"<innerScalar>11</innerScalar>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void ForEachRecordsetIndexNotToBeReplacedWorkFlow()
        {

            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/ForEachWithStarAndStaticIndex");
            const string expected = "DataList><results index=\"1\"><res>50</res></results></DataList";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(ResponseData, expected);
        }

        #endregion ForEach Behaviour Tests

        #region Iteration Number Tests

        [TestMethod]
        public void ForEachNumber()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/NewForEachNumber");
            const string expected = "<DataList><Rec index=\"1\"><Each>0</Each></Rec><Rec index=\"2\"><Each>1</Each></Rec><Rec index=\"3\"><Each>2</Each></Rec><Rec index=\"4\"><Each>4</Each></Rec><Rec index=\"5\"><Each>8</Each></Rec><Rec index=\"6\"><Each>16</Each></Rec></DataList>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(expected, ResponseData);
        }


        // Sashen: 28-01-2012 : Once the fix is made and this test passes, please put your name and a comment regarding the test.
        // Bug 8366
        [TestMethod]
        public void ForEachAssign_Expected_AssignWorksForEveryIteration()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/NewForEachAssign");
            string expected = @"<Result> Dummy_String Dummy_String_Inner Dummy_String_Inner Dummy_String_Inner Dummy_String_Inner</Result>    <Input>Dummy_String_Inner</Input>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Iteration Number Tests

        #region Scalar Tests

        [TestMethod]
        // TODO : Update WF in TFS
        public void ForEachInputOutputMappingTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/NewForEachScalarTest");
            const string expected = "<DataList><var>5</var><recset index=\"1\"><rec1>1</rec1></recset><recset index=\"2\"><rec1>2</rec1></recset><recset index=\"3\"><rec1>3</rec1></recset><recset index=\"4\"><rec1>4</rec1></recset><recset index=\"5\"><rec1>5</rec1></recset><recset index=\"6\"><rec1>6</rec1></recset></DataList>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Scalar Tests

        #region All Tools Test

        [TestMethod]
        public void ForEachAllToolsTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "MO/ForEachUpgradeTest");
            const string expected = "PASS";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Scalar Tests

        #region Bugs

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ForEach_WithDbData")]
        public void ForEach_WithData_WhenInvoking_ExpectData()
        {
            //------------Setup for test--------------------------
            const string expected = @"<Result>PASS</Result>";

            //------------Execute Test---------------------------
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/Bug_11463_WF");

            //------------Assert Results-------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(ResponseData, expected);
        }
        #endregion

    }
}
