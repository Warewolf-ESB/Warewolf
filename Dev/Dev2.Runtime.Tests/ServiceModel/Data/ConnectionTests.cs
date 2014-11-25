
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
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
        [Owner("Travis Frisinger")]
        [TestCategory("Connections_GetTestConnectionAddress")]
        public void Connection_GetTestConnectionAddress_WhenDsfPresent_ExpectSameAddress()
        {
            //------------Setup for test--------------------------
            const string address = "http://localhost:3142/dsf";
            var conn = new Connection
            {
                ResourceType = ResourceType.Server,
                Address = address
            };


            //------------Execute Test---------------------------
            var result = conn.FetchTestConnectionAddress();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, address);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Connections_GetTestConnectionAddress")]
        public void Connection_GetTestConnectionAddress_WhenOnlySlashPresent_ExpectDsfAdded()
        {

            //------------Setup for test--------------------------
            const string address = "http://localhost:3142/";
            const string expected = address + "dsf";
            var conn = new Connection
            {
                ResourceType = ResourceType.Server,
                Address = address
            };


            //------------Execute Test---------------------------
            var result = conn.FetchTestConnectionAddress();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Connections_GetTestConnectionAddress")]
        public void Connection_GetTestConnectionAddress_WhenNoSlashPresent_ExpectSlashAndDsfAdded()
        {
            //------------Setup for test--------------------------
            const string address = "http://localhost:3142";
            const string expected = address + "/dsf";
            var conn = new Connection
            {
                ResourceType = ResourceType.Server,
                Address = address
            };


            //------------Execute Test---------------------------
            var result = conn.FetchTestConnectionAddress();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Connections_Test_InvalidPortNumber_InvalidResult()
        {
            Verify_Test("http://testurl:-1", "Invalid URI: Invalid port specified.", () => null);
        }

        [TestMethod]
        public void Connections_Test_WebExceptionThrown_InvalidResult()
        {
            const string Error = "Connection Error : Unable to connect to the remote server";
            const WebExceptionStatus Status = WebExceptionStatus.ConnectFailure;
            Verify_Test("http://testurl:333", Error, () => { throw new WebException(Error, Status); });
        }

        [TestMethod]
        public void Connections_Test_ExceptionThrown_InvalidResult()
        {
            const string Error = "Connection Error : An unexpected error occurred";
            Verify_Test("http://testurl:333", Error, () => { throw new Exception(Error); });
        }

        [TestMethod]
        public void Connections_Test_FatalErrorThrown_InvalidResult()
        {
            const string Message = "An internal error occurred while executing the service request";
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
        [Owner("Travis Frisinger")]
        [TestCategory("Connection_Save")]
        public void Connection_Save_WhenPublicUserType_ExpectConnectionWithSingleSlashStringAsUser()
        {
            //------------Setup for test--------------------------
            var connections = CreateConnection();
            const string conStr = @"<Source ID=""2aa3fdba-e0c3-47dd-8dd5-e6f24aaf5c7a"" Name=""test server"" Type=""Dev2Server"" ConnectionString=""AppServerUri=http://178.63.172.163:3142/dsf;WebServerPort=3142;AuthenticationType=Public;UserName=;Password="" Version=""1.0"" ResourceType=""Server"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <TypeOf>Dev2Server</TypeOf>
  <DisplayName>test server</DisplayName>
  <Category>WAREWOLF SERVERS</Category>
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>1ia51dqx+BIMQ4QgLt+DuKtTBUk=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Wqd39EqkFE66XVETuuAqZveoTk3JiWtAk8m1m4QykeqY4/xQmdqRRSaEfYBr7EHsycI3STuILCjsz4OZgYQ2QL41jorbwULO3NxAEhu4nrb2EolpoNSJkahfL/N9X5CvLNwpburD4/bPMG2jYegVublIxE50yF6ZZWG5XiB6SF8=</SignatureValue>
  </Signature>
</Source>";

            var xe = XElement.Parse(conStr);
            var conObj = new Connection(xe);
            var jsonObj = JsonConvert.SerializeObject(conObj);

            //------------Execute Test---------------------------
            var savedConObj = connections.Save(jsonObj, Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            var connection = JsonConvert.DeserializeObject<Connection>(savedConObj);

            Assert.AreEqual("\\", connection.UserName);
            Assert.AreEqual(string.Empty, connection.Password);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Connection_Save")]
        public void Connection_Save_WhenPortNumberDifferent_ExpectConnectionNonDefaultPortNumber()
        {
            //------------Setup for test--------------------------
            var connections = CreateConnection();
            const string conStr = @"<Source ID=""2aa3fdba-e0c3-47dd-8dd5-e6f24aaf5c7a"" Name=""test server"" Type=""Dev2Server"" ConnectionString=""AppServerUri=http://178.63.172.163:4142/dsf;WebServerPort=3142;AuthenticationType=Public;UserName=;Password="" Version=""1.0"" ResourceType=""Server"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <TypeOf>Dev2Server</TypeOf>
  <DisplayName>test server</DisplayName>
  <Category>WAREWOLF SERVERS</Category>
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>1ia51dqx+BIMQ4QgLt+DuKtTBUk=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Wqd39EqkFE66XVETuuAqZveoTk3JiWtAk8m1m4QykeqY4/xQmdqRRSaEfYBr7EHsycI3STuILCjsz4OZgYQ2QL41jorbwULO3NxAEhu4nrb2EolpoNSJkahfL/N9X5CvLNwpburD4/bPMG2jYegVublIxE50yF6ZZWG5XiB6SF8=</SignatureValue>
  </Signature>
</Source>";

            var xe = XElement.Parse(conStr);
            var conObj = new Connection(xe);
            var jsonObj = JsonConvert.SerializeObject(conObj);

            //------------Execute Test---------------------------
            var savedConObj = connections.Save(jsonObj, Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            var connection = JsonConvert.DeserializeObject<Connection>(savedConObj);
            Assert.AreEqual(4142, connection.WebServerPort);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Connection_Save")]
        public void Connection_Save_WhenBlankUserAndPass_ExpectConnectionWithEmptyStringAsUser()
        {
            //------------Setup for test--------------------------
            var connections = CreateConnection();
            const string conStr = @"<Source ID=""2aa3fdba-e0c3-47dd-8dd5-e6f24aaf5c7a"" Name=""test server"" Type=""Dev2Server"" ConnectionString=""AppServerUri=http://178.63.172.163:3142/dsf;WebServerPort=3142;AuthenticationType=User;UserName=;Password="" Version=""1.0"" ResourceType=""Server"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <TypeOf>Dev2Server</TypeOf>
  <DisplayName>test server</DisplayName>
  <Category>WAREWOLF SERVERS</Category>
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>1ia51dqx+BIMQ4QgLt+DuKtTBUk=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Wqd39EqkFE66XVETuuAqZveoTk3JiWtAk8m1m4QykeqY4/xQmdqRRSaEfYBr7EHsycI3STuILCjsz4OZgYQ2QL41jorbwULO3NxAEhu4nrb2EolpoNSJkahfL/N9X5CvLNwpburD4/bPMG2jYegVublIxE50yF6ZZWG5XiB6SF8=</SignatureValue>
  </Signature>
</Source>";

            var xe = XElement.Parse(conStr);
            var conObj = new Connection(xe);
            var jsonObj = JsonConvert.SerializeObject(conObj);

            //------------Execute Test---------------------------
            var savedConObj = connections.Save(jsonObj, Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            var connection = JsonConvert.DeserializeObject<Connection>(savedConObj);

            Assert.AreEqual(String.Empty, connection.UserName);
            Assert.AreEqual(string.Empty, connection.Password);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Connection_Save")]
        public void Connection_Get_WhenBlankUserAndPass_ExpectConnectionWithEmptyAsUser()
        {
            //------------Setup for test--------------------------
            const string fetchGuid = "2aa3fdba-e0c3-47dd-8dd5-e6f24aaf5c7a";

            var connections = CreateConnection();

            const string conStr = @"<Source ID=""2aa3fdba-e0c3-47dd-8dd5-e6f24aaf5c7a"" Name=""test server"" Type=""Dev2Server"" ConnectionString=""AppServerUri=http://178.63.172.163:3142/dsf;WebServerPort=3142;AuthenticationType=User;UserName=;Password="" Version=""1.0"" ResourceType=""Server"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <TypeOf>Dev2Server</TypeOf>
  <DisplayName>test server</DisplayName>
  <Category>WAREWOLF SERVERS</Category>
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>1ia51dqx+BIMQ4QgLt+DuKtTBUk=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Wqd39EqkFE66XVETuuAqZveoTk3JiWtAk8m1m4QykeqY4/xQmdqRRSaEfYBr7EHsycI3STuILCjsz4OZgYQ2QL41jorbwULO3NxAEhu4nrb2EolpoNSJkahfL/N9X5CvLNwpburD4/bPMG2jYegVublIxE50yF6ZZWG5XiB6SF8=</SignatureValue>
  </Signature>
</Source>";

            // create it save it and finally fetch it to ensure it has th

            var xe = XElement.Parse(conStr);
            var conObj = new Connection(xe);
            var jsonObj = JsonConvert.SerializeObject(conObj);
            var savedObj = connections.Save(jsonObj, Guid.Empty, Guid.Empty);
            var connection = JsonConvert.DeserializeObject<Connection>(savedObj);

            //Pre-asserts
            Assert.AreEqual(string.Empty, connection.UserName);
            Assert.AreEqual(String.Empty, connection.Password);

            //------------Execute Test---------------------------
            var fetchedObj = connections.Get(fetchGuid, Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------

            // ensure it was returned with empty and empty

            Assert.AreEqual(String.Empty, fetchedObj.UserName);
            Assert.AreEqual(string.Empty, fetchedObj.Password);

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
            Connection testConnection = new Connection { Address = "http://someAddressIMadeUpToTest:7654/Server", AuthenticationType = AuthenticationType.Windows, Password = "secret", ResourceID = Guid.NewGuid(), ResourceName = "TestResourceIMadeUp", ResourcePath = @"host\Server", ResourceType = ResourceType.Server, UserName = @"Domain\User", WebServerPort = 8080 };

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
