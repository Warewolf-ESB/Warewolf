using System;
using System.Linq;
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
    public class GetDatabaseTablesServiceTest
    {
        readonly string _webserverURI = ServerSettings.WebserverURI;

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
            var dbSource = new DbSource { DatabaseName = "Cities", Server = "RSAKLFSVRGENDEV", AuthenticationType = AuthenticationType.User, ServerType = enSourceType.SqlDatabase, UserID = "testUser", Password = "test123" };
            //dbSource.ConnectionString = "Data Source=RSAKLFSVRGENDEV,1433;Initial Catalog=Cities;User ID=testUser;Password=test123;";

            string postData = String.Format("{0}{1}", _webserverURI, string.Format("GetDatabaseTablesService?Database={0}", dbSource));
            var response = TestHelper.PostDataToWebserver(postData);
            var tables = JsonConvert.DeserializeObject<DbTableList>(response);

            Assert.IsFalse(tables.HasErrors);

            var expectedTables = new[] { "City", "Country", "DummyInsert", "sysdiagrams", "WorldCities" };
            var actualTables = tables.Items.OrderBy(i => i.TableName).Select(i => i.TableName).ToArray();
            CollectionAssert.AreEqual(expectedTables, actualTables);
        }


        [TestMethod]
        public void GetTablesForDatabase_GivenDatabaseSource_InvalidCredentials_Expected_HasErrors()
        {
            var dbSource = new DbSource { DatabaseName = "Cities", Server = "RSAKLFSVRGENDEV", AuthenticationType = AuthenticationType.User, ServerType = enSourceType.SqlDatabase, UserID = "testUser", Password = Guid.NewGuid().ToString() };

            string postData = String.Format("{0}{1}", _webserverURI, string.Format("GetDatabaseTablesService?Database={0}", dbSource));
            var response = TestHelper.PostDataToWebserver(postData);
            var tables = JsonConvert.DeserializeObject<DbTableList>(response);

            Assert.IsTrue(tables.HasErrors);
            Assert.AreEqual("Login failed for user 'testUser'.\r\n", tables.Errors);
            Assert.AreEqual(0, tables.Items.Count);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GetDatabaseColumnsForTable_Execute")]
        public void GetDatabaseColumnsForTable_Execute_NoTables_HasPermissionsError()
        {
            //------------Setup for test--------------------------

            var dbSource = new DbSource { DatabaseName = "DemoDB", Server = "RSAKLFSVRGENDEV", AuthenticationType = AuthenticationType.User, ServerType = enSourceType.SqlDatabase, UserID = "DemoDBPublicUser", Password = "P@ssword1" };

            //------------Execute Test---------------------------
            string postData = String.Format("{0}{1}", _webserverURI, string.Format("GetDatabaseTablesService?Database={0}", dbSource));
            var response = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            var tables = JsonConvert.DeserializeObject<DbTableList>(response);
            Assert.AreEqual(0, tables.Items.Count);

            const string ErrorFormat = "The login provided in the database source uses {0} and most probably does not have permissions to perform the following query: "
                      + "\r\n\r\n{1}SELECT * FROM INFORMATION_SCHEMA.TABLES;{2}";

            string expectedMessage;
            if(dbSource.AuthenticationType == AuthenticationType.User)
            {
                expectedMessage = string.Format(ErrorFormat,
                    "SQL Authentication (User: '" + dbSource.UserID + "')",
                    "EXECUTE AS USER = '" + dbSource.UserID + "';\r\n",
                    "\r\nREVERT;");
            }
            else
            {
                expectedMessage = string.Format(ErrorFormat, "Windows Authentication", "", "");
            }

            Assert.IsTrue(tables.HasErrors);
            Assert.AreEqual(expectedMessage, tables.Errors);
        }
    }
}