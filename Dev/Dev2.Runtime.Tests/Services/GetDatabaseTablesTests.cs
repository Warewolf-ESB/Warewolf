using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Assert.AreEqual("Invalid database sent {Val:1}.", result.Errors);
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
            Assert.AreEqual("<DataList><Database ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}