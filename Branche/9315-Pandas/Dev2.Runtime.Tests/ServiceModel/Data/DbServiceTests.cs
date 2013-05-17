using System.Xml.Linq;
using Dev2.Common.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [TestClass]
    public class DbServiceTests
    {
        public DbServiceTests()
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
        public void DbServiceCtorExpectedCorrectDbService()
        {
            string xmlDataString = @"<Service ID=""af8d2d38-22b5-4599-8357-adce196beb83"" Name=""TravsTestService"" ResourceType=""DbService"">
  <Actions>
    <Action Name=""dbo.InsertDummyUser"" Type=""InvokeStoredProc"" SourceID=""ebba47dc-e5d4-4303-a203-09e2e9761d16"" SourceName=""testingDBSrc"" SourceMethod=""dbo.InsertDummyUser"">
      <Inputs>
        <Input Name=""fname"" Source=""fname"" EmptyToNull=""false"" DefaultValue="""" />
        <Input Name=""lname"" Source=""lname"" EmptyToNull=""false"" DefaultValue="""" />
        <Input Name=""username"" Source=""username"" EmptyToNull=""false"" DefaultValue="""" />
        <Input Name=""password"" Source=""password"" EmptyToNull=""false"" DefaultValue="""" />
        <Input Name=""lastAccessDate"" Source=""lastAccessDate"" EmptyToNull=""false"" DefaultValue="""" />
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
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>TravsTestService</DisplayName>
  <Category>WEBPART_WIZARDS</Category>
</Service>";
            XElement testElm = XElement.Parse(xmlDataString);
            DbService dbService = new DbService(testElm);

            Assert.AreEqual("TravsTestService", dbService.ResourceName);
            Assert.AreEqual(ResourceType.DbService, dbService.ResourceType);
            Assert.AreEqual("af8d2d38-22b5-4599-8357-adce196beb83", dbService.ResourceID.ToString());
            Assert.AreEqual("WEBPART_WIZARDS", dbService.ResourcePath);
        }

        #endregion

        #region ToXml Tests

        [TestMethod]
        public void ToXmlExpectedCorrectXml()
        {
            string xmlDataString = @"<Service ID=""af8d2d38-22b5-4599-8357-adce196beb83"" Version=""1.0"" Name=""TravsTestService"" ResourceType=""DbService"">
  <Actions>
    <Action Name=""dbo.InsertDummyUser"" Type=""InvokeStoredProc"" SourceID=""ebba47dc-e5d4-4303-a203-09e2e9761d16"" SourceName=""testingDBSrc"" SourceMethod=""dbo.InsertDummyUser"">
      <Inputs>
        <Input Name=""fname"" Source=""fname"" EmptyToNull=""false"" DefaultValue="""" />
        <Input Name=""lname"" Source=""lname"" EmptyToNull=""false"" DefaultValue="""" />
        <Input Name=""username"" Source=""username"" EmptyToNull=""false"" DefaultValue="""" />
        <Input Name=""password"" Source=""password"" EmptyToNull=""false"" DefaultValue="""" />
        <Input Name=""lastAccessDate"" Source=""lastAccessDate"" EmptyToNull=""false"" DefaultValue="""" />
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
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>TravsTestService</DisplayName>
  <Category>WEBPART_WIZARDS</Category>
  <AuthorRoles></AuthorRoles>
</Service>";
            XElement testElm = XElement.Parse(xmlDataString);
            DbService dbService = new DbService(testElm);
            XElement returnedXelm = dbService.ToXml();
            string actual = returnedXelm.ToString();

            Assert.AreEqual(xmlDataString, actual);
        }

        #endregion
    }
}
