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
        [Owner("Security Review")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_SqlDatabase_DefaultsToValidatingCertificate()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource
            {
                Server = "myserver",
                ServerType = enSourceType.SqlDatabase,
                AuthenticationType = AuthenticationType.Windows,
                DatabaseName = "testdb",
                Port = 1433
            };
            //------------Execute Test---------------------------
            var connectionString = dbSource.ConnectionString;
            //------------Assert Results-------------------------
            Assert.IsFalse(dbSource.TrustServerCertificate, "TrustServerCertificate must default to false (secure).");
            Assert.IsFalse(connectionString.Contains("TrustServerCertificate", StringComparison.OrdinalIgnoreCase),
                $"Production connection string must not disable certificate validation: {connectionString}");
        }

        [TestMethod]
        [Owner("Security Review")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_SqlDatabase_WhenTrustServerCertificate_SkipsValidation()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource
            {
                Server = "myserver",
                ServerType = enSourceType.SqlDatabase,
                AuthenticationType = AuthenticationType.Windows,
                DatabaseName = "testdb",
                Port = 1433,
                TrustServerCertificate = true
            };
            //------------Execute Test---------------------------
            var connectionString = dbSource.ConnectionString;
            //------------Assert Results-------------------------
            StringAssert.Contains(connectionString, "TrustServerCertificate=True");
        }

        [TestMethod]
        [Owner("Security Review")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_SqlDatabase_TrustServerCertificate_RoundTrips()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource { ServerType = enSourceType.SqlDatabase };
            //------------Execute Test---------------------------
            dbSource.ConnectionString = "Data Source=myserver,1433;Initial Catalog=testdb;User ID=u;Password=p;Connection Timeout=30;TrustServerCertificate=True";
            //------------Assert Results-------------------------
            Assert.IsTrue(dbSource.TrustServerCertificate, "Flag must round-trip from the persisted connection string.");
            StringAssert.Contains(dbSource.ConnectionString, "TrustServerCertificate=True");
        }

        [TestMethod]
        [Owner("Security Review")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_SqlDatabase_WithoutTrustServerCertificate_RoundTripsAsValidated()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource { ServerType = enSourceType.SqlDatabase };
            //------------Execute Test---------------------------
            dbSource.ConnectionString = "Data Source=myserver,1433;Initial Catalog=testdb;User ID=u;Password=p;Connection Timeout=30";
            //------------Assert Results-------------------------
            Assert.IsFalse(dbSource.TrustServerCertificate, "Absent keyword must parse as secure (false).");
            Assert.IsFalse(dbSource.ConnectionString.Contains("TrustServerCertificate", StringComparison.OrdinalIgnoreCase));
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

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_Oracle_WithDatabaseName_ShouldIncludeDatabaseParameter()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource
            {
                Server = "localhost:1521/XEPDB1", 
                ServerType = enSourceType.Oracle, 
                AuthenticationType = AuthenticationType.User,
                UserID = "orderdb",
                Password = "orderdb123", 
                DatabaseName = "ORDERDB",
                ConnectionTimeout = 30
            };
            //------------Execute Test---------------------------
            var connectionString = dbSource.ConnectionString;
            //------------Assert Results-------------------------
            StringAssert.Contains(connectionString, "Database=ORDERDB;");
            StringAssert.Contains(connectionString, "User Id=orderdb");
            StringAssert.Contains(connectionString, "Password=orderdb123");
            StringAssert.Contains(connectionString, "Data Source=localhost:1521/XEPDB1");
            StringAssert.Contains(connectionString, "Connection Timeout=30");
            // Ensure no double semicolons
            Assert.IsFalse(connectionString.Contains(";;"));
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_Oracle_WithoutDatabaseName_ShouldNotIncludeDatabaseParameter()
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource
            {
                Server = "localhost:1521/XEPDB1", 
                ServerType = enSourceType.Oracle, 
                AuthenticationType = AuthenticationType.User,
                UserID = "orderdb",
                Password = "orderdb123", 
                DatabaseName = null,
                ConnectionTimeout = 30
            };
            //------------Execute Test---------------------------
            var connectionString = dbSource.ConnectionString;
            //------------Assert Results-------------------------
            Assert.IsFalse(connectionString.Contains("Database="));
            StringAssert.Contains(connectionString, "User Id=orderdb");
            StringAssert.Contains(connectionString, "Data Source=localhost:1521/XEPDB1");
            // Ensure no double semicolons
            Assert.IsFalse(connectionString.Contains(";;"));
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("DbSource_ConnectionString")]
        public void DbSource_ConnectionString_Oracle_ParsesAndRetainsDatabaseName()
        {
            //------------Setup for test--------------------------
            var connectionString = "User Id=orderdb;Password=orderdb123;Data Source=localhost:1521/XEPDB1;Database=ORDERDB;Connection Timeout=30;";
            var dbSource = new DbSource
            {
                ServerType = enSourceType.Oracle
            };
            //------------Execute Test---------------------------
            dbSource.ConnectionString = connectionString;
            //------------Assert Results-------------------------
            Assert.AreEqual("ORDERDB", dbSource.DatabaseName);
            Assert.AreEqual("orderdb", dbSource.UserID);
            Assert.AreEqual("orderdb123", dbSource.Password);
            Assert.AreEqual("localhost:1521/XEPDB1", dbSource.Server);
            Assert.AreEqual(30, dbSource.ConnectionTimeout);
            
            // Now get the connection string back
            var regeneratedConnectionString = dbSource.ConnectionString;
            StringAssert.Contains(regeneratedConnectionString, "Database=ORDERDB;");
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
