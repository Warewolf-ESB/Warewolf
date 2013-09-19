using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for AddResourceServiceTest
    /// </summary>
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class AddResourceServiceTest
    {
        public AddResourceServiceTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        private string _webserverURI = ServerSettings.WebserverURI;
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
        public void AddResource_ResourceContaingFullServiceDefinition_Expected_ServiceAddedToDynamicServices()
        {
            Guid guid = Guid.NewGuid();
            string service = CreateService(guid.ToString(), "[[Id]]");
            _webserverURI = _webserverURI + "SaveResourceService?" + service;
            string actual = TestHelper.PostDataToWebserver(_webserverURI);
            // Added DbService '5a94ea2a-2315-40f6-8c01-42d1c1174913'
            var expected = string.Format("<Dev2System.ManagmentServicePayload>Added DbService '{0}'</Dev2System.ManagmentServicePayload>", guid);
            StringAssert.Contains(actual,expected, "Got [ " + actual + " ]");

        }


        private string CreateService(string serviceName, string serviceInputAction)
        {
            string service = @"<Payload><Roles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,DEV2 Limited Internet Access</Roles><ResourceXml><Service Name=""" + serviceName + @"""><Actions><Action Name=""" + serviceInputAction + @""" Type=""InvokeStoredProc"" SourceName=""SashenDB"" SourceMethod=""dbo.TestingStoredProcedure""><Inputs><Input Name=""Id"" Source="""" ></Input></Inputs><Outputs><Output Name=""Id"" MapsTo=""Id"" Value=""[[Id]]""/><Output Name=""Name"" MapsTo=""Name"" Value=""[[Name]]""/><Output Name=""Surname"" MapsTo=""Surname"" Value=""[[Surname]]""/></Outputs><OutputDescription><![CDATA[<Dev2XMLResult>  <z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/"">    <d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">      <d2p1:anyType i:type=""d1p1:DataSourceShape"">        <d1p1:Paths>          <d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Xml"" i:type=""d5p1:XmlPath"">            <ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Id</ActualPath>            <DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Id</DisplayPath>            <OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" >[[Id]]</OutputExpression>            <SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">10</SampleData>          </d2p1:anyType>          <d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Xml"" i:type=""d5p1:XmlPath"">            <ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Name</ActualPath>            <DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Name</DisplayPath>            <OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" >[[Name]]</OutputExpression>            <SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Sashen</SampleData>          </d2p1:anyType>          <d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Xml"" i:type=""d5p1:XmlPath"">            <ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Surname</ActualPath>            <DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Surname</DisplayPath>            <OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" >[[Surname]]</OutputExpression>            <SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Naidoo</SampleData>          </Outputs></d2p1:anyType>        </d1p1:Paths>      </d2p1:anyType>    </d1p1:DataSourceShapes>    <d1p1:Format>ShapedXML</d1p1:Format>  </z:anyType>  <JSON /></Dev2XMLResult>]]></OutputDescription></Action></Actions><AuthorRoles>Domain Users,All Users,Company Users,Business Design Studio Developers,Build Configuration Engineers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles><Comment></Comment><Category>DATABASE</Category><Tags></Tags><HelpLink></HelpLink><UnitTestTargetWorkflowService></UnitTestTargetWorkflowService><BizRule /><WorkflowActivityDef /><Source /><XamlDefinition /><DisplayName>Service</DisplayName><DataList /><TypeOf>InvokeStoredProc</TypeOf></Service></ResourceXml></Payload>";

            return service;
        }
    }
}
