using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using System.Text.RegularExpressions;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests {


    /// <summary>
    /// Summary description for DsfForEachActivityWFTests
    /// </summary>
    [TestClass][Ignore]//Ashley: round 2 hunting the evil test
    public class DsfForEachActivityWFTests {
        readonly string WebserverURI = ServerSettings.WebserverURI;
        public DsfForEachActivityWFTests() {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region ForEach Behaviour Tests

        // Blocked by Bug 7926
        
        [TestMethod]
        public void ForEachNestedWorkFlow()
        {

            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachNestedForEachTest");
            string expected = @"<innerScalar>11</innerScalar>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }

        [TestMethod]
        public void ForEachRecordsetIndexNotToBeReplacedWorkFlow()
        {

            string PostData = String.Format("{0}{1}", WebserverURI, "ForEachWithStarAndStaticIndex");
            string expected = @"DataList><results><res>50</res></results></DataList";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }
        
        #endregion ForEach Behaviour Tests

        #region Iteration Number Tests

        [TestMethod]
        public void ForEachNumber() {
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachNumber");
            string expected = "<Rec><Each>1</Each></Rec><Rec><Each>2</Each></Rec><Rec><Each>4</Each></Rec><Rec><Each>8</Each></Rec><Rec><Each>16</Each></Rec>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }


        // Sashen: 28-01-2012 : Once the fix is made and this test passes, please put your name and a comment regarding the test.
        // Bug 8366
        [TestMethod]   
        public void ForEachAssign_Expected_AssignWorksForEveryIteration() {
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachAssign");
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
        public void ForEachInputOutputMappingTest() {          
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachScalarTest");
            string expected = @"<var>5</var><recset><rec1>1</rec1></recset><recset><rec1>2</rec1></recset><recset><rec1>3</rec1></recset><recset><rec1>4</rec1></recset><recset><rec1>5</rec1></recset><recset><rec1>6</rec1></recset>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }

        #endregion Scalar Tests

        #region All Tools Test

        [TestMethod]
        public void ForEachAllToolsTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "ForEachUpgradeTest");
            string expected = @"<Result>ForEach: Success</Result>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }

        #endregion Scalar Tests
    }
}
