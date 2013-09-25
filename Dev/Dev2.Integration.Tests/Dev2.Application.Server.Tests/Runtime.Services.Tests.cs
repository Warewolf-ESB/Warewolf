using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dev2.Data.ServiceModel;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.Test_utils;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    [TestClass][Ignore]//Ashley: round 2 hunting the evil test
    public class RuntimeServicesTests
    {
        #region Plugin

        #region Successfully saving and loading 

        [TestMethod]
        [Ignore]
        public void LoadExpectedXmlReturnedProperly()
        {
            string webServerAddress = ServerSettings.WebserverURI.Replace("services/", "wwwroot/sources/pluginsource?rid=2f93aa19-d507-4ed0-9b7e-a8b1b07ce12f#");

            var result = TestHelper.GetResponseFromServer(webServerAddress);

            var allKeys = result.Headers.AllKeys;
            const string ContentType = "Content-Type";
            const string ContentDisposition = "Content-Disposition";
            CollectionAssert.Contains(allKeys, ContentType);
            CollectionAssert.DoesNotContain(allKeys, ContentDisposition);

            var contentTypeValue = result.Headers.Get(ContentType);

            Assert.AreNotEqual("http/xml", contentTypeValue);
        }

        [TestMethod]
        public void SaveExpectsResponse()
        {
            string webServerAddress = ServerSettings.WebserverURI.Replace("services/", "Service/PluginSources/Save");
            string postData = String.Format("{0}{1}", webServerAddress, "{\"resourceID\":\"2f93aa19-d507-4ed0-9b7e-a8b1b07ce12f\",\"resourceType\":\"PluginSource\",\"resourceName\":\"Anything To Xml Hook Plugin\",\"resourcePath\":\"Conversion\",\"assemblyName\":\"\",\"assemblyLocation\":\"" + ServerCommonDirectory.Plugins + "\\Dev2.AnytingToXmlHook.Plugin.dll\"}");

            string responseData = TestHelper.PostDataToWebserver(postData);

            Assert.IsFalse(string.IsNullOrEmpty(responseData));
        }

        #endregion

        #endregion

        #region Db Source

        #region Test Db Source

        [TestMethod]
        public void TestWithInvalidUriFormatExpectedReturnsInvalidResult()
        {
            var conn = new DbSource
            {
                ResourceType = ResourceType.DbSource,
                Server = "http://www.google.co.za"
            };
            var dbSources = new DbSources();
            var result = dbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(false, result.IsValid);

        }

        //Massimo.Guerrera - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        public void TestWithValidUserDetailsForValidHostExpectedReturnsValidResult()
        {
            string username = "DEV2\\IntegrationTester";
            string password = "I73573r0";
            var conn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var DbSources = new DbSources();
            var result = DbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(true, result.IsValid);
        }

        //Massimo.Guerrera - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        public void TestWithInvalidUserDetailsForValidHostExpectedReturnsInvalidResult()
        {
            string username = "DEV2\\Billy.Jane";
            string password = "invalidPassword";
            var conn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var DbSources = new DbSources();
            var result = DbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(false, result.IsValid);
        }

        [TestMethod]
        public void TestWithValidUriFormatExpectedReturnsValidResult()
        {
            var conn = new DbSource
            {
                ResourceType = ResourceType.DbSource,
                Server = "192.168.13.42"
            };
            var DbSources = new DbSources();
            var result = DbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(false, result.IsValid);
        }

        [TestMethod]
        public void TestWithInvalidDBFormatUsernameExpectedReturnsInvalidResult()
        {
            string username = "Billy.Jane";
            string password = "invalidPassword";
            var conn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var DbSources = new DbSources();
            var result = DbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(false, result.IsValid);
        }

        [TestMethod]
        public void TestWithValidDBFormatUsernameExpectedReturnsValidResult()
        {
            var username = "testUser";
            var password = "test123";
            var conn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var DbSources = new DbSources();
            var result = DbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(true, result.IsValid);
        }

        // http://127.0.0.1:1234/services/Bug9394

        [TestMethod]
        [Ignore]
        public void TestDomainUserCanExecut()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Bug9394");
            string expected = @"<countries><CountryID>10</CountryID><Description>Azerbaijan</Description></countries>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            // Ensure we got result from executing ;)
            Assert.IsTrue(ResponseData.IndexOf(expected) >= 0);            

            //Assert.Inconclusive("Test is failing because of plugins");
        }
        #endregion

        #region Get Service Methods

        //Ashley.Lewis - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        public void GetServiceMethodsWithValidDetailsExpectedServiceMethods()
        {
            string username = "DEV2\\IntegrationTester";
            string password = "I73573r0";
            var conn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var MsSqlBroker = new MsSqlBroker();
            var result = MsSqlBroker.GetServiceMethods(conn);
            Assert.AreEqual(true, result.Count > 0);
        }

        //Ashley.Lewis - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetServiceMethodsWithValidDetailsExpectedReturnsInvalidResult()
        {
            string username = "DEV2\\Billy.Jane";
            string password = "invalidPassword";
            var conn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var MsSqlBroker = new MsSqlBroker();
            MsSqlBroker.GetServiceMethods(conn);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void GetServiceMethodsWithInvalidDBFormatUsernameExpectedReturnsInvalidResult()
        {
            string username = "Billy.Jane";
            string password = "invalidPassword";
            var conn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var MsSqlBroker = new MsSqlBroker();
            MsSqlBroker.GetServiceMethods(conn);
        }

        [TestMethod]
        public void GetServiceMethodsWithValidDBFormatUsernameExpectedReturnsValidResult()
        {
            var username = "testUser";
            var password = "test123";
            var conn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var MsSqlBroker = new MsSqlBroker();
            var result = MsSqlBroker.GetServiceMethods(conn);
            Assert.AreEqual(true, result.Count > 0);
        }

        #endregion

        #endregion

        #region Connection

        #region Test Connection

        [TestMethod]
        public void ConnectionsTestValidServerExpectedPositiveValidationResult()
        {
            //Create Connection
            Connection conn = SetupDefaultConnection();
            Connections connections = new Connections();

            //Attemp to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);

            Assert.IsTrue(validationResult.IsValid);

        }

        [TestMethod]
        public void ConnectionsTestInvalidServerExpectedNegativeValidationResult()
        {
            //Create Connection
            Connection conn = SetupDefaultConnection();
            // Invalidate connection
            conn.Address = "http://someserverImadeup:77/dsf";
            Connections connections = new Connections();
            // Attempt to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);
            Assert.IsFalse(validationResult.IsValid);

        }

        #endregion

        #endregion

        #region Db Service

        #region Test Db Service

        //Massimo.Guerrera - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        public void TestServiceWithValidUserDetailsForValidHostExpectedReturnsValidResult()
        {
            string username = "DEV2\\IntegrationTester";
            string password = "I73573r0";
            var sourceConn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var serviceConn = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DatabaseService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test",
                AuthorRoles = "",
                Dependencies = new List<ResourceForTree>(),
                FilePath = null,
                IsUpgraded = true,
                Method = new ServiceMethod("dbo.fn_diagramobjects", "\r\n\tCREATE FUNCTION dbo.fn_diagramobjects() \r\n\tRETURNS int\r\n\tWITH EXECUTE AS N'dbo'\r\n\tAS\r\n\tBEGIN\r\n\t\tdeclare @id_upgraddiagrams\t\tint\r\n\t\tdeclare @id_sysdiagrams\t\t\tint\r\n\t\tdeclare @id_helpdiagrams\t\tint\r\n\t\tdeclare @id_helpdiagramdefinition\tint\r\n\t\tdeclare @id_creatediagram\tint\r\n\t\tdeclare @id_renamediagram\tint\r\n\t\tdeclare @id_alterdiagram \tint \r\n\t\tdeclare @id_dropdiagram\t\tint\r\n\t\tdeclare @InstalledObjects\tint\r\n\r\n\t\tselect @InstalledObjects = 0\r\n\r\n\t\tselect \t@id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),\r\n\t\t\t@id_sysdiagrams = object_id(N'dbo.sysdiagrams'),\r\n\t\t\t@id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),\r\n\t\t\t@id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),\r\n\t\t\t@id_creatediagram = object_id(N'dbo.sp_creatediagram'),\r\n\t\t\t@id_renamediagram = object_id(N'dbo.sp_renamediagram'),\r\n\t\t\t@id_alterdiagram = object_id(N'dbo.sp_alterdiagram'), \r\n\t\t\t@id_dropdiagram = object_id(N'dbo.sp_dropdiagram')\r\n\r\n\t\tif @id_upgraddiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 1\r\n\t\tif @id_sysdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 2\r\n\t\tif @id_helpdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 4\r\n\t\tif @id_helpdiagramdefinition is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 8\r\n\t\tif @id_creatediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 16\r\n\t\tif @id_renamediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 32\r\n\t\tif @id_alterdiagram  is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 64\r\n\t\tif @id_dropdiagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 128\r\n\t\t\r\n\t\treturn @InstalledObjects \r\n\tEND\r\n\t", null, null, null),
                Recordset = new Recordset(),
                Source = sourceConn
            };
            var MsSqlBroker = new MsSqlBroker();
            var result = MsSqlBroker.TestService(serviceConn);
            Assert.AreEqual(OutputFormats.ShapedXML, result.Format);
        }

        //Massimo.Guerrera - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestServiceWithInvalidUserDetailsForValidHostExpectedReturnsInvalidResult()
        {
            string username = "DEV2\\Billy.Jane";
            string password = "invalidPassword";
            var sourceConn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var serviceConn = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DatabaseService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test",
                AuthorRoles = "",
                Dependencies = new List<ResourceForTree>(),
                FilePath = null,
                IsUpgraded = true,
                Method = new ServiceMethod("dbo.fn_diagramobjects", "\r\n\tCREATE FUNCTION dbo.fn_diagramobjects() \r\n\tRETURNS int\r\n\tWITH EXECUTE AS N'dbo'\r\n\tAS\r\n\tBEGIN\r\n\t\tdeclare @id_upgraddiagrams\t\tint\r\n\t\tdeclare @id_sysdiagrams\t\t\tint\r\n\t\tdeclare @id_helpdiagrams\t\tint\r\n\t\tdeclare @id_helpdiagramdefinition\tint\r\n\t\tdeclare @id_creatediagram\tint\r\n\t\tdeclare @id_renamediagram\tint\r\n\t\tdeclare @id_alterdiagram \tint \r\n\t\tdeclare @id_dropdiagram\t\tint\r\n\t\tdeclare @InstalledObjects\tint\r\n\r\n\t\tselect @InstalledObjects = 0\r\n\r\n\t\tselect \t@id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),\r\n\t\t\t@id_sysdiagrams = object_id(N'dbo.sysdiagrams'),\r\n\t\t\t@id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),\r\n\t\t\t@id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),\r\n\t\t\t@id_creatediagram = object_id(N'dbo.sp_creatediagram'),\r\n\t\t\t@id_renamediagram = object_id(N'dbo.sp_renamediagram'),\r\n\t\t\t@id_alterdiagram = object_id(N'dbo.sp_alterdiagram'), \r\n\t\t\t@id_dropdiagram = object_id(N'dbo.sp_dropdiagram')\r\n\r\n\t\tif @id_upgraddiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 1\r\n\t\tif @id_sysdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 2\r\n\t\tif @id_helpdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 4\r\n\t\tif @id_helpdiagramdefinition is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 8\r\n\t\tif @id_creatediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 16\r\n\t\tif @id_renamediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 32\r\n\t\tif @id_alterdiagram  is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 64\r\n\t\tif @id_dropdiagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 128\r\n\t\t\r\n\t\treturn @InstalledObjects \r\n\tEND\r\n\t", null, null, null),
                Recordset = new Recordset(),
                Source = sourceConn
            };
            var MsSqlBroker = new MsSqlBroker();
            MsSqlBroker.TestService(serviceConn);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void TestServiceWithInvalidDBFormatUsernameExpectedReturnsInvalidResult()
        {
            string username = "Billy.Jane";
            string password = "invalidPassword";
            var sourceConn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var serviceConn = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DatabaseService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test",
                AuthorRoles = "",
                Dependencies = new List<ResourceForTree>(),
                FilePath = null,
                IsUpgraded = true,
                Method = new ServiceMethod("dbo.fn_diagramobjects", "\r\n\tCREATE FUNCTION dbo.fn_diagramobjects() \r\n\tRETURNS int\r\n\tWITH EXECUTE AS N'dbo'\r\n\tAS\r\n\tBEGIN\r\n\t\tdeclare @id_upgraddiagrams\t\tint\r\n\t\tdeclare @id_sysdiagrams\t\t\tint\r\n\t\tdeclare @id_helpdiagrams\t\tint\r\n\t\tdeclare @id_helpdiagramdefinition\tint\r\n\t\tdeclare @id_creatediagram\tint\r\n\t\tdeclare @id_renamediagram\tint\r\n\t\tdeclare @id_alterdiagram \tint \r\n\t\tdeclare @id_dropdiagram\t\tint\r\n\t\tdeclare @InstalledObjects\tint\r\n\r\n\t\tselect @InstalledObjects = 0\r\n\r\n\t\tselect \t@id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),\r\n\t\t\t@id_sysdiagrams = object_id(N'dbo.sysdiagrams'),\r\n\t\t\t@id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),\r\n\t\t\t@id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),\r\n\t\t\t@id_creatediagram = object_id(N'dbo.sp_creatediagram'),\r\n\t\t\t@id_renamediagram = object_id(N'dbo.sp_renamediagram'),\r\n\t\t\t@id_alterdiagram = object_id(N'dbo.sp_alterdiagram'), \r\n\t\t\t@id_dropdiagram = object_id(N'dbo.sp_dropdiagram')\r\n\r\n\t\tif @id_upgraddiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 1\r\n\t\tif @id_sysdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 2\r\n\t\tif @id_helpdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 4\r\n\t\tif @id_helpdiagramdefinition is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 8\r\n\t\tif @id_creatediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 16\r\n\t\tif @id_renamediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 32\r\n\t\tif @id_alterdiagram  is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 64\r\n\t\tif @id_dropdiagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 128\r\n\t\t\r\n\t\treturn @InstalledObjects \r\n\tEND\r\n\t", null, null, null),
                Recordset = new Recordset(),
                Source = sourceConn
            };
            var MsSqlBroker = new MsSqlBroker();
            MsSqlBroker.TestService(serviceConn);
        }

        [TestMethod]
        public void TestServiceWithValidDBFormatUsernameExpectedReturnsValidResult()
        {
            var username = "testUser";
            var password = "test123";
            var sourceConn = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CitiesDB",
                ResourceType = ResourceType.DbSource,
                ResourcePath = "Test",
                Server = "RSAKLFSVRGENDEV",
                DatabaseName = "Cities",
                AuthenticationType = AuthenticationType.User,
                UserID = username,
                Password = password
            };
            var serviceConn = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DatabaseService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test",
                AuthorRoles = "",
                Dependencies = new List<ResourceForTree>(),
                FilePath = null,
                IsUpgraded = true,
                Method = new ServiceMethod("dbo.fn_diagramobjects", "\r\n\tCREATE FUNCTION dbo.fn_diagramobjects() \r\n\tRETURNS int\r\n\tWITH EXECUTE AS N'dbo'\r\n\tAS\r\n\tBEGIN\r\n\t\tdeclare @id_upgraddiagrams\t\tint\r\n\t\tdeclare @id_sysdiagrams\t\t\tint\r\n\t\tdeclare @id_helpdiagrams\t\tint\r\n\t\tdeclare @id_helpdiagramdefinition\tint\r\n\t\tdeclare @id_creatediagram\tint\r\n\t\tdeclare @id_renamediagram\tint\r\n\t\tdeclare @id_alterdiagram \tint \r\n\t\tdeclare @id_dropdiagram\t\tint\r\n\t\tdeclare @InstalledObjects\tint\r\n\r\n\t\tselect @InstalledObjects = 0\r\n\r\n\t\tselect \t@id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),\r\n\t\t\t@id_sysdiagrams = object_id(N'dbo.sysdiagrams'),\r\n\t\t\t@id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),\r\n\t\t\t@id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),\r\n\t\t\t@id_creatediagram = object_id(N'dbo.sp_creatediagram'),\r\n\t\t\t@id_renamediagram = object_id(N'dbo.sp_renamediagram'),\r\n\t\t\t@id_alterdiagram = object_id(N'dbo.sp_alterdiagram'), \r\n\t\t\t@id_dropdiagram = object_id(N'dbo.sp_dropdiagram')\r\n\r\n\t\tif @id_upgraddiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 1\r\n\t\tif @id_sysdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 2\r\n\t\tif @id_helpdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 4\r\n\t\tif @id_helpdiagramdefinition is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 8\r\n\t\tif @id_creatediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 16\r\n\t\tif @id_renamediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 32\r\n\t\tif @id_alterdiagram  is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 64\r\n\t\tif @id_dropdiagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 128\r\n\t\t\r\n\t\treturn @InstalledObjects \r\n\tEND\r\n\t", null, null, null),
                Recordset = new Recordset(),
                Source = sourceConn
            };
            var MsSqlBroker = new MsSqlBroker();
            var result = MsSqlBroker.TestService(serviceConn);
            Assert.AreEqual(OutputFormats.ShapedXML, result.Format);
        }

        #endregion

        #endregion

        #region Private Test Methods

        private Connection SetupDefaultConnection()
        {
            var testConnection = new Connection
            {
                Address = "http://localhost:77/dsf",
                AuthenticationType = AuthenticationType.Windows,
                Password = "secret",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourcePath = @"host\Server",
                ResourceType = ResourceType.Server,
                UserName = @"Domain\User",
                WebServerPort = 1234
            };

            return testConnection;
        }

        #endregion Private Test Methods
    }
}
