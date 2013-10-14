using System;
using Dev2.DynamicServices;
using Dev2.Integration.Tests.Helpers;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for FindDependenciesServiceTest
    /// </summary>
    [TestClass]
    public class GetGetDatabaseColumnsForTableServiceTest {
        
        private string _webserverURI = ServerSettings.WebserverURI;

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
            var dbSource = new DbSource();
            dbSource.DatabaseName = "Cities";
            dbSource.Server = "RSAKLFSVRGENDEV";
            dbSource.AuthenticationType = AuthenticationType.User;
            dbSource.ServerType = enSourceType.SqlDatabase;
            dbSource.UserID = "testUser";
            dbSource.Password = "test123";
            //dbSource.ConnectionString = "Data Source=RSAKLFSVRGENDEV,1433;Initial Catalog=Cities;User ID=testUser;Password=test123;";

            string postData = String.Format("{0}{1}", _webserverURI, string.Format("GetDatabaseColumnsForTableService?Database={0}&TableName={1}", dbSource, "Country"));
            var response = TestHelper.PostDataToWebserver(postData);
            StringAssert.Contains(response, "\"ColumnName\":\"CountryID\"");
            StringAssert.Contains(response, "\"SqlDataType\":8");
            StringAssert.Contains(response, "\"DataTypeName\":\"int\"");
            StringAssert.Contains(response, "\"SystemDataType\":\"Int32\"");
            StringAssert.Contains(response, "\"ColumnName\":\"Description\"");
            StringAssert.Contains(response, "\"SqlDataType\":22");
            StringAssert.Contains(response, "\"MaxLength\":50");
            StringAssert.Contains(response, "\"DataTypeName\":\"varchar (50)\"");
            StringAssert.Contains(response, "\"SystemDataType\":\"String(50)\"");
        }
    }
}