using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchResourceDefinitionTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenForDeployment_ShouldDecryptPassword()
        {
            //------------Setup for test--------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            mockWorkspace.Setup(workspace => workspace.ID).Returns(Guid.Empty);
            var resourceID = Guid.NewGuid();
            var dbSource = CreateDev2TestingDbSource(resourceID);
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource, "");
            var fetchResourceDefinition = new FetchResourceDefinition();
            var values = new Dictionary<string, StringBuilder>
            {
                { "ResourceID",new StringBuilder(resourceID.ToString()) },
                { "PrepairForDeployment",new StringBuilder("true") }
            };
            
            //------------Execute Test---------------------------
            var result = fetchResourceDefinition.Execute(values, mockWorkspace.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("RSAKLFSVRGENDEV"));
            Assert.IsTrue(result.Contains("testUser"));
            Assert.IsTrue(result.Contains("test123"));            
        }

        DbSource CreateDev2TestingDbSource(Guid resourceID)
        {
            var dbSource = new DbSource
            {
                ResourceID = resourceID,
                ResourceName = "Dev2TestingDB",
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
    }
}
