using System;
using System.Data;
using Dev2.DynamicServices;
using Dev2.Integration.Tests.Helpers;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for FindDependenciesServiceTest
    /// </summary>
    [TestClass]
    public class GetGetDatabaseColumnsForTableServiceTest
    {

        private readonly string _webserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void GetDatabaseColumnsForTable_GivenDatabaseSourceWithTableName_Expected_AllColumnsForTableInDatabase()
        {
            var dbSource = new DbSource { DatabaseName = "Cities", Server = "RSAKLFSVRGENDEV", AuthenticationType = AuthenticationType.User, ServerType = enSourceType.SqlDatabase, UserID = "testUser", Password = "test123" };

            string postData = String.Format("{0}{1}", _webserverURI, string.Format("GetDatabaseColumnsForTableService?Database={0}&TableName={1}", dbSource, "Country"));
            var response = TestHelper.PostDataToWebserver(postData);

            var columns = JsonConvert.DeserializeObject<DbColumnList>(response);

            Assert.IsFalse(columns.HasErrors);
            Assert.AreEqual(2, columns.Items.Count);

            var col = columns.Items[0];
            Assert.AreEqual("CountryID", col.ColumnName);
            Assert.AreEqual(typeof(int), col.DataType);
            Assert.AreEqual("int", col.DataTypeName);
            Assert.AreEqual(-1, col.MaxLength);
            Assert.AreEqual(SqlDbType.Int, col.SqlDataType);
            Assert.AreEqual("Int32", col.SystemDataType);

            col = columns.Items[1];
            Assert.AreEqual("Description", col.ColumnName);
            Assert.AreEqual(typeof(string), col.DataType);
            Assert.AreEqual("varchar (50)", col.DataTypeName);
            Assert.AreEqual(50, col.MaxLength);
            Assert.AreEqual(SqlDbType.VarChar, col.SqlDataType);
            Assert.AreEqual("String(50)", col.SystemDataType);
        }


        [TestMethod]
        public void GetDatabaseColumnsForTable_GivenDatabaseSource_InvalidCredentials_Expected_HasErrors()
        {
            var dbSource = new DbSource { DatabaseName = "Cities", Server = "RSAKLFSVRGENDEV", AuthenticationType = AuthenticationType.User, ServerType = enSourceType.SqlDatabase, UserID = "testUser", Password = Guid.NewGuid().ToString() };

            string postData = String.Format("{0}{1}", _webserverURI, string.Format("GetDatabaseColumnsForTableService?Database={0}&TableName={1}", dbSource, "Country"));
            var response = TestHelper.PostDataToWebserver(postData);

            var tables = JsonConvert.DeserializeObject<DbColumnList>(response);

            Assert.IsTrue(tables.HasErrors);
            Assert.AreEqual("Login failed for user 'testUser'.\r\n", tables.Errors);
            Assert.AreEqual(0, tables.Items.Count);
        }
    }
}