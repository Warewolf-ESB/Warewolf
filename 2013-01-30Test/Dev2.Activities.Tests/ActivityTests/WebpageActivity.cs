using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2;
using System.Xml.Linq;

namespace ActivityUnitTests.ActivityTest {
    /// <summary>
    /// Summary description for WebpageActivity
    /// </summary>
    [TestClass]
    public class WebpageActivity : BaseActivityUnitTest {
        public WebpageActivity() : base() {
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

        //[TestMethod]
        //public void WebpageNormal() {
        //    TestStartNode = new FlowStep {
        //        Action = new DsfWebPageActivity { OutputMapping = null, XMLConfiguration = ActivityStrings.testWebpageXMLconfig, WebsiteServiceName = null }
        //    };

        //    TestData = ActivityStrings.scalarShape;
        //    // delimiting makes moq a big nasty
        //    CallBackData = "&amp;amp;lt;div&amp;amp;gt; this is test text&amp;amp;lt;/div&amp;amp;gt;";

        //    UnlimitedObject result = ExecuteProcess();

        //    Assert.IsTrue(result.XmlString.Contains(ActivityStrings.webpageMagicString));
        //}

    }
}
