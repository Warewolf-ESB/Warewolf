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
    public class PluginServiceTests
    {
        public PluginServiceTests()
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
            string xmlDataString = @"<Service Name=""SVN Author Fetch Worker"" ID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
	<Actions>
	</Actions>
	<Category>System</Category>
</Service>";
            //------------Execute Test---------------------------
            XElement testElm = XElement.Parse(xmlDataString);
            PluginService pluginService = new PluginService(testElm);
            //------------Assert Results-------------------------
            Assert.AreEqual("SVN Author Fetch Worker", pluginService.ResourceName);
            Assert.AreEqual(ResourceType.PluginService, pluginService.ResourceType);
            Assert.AreEqual("51a58300-7e9d-4927-a57b-e5d700b11b55", pluginService.ResourceID.ToString());
            Assert.AreEqual("System", pluginService.ResourcePath);
            Assert.IsNull(pluginService.Source);
        }

        [TestMethod]
        public void PluginServiceCtorExpectedCorrectPluginService()
        {
            //------------Setup for test--------------------------
            string xmlDataString = @"<Service Name=""SVN Author Fetch Worker"" ID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
	<Actions>
		<Action Name=""SVN_Author_Fetch_Worker"" Type=""Plugin"" SourceName=""SVN Author Fetch"" SourceMethod=""GetSvnCommitUserForRevision"">
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
            PluginService pluginService = new PluginService(testElm);
            //------------Assert Results-------------------------
            Assert.AreEqual("SVN Author Fetch Worker", pluginService.ResourceName);
            Assert.AreEqual(ResourceType.PluginService, pluginService.ResourceType);
            Assert.AreEqual("51a58300-7e9d-4927-a57b-e5d700b11b55", pluginService.ResourceID.ToString());
            Assert.AreEqual("System", pluginService.ResourcePath);
            Assert.IsNotNull(pluginService.Source);
        }

        #endregion

        #region ToXml Tests
        [Ignore]
        [TestMethod]
        public void ToXmlExpectedCorrectXml()
        {
            //------------Setup for test--------------------------
            string xmlDataString = @"<Service ID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""SVN Author Fetch Worker"" ResourceType=""Plugin"">
  <Actions>
   <Action Name=""SVN_Author_Fetch_Worker"" Type=""Plugin"" SourceID=""00000000-0000-0000-0000-000000000000"" SourceName=""SVN Author Fetch"" SourceMethod=""GetSvnCommitUserForRevision"">
    <Inputs>
     <Input Name=""Path"" Source=""Path"" EmptyToNull=""false"" DefaultValue="""">
      <Validator Type=""Required"" />
     </Input>
     <Input Name=""Revision"" Source=""Revision"" EmptyToNull=""false"" DefaultValue="""">
      <Validator Type=""Required"" />
     </Input>
     <Input Name=""Username"" Source=""Username"" EmptyToNull=""false"" DefaultValue="""">
      <Validator Type=""Required"" />
     </Input>
     <Input Name=""Password"" Source=""Password"" EmptyToNull=""false"" DefaultValue="""">
      <Validator Type=""Required"" />
     </Input>
    </Inputs>
    <Outputs />
    <OutputDescription><![CDATA[<z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""><d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:anyType i:type=""d1p1:DataSourceShape""><d1p1:Paths /></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType>]]></OutputDescription>
   </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>Plugin</TypeOf>
  <DisplayName>SVN Author Fetch Worker</DisplayName>
  <Category>System</Category>
</Service>";
            XElement testElm = XElement.Parse(xmlDataString);
            PluginService pluginService = new PluginService(testElm);
            //------------Execute Test---------------------------
            XElement returnedXelm = pluginService.ToXml();
            string actual = returnedXelm.ToString();
            //------------Assert Results-------------------------
            Assert.AreEqual(xmlDataString, actual);
        }

        #endregion
    }
}