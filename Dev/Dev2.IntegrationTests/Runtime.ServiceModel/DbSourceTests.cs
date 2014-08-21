using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Integration.Tests.Services.Sql;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;

namespace Dev2.Integration.Tests.Runtime.ServiceModel
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DbSourceTests
    {
        #region Save

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DBSources_Save")]
        public void DbSource_Save_ExistingSource_ServerWorkspaceUpdated()
        {
            //Initialize test resource, save then change path
            string uniquePathText = Guid.NewGuid().ToString() + "\\test db source";
            var testResource = new Resource { ResourceName = "test db source", ResourcePath = "initialpath\test db source", ResourceType = ResourceType.DbSource, ResourceID = Guid.NewGuid() };
            new DbSources().Save(testResource.ToString(), GlobalConstants.ServerWorkspaceID, Guid.Empty);
            testResource.ResourcePath = uniquePathText;

            //Execute save again on test resource
            new DbSources().Save(testResource.ToString(), GlobalConstants.ServerWorkspaceID, Guid.Empty);

            //Assert resource saved
            var getSavedResource = Resources.ReadXml(GlobalConstants.ServerWorkspaceID, ResourceType.DbSource, testResource.ResourceID.ToString());
            const string PathStartText = "<Category>";
            int start = getSavedResource.IndexOf(PathStartText, StringComparison.Ordinal);
            if(start > 0)
            {
                start += PathStartText.Length;
                int end = (getSavedResource.IndexOf("</Category>", start, StringComparison.Ordinal));
                var savedPath = getSavedResource.Substring(start, end - start);
                Assert.AreEqual(uniquePathText, savedPath);
            }
            else
            {
                Assert.Fail("Resource xml malformed after save");
            }
        }

        #endregion

        #region Test

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DbSources_Test")]
        public void DbSources_Test_InvokesDatabaseBrokerTestMethod_Done()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
            var args = JsonConvert.SerializeObject(dbSource);

            var dbBroker = new Mock<SqlDatabaseBroker>();
            dbBroker.Setup(b => b.GetDatabases(It.IsAny<DbSource>())).Verifiable();

            var dbSources = new TestDbSources(dbBroker.Object);

            //------------Execute Test---------------------------
            var result = dbSources.Test(args, Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            dbBroker.Verify(b => b.GetDatabases(It.IsAny<DbSource>()));

            Assert.IsNotNull(result);
        }

        #endregion
    }
}
