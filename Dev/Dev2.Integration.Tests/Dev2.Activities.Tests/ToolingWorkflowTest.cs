using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for ToolingWorkflowTest
    /// </summary>
    [TestClass]
    public class ToolingWorkflowTest
    {
        string WebserverURI = ServerSettings.WebserverURI;

        public ToolingWorkflowTest()
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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        [Ignore]
        public void AllToolsTestExpectPass()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Tool Testing");
            
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            string Expected = @"<Test><Result>ForEach: Success</Result></Test><Test><Result>Switch: PASS</Result></Test><Test><Result>Decision: PASS</Result></Test><Test><Result>Count Records: PASS</Result></Test><Test><Result>Delete Record: PASS</Result></Test><Test><Result>Sort Records: PASS</Result></Test><Test><Result>Find Record Index: PASS</Result></Test><Test><Result>Assign: PASS</Result></Test><Test><Result>Base Conversion: PASS</Result></Test><Test><Result>Case Convert: PASS</Result></Test><Test><Result>Data Merge: FAIL</Result></Test><Test><Result>Data Split: PASS</Result></Test><Test><Result>Date and Time: PASS</Result></Test><Test><Result>Date and Time Difference: PASS</Result></Test><Test><Result>Find Index: PASS</Result></Test><Test><Result>Format Number: INCONCLUSIVE</Result></Test><Test><Result>Replace: INCONCLUSIVE</Result></Test>";

            Assert.IsTrue(ResponseData.Contains(Expected), "Expected [ " + Expected + ", But Got [ " + ResponseData + " ]");
        }

        [TestMethod]
        public void ServiceExecutionTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "ServiceExecutionTest");

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            string Expected = @"<DataList><rs><val>1</val><result>res = 1</result></rs><rs><val>2</val><result>res = 2</result></rs><rs><val>3</val><result>res = 3</result></rs><rs><val></val><result>res = 3</result></rs></DataList>";

            Assert.IsTrue(ResponseData.Contains(Expected), "Got [ " + Expected + " ] Expected [ " + Expected + " ]");
        }
    }
}
