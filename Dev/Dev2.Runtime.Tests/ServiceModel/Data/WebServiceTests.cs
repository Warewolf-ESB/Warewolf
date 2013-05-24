using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [TestClass]
    public class WebServiceTests
    {
        public WebServiceTests()
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

        #region Ctor Tests

        [TestMethod]
        public void ConstructorWhereXMLDoesNotContainActionElementExpectServiceReturn()
        {
            //------------Setup for test--------------------------
            string xmlDataString = @"<Service Name=""Test WebService"" ID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
	<Actions>
	</Actions>
	<Category>System</Category>
</Service>";
            //------------Execute Test---------------------------
            XElement testElm = XElement.Parse(xmlDataString);
            var webService = new WebService(testElm);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test WebService", webService.ResourceName);
            Assert.AreEqual(ResourceType.WebService, webService.ResourceType);
            Assert.AreEqual("51a58300-7e9d-4927-a57b-e5d700b11b55", webService.ResourceID.ToString());
            Assert.AreEqual("System", webService.ResourcePath);
            Assert.IsNull(webService.Source);
        }

        [TestMethod]
        public void WebServiceCtorExpectedCorrectWebService()
        {
            //------------Setup for test--------------------------
            string xmlDataString = @"<Service Name=""Test WebService"" ID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
	<Actions>
		<Action Name=""Test_WebService"" Type=""WebService"" SourceName=""Test WebService"" SourceMethod=""Get"">
			<Inputs>
				<Input Name=""Path"" Source=""Path"">
					<Validator Type=""Required"" />
				</Input>
				<Input Name=""Revision"" Source=""Revision"">
					<Validator Type=""Required"" />
				</Input>
				<Input Name=""Username"" Source=""Username"">
					<Validator Type=""Required"" />
				</Input>
				<Input Name=""Password"" Source=""Password"">
					<Validator Type=""Required"" />
				</Input>
			</Inputs>
			<Outputs>
				<Output Name=""error"" MapsTo=""error"" Value=""[[Error]]"" />
				<Output Name=""author"" MapsTo=""author"" Value=""[[SVNLog().Author]]"" Recordset=""result"" />
			</Outputs>
		</Action>
	</Actions>
	<Category>System</Category>
</Service>";
            //------------Execute Test---------------------------
            XElement testElm = XElement.Parse(xmlDataString);
            var webService = new WebService(testElm);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test WebService", webService.ResourceName);
            Assert.AreEqual(ResourceType.WebService, webService.ResourceType);
            Assert.AreEqual("51a58300-7e9d-4927-a57b-e5d700b11b55", webService.ResourceID.ToString());
            Assert.AreEqual("System", webService.ResourcePath);
            Assert.IsNotNull(webService.Source);
        }

        #endregion

        
    }
}