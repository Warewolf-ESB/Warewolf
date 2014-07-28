using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.Integration.Tests.Dev2.Application.Server.Tests.Workspace.XML;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.Services.Sql;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.Ouput;

// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class DbServiceTests
    {
        #region Save

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_Save")]
        public void DbServices_Save_NullArgs_ReturnsErrorValidationResult()
        {
            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.Save(null, Guid.Empty, Guid.Empty);

            //Assert Returns Error Validation Result
            var validationResult = JsonConvert.DeserializeObject<ValidationResult>(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_Save")]
        public void DbServices_Save_InvalidArgs_ReturnsErrorValidationResult()
        {
            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.Save("xxxxx", Guid.Empty, Guid.Empty);

            //Assert Returns Error Validation Result
            var validationResult = JsonConvert.DeserializeObject<ValidationResult>(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_Save")]
        public void DbServices_Save_ValidArgsAndEmptyResourceID_AssignsNewResourceID()
        {
            var svc = CreateDev2TestingDbService();
            svc.ResourceID = Guid.Empty;
            var args = svc.ToString();

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                var services = new TestDbServices();

                //------------Execute Test---------------------------
                var result = services.Save(args, workspaceID, Guid.Empty);

                //Assert Assigns New Resource ID
                var service = JsonConvert.DeserializeObject<Service>(result);
                Assert.AreNotEqual(Guid.Empty, service.ResourceID);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_Save")]
        public void DbServices_Save_ValidArgsAndResourceID_DoesNotAssignNewResourceID()
        {
            var svc = CreateDev2TestingDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                var services = new TestDbServices();

                //------------Execute Test---------------------------
                var result = services.Save(args, workspaceID, Guid.Empty);

                //Assert Does Not Assign New Resource ID
                var service = JsonConvert.DeserializeObject<Service>(result);
                Assert.AreEqual(svc.ResourceID, service.ResourceID);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_Save")]
        public void DbServices_Save_ValidArgs_SavesXmlToDisk()
        {
            var svc = CreateDev2TestingDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = workspacePath;
            var fileName = String.Format("{0}\\{1}.xml", path, svc.ResourcePath);
            try
            {
                var services = new TestDbServices();

                //------------Execute Test---------------------------
                services.Save(args, workspaceID, Guid.Empty);

                //Assert Saves Xml To Disk
                var exists = File.Exists(fileName);
                Assert.IsTrue(exists);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        #endregion

        #region Get

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_DbMethods")]
        public void DBServices_Get_ValidArgs_ReturnsService()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                //------------Setup for test--------------------------
                var svc = CreateDev2TestingDbService();
                var getArgs = string.Format("{{\"resourceID\":\"{0}\",\"resourceType\":\"{1}\"}}", svc.ResourceID, ResourceType.DbService);

                var resourceCatalog = new Mock<IResourceCatalog>();
                resourceCatalog.Setup(c => c.GetResourceContents(workspaceID, svc.ResourceID, It.IsAny<Version>()))
                    .Returns((Guid wsID, Guid resourceID, Version version) => svc.ToXml().ToStringBuilder())
                    .Verifiable();

                var services = new TestDbServices(resourceCatalog.Object);

                //------------Execute Test---------------------------
                var getResult = services.Get(getArgs, workspaceID, Guid.Empty);

                //------------Assert Results-------------------------
                resourceCatalog.Verify(c => c.GetResourceContents(workspaceID, svc.ResourceID, It.IsAny<Version>()));

                Assert.AreEqual(svc.ResourceID, getResult.ResourceID);
                Assert.AreEqual(svc.ResourceName, getResult.ResourceName);
                Assert.AreEqual(svc.ResourcePath, getResult.ResourcePath);
                Assert.AreEqual(svc.ResourceType, getResult.ResourceType);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        #endregion

        #region FetchMethods

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DbServices_DbMethods")]
        public void DbServices_DbMethods_InvokesDatabaseBrokerGetServiceMethodsMethod_Done()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
            var args = JsonConvert.SerializeObject(dbSource);

            var outputDescription = new Mock<IOutputDescription>();
            outputDescription.Setup(d => d.DataSourceShapes).Returns(new List<IDataSourceShape> { new DataSourceShape() });

            var dbBroker = new Mock<SqlDatabaseBroker>();
            dbBroker.Setup(b => b.GetServiceMethods(It.IsAny<DbSource>())).Verifiable();

            var dbServices = new TestDbServices(dbBroker.Object);

            //------------Execute Test---------------------------
            var result = dbServices.DbMethods(args, Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            dbBroker.Verify(b => b.GetServiceMethods(It.IsAny<DbSource>()));

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_DbMethods")]
        public void DBServices_DbMethods_ValidArgs_ReturnsList()
        {
            var source = SqlServerTests.CreateDev2TestingDbSource();
            var args = source.ToString();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.DbMethods(args, workspaceID, Guid.Empty);

            // Assert Returns Valid List
            Assert.IsTrue(result.Count >= 25, "DbMethods does not return a list");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_DBMethods")]
        public void DBServices_DBMethods_InvalidArgs_ErrorList()
        {
            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.DbMethods("xxxx", Guid.Empty, Guid.Empty);

            // Assert Error List
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Name.StartsWith("Error"), "Invalid args did not return error result");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBServices_DBMethods")]
        public void DBServices_DBMethods_Null_ReturnEmptyList()
        {
            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.DbMethods(null, Guid.Empty, Guid.Empty);

            // Assert Empty List
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region DbTest

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_InvokesDatabaseBrokerTestServiceMethod_Done()
        {
            //------------Setup for test--------------------------
            var dbService = new DbService
            {
                Recordset = new Recordset(),
                Method = new ServiceMethod(),
                Source = SqlServerTests.CreateDev2TestingDbSource()
            };
            var args = JsonConvert.SerializeObject(dbService);

            var outputDescription = new Mock<IOutputDescription>();
            outputDescription.Setup(d => d.DataSourceShapes).Returns(new List<IDataSourceShape> { new DataSourceShape() });

            var dbBroker = new Mock<SqlDatabaseBroker>();
            dbBroker.Setup(b => b.TestService(It.IsAny<DbService>())).Returns(outputDescription.Object).Verifiable();

            var dbServices = new TestDbServices(dbBroker.Object);

            //------------Execute Test---------------------------
            var result = dbServices.DbTest(args, Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            dbBroker.Verify(b => b.TestService(It.IsAny<DbService>()));

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_NullArgs_RecordsetWithError()
        {
            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.DbTest(null, Guid.Empty, Guid.Empty);

            // Assert Recordset With Error
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_InvalidArgs_RecordsetWithError()
        {
            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.DbTest("xxx", Guid.Empty, Guid.Empty);

            // Assert Recordset With Error
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_ValidArgsAndNoRecordsetName_UpdatesRecordsetNameToServiceMethodName()
        {
            var service = CreateDev2TestingDbService();
            service.Recordset.Name = null;
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();
            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.DbTest(args, workspaceID, Guid.Empty);

            //Assert Updates Recordset Name To Service Method Name
            Assert.AreEqual(service.Method.Name.Replace('.', '_'), result.Name, "Recordset name not defaulting to service method name when null");
            Assert.IsFalse(result.HasErrors, "Valid DB service returned error on test");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_ValidArgsAndValidRecordsetName_UpdatesRecordsetNameToServiceMethodName()
        {
            var service = CreateDev2TestingDbService();
            service.Recordset.Name = "MyCities";
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();
            var services = new TestDbServices();

            //------------Execute Test---------------------------
            var result = services.DbTest(args, workspaceID, Guid.Empty);

            //Assert Updates Recordset Name To Service Method Name
            Assert.AreEqual(service.Recordset.Name, result.Name, "Recordset name is defaulting to service method name when not null");
            Assert.IsFalse(result.HasErrors, "Valid DB service returned error on test");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_ValidArgsAndRecordsetFields_DoesNotAddRecordsetFields()
        {
            var svc = CreateDev2TestingDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            var services = new Mock<TestDbServices>();
            services.Setup(serv => serv.FetchRecordset(It.IsAny<DbService>(), It.IsAny<bool>())).Returns(svc.Recordset).Verifiable();

            //------------Execute Test---------------------------
            var result = services.Object.DbTest(args, workspaceID, Guid.Empty);

            //Assert Does Not Add Recordset Fields
            services.Verify(serv => serv.FetchRecordset(It.IsAny<DbService>(), It.IsAny<bool>()), Times.Once());
            Assert.AreEqual(svc.Recordset.Fields.Count, result.Fields.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_ValidArgsAndRecordsetFields_AddsRecordsetFields()
        {
            var svc = CreateDev2TestingDbService();
            svc.Recordset.Fields.Clear();

            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            var services = new Mock<TestDbServices>();
            services.Setup(serv => serv.FetchRecordset(It.IsAny<DbService>(), It.IsAny<bool>())).Returns(svc.Recordset).Verifiable();

            //------------Execute Test---------------------------
            services.Object.DbTest(args, workspaceID, Guid.Empty);

            //Assert Adds Recordset Fields
            services.Verify(serv => serv.FetchRecordset(It.IsAny<DbService>(), It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_ValidArgs_dFetchesRecordset()
        {
            var svc = CreateDev2TestingDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            var services = new Mock<TestDbServices>();
            services.Setup(serv => serv.FetchRecordset(It.IsAny<DbService>(), It.IsAny<bool>())).Returns(svc.Recordset).Verifiable();

            //------------Execute Test---------------------------
            var result = services.Object.DbTest(args, workspaceID, Guid.Empty);

            //Assert Fetches Recordset
            services.Verify(serv => serv.FetchRecordset(It.IsAny<DbService>(), It.IsAny<bool>()), Times.Once());
            Assert.AreEqual(result.Name, svc.Recordset.Name);
            Assert.AreEqual(result.Fields.Count, svc.Recordset.Fields.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DbServices_TestService")]
        public void DbTestWithValidArgsExpectedClearsRecordsFirst()
        {
            var service = CreateDev2TestingDbService();
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();
            var services = new Mock<TestDbServices>();
            services.Setup(serv => serv.FetchRecordset(It.IsAny<DbService>(), It.IsAny<bool>())).Returns(service.Recordset).Verifiable();

            //------------Execute Test---------------------------
            var result = services.Object.DbTest(args, workspaceID, Guid.Empty);

            //Assert Clears Records First
            services.Verify(serv => serv.FetchRecordset(It.IsAny<DbService>(), It.IsAny<bool>()), Times.Once());
            Assert.AreEqual(0, result.Records.Count);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceInvoker_Invoke")]
        public void ServiceInvoker_Invoke_WhenDbTest_ExpectValidDbRecordsetDataWithCommasReplacedForUIParsing()
        {
            //------------Setup for test--------------------------
            var services = new TestDbServices();

            const string args = @"{""resourceID"":""00000000-0000-0000-0000-000000000000"",""resourceType"":""DbService"",""resourceName"":null,""resourcePath"":null,""source"":{""ServerType"":""SqlDatabase"",""Server"":""RSAKLFSVRGENDEV"",""DatabaseName"":""Dev2TestingDB"",""Port"":1433,""AuthenticationType"":""User"",""UserID"":""testUser"",""Password"":""test123"",""ConnectionString"":""Data Source=RSAKLFSVRGENDEV,1433;Initial Catalog=Dev2TestingDB;User ID=testUser;Password=test123;"",""ResourceID"":""eb2de0a3-4814-40b8-b825-f4601bfdb155"",""Version"":""1.0"",""ResourceType"":""DbSource"",""ResourceName"":""TU Greenpoint DB"",""ResourcePath"":""SQL SRC"",""IsValid"":false,""Errors"":null,""ReloadActions"":false},""method"":{""Name"":""dbo.pr_MapLocationsGetAll"",""ExecAction"":""dbo.pr_MapLocationsGetAll"",""SourceCode"":""-- =============================================\r<br />-- Author:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<Author,,Name>\r<br />-- Create date: <Create Date,,>\r<br />-- Description:&nbsp;&nbsp;&nbsp;&nbsp;<Description,,>\r<br />-- =============================================\r<br />CREATE PROCEDURE dbo.proc_GetAllMapLocations \r<br />&nbsp;&nbsp;&nbsp;&nbsp;\r<br />AS\r<br />BEGIN\r<br />&nbsp;&nbsp;&nbsp;&nbsp;SET NOCOUNT ON;\r<br />\r<br />    SELECT MapLocationID, StreetAddress,Latitude,Longitude FROM dbo.MapLocation ORDER BY MapLocationID ASC\r<br />END\r<br />"",""Parameters"":[]},""recordset"":{""Name"":""dbo_proc_GetAllMapLocations"",""Fields"":[],""Records"":[],""HasErrors"":false,""ErrorMessage"":""""}}";

            //------------Execute Test---------------------------

            var result = services.DbTest(args, Guid.NewGuid(), Guid.Empty);

            // __COMMA__ is expected as this means sample has been delimited properly by the server.
            // It also means there is an implicit contract between the UI and server to handle __COMMA__ back into ,

            //
            // 2013.12.11 - COMMUNITY BUG - 341463 - Fixed expected to include empty cells
            //
            const string expected = "{\"Name\":\"dbo_proc_GetAllMapLocations\",\"HasErrors\":false,\"ErrorMessage\":\"\",\"Fields\":[{\"Name\":\"MapLocationID\",\"Alias\":\"MapLocationID\",\"RecordsetAlias\":null,\"Path\":{\"$type\":\"Dev2.Converters.Graph.DataTable.DataTablePath, Dev2.Core\",\"ActualPath\":\"MapLocationID\",\"DisplayPath\":\"MapLocationID\",\"SampleData\":\"1__COMMA__2__COMMA__3__COMMA__4__COMMA__5__COMMA__7__COMMA__8__COMMA__9__COMMA__10__COMMA__11__COMMA__\",\"OutputExpression\":\"\"}},{\"Name\":\"StreetAddress\",\"Alias\":\"StreetAddress\",\"RecordsetAlias\":null,\"Path\":{\"$type\":\"Dev2.Converters.Graph.DataTable.DataTablePath, Dev2.Core\",\"ActualPath\":\"StreetAddress\",\"DisplayPath\":\"StreetAddress\",\"SampleData\":\"19 Pineside Road, New Germany__COMMA__1244 Old North Coast Rd, Redhill__COMMA__Westmead Road, Westmead__COMMA__Turquoise Road, Queensmead__COMMA__Old Main Road, Isipingo__COMMA__2 Brook Street North, Warwick Junction__COMMA__Bellair Road, Corner Bellair & Edwin Swales/Sarnia Arterial__COMMA__Riverside Road, Durban North__COMMA__Malacca Road, Durban North__COMMA__Glanville Road, Woodlands__COMMA__\",\"OutputExpression\":\"\"}},{\"Name\":\"Latitude\",\"Alias\":\"Latitude\",\"RecordsetAlias\":null,\"Path\":{\"$type\":\"Dev2.Converters.Graph.DataTable.DataTablePath, Dev2.Core\",\"ActualPath\":\"Latitude\",\"DisplayPath\":\"Latitude\",\"SampleData\":\"99.99__COMMA__99.99__COMMA__99.99__COMMA__99.99__COMMA__99.99__COMMA__99.99__COMMA__99.99__COMMA__99.99__COMMA__99.99__COMMA__99.99__COMMA__\",\"OutputExpression\":\"\"}},{\"Name\":\"Longitude\",\"Alias\":\"Longitude\",\"RecordsetAlias\":null,\"Path\":{\"$type\":\"Dev2.Converters.Graph.DataTable.DataTablePath, Dev2.Core\",\"ActualPath\":\"Longitude\",\"DisplayPath\":\"Longitude\",\"SampleData\":\"66.6__COMMA__66.6__COMMA__66.6__COMMA__66.6__COMMA__66.6__COMMA__66.6__COMMA__66.6__COMMA__66.6__COMMA__66.6__COMMA__66.6__COMMA__\",\"OutputExpression\":\"\"}}],\"Records\":[{\"Label\":\"dbo_proc_GetAllMapLocations(1)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(1).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"1\"},{\"Name\":\"dbo_proc_GetAllMapLocations(1).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"19 Pineside Road, New Germany\"},{\"Name\":\"dbo_proc_GetAllMapLocations(1).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(1).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(2)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(2).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"2\"},{\"Name\":\"dbo_proc_GetAllMapLocations(2).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"1244 Old North Coast Rd, Redhill\"},{\"Name\":\"dbo_proc_GetAllMapLocations(2).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(2).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(3)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(3).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"3\"},{\"Name\":\"dbo_proc_GetAllMapLocations(3).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"Westmead Road, Westmead\"},{\"Name\":\"dbo_proc_GetAllMapLocations(3).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(3).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(4)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(4).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"4\"},{\"Name\":\"dbo_proc_GetAllMapLocations(4).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"Turquoise Road, Queensmead\"},{\"Name\":\"dbo_proc_GetAllMapLocations(4).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(4).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(5)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(5).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"5\"},{\"Name\":\"dbo_proc_GetAllMapLocations(5).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"Old Main Road, Isipingo\"},{\"Name\":\"dbo_proc_GetAllMapLocations(5).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(5).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(6)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(6).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"7\"},{\"Name\":\"dbo_proc_GetAllMapLocations(6).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"2 Brook Street North, Warwick Junction\"},{\"Name\":\"dbo_proc_GetAllMapLocations(6).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(6).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(7)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(7).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"8\"},{\"Name\":\"dbo_proc_GetAllMapLocations(7).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"Bellair Road, Corner Bellair & Edwin Swales/Sarnia Arterial\"},{\"Name\":\"dbo_proc_GetAllMapLocations(7).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(7).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(8)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(8).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"9\"},{\"Name\":\"dbo_proc_GetAllMapLocations(8).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"Riverside Road, Durban North\"},{\"Name\":\"dbo_proc_GetAllMapLocations(8).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(8).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(9)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(9).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"10\"},{\"Name\":\"dbo_proc_GetAllMapLocations(9).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"Malacca Road, Durban North\"},{\"Name\":\"dbo_proc_GetAllMapLocations(9).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(9).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(10)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(10).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"11\"},{\"Name\":\"dbo_proc_GetAllMapLocations(10).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"Glanville Road, Woodlands\"},{\"Name\":\"dbo_proc_GetAllMapLocations(10).Latitude\",\"Label\":\"Latitude\",\"Value\":\"99.99\"},{\"Name\":\"dbo_proc_GetAllMapLocations(10).Longitude\",\"Label\":\"Longitude\",\"Value\":\"66.6\"}]},{\"Label\":\"dbo_proc_GetAllMapLocations(11)\",\"Name\":\"dbo_proc_GetAllMapLocations\",\"Count\":4,\"Cells\":[{\"Name\":\"dbo_proc_GetAllMapLocations(11).MapLocationID\",\"Label\":\"MapLocationID\",\"Value\":\"\"},{\"Name\":\"dbo_proc_GetAllMapLocations(11).StreetAddress\",\"Label\":\"StreetAddress\",\"Value\":\"\"},{\"Name\":\"dbo_proc_GetAllMapLocations(11).Latitude\",\"Label\":\"Latitude\",\"Value\":\"\"},{\"Name\":\"dbo_proc_GetAllMapLocations(11).Longitude\",\"Label\":\"Longitude\",\"Value\":\"\"}]}]}";

            //------------Assert Results-------------------------
            var res = result.ToString();
            Assert.AreEqual(expected, res);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DbServices_TestService")]
        public void DbServices_TestService_NestedTransactions_DoesNotThrowException()
        {
            //------------Setup for test--------------------------
            var dbService = new DbService
            {
                Recordset = new Recordset(),
                Method = new ServiceMethod
                {
                    Name = "dbo.Pr_GeneralTestNestedTransaction",
                    ExecuteAction = "dbo.Pr_GeneralTestNestedTransaction"

                    //Name = "dbo.Pr_TestTransactionsNested"
                },
                Source = SqlServerTests.CreateDev2TestingDbSource()
            };
            var args = JsonConvert.SerializeObject(dbService);

            var dbServices = new TestDbServices(new Mock<IResourceCatalog>().Object, new Mock<IAuthorizationService>().Object);

            //------------Execute Test---------------------------
            var result = dbServices.DbTest(args, Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
        }
        #endregion

        #region Mappings

        readonly string _webserverURI = ServerSettings.WebserverURI;

        [TestMethod]
        public void CanExecuteDbServiceAndReturnItsOutput()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "TestCategory/Bug9139");
            const string expected = @"<DataList><result>PASS</result></DataList>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData, expected, "Expected [ " + expected + " ] But Got [ " + responseData + " ]");
        }

        [TestMethod]
        public void CanReturnDataInCorrectCase()
        {
            string postData = String.Format("{0}{1}", _webserverURI, "TestCategory/Bug9490");
            const string expected = @"<result index=""1""><val>abc_def_hij</val></result><result index=""2""><val>ABC_DEF_HIJ</val></result>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_MappedOutputsFetchedInInnerWorkflow_WhenFetchedWithDiffernedColumnsThanFetched_DataReturned()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", _webserverURI, "QA/Bug 10475 Outer WF");
            const string expected = @"<Row index=""1""><ID>1</ID>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_WithInputsAndNoOutputs_WhenInsertingFromDataList_SameDataReturned()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", _webserverURI, "INTEGRATION TEST SERVICES/DB Service With No Output");
            const string expected = @"<Result>PASS</Result>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, expected);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_CanMapToMultipleRecordsets_WhenStraightFromDBService_ExpectPass()
        {

            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", _webserverURI, "Service Output To Multiple Recordsets");
            const string expected = @"<result>PASS</result>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);


            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, expected);

        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DatabaseService_Execute")]
        public void DatabaseService_Execute_CustomOutputMappings_DataReturned()
        {
            //------------Setup for test--------------------------
            var postData = String.Format("{0}{1}", _webserverURI, "TWR/10638 - Service IO - TEST");

            var expectedXml = XmlResource.Fetch("BUG_10638_Result.xml");
            var expected = expectedXml.ToString(SaveOptions.None);

            //------------Execute Test---------------------------
            var responseData = TestHelper.PostDataToWebserver(postData);

            var actualXml = XElement.Parse(responseData);
            var actual = actualXml.ToString(SaveOptions.None);

            //------------Assert Results-------------------------
            StringAssert.Contains(expected, actual);
        }

        #endregion

        #region CreateDev2TestingDbService

        public static DbService CreateDev2TestingDbService()
        {
            var service = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dev2TestingService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test\\Dev2TestingService",
                Method = new ServiceMethod
                {
                    Name = "dbo.Pr_CitiesGetCountries",
                    Parameters = new List<MethodParameter>(new[]
                    {
                        new MethodParameter { Name = "Prefix", EmptyToNull = false, IsRequired = true, Value = "b" }
                    }),
                    ExecuteAction = "dbo.Pr_CitiesGetCountries"
                },
                Recordset = new Recordset
                {
                    Name = "Countries",
                },
                Source = SqlServerTests.CreateDev2TestingDbSource()
            };
            service.Recordset.Fields.AddRange(new[]
            {
                new RecordsetField { Name = "CountryID", Alias = "CountryID" },
                new RecordsetField { Name = "Description", Alias = "Name" }
            });

            return service;
        }

        #endregion
    }
}
