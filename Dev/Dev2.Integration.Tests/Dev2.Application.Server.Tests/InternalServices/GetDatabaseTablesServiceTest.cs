using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.DynamicServices;
using Dev2.Integration.Tests.Helpers;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for FindDependenciesServiceTest
    /// </summary>
    [TestClass]
    public class GetDatabaseTablesServiceTest {
        
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
        public void GetTablesForDatabase_GivenDatabaseSource_Expected_AllTablesForDatabase()
        {
            var dbSource = new DbSource();
            dbSource.DatabaseName = "Cities";
            dbSource.Server = "RSAKLFSVRGENDEV";
            dbSource.AuthenticationType = AuthenticationType.User;
            dbSource.ServerType = enSourceType.SqlDatabase;
            dbSource.UserID = "testUser";
            dbSource.Password = "test123";
            //dbSource.ConnectionString = "Data Source=RSAKLFSVRGENDEV,1433;Initial Catalog=Cities;User ID=testUser;Password=test123;";

            string postData = String.Format("{0}{1}", _webserverURI, string.Format("GetDatabaseTablesService?Database={0}", dbSource));
            var response = TestHelper.PostDataToWebserver(postData);
            StringAssert.Contains(response,"\"TableName\":\"sysdiagrams\"");
            StringAssert.Contains(response,"\"TableName\":\"DummyInsert\"");
            StringAssert.Contains(response,"\"TableName\":\"City\"");
            StringAssert.Contains(response,"\"TableName\":\"Country\"");
            StringAssert.Contains(response,"\"TableName\":\"WorldCities\"");
        }

    }
}