using System;
using System.Net;
using System.Text;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Network;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Build.Test
{
    [TestClass]
    public class ApplicationServerBuildTests
    {
        static IEnvironmentConnection _connection;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Server Listening Tests

        static IEnvironmentConnection SetupEnvironmentConnection()
        {
            IEnvironmentConnection connection = new ServerProxy(new Uri(ServerSettings.DsfAddress));
            return connection;
        }

        [TestMethod]
        public void EnsureServerListensOnLocalhost_ExpectedConnectionSuccessful()
        {

            var setupEnvironmentConnection = SetupEnvironmentConnection();
            IEnvironmentConnection conn = setupEnvironmentConnection;

            conn.Connect();
            Assert.IsTrue(conn.IsConnected);
            conn.Disconnect();
        }

        [TestMethod]
        public void EnsureServerListensOnPcName_ExpectedConnectionSuccessful()
        {

            var setupEnvironmentConnection = SetupEnvironmentConnection();
            IEnvironmentConnection conn = setupEnvironmentConnection;

            conn.Connect();
            var res = conn.IsConnected;
            conn.Disconnect();
            Assert.IsTrue(res);

        }

        #endregion Server Listening Tests

        #region Additional test attributes

        /// <summary>
        /// We are setting MEF up here to retrieve all exports and use them for dependency injection
        /// </summary>
        [TestInitialize]
        public void EnvironmentTestsInitialize()
        {
            var setupEnvironmentConnection = SetupEnvironmentConnection();
            _connection = setupEnvironmentConnection;
            _connection.Connect();
        }
        #endregion

        #region Studio Server Integration Tests

        [TestMethod]
        public void Environment_ServiceNotExistsOnService_ExpectedErrorMessageServiceNotExist()
        {
            // BUG 8593: 2013.02.17 - TWR - changed code to test POST web request
            //This has no mapping in the webapi there error
            var urls = new[]
            {
                String.Format("{0}{1}", ServerSettings.WebserverURI, "test/test"),
                String.Format("{0}{1}", ServerSettings.WebserverURI, "/")
            };
            var client = new WebClient { Credentials = CredentialCache.DefaultCredentials };
            try
            {
                client.UploadString(urls[0], "hello");
            }
            catch(WebException wex)
            {
                var response = (HttpWebResponse)wex.Response;
                Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            }
            try
            {
                client.UploadString(urls[1], "hello");
            }
            catch(WebException wex)
            {
                var response = (HttpWebResponse)wex.Response;
                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            }

        }

        private string CreateService(string serviceName, string serviceInputAction)
        {
            string service = @"<Service Name=""" + serviceName + @"""><Actions><Action Name=""" + serviceInputAction + @""" Type=""InvokeStoredProc"" SourceName=""SashenDB"" SourceMethod=""dbo.TestingStoredProcedure""><Inputs><Input Name=""Id"" Source="""" ></Input></Inputs><Outputs><Output Name=""Id"" MapsTo=""Id"" Value=""[[Id]]""/><Output Name=""Name"" MapsTo=""Name"" Value=""[[Name]]""/><Output Name=""Surname"" MapsTo=""Surname"" Value=""[[Surname]]""/></Outputs><OutputDescription><![CDATA[<Dev2XMLResult>  <z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/"">    <d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">      <d2p1:anyType i:type=""d1p1:DataSourceShape"">        <d1p1:Paths>          <d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Xml"" i:type=""d5p1:XmlPath"">            <ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Id</ActualPath>            <DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Id</DisplayPath>            <OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" >[[Id]]</OutputExpression>            <SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">10</SampleData>          </d2p1:anyType>          <d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Xml"" i:type=""d5p1:XmlPath"">            <ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Name</ActualPath>            <DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Name</DisplayPath>            <OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" >[[Name]]</OutputExpression>            <SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Sashen</SampleData>          </d2p1:anyType>          <d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Xml"" i:type=""d5p1:XmlPath"">            <ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Surname</ActualPath>            <DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">NewDataSet.Table.Surname</DisplayPath>            <OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" >[[Surname]]</OutputExpression>            <SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Naidoo</SampleData>          </Outputs></d2p1:anyType>        </d1p1:Paths>      </d2p1:anyType>    </d1p1:DataSourceShapes>    <d1p1:Format>ShapedXML</d1p1:Format>  </z:anyType>  <JSON /></Dev2XMLResult>]]></OutputDescription></Action></Actions><AuthorRoles>Domain Users,All Users,Company Users,Business Design Studio Developers,Build Configuration Engineers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles><Comment></Comment><Category>DATABASE</Category><Tags></Tags><HelpLink></HelpLink><UnitTestTargetWorkflowService></UnitTestTargetWorkflowService><BizRule /><WorkflowActivityDef /><Source /><XamlDefinition /><DisplayName>Service</DisplayName><DataList /><TypeOf>InvokeStoredProc</TypeOf></Service>";

            return service;
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AppServer_SaveResource")]
        public void AppServer_SaveResource_WhenSavingWorkerService_ExpectSaved()
        {
            //------------Setup for test--------------------------
            CommunicationController coms = new CommunicationController { ServiceName = "SaveResourceService" };

            var id = Guid.NewGuid().ToString();
            var tmp = new StringBuilder(CreateService(id, "[[Id]]"));

            coms.AddPayloadArgument("ResourceXml", tmp);
            coms.AddPayloadArgument("WorkspaceID", Guid.Empty.ToString());

            string expected = string.Format("Added DbService '{0}'", id);

            //------------Execute Test---------------------------
            var result = coms.ExecuteCommand<ExecuteMessage>(_connection, Guid.Empty);

            //------------Assert Results-------------------------
            StringAssert.Contains(result.Message.ToString(), expected, "Got [ " + result.Message + " ]");

        }

        /// <summary>
        ///Validate that resources are loaded from a valid environment
        ///</summary>
        [TestMethod]
        public void AppServer_Update_Resource_Correctly()
        {
            CommunicationController coms = new CommunicationController { ServiceName = "SaveResourceService" };

            var tmp = new StringBuilder(TestResource.Service_Update_Request_String);
            var xe = tmp.ToXElement();
            var xml = xe.Element("ResourceXml");

            var wtf = xml.ToStringBuilder().Unescape();
            wtf = wtf.Replace("<XmlData>", "").Replace("</XmlData>", "").Replace("<ResourceXml>", "").Replace("</ResourceXml>", "");

            coms.AddPayloadArgument("ResourceXml", wtf);
            coms.AddPayloadArgument("WorkspaceID", Guid.Empty.ToString());

            const string expected = @"Updated WorkflowService 'ServiceToBindFrom'";

            var result = coms.ExecuteCommand<ExecuteMessage>(_connection, Guid.Empty);

            StringAssert.Contains(result.Message.ToString(), expected, "Got [ " + result.Message + " ]");

        }

        #endregion Studio Server Integration

    }
}
