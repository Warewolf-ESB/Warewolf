
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
    public class GetDatabaseTablesTests
    {
        #region Execute

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDatabaseTables_UnitTest_ExecuteWithNullValues_ExpectedInvalidDataContractException()
        {
            var esb = new GetDatabaseTables();
            var actual = esb.Execute(null, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseTables_UnitTest_ExecuteWithNoDatabaseInValues_ExpectedInvalidHasErrors()
        {
            var esb = new GetDatabaseTables();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", null } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbTableList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("No database set.", result.Errors);
        }


        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseTables_UnitTest_ExecuteWithBlankDatabase_ExpectHasErrors()
        {
            var esb = new GetDatabaseTables();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder() } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbTableList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("No database set.", result.Errors);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseTables_UnitTest_ExecuteWithDatabaseNotValidJson_ExpectedHasErrors()
        {
            var esb = new GetDatabaseTables();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder("Test") } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbTableList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Invalid JSON data for Database parameter. Exception: Unexpected character encountered while parsing value: T. Path '', line 0, position 0.", result.Errors);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseTables_UnitTest_ExecuteWithNotDbSourceJson_ExpectedHasErrors()
        {
            const string someJsonData = "{Val:1}";
            var esb = new GetDatabaseTables();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder(someJsonData) } }, null);
            Assert.IsNotNull(actual);
            var result = JsonConvert.DeserializeObject<DbTableList>(actual.ToString());
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Invalid Database source", result.Errors);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetDatabaseTables_Execute")]
        public void GetDatabaseTables_Execute_ValidDatabaseSource()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource();
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource);
            string someJsonData = JsonConvert.SerializeObject(dbSource);
            var esb = new GetDatabaseTables();
            var mockWorkspace = new Mock<IWorkspace>();
            mockWorkspace.Setup(workspace => workspace.ID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder(someJsonData) } }, mockWorkspace.Object);
            //------------Assert Results-------------------------
            var value = actual.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(value));
            var result = JsonConvert.DeserializeObject<DbTableList>(value);
            Assert.IsTrue(result.Items.Count > 2);
            var duplicateTables = result.Items.FindAll(table => table.TableName.Contains("[City]"));
            Assert.AreEqual(2, duplicateTables.Count);
            var dboCityTable = duplicateTables.Find(table => table.Schema == "dbo");
            var warewolfCityTable = duplicateTables.Find(table => table.Schema == "Warewolf");
            Assert.IsNotNull(dboCityTable);
            Assert.AreEqual("dbo", dboCityTable.Schema);
            Assert.IsNotNull(warewolfCityTable);
            Assert.AreEqual("Warewolf", warewolfCityTable.Schema);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetDatabaseTables_Execute")]
        public void GetDatabaseTables_Execute_InValidDatabaseName()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource(true);
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource);
            string someJsonData = JsonConvert.SerializeObject(dbSource);
            var esb = new GetDatabaseTables();
            var mockWorkspace = new Mock<IWorkspace>();
            mockWorkspace.Setup(workspace => workspace.ID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Database", new StringBuilder(someJsonData) } }, mockWorkspace.Object);
            //------------Assert Results-------------------------
            var value = actual.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(value));
            var result = JsonConvert.DeserializeObject<DbTableList>(value);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(value.Contains("Invalid database sent"));
        }

        DbSource CreateDev2TestingDbSource(bool emptyDBName=false)
        {
            var dbSource = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dev2TestingDB",
                ResourcePath = "Test",
                DatabaseName = emptyDBName?"": "Dev2TestingDB",
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
        public void GetDatabaseTables_UnitTest_HandlesType_ExpectedReturnsGetDatabaseTablesService()
        {
            var esb = new GetDatabaseTables();
            var result = esb.HandlesType();
            Assert.AreEqual("GetDatabaseTablesService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void GetDatabaseTables_UnitTest_CreateServiceEntry_ExpectedReturnsDynamicService()
        {
            var esb = new GetDatabaseTables();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Database ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
