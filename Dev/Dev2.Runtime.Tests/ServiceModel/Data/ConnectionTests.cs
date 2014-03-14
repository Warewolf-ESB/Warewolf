using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConnectionTests
    {
        [TestMethod]
        public void Connections_Test_InvalidPortNumber_InvalidResult()
        {
            Verify_Test("http://testurl:-1", "Invalid URI: Invalid port specified.", () => null);
        }

        [TestMethod]
        public void Connections_Test_WebExceptionThrown_InvalidResult()
        {
            const string Error = "Unable to connect to the remote server";
            const WebExceptionStatus Status = WebExceptionStatus.ConnectFailure;
            Verify_Test("http://testurl:333", string.Format("{0} - {1}", Status, Error), () => { throw new WebException(Error, Status); });
        }

        [TestMethod]
        public void Connections_Test_ExceptionThrown_InvalidResult()
            {
            const string Error = "An unexpected error occurre";
            Verify_Test("http://testurl:333", Error, () => { throw new Exception(Error); });
        }

        [TestMethod]
        public void Connections_Test_FatalErrorThrown_InvalidResult()
        {
            const string Message = "An internal error occured while executing the service request";
            const string InnerError = "Service [ ping ] not found.";
            const string FatalError = "<FatalError><Message>" + Message + "</Message><InnerError>" + InnerError + "</InnerError></FatalError>";
            const string Error = Message + " - " + InnerError;

            Verify_Test("http://testurl:333", Error, () => FatalError);
        }

        [TestMethod]
        public void Connections_Test_ValidUri_ValidResult()
        {
            Verify_Test("http://testurl:333", "", () => "<DataList/>");
        }

        static void Verify_Test(string address, string errorMessage, Func<string> connectToServerResult)
        {
            var conn = new Connection
            {
                ResourceType = ResourceType.Server,
                Address = address
            };
            var connections = new ConnectionsMock(connectToServerResult);
            var result = connections.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);

            Assert.AreEqual(string.IsNullOrEmpty(errorMessage), result.IsValid);
            Assert.AreEqual(errorMessage, result.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_Search")]
        public void Connections_Search_NoSearchTerm_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            var connections = CreateConnection();
            
            //------------Execute Test---------------------------
            var search = connections.Search("", Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            var upperedSearchString = search.ToUpper();
            StringAssert.Contains(upperedSearchString, "[");
            StringAssert.Contains(upperedSearchString, "rsaklfsvrtfsbld".ToUpper());
            StringAssert.Contains(upperedSearchString, "rsaklfsvrgendev".ToUpper());
            StringAssert.Contains(upperedSearchString, "]");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_Search")]
        public void Connections_Search_NullSearchTerm_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            var connections = CreateConnection();
            
            //------------Execute Test---------------------------
            var search = connections.Search(null, Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            var upperedSearchString = search.ToUpper();
            StringAssert.Contains(upperedSearchString, "[");
            StringAssert.Contains(upperedSearchString, "rsaklfsvrtfsbld".ToUpper());
            StringAssert.Contains(upperedSearchString, "rsaklfsvrgendev".ToUpper());
            StringAssert.Contains(upperedSearchString, "]");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_Search")]
        public void Connections_Search_WithSearchTerm_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            var connections = CreateConnection();
            
            //------------Execute Test---------------------------
            var search = connections.Search("gendev", Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            var upperedSearchString = search.ToUpper();
            StringAssert.Contains(upperedSearchString, "[");
            StringAssert.Contains(upperedSearchString, "rsaklfsvrgendev".ToUpper());
            StringAssert.Contains(upperedSearchString, "]");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_Search")]
        public void Connections_Search_WithSearchTermNoComputerFound_ShouldReturnEmptyResults()
        {
            //------------Setup for test--------------------------
            var connections = CreateConnection();
            
            //------------Execute Test---------------------------
            var search = connections.Search("testgreenmonster", Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual("[]", search);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_GetTestAddress")]
        public void Connections_GetTestAddress_HasDsf_DsfRemoved()
        {
            //------------Setup for test--------------------------
            var connections = new Connections();
            var connection = new Connection();
            connection.Address = "http://localhost:3142/dsf";
            //------------Execute Test---------------------------
            var testAddress = connections.GetTestAddress(connection);
            //------------Assert Results-------------------------
            Assert.AreEqual("http://localhost:3142", testAddress);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_GetTestAddress")]
        public void Connections_GetTestAddress_NoDsf_Nothing()
        {
            //------------Setup for test--------------------------
            var connections = new Connections();
            var connection = new Connection();
            connection.Address = "http://localhost:3142";
            //------------Execute Test---------------------------
            var testAddress = connections.GetTestAddress(connection);
            //------------Assert Results-------------------------
            Assert.AreEqual("http://localhost:3142", testAddress);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_GetTestAddress")]
        public void Connections_GetTestAddress_HasNullConnection_DsfRemoved()
        {
            //------------Setup for test--------------------------
            var connections = new Connections();
            //------------Execute Test---------------------------
            var testAddress = connections.GetTestAddress(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("", testAddress);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_GetTestAddress")]
        public void Connections_GetTestAddress_HasNullAddress_DsfRemoved()
        {
            //------------Setup for test--------------------------
            var connections = new Connections();
            var connection = new Connection();
            connection.Address = null;
            //------------Execute Test---------------------------
            var testAddress = connections.GetTestAddress(connection);
            //------------Assert Results-------------------------
            Assert.AreEqual("", testAddress);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connections_GetTestAddress")]
        public void Connections_GetTestAddress_HasEmptyAddress_DsfRemoved()
        {
            //------------Setup for test--------------------------
            var connections = new Connections();
            var connection = new Connection();
            connection.Address = "";
            //------------Execute Test---------------------------
            var testAddress = connections.GetTestAddress(connection);
            //------------Assert Results-------------------------
            Assert.AreEqual("", testAddress);
        }

        #region ToString Tests

        [TestMethod]
        public void ToString_FullySetupObject_Expected_JSONSerializedObjectReturnedAsString()
        {
            Connection testConnection = SetupDefaultConnection();
            string actualConnectionToString = testConnection.ToString();
            string expected = JsonConvert.SerializeObject(testConnection);
            Assert.AreEqual(expected, actualConnectionToString);
        }

        [TestMethod]
        public void ToString_EmptyObject_Expected_()
        {
            Connection testConnection = new Connection();
            string actualSerializedConnection = testConnection.ToString();
            string expected = JsonConvert.SerializeObject(testConnection);
            Assert.AreEqual(expected, actualSerializedConnection);
        }

        #endregion ToString Tests

        #region ToXml Tests

        [TestMethod]
        public void ToXml_AllPropertiesSetup_Expected_XElementContainingAllObjectInformation()
        {

            Connection testConnection = SetupDefaultConnection();
            XElement expectedXml = testConnection.ToXml();

            IEnumerable<XAttribute> attrib = expectedXml.Attributes();
            IEnumerator<XAttribute> attribEnum = attrib.GetEnumerator();
            while(attribEnum.MoveNext())
            {
                if(attribEnum.Current.Name == "Name")
                {
                    Assert.AreEqual("TestResourceIMadeUp", attribEnum.Current.Value);
                    break;
                }
            }
        }

        [TestMethod]
        public void ToXml_EmptyObject_Expected_XElementContainingNoInformationRegardingSource()
        {
            Connection testConnection = new Connection();
            XElement expectedXml = testConnection.ToXml();

            IEnumerable<XAttribute> attrib = expectedXml.Attributes();
            IEnumerator<XAttribute> attribEnum = attrib.GetEnumerator();
            while(attribEnum.MoveNext())
            {
                if(attribEnum.Current.Name == "Name")
                {
                    Assert.AreEqual(string.Empty, attribEnum.Current.Value);
                    break;
                }
            }
        }

        #endregion ToXml Tests

        #region Private Test Methods

        private Connection SetupDefaultConnection()
        {
            Connection testConnection = new Connection();
            testConnection.Address = "http://someAddressIMadeUpToTest:7654/Server";
            testConnection.AuthenticationType = AuthenticationType.Windows;
            testConnection.Password = "secret";
            testConnection.ResourceID = Guid.NewGuid();
            testConnection.ResourceName = "TestResourceIMadeUp";
            testConnection.ResourcePath = @"host\Server";
            testConnection.ResourceType = ResourceType.Server;
            testConnection.UserName = @"Domain\User";
            testConnection.WebServerPort = 8080;

            return testConnection;
        }

        private Connections CreateConnection()
        {
            return new Connections(CreateQueryFn());
        }

        // Proper Unit Testing, Avoid External Resource ;)
        private Func<List<string>> CreateQueryFn()
        {
            return () => new List<string> { "RSAKLFSVRGENDEV", "RSAKLFSVRTFSBLD" };
        } 

        #endregion Private Test Methods
    }
}
