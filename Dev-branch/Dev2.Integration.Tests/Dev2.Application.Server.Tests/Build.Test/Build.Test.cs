using System.Security.Principal;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.MEF;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using Moq;
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

        IEnvironmentConnection _connection;
        IStudioEsbChannel _dataChannel;
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

        #region Server Listening Tests
        static IEnvironmentConnection SetupEnvironmentConnection()
        {
            var securityMock = new Mock<IFrameworkSecurityContext>();
            securityMock.Setup(context => context.UserIdentity).Returns(WindowsIdentity.GetCurrent);
            var eventAggregator = new Mock<IEventAggregator>();
            TcpConnection connection = new TcpConnection(securityMock.Object, new Uri(ServerSettings.DsfAddress), 77, eventAggregator.Object);
            return connection;
        }

        [TestMethod]
        public void EnsureServerListensOnLocalhost_ExpectedConnectionSuccessful()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            var setupEnvironmentConnection = SetupEnvironmentConnection();
            IEnvironmentConnection conn = setupEnvironmentConnection;

            conn.Connect();
            Assert.IsTrue(conn.IsConnected);
            conn.Disconnect();
        }

        [TestMethod]
        public void EnsureServerListensOnPcName_ExpectedConnectionSuccessful()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            var setupEnvironmentConnection = SetupEnvironmentConnection();
            IEnvironmentConnection conn = setupEnvironmentConnection;

            conn.Connect();
            Assert.IsTrue(conn.IsConnected);
            conn.Disconnect();
        }

        #endregion Server Listening Tests

        #region Additional test attributes

        /// <summary>
        /// We are setting MEF up here to retrieve all exports and use them for dependency injection
        /// </summary>
        [TestInitialize]
        public void EnvironmentTestsInitialize()
        {
//            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();
            //MefImportSatisfier mefImportSatisfier = new MefImportSatisfier();
            //_connection = CreateLocalEnvironment();// mefImportSatisfier.CreateLocalEnvironmentConnection();
            var setupEnvironmentConnection = SetupEnvironmentConnection();
            _connection = setupEnvironmentConnection;
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

            string result = _dataChannel.ExecuteCommand(dataObj.XmlString, Guid.Empty, Guid.Empty);
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

            string result = _dataChannel.ExecuteCommand(dataObj.XmlString, Guid.Empty, Guid.Empty);
            dynamic resultingObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);

            dynamic wfServices = resultingObject.Service;
            foreach(var item in wfServices)
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

            string result = _dataChannel.ExecuteCommand(dataObj.XmlString, Guid.Empty, Guid.Empty);
            dynamic resultingObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);

            dynamic wfServices = resultingObject.Source;
            foreach(var item in wfServices)
            {
                Assert.IsTrue(item.XmlString.Contains("<Source"));
            }
        }

        [TestMethod]
        public void Environment_ServiceNotExistsOnService_ExpectedErrorMessageServiceNotExist()
        {
            // BUG 8593: 2013.02.17 - TWR - changed code to test POST web request
            var urls = new[]
            {
                String.Format("{0}{1}", ServerSettings.WebserverURI, "%3Ctest%3E/test"),
                String.Format("{0}{1}", ServerSettings.WebserverURI, "/")
            };

            var client = new WebClient();
            foreach(var url in urls)
            {
                try
                {
                    client.UploadString(url, "hello");
                }
                catch(WebException wex)
                {
                    var response = (HttpWebResponse)wex.Response;
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                }
            }
        }

        /// <summary>
        ///Validate that resources are loaded from a valid environment
        ///</summary>
        ///

        [TestMethod()]
        public void AppServerr_Update_Resource_Correctly()
        {
            string expected = @"<CompilerMessage>Updated Workflow Service 'ServiceToBindFrom'</CompilerMessage>";
            string Command = TestResource.Service_Update_Request_String;
            
            //Execute twice to ensure that the resource is actually there
            string actual = _dataChannel.ExecuteCommand(Command, Guid.Empty, Guid.Empty);
            actual = _dataChannel.ExecuteCommand(Command, Guid.Empty, Guid.Empty);

            actual = TestHelper.CleanUp(actual);
            expected = TestHelper.CleanUp(expected);
            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void DsfRequest_MalformedXml_Expected_ErrorInDsfREsponse()
        {
            string expected = @"<Error><InnerError>Unexpected end of file has occurred. The following elements are not closed: x, x. Line 1, position 62.</InnerError></Error>";
            string xmlRequest = string.Format("<x><Service>{0}</Service><x>", Guid.NewGuid().ToString());
            string actual = _dataChannel.ExecuteCommand(xmlRequest, Guid.Empty, Guid.NewGuid());

            actual = TestHelper.CleanUp(actual);
            expected = TestHelper.CleanUp(expected);

            Assert.AreEqual(expected, actual);
        }

        #endregion Studio Server Integration

        
    }
}
