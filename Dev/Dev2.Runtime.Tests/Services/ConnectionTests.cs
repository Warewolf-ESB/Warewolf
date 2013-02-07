using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class ConnectionTests
    {
        #region Test

        [TestMethod]
        public void Test_With_InvalidUriFormat_Expected_ReturnsInvalidResult()
        {
            var conn = new Connection
            {
                ResourceType = enSourceType.Dev2Server,
                Address = "http://www.google.co.za"
            };
            var connections = new Connections();
            var result = connections.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(false, result.IsValid);

        }

        //[TestMethod]
        //public void Test_With_ValidUriFormatForInvalidHost_Expected_ReturnsInvalidResult()
        //{
        //    var conn = new Connection
        //    {
        //        ResourceType = enSourceType.Dev2Server,
        //        //Address = "http://localhost:77/dsf"
        //        Address = "http://localhost:77/dsf"
        //    };
        //    var connections = new Connections();
        //    var result = connections.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
        //    Assert.AreEqual(false, result.IsValid);
        //}

        [TestMethod]
        public void Test_With_ValidUriFormat_Expected_ReturnsValidResult()
        {
            var conn = new Connection
            {
                ResourceType = enSourceType.Dev2Server,
                Address = "http://192.168.13.42:788/dsf"
            };
            var connections = new ConnectionsMock { CanConnectToTcpClientHitCount = 0 };
            var result = connections.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(1, connections.CanConnectToTcpClientHitCount);
            Assert.AreEqual(true, result.IsValid);
        }

        #endregion

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
            testConnection.ResourceType = DynamicServices.enSourceType.Dev2Server;
            testConnection.UserName = @"Domain\User";
            testConnection.WebServerPort = 8080;

            return testConnection;
        }

        #endregion Private Test Methods
    }
}
