using Dev2.Composition;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.MEF;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Unlimited.Framework;

namespace Dev2.Integration.Tests.Build.Tests
{
    [TestClass]
    public class ApplicationServerBuildTests
    {
        public ApplicationServerBuildTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private static ImportServiceContext _importServiceContext;

        IEnvironmentConnection _connection;
        IFrameworkDataChannel _dataChannel;
        MockSecurityProvider _mockSecurityProvider;

        private TestContext testContextInstance;
        protected const string WebserverUrl = "http://localhost:2234/";
        private string DsfChannelUrl = ServerSettings.DsfAddress;

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


        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _importServiceContext = CompositionInitializer.DefaultInitialize();
        }

        #region Server Listening Tests

        [TestMethod]
        public void EnsureServerListensOnLocalhost_ExpectedConnectionSuccessful()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "cake");

            conn.Address = new Uri(ServerSettings.DsfAddress);
            conn.Connect();
            Assert.IsTrue(conn.IsConnected);
            conn.Disconnect();
        }

        [TestMethod]
        public void EnsureServerListensOnPcName_ExpectedConnectionSuccessful()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "cake");

            conn.Address = new Uri(string.Format(ServerSettings.DsfAddressFormat, Environment.MachineName));
            conn.Connect();
            Assert.IsTrue(conn.IsConnected);
            conn.Disconnect();
        }

        #endregion Server Listening Tests

        #region Additional test attributes
        /// <summary>
        /// We are setting MEF up here to retrieve all exports and use them for dependency injection
        /// </summary>
        [TestInitialize()]
        public void EnvironmentTestsInitialize()
        {

            //MefImportSatisfier mefImportSatisfier = new MefImportSatisfier();
            _connection = CreateLocalEnvironment();// mefImportSatisfier.CreateLocalEnvironmentConnection();

            Uri uri = new Uri(DsfChannelUrl);


            _connection.Address = uri;
            _connection.Connect();
            _dataChannel = _connection.DataChannel;

            _mockSecurityProvider = new MockSecurityProvider("Administrator");
        }
        #endregion

        #region Studio Server Integration Tests

        /// <summary>
        ///Validate that a connection can be made to a valid environment
        ///</summary>

        [TestMethod()]
        public void Environment_RetrieveWorkflowServices_ExpectedListContainingServicesSerializedToXML()
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourceService";
            dataObj.ResourceName = "*";
            dataObj.ResourceType = ResourceType.WorkflowService;
            dataObj.Roles = string.Join(",", _mockSecurityProvider.Roles);

            string result = _dataChannel.ExecuteCommand(dataObj.XmlString, Guid.Empty, Guid.NewGuid());
            dynamic resultingObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);

            dynamic wfServices = resultingObject.Service;

            Assert.IsTrue(wfServices is List<UnlimitedObject> && wfServices.Count > 0);
        }

        //Ammend Assert to Contains("<Actions>")

        [TestMethod()]
        public void Environment_RetrieveServices_ExpectedListContainingServicesSerializedToXML()
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourceService";
            dataObj.ResourceName = "*";
            dataObj.ResourceType = ResourceType.Service;
            dataObj.Roles = string.Join(",", _mockSecurityProvider.Roles);

            string result = _dataChannel.ExecuteCommand(dataObj.XmlString, Guid.Empty, Guid.NewGuid());
            dynamic resultingObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);

            dynamic wfServices = resultingObject.Service;
            foreach (var item in wfServices)
            {
                Assert.IsTrue(item.XmlString.Contains("<Action"));
            }

        }

        [TestMethod()]
        public void Environment_RetrieveSources_ExpectedListContainingServicesSerializedToXML()
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourceService";
            dataObj.ResourceName = "*";
            dataObj.ResourceType = ResourceType.Source;
            dataObj.Roles = string.Join(",", _mockSecurityProvider.Roles);

            string result = _dataChannel.ExecuteCommand(dataObj.XmlString, Guid.Empty, Guid.NewGuid());
            dynamic resultingObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);

            dynamic wfServices = resultingObject.Source;
            foreach (var item in wfServices)
            {
                Assert.IsTrue(item.XmlString.Contains("<Source"));
            }

        }


        // Sashen :25-01-2013 : This test checks that a service returns the correct message
        // Currently returning <Error>Cannot fetch
        // Bug 8374
        [TestMethod]
        public void Environment_ServiceNotExistsOnService_ExpectedErrorMessageServiceNotExist()
        {
            string expected = @"<center><h1>404</h1>Error: Service not found in catalog</center>";
            EnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");
            conn.Address = new Uri(ServerSettings.DsfAddress);
            conn.Connect();
            string actual = conn.DataChannel.ExecuteCommand(string.Format("<x><Service>{0}</Service></x>", Guid.NewGuid().ToString()), Guid.Empty, Guid.NewGuid());
            //string actual = _dataChannel.ExecuteCommand(string.Format("<x><Service>{0}</Service></x>", Guid.NewGuid().ToString()), Guid.Empty, Guid.NewGuid());
            actual = actual.Replace("\r", "").Replace("\n", "");
            StringAssert.Contains(actual, expected);

        }

        /// <summary>
        ///Validate that resources are loaded from a valid environment
        ///</summary>
        ///

        [TestMethod()]
        public void AppServerr_Update_Resource_Correctly()
        {
            string expected = @"<XmlData>
              <XmlData>
                <CompilerMessage>Updated Service 'ServiceToBindFrom'</CompilerMessage>
              </XmlData>
            </XmlData>";
            string Command = TestResource.Service_Update_Request_String;
            string actual = _dataChannel.ExecuteCommand(Command, Guid.Empty, Guid.NewGuid());

            actual = TestHelper.CleanUp(actual);
            expected = TestHelper.CleanUp(expected);
            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void DsfRequest_MalformedXml_Expected_ErrorInDsfREsponse()
        {
            string expected = @"<XmlData>
          <Error>Unexpected end of file has occurred. The following elements are not closed: x, x. Line 1, position 62.</Error>
          <DynamicServiceFrameworkMessage>
            <Error>Error in request - inbound message contains error tag</Error>
          </DynamicServiceFrameworkMessage>
        </XmlData>";
            string xmlRequest = string.Format("<x><Service>{0}</Service><x>", Guid.NewGuid().ToString());
            string actual = _dataChannel.ExecuteCommand(xmlRequest, Guid.Empty, Guid.NewGuid());

            actual = TestHelper.CleanUp(actual);
            expected = TestHelper.CleanUp(expected);

            Assert.AreEqual(expected, actual);
        }

        #endregion Studio Server Integration

        private IEnvironmentConnection CreateLocalEnvironment()
        {
            IEnvironmentConnection envConn = new EnvironmentConnection();
            envConn.Address = new Uri(ServerSettings.DsfAddress);
            return envConn;
        }
    }
}
