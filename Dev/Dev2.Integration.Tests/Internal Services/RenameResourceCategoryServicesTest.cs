using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Internal_Services
{
    /// <summary>
    /// Summary description for SystemServices
    /// </summary>
    [TestClass]
    public class RenameResourceCategoryServicesTest
    {
        private string _webServerURI = ServerSettings.WebserverURI;

        public RenameResourceCategoryServicesTest()
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
        [Description("Can call to RenameResourceCategory with correct arguments")]
        [Owner("Huggs")]
        public void RenameResourceCategoryService_WithValidArguments_ExpectSuccessResult()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "RenameResourceCategoryService", "OldCategory=Bugs&NewCategory=TestCategory&ResourceType=WorkflowService");
            string actual = TestHelper.PostDataToWebserver(postData);
            Assert.IsFalse(string.IsNullOrEmpty(actual));
            Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload><CompilerMessage>Updated Category from 'Bugs' to 'TestCategory'</CompilerMessage></Dev2System.ManagmentServicePayload></DataList>", actual);
        }
        
    }
}