using System;
using System.Net;
using System.Text;
using Dev2.Network;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }

        }

        /// <summary>
        ///Validate that resources are loaded from a valid environment
        ///</summary>
        ///
        [TestMethod]
        public void AppServer_Update_Resource_Correctly()
        {
            string expected = @"Updated Workflow Service 'ServiceToBindFrom'";
            var Command = new StringBuilder(TestResource.Service_Update_Request_String);

            //Execute twice to ensure that the resource is actually there
            var actual = _connection.ExecuteCommand(Command, Guid.Empty, Guid.Empty);
            actual = _connection.ExecuteCommand(Command, Guid.Empty, Guid.Empty);

            //actual = TestHelper.CleanUp(actual);
            //expected = TestHelper.CleanUp(expected);
            StringAssert.Contains(actual.ToString(), expected, "Got [ " + actual + " ]");
        }

        [TestMethod]
        public void DsfRequest_MalformedXml_Expected_ErrorInDsfREsponse()
        {
            var expected = @"<Error><InnerError>Unexpected end of file has occurred. The following elements are not closed: x, x. Line 1, position 62.</InnerError></Error>";
            var xmlRequest = new StringBuilder(string.Format("<x><Service>{0}</Service><x>", Guid.NewGuid().ToString()));
            var actual = _connection.ExecuteCommand(xmlRequest, Guid.Empty, Guid.NewGuid());

            //actual = TestHelper.CleanUp(actual);
            //expected = TestHelper.CleanUp(expected);

            Assert.AreEqual(expected, actual);
        }

        #endregion Studio Server Integration

    }
}
