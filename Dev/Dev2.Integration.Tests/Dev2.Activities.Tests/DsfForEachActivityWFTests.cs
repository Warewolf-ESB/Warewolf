using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using System.Text.RegularExpressions;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests {


    /// <summary>
    /// Summary description for DsfForEachActivityWFTests
    /// </summary>
    [TestClass]
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
            string expected = @"<newset><newrec>TestVal1</newrec><newrec2>RecVal1</newrec2></newset><newset><newrec>TestVal2</newrec><newrec2>RecVal2</newrec2></newset><newset><newrec></newrec><newrec2>RecVal3</newrec2></newset><newset><newrec></newrec><newrec2>RecVal4</newrec2></newset><newset><newrec></newrec><newrec2>RecVal5</newrec2></newset><newset><newrec>TestVal1</newrec><newrec2>RecVal1</newrec2></newset><newset><newrec>TestVal2</newrec><newrec2>RecVal2</newrec2></newset><newset><newrec></newrec><newrec2>RecVal3</newrec2></newset><newset><newrec></newrec><newrec2>RecVal4</newrec2></newset><newset><newrec></newrec><newrec2>RecVal5</newrec2></newset>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }
        
        #endregion ForEach Behaviour Tests

        #region RecordSet Tests

        [TestMethod]
        public void ForEachMappingToNewRecset() {
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachMappingToNewRecSetTest");
            string expected = @"<DataList><Valriable>testScalar</Valriable><recset><rec>RecVal1</rec><rec2>Rec2Val1</rec2></recset><recset><rec>RecVal2</rec><rec2>Rec2Val2</rec2></recset><recset><rec>RecVal3</rec><rec2></rec2></recset><recset><rec>RecVal4</rec><rec2></rec2></recset><recset><rec>RecVal5</rec><rec2></rec2></recset><firstVar></firstVar><testing><test>testVal1</test></testing><testing><test>testVal2</test></testing><newset><newrec>testScalar</newrec><newrec2>RecVal1</newrec2></newset><newset><newrec>testScalar</newrec><newrec2>RecVal2</newrec2></newset><newset><newrec>testScalar</newrec><newrec2>RecVal3</newrec2></newset><newset><newrec>testScalar</newrec><newrec2>RecVal4</newrec2></newset><newset><newrec>testScalar</newrec><newrec2>RecVal5</newrec2></newset>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }

        [TestMethod]
        public void ForEachMappingBackToRecset() {
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachMappingBackToRecSetTest");
            string expected = @"<recset><rec>hello</rec><rec2>test1</rec2></recset><recset><rec>hello</rec><rec2>test2</rec2></recset><recset><rec>hello</rec><rec2>test3</rec2></recset><recset><rec>hello</rec><rec2>test4</rec2></recset><recset><rec>hello</rec><rec2>test5</rec2></recset><test>hello</test><testrecset></testrecset>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }

        #endregion RecordSet Tests

        #region Iteration Number Tests

        [TestMethod]
        public void ForEachNumber() {
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachNumber");
            string expected = "<var>Static_Scalar</var><resultVar>recVal2</resultVar><recset><rec>recVal1</rec><rec2>rec2Val1.outer</rec2></recset><recset><rec>recVal2</rec><rec2>rec2Val2.outer</rec2></recset><recset><rec>recVal2</rec><rec2>rec2Val2.outer</rec2></recset><recset><rec>recVal4</rec><rec2></rec2></recset><recset><rec>recVal5</rec><rec2></rec2></recset><testing><test>testVal1</test></testing>";

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
        public void ForEachScalar() {          
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachScalarTest");
            string expected = @"<var>5</var><recset><rec>recVal1</rec><rec2>rec2Val1</rec2></recset><recset><rec>recVal2</rec><rec2>rec2Val2</rec2></recset><recset><rec>recVal3</rec><rec2></rec2></recset><recset><rec>recVal4</rec><rec2></rec2></recset><recset><rec>recVal5</rec><rec2></rec2></recset><testing><test>testVal1</test></testing><testing><test>testVal2</test></testing><resultVar></resultVar>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected));
        }

        #endregion Scalar Tests
    }
}
