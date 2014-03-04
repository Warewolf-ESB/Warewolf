using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class GetDatabaseColumnsForTableTests
    {

        #region Execute

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDatabaseColumnsForTable_UnitTest_ExecuteWithNullValues_ExpectedInvalidDataContractException()
        {
            var esb = new GetDatabaseColumnsForTable();
            var actual = esb.Execute(null, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseColumnsForTable_UnitTest_ExecuteWithNoDatabaseInValues_ExpectedHasErrors()
        {
            var esb = new GetDatabaseColumnsForTable();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", null } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("No database set.", result.Errors);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseColumnsForTable_UnitTest_ExecuteWithNullDatabase_ExpectedHasErrors()
        {

            var esb = new GetDatabaseColumnsForTable();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", null } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("No database set.", result.Errors);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseColumnsForTable_UnitTest_ExecuteWithBlankDatabase_ExpectedHasErrors()
        {
            var esb = new GetDatabaseColumnsForTable();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder() } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("No database set.", result.Errors);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseColumnsForTable_UnitTest_ExecuteWithNoTableNameInValues_ExpectedHasErrors()
        {

            var esb = new GetDatabaseColumnsForTable();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder("Test") }, { "Something", null } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("No table name set.", result.Errors);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseColumnsForTable_UnitTest_ExecuteWithNullTableNameExpectedHasErrors()
        {

            var esb = new GetDatabaseColumnsForTable();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder("Test") }, { "TableName", null } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("No table name set.", result.Errors);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseColumnsForTable_UnitTest_ExecuteWithBlankTableName_ExpectedHasErrors()
        {
            var esb = new GetDatabaseColumnsForTable();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder("Test") }, { "TableName", new StringBuilder() } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("No table name set.", result.Errors);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetDatabaseTables_Execute")]
        public void GetDatabaseColumnsForTable_Execute_ValidDatabaseSource()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource();
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource);
            string someJsonData = JsonConvert.SerializeObject(dbSource);
            var esb = new GetDatabaseColumnsForTable();
            var mockWorkspace = new Mock<IWorkspace>();
            mockWorkspace.Setup(workspace => workspace.ID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder(someJsonData) }, { "TableName", new StringBuilder("SqlBulkInsertSpecFlowTestTable") } }, mockWorkspace.Object);
            //------------Assert Results-------------------------
            var value = actual.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(value));
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString());
            Assert.IsTrue(result.Items.Count > 2);
        }

        DbSource CreateDev2TestingDbSource()
        {
            var dbSource = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dev2TestingDB",
                ResourcePath = "Test",
                DatabaseName = "Dev2TestingDB",
                Server = "RSAKLFSVRGENDEV",
                AuthenticationType = AuthenticationType.User,
                ServerType = enSourceType.SqlDatabase,
                ReloadActions = true,
                UserID = "testUser",
                Password = "test123"
            };
            return dbSource;
        }
        #endregion

        #region HandlesType

        [TestMethod]
        [Owner("Huggs")]
        public void GetDatabaseColumnsForTable_UnitTest_HandlesType_ExpectedReturnsGetDatabaseColumnsForTableService()
        {
            var esb = new GetDatabaseColumnsForTable();
            var result = esb.HandlesType();
            Assert.AreEqual("GetDatabaseColumnsForTableService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseColumnsForTable_UnitTest_CreateServiceEntry_ExpectedReturnsDynamicService()
        {
            var esb = new GetDatabaseColumnsForTable();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Database ColumnIODirection=\"Input\"/><TableName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}