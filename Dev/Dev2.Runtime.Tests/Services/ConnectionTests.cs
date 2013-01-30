using Dev2.Runtime.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Dev2.Runtime.Services.Tests
{
    [TestClass]
    public class ConnectionTests
    {
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
