/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;


namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
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
                ResourceType = "Server",
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
                ResourceType = "Server",
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
                ResourceType = "Server",
                Address = address
            };


            //------------Execute Test---------------------------
            var result = conn.FetchTestConnectionAddress();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, expected);
        }
        


        #region ToString Tests

        [TestMethod]
        public void ToString_FullySetupObject_Expected_JSONSerializedObjectReturnedAsString()
        {
            var testConnection = SetupDefaultConnection();
            var actualConnectionToString = testConnection.ToString();
            var expected = JsonConvert.SerializeObject(testConnection);
            Assert.AreEqual(expected, actualConnectionToString);
        }

        [TestMethod]
        public void ToString_EmptyObject_Expected_()
        {
            var testConnection = new Connection();
            var actualSerializedConnection = testConnection.ToString();
            var expected = JsonConvert.SerializeObject(testConnection);
            Assert.AreEqual(expected, actualSerializedConnection);
        }

        #endregion ToString Tests

        #region ToXml Tests

        [TestMethod]
        public void ToXml_AllPropertiesSetup_Expected_XElementContainingAllObjectInformation()
        {

            var testConnection = SetupDefaultConnection();
            var expectedXml = testConnection.ToXml();

            var attrib = expectedXml.Attributes();
            var attribEnum = attrib.GetEnumerator();
            while (attribEnum.MoveNext())
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
            var testConnection = new Connection();
            var expectedXml = testConnection.ToXml();

            var attrib = expectedXml.Attributes();
            var attribEnum = attrib.GetEnumerator();
            while (attribEnum.MoveNext())
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

        Connection SetupDefaultConnection()
        {
            var testConnection = new Connection { Address = "http://someAddressIMadeUpToTest:7654/Server", AuthenticationType = AuthenticationType.Windows, Password = "secret", ResourceID = Guid.NewGuid(), ResourceName = "TestResourceIMadeUp", ResourceType = "Server", UserName = @"Domain\User", WebServerPort = 8080 };

            return testConnection;
        }

        #endregion Private Test Methods
    }
}
