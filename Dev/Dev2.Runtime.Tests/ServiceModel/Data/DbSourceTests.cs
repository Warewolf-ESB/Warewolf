/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;


namespace Dev2.Tests.Runtime.ServiceModel

{
    [TestClass]
    [TestCategory("Runtime Hosting")]
    public class DbSourceTests
    {
        #region ToString Tests

        [TestMethod]
        public void ToStringFullySetupObjectExpectedJsonSerializedObjectReturnedAsString()
        {
            var testDbSource = SetupDefaultDbSource();
            var actualDbSourceToString = testDbSource.ToString();
            var expected = JsonConvert.SerializeObject(testDbSource);
            Assert.AreEqual(expected, actualDbSourceToString);
        }

        [TestMethod]
        public void ToStringEmptyObjectExpected()
        {
            var testDbSource = new DbSource();
            var actualSerializedDbSource = testDbSource.ToString();
            var expected = JsonConvert.SerializeObject(testDbSource);
            Assert.AreEqual(expected, actualSerializedDbSource);
        }

        #endregion ToString Tests

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_NotNamedInstance_ShouldUsePortNumber()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource
            {
                Server = "myserver", 
                ServerType = enSourceType.SqlDatabase, 
                AuthenticationType = AuthenticationType.Windows, 
                DatabaseName = "testdb",
                Port=1433
            };
            //------------Execute Test---------------------------
            var connectionString = dbSource.ConnectionString;
            //------------Assert Results-------------------------
            StringAssert.Contains(connectionString,",1433");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_NamedInstanceDefaultPort_ShouldNotUsePort()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource
            {
                Server = "myserver\\instance", 
                ServerType = enSourceType.SqlDatabase, 
                AuthenticationType = AuthenticationType.Windows, 
                DatabaseName = "testdb",
                Port=1433
            };
            //------------Execute Test---------------------------
            var connectionString = dbSource.ConnectionString;
            //------------Assert Results-------------------------
            var contains = connectionString.Contains(",1433");
            Assert.IsFalse(contains);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_NamedInstanceNotDefaultPort_ShouldUsePort()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource
            {
                Server = "myserver\\instance", 
                ServerType = enSourceType.SqlDatabase, 
                AuthenticationType = AuthenticationType.Windows, 
                DatabaseName = "testdb",
                Port=2011
            };
            //------------Execute Test---------------------------
            var connectionString = dbSource.ConnectionString;
            //------------Assert Results-------------------------
            var contains = connectionString.Contains(",2011");
            Assert.IsTrue(contains);
        }

        #region ToXml Tests

        [TestMethod]
        public void ToXmlAllPropertiesSetupExpectedXElementContainingAllObjectInformation()
        {
            var testDbSource = SetupDefaultDbSource();
            var expectedXml = testDbSource.ToXml();
            var workflowXamlDefintion = expectedXml.Element("XamlDefinition");
            var attrib = expectedXml.Attributes();
            var attribEnum = attrib.GetEnumerator();
            while(attribEnum.MoveNext())
            {
                if(attribEnum.Current.Name == "Name")
                {
                    Assert.AreEqual("TestResourceIMadeUp", attribEnum.Current.Value);
                    break;
                }
            }
            Assert.IsNull(workflowXamlDefintion);
        }

        [TestMethod]
        public void ToXmlEmptyObjectExpectedXElementContainingNoInformationRegardingSource()
        {
            var testDbSource = new DbSource();
            var expectedXml = testDbSource.ToXml();

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

        DbSource SetupDefaultDbSource()
        {
            var testDbSource = new DbSource
            {
                Server = "someServerIMadeUpToTest",
                Port = 420,
                AuthenticationType = AuthenticationType.Windows,
                UserID = @"Domain\User",
                Password = "secret",
                DatabaseName = "someDatabaseNameIMadeUpToTest",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourceType = "DbSource"
            };

            return testDbSource;
        }

        #endregion Private Test Methods
    }
}
