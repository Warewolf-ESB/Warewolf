
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
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
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
        [TestCategory("GetDatabaseColumnsForTable_Execute")]
        public void GetDatabaseColumnsForTable_Execute_ValidDatabaseSource_WithSchema_OnlyReturnsForThatSchema()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource();
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource);
            string someJsonData = JsonConvert.SerializeObject(dbSource, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });
            var esb = new GetDatabaseColumnsForTable();
            var mockWorkspace = new Mock<IWorkspace>();
            mockWorkspace.Setup(workspace => workspace.ID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder(someJsonData) }, { "TableName", new StringBuilder("City") }, { "Schema", new StringBuilder("Warewolf") } }, mockWorkspace.Object);
            //------------Assert Results-------------------------
            var value = actual.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(value));
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });
            Assert.AreEqual(4, result.Items.Count);

            // Check Columns Returned ;)
            Assert.IsFalse(result.Items[0].IsNullable);
            Assert.IsTrue(result.Items[0].IsAutoIncrement);
            StringAssert.Contains(result.Items[0].ColumnName, "CityID");
            StringAssert.Contains(result.Items[0].SqlDataType.ToString(), "Int");

            Assert.IsFalse(result.Items[1].IsNullable);
            Assert.IsFalse(result.Items[1].IsAutoIncrement);
            StringAssert.Contains(result.Items[1].ColumnName, "Description");
            StringAssert.Contains(result.Items[1].SqlDataType.ToString(), "VarChar");

            Assert.IsFalse(result.Items[2].IsNullable);
            Assert.IsFalse(result.Items[2].IsAutoIncrement);
            StringAssert.Contains(result.Items[2].ColumnName, "CountryID");
            StringAssert.Contains(result.Items[2].SqlDataType.ToString(), "Int");

            Assert.IsTrue(result.Items[3].IsNullable);
            Assert.IsFalse(result.Items[3].IsAutoIncrement);
            StringAssert.Contains(result.Items[3].ColumnName, "TestCol");
            StringAssert.Contains(result.Items[3].SqlDataType.ToString(), "NChar");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetDatabaseColumnsForTable_Execute")]
        public void GetDatabaseColumnsForTable_Execute_NullSchema_ValidDatabaseSource_ReturnsFromAllSchemas()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource();
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource);
            string someJsonData = JsonConvert.SerializeObject(dbSource,new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });
            var esb = new GetDatabaseColumnsForTable();
            var mockWorkspace = new Mock<IWorkspace>();
            mockWorkspace.Setup(workspace => workspace.ID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder(someJsonData) }, { "TableName", new StringBuilder("City") }, { "Schema", null } }, mockWorkspace.Object);
            //------------Assert Results-------------------------
            var value = actual.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(value));
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });
            Assert.AreEqual(3, result.Items.Count);

            // Check Columns Returned ;)
            Assert.IsFalse(result.Items[0].IsNullable);
            Assert.IsFalse(result.Items[0].IsAutoIncrement);
            StringAssert.Contains(result.Items[0].ColumnName, "CityID");
            StringAssert.Contains(result.Items[0].SqlDataType.ToString(), "Int");

            Assert.IsFalse(result.Items[1].IsNullable);
            Assert.IsFalse(result.Items[1].IsAutoIncrement);
            StringAssert.Contains(result.Items[1].ColumnName, "Description");
            StringAssert.Contains(result.Items[1].SqlDataType.ToString(), "VarChar");

            Assert.IsFalse(result.Items[2].IsNullable);
            Assert.IsFalse(result.Items[2].IsAutoIncrement);
            StringAssert.Contains(result.Items[2].ColumnName, "CountryID");
            StringAssert.Contains(result.Items[2].SqlDataType.ToString(), "Int");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetDatabaseColumnsForTable_Execute")]
        public void GetDatabaseColumnsForTable_Execute_EmptySchema_ValidDatabaseSource_ReturnsFromAllSchemas()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource();
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource);
            string someJsonData = JsonConvert.SerializeObject(dbSource, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });
            var esb = new GetDatabaseColumnsForTable();
            var mockWorkspace = new Mock<IWorkspace>();
            mockWorkspace.Setup(workspace => workspace.ID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder(someJsonData) }, { "TableName", new StringBuilder("City") }, { "Schema", new StringBuilder("") } }, mockWorkspace.Object);
            //------------Assert Results-------------------------
            var value = actual.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(value));
            var result = JsonConvert.DeserializeObject<DbColumnList>(actual.ToString(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });
            Assert.AreEqual(3, result.Items.Count);

            // Check Columns Returned ;)
            Assert.IsFalse(result.Items[0].IsNullable);
            Assert.IsFalse(result.Items[0].IsAutoIncrement);
            StringAssert.Contains(result.Items[0].ColumnName, "CityID");
            StringAssert.Contains(result.Items[0].SqlDataType.ToString(), "Int");

            Assert.IsFalse(result.Items[1].IsNullable);
            Assert.IsFalse(result.Items[1].IsAutoIncrement);
            StringAssert.Contains(result.Items[1].ColumnName, "Description");
            StringAssert.Contains(result.Items[1].SqlDataType.ToString(), "VarChar");

            Assert.IsFalse(result.Items[2].IsNullable);
            Assert.IsFalse(result.Items[2].IsAutoIncrement);
            StringAssert.Contains(result.Items[2].ColumnName, "CountryID");
            StringAssert.Contains(result.Items[2].SqlDataType.ToString(), "Int");
        }

        static DbSource CreateDev2TestingDbSource()
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
            Assert.AreEqual("<DataList><Database ColumnIODirection=\"Input\"/><TableName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
