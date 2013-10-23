using System.Collections.Generic;
using System.Runtime.Serialization;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass][ExcludeFromCodeCoverage]
    public class GetDatabaseTablesTests
    {
        #region Static Class Init

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #endregion

        

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
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDatabaseTables_UnitTest_ExecuteWithNoDatabaseInValues_ExpectedInvalidDataContractException()
        {

            var esb = new GetDatabaseTables();
            var actual = esb.Execute(new Dictionary<string, string>{{"Database",null}}, null);
            Assert.AreEqual(string.Empty, actual);
        }
        

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDatabaseTables_UnitTest_ExecuteWithBlankDatabase_ExpectInvalidDataContractException()
        {

            var esb = new GetDatabaseTables();
            var actual = esb.Execute(new Dictionary<string, string> { { "Database", "" } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDatabaseTables_UnitTest_ExecuteWithDatabaseNotValidJson_ExpectedInvalidDataContractException()
        {

            var esb = new GetDatabaseTables();
            var actual = esb.Execute(new Dictionary<string, string> { { "Database", "Test" }}, null);
            Assert.AreEqual(string.Empty, actual);
        }
        
        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDatabaseTables_UnitTest_ExecuteWithNotDbSourceJson_ExpectedInvalidDataContractException()
        {
            var someJsonData = "{Val:1}";
            var esb = new GetDatabaseTables();
            var actual = esb.Execute(new Dictionary<string, string> { { "Database", someJsonData } }, null);
            Assert.AreEqual(string.Empty, actual);
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
            Assert.AreEqual("<DataList><Database/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}