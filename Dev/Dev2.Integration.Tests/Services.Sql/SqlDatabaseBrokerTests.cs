using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Integration.Tests.Services.Sql
{
    [TestClass]
    public class SqlDatabaseBrokerTests
    {
        //Ashley.Lewis - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("SqlDatabaseBroker_GetServiceMethods")]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_GetServiceMethods_WindowsUserWithDbAccess_GetsMethods()
// ReSharper restore InconsistentNaming
        {
            Impersonator.RunAs("IntegrationTester", "DEV2", "I73573r0", () =>
            {
                var dbSource = SqlServerTests.CreateDev2TestingDbSource(AuthenticationType.Windows);
                var broker = new SqlDatabaseBroker();
                var result = broker.GetServiceMethods(dbSource);
                Assert.AreEqual(true, result.Count > 0);
            });
        }

        //Ashley.Lewis - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("SqlDatabaseBroker_GetServiceMethods")]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_GetServiceMethods_WindowsUserWithoutDbAccess_ThrowsLoginFailedException()
// ReSharper restore InconsistentNaming
        {

            Impersonator.RunAs("NoDBAccessTest", "DEV2", "One23456", () =>
            {
                var dbSource = SqlServerTests.CreateDev2TestingDbSource(AuthenticationType.Windows);
                var broker = new SqlDatabaseBroker();
                try
                {
                    broker.GetServiceMethods(dbSource);
                    Assert.Fail();
                }
                catch(Exception ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsInstanceOfType(ex, typeof(SqlException));
                    Assert.AreEqual("Login failed for user 'DEV2\\NoDBAccessTest'.", ex.Message);
                }
            });
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("SqlDatabaseBroker_GetServiceMethods")]
        [ExpectedException(typeof(SqlException))]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_GetServiceMethods_SqlUserWithInvalidUsername_ThrowsLoginFailedException()
// ReSharper restore InconsistentNaming
        {
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
            dbSource.UserID = "Billy.Jane";
            dbSource.Password = "invalidPassword";

            var broker = new SqlDatabaseBroker();
            broker.GetServiceMethods(dbSource);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("SqlDatabaseBroker_GetServiceMethods")]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_GetServiceMethods_SqlUserWithValidUsername_GetsMethods()
// ReSharper restore InconsistentNaming
        {
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
            var broker = new SqlDatabaseBroker();
            var result = broker.GetServiceMethods(dbSource);
            Assert.AreEqual(true, result.Count > 0);
        }

        //Massimo.Guerrera - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        [Owner("Massimo.Guerrera")]
        [TestCategory("SqlDatabaseBroker_TestService")]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_TestService_WindowsUserWithDbAccess_ReturnsValidResult()
// ReSharper restore InconsistentNaming
        {
            Impersonator.RunAs("IntegrationTester", "DEV2", "I73573r0", () =>
            {
                var dbSource = SqlServerTests.CreateDev2TestingDbSource(AuthenticationType.Windows);
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
                    Method = new ServiceMethod("dbo.fn_diagramobjects", "\r\n\tCREATE FUNCTION dbo.fn_diagramobjects() \r\n\tRETURNS int\r\n\tWITH EXECUTE AS N'dbo'\r\n\tAS\r\n\tBEGIN\r\n\t\tdeclare @id_upgraddiagrams\t\tint\r\n\t\tdeclare @id_sysdiagrams\t\t\tint\r\n\t\tdeclare @id_helpdiagrams\t\tint\r\n\t\tdeclare @id_helpdiagramdefinition\tint\r\n\t\tdeclare @id_creatediagram\tint\r\n\t\tdeclare @id_renamediagram\tint\r\n\t\tdeclare @id_alterdiagram \tint \r\n\t\tdeclare @id_dropdiagram\t\tint\r\n\t\tdeclare @InstalledObjects\tint\r\n\r\n\t\tselect @InstalledObjects = 0\r\n\r\n\t\tselect \t@id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),\r\n\t\t\t@id_sysdiagrams = object_id(N'dbo.sysdiagrams'),\r\n\t\t\t@id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),\r\n\t\t\t@id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),\r\n\t\t\t@id_creatediagram = object_id(N'dbo.sp_creatediagram'),\r\n\t\t\t@id_renamediagram = object_id(N'dbo.sp_renamediagram'),\r\n\t\t\t@id_alterdiagram = object_id(N'dbo.sp_alterdiagram'), \r\n\t\t\t@id_dropdiagram = object_id(N'dbo.sp_dropdiagram')\r\n\r\n\t\tif @id_upgraddiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 1\r\n\t\tif @id_sysdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 2\r\n\t\tif @id_helpdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 4\r\n\t\tif @id_helpdiagramdefinition is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 8\r\n\t\tif @id_creatediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 16\r\n\t\tif @id_renamediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 32\r\n\t\tif @id_alterdiagram  is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 64\r\n\t\tif @id_dropdiagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 128\r\n\t\t\r\n\t\treturn @InstalledObjects \r\n\tEND\r\n\t", null, null, null, "dbo.fn_diagramobjects"),
                    Recordset = new Recordset(),
                    Source = dbSource
                };
                var broker = new SqlDatabaseBroker();
                var result = broker.TestService(serviceConn);
                Assert.AreEqual(OutputFormats.ShapedXML, result.Format);
            });
        }

        //Massimo.Guerrera - 10.05.2013 - Added for Bug 9394
        [TestMethod]
        [Owner("Massimo.Guerrera")]
        [TestCategory("SqlDatabaseBroker_TestService")]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_TestService_WindowsUserWithoutDbAccess_ReturnsInvalidResult()
// ReSharper restore InconsistentNaming
        {
            Exception exception = null;

            Impersonator.RunAs("NoDBAccessTest", "DEV2", "One23456", () =>
            {
                var dbSource = SqlServerTests.CreateDev2TestingDbSource(AuthenticationType.Windows);

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
                    Method = new ServiceMethod("dbo.fn_diagramobjects", "\r\n\tCREATE FUNCTION dbo.fn_diagramobjects() \r\n\tRETURNS int\r\n\tWITH EXECUTE AS N'dbo'\r\n\tAS\r\n\tBEGIN\r\n\t\tdeclare @id_upgraddiagrams\t\tint\r\n\t\tdeclare @id_sysdiagrams\t\t\tint\r\n\t\tdeclare @id_helpdiagrams\t\tint\r\n\t\tdeclare @id_helpdiagramdefinition\tint\r\n\t\tdeclare @id_creatediagram\tint\r\n\t\tdeclare @id_renamediagram\tint\r\n\t\tdeclare @id_alterdiagram \tint \r\n\t\tdeclare @id_dropdiagram\t\tint\r\n\t\tdeclare @InstalledObjects\tint\r\n\r\n\t\tselect @InstalledObjects = 0\r\n\r\n\t\tselect \t@id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),\r\n\t\t\t@id_sysdiagrams = object_id(N'dbo.sysdiagrams'),\r\n\t\t\t@id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),\r\n\t\t\t@id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),\r\n\t\t\t@id_creatediagram = object_id(N'dbo.sp_creatediagram'),\r\n\t\t\t@id_renamediagram = object_id(N'dbo.sp_renamediagram'),\r\n\t\t\t@id_alterdiagram = object_id(N'dbo.sp_alterdiagram'), \r\n\t\t\t@id_dropdiagram = object_id(N'dbo.sp_dropdiagram')\r\n\r\n\t\tif @id_upgraddiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 1\r\n\t\tif @id_sysdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 2\r\n\t\tif @id_helpdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 4\r\n\t\tif @id_helpdiagramdefinition is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 8\r\n\t\tif @id_creatediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 16\r\n\t\tif @id_renamediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 32\r\n\t\tif @id_alterdiagram  is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 64\r\n\t\tif @id_dropdiagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 128\r\n\t\t\r\n\t\treturn @InstalledObjects \r\n\tEND\r\n\t", null, null, null,null),
                    Recordset = new Recordset(),
                    Source = dbSource
                };
                var broker = new SqlDatabaseBroker();
                try
                {
                    broker.TestService(serviceConn);
                }
                catch(Exception ex)
                {
                    // Need to do this because exceptions get swallowed by impersonator
                    exception = ex;
                }

                Assert.IsNotNull(exception);
                Assert.IsInstanceOfType(exception, typeof(SqlException));
                Assert.AreEqual("Login failed for user 'DEV2\\NoDBAccessTest'.", exception.Message);

            });

            
        }

        [TestMethod]
        [Owner("Massimo.Guerrera")]
        [TestCategory("SqlDatabaseBroker_TestService")]
        [ExpectedException(typeof(SqlException))]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_TestService_SqlUserWithInvalidUsername_ReturnsInvalidResult()
// ReSharper restore InconsistentNaming
        {
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
            dbSource.UserID = "Billy.Jane";
            dbSource.Password = "invalidPassword";

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
                Method = new ServiceMethod("dbo.fn_diagramobjects", "\r\n\tCREATE FUNCTION dbo.fn_diagramobjects() \r\n\tRETURNS int\r\n\tWITH EXECUTE AS N'dbo'\r\n\tAS\r\n\tBEGIN\r\n\t\tdeclare @id_upgraddiagrams\t\tint\r\n\t\tdeclare @id_sysdiagrams\t\t\tint\r\n\t\tdeclare @id_helpdiagrams\t\tint\r\n\t\tdeclare @id_helpdiagramdefinition\tint\r\n\t\tdeclare @id_creatediagram\tint\r\n\t\tdeclare @id_renamediagram\tint\r\n\t\tdeclare @id_alterdiagram \tint \r\n\t\tdeclare @id_dropdiagram\t\tint\r\n\t\tdeclare @InstalledObjects\tint\r\n\r\n\t\tselect @InstalledObjects = 0\r\n\r\n\t\tselect \t@id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),\r\n\t\t\t@id_sysdiagrams = object_id(N'dbo.sysdiagrams'),\r\n\t\t\t@id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),\r\n\t\t\t@id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),\r\n\t\t\t@id_creatediagram = object_id(N'dbo.sp_creatediagram'),\r\n\t\t\t@id_renamediagram = object_id(N'dbo.sp_renamediagram'),\r\n\t\t\t@id_alterdiagram = object_id(N'dbo.sp_alterdiagram'), \r\n\t\t\t@id_dropdiagram = object_id(N'dbo.sp_dropdiagram')\r\n\r\n\t\tif @id_upgraddiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 1\r\n\t\tif @id_sysdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 2\r\n\t\tif @id_helpdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 4\r\n\t\tif @id_helpdiagramdefinition is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 8\r\n\t\tif @id_creatediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 16\r\n\t\tif @id_renamediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 32\r\n\t\tif @id_alterdiagram  is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 64\r\n\t\tif @id_dropdiagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 128\r\n\t\t\r\n\t\treturn @InstalledObjects \r\n\tEND\r\n\t", null, null, null,null),
                Recordset = new Recordset(),
                Source = dbSource
            };
            var broker = new SqlDatabaseBroker();
            broker.TestService(serviceConn);
        }

        [TestMethod]
        [Owner("Massimo.Guerrera")]
        [TestCategory("SqlDatabaseBroker_TestService")]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_TestService_SqlUserWithValidUsername_ReturnsValidResult()
// ReSharper restore InconsistentNaming
        {
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
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
                Method = new ServiceMethod("dbo.fn_diagramobjects", "\r\n\tCREATE FUNCTION dbo.fn_diagramobjects() \r\n\tRETURNS int\r\n\tWITH EXECUTE AS N'dbo'\r\n\tAS\r\n\tBEGIN\r\n\t\tdeclare @id_upgraddiagrams\t\tint\r\n\t\tdeclare @id_sysdiagrams\t\t\tint\r\n\t\tdeclare @id_helpdiagrams\t\tint\r\n\t\tdeclare @id_helpdiagramdefinition\tint\r\n\t\tdeclare @id_creatediagram\tint\r\n\t\tdeclare @id_renamediagram\tint\r\n\t\tdeclare @id_alterdiagram \tint \r\n\t\tdeclare @id_dropdiagram\t\tint\r\n\t\tdeclare @InstalledObjects\tint\r\n\r\n\t\tselect @InstalledObjects = 0\r\n\r\n\t\tselect \t@id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),\r\n\t\t\t@id_sysdiagrams = object_id(N'dbo.sysdiagrams'),\r\n\t\t\t@id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),\r\n\t\t\t@id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),\r\n\t\t\t@id_creatediagram = object_id(N'dbo.sp_creatediagram'),\r\n\t\t\t@id_renamediagram = object_id(N'dbo.sp_renamediagram'),\r\n\t\t\t@id_alterdiagram = object_id(N'dbo.sp_alterdiagram'), \r\n\t\t\t@id_dropdiagram = object_id(N'dbo.sp_dropdiagram')\r\n\r\n\t\tif @id_upgraddiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 1\r\n\t\tif @id_sysdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 2\r\n\t\tif @id_helpdiagrams is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 4\r\n\t\tif @id_helpdiagramdefinition is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 8\r\n\t\tif @id_creatediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 16\r\n\t\tif @id_renamediagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 32\r\n\t\tif @id_alterdiagram  is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 64\r\n\t\tif @id_dropdiagram is not null\r\n\t\t\tselect @InstalledObjects = @InstalledObjects + 128\r\n\t\t\r\n\t\treturn @InstalledObjects \r\n\tEND\r\n\t", null, null, null, "dbo.fn_diagramobjects"),
                Recordset = new Recordset(),
                Source = dbSource
            };
            var broker = new SqlDatabaseBroker();
            var result = broker.TestService(serviceConn);
            Assert.AreEqual(OutputFormats.ShapedXML, result.Format);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SqlDatabaseBroker_TestService")]
// ReSharper disable InconsistentNaming
        public void SqlDatabaseBroker_TestService_ValidDbServiceThatReturnsNull_RecordsetWithNullColumn()
// ReSharper restore InconsistentNaming
        {
            var service = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "NullService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test",
                Method = new ServiceMethod
                {
                    Name = "Pr_GeneralTestColumnData",
                    Parameters = new List<MethodParameter>(),
                    ExecuteAction = "Pr_GeneralTestColumnData"
                },
                Recordset = new Recordset
                {
                    Name = "Collections",
                },
                Source = SqlServerTests.CreateDev2TestingDbSource()
            };

            var broker = new SqlDatabaseBroker();
            IOutputDescription outputDescription = broker.TestService(service);
            Assert.AreEqual(1, outputDescription.DataSourceShapes.Count);
            IDataSourceShape dataSourceShape = outputDescription.DataSourceShapes[0];
            Assert.IsNotNull(dataSourceShape);
            Assert.AreEqual(3, dataSourceShape.Paths.Count);
            StringAssert.Contains(dataSourceShape.Paths[2].DisplayPath, "TestTextNull"); //This is the field that contains a null value. Previously this column would not have been returned.
        }
    }
}
