using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Enums;
using System.Linq;
using Dev2.Runtime.Hosting;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchResourceDefinitionTests
    {

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnCorreclty()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IResourceDefinationCleaner>();
            var mockCat = new Mock<IResourceCatalog>();
            mock.Setup(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
            var resource = XML.XmlResource.Fetch("Calculate_RecordSet_Subtract");
            mockCat.Setup(p => p.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(resource.ToString()));
            mock.Setup(p => p.GetResourceDefinition(true, It.IsAny<Guid>(), It.IsAny<StringBuilder>())).Returns(new StringBuilder(resource.ToString()));
            var fetchResourceDefinition = new FetchResourceDefinition();

            //------------Execute Test---------------------------
            var resourceId = Guid.NewGuid().ToString();
            var resId = fetchResourceDefinition.GetResourceID(new Dictionary<string, StringBuilder>()
            {
                {"ResourceID", new StringBuilder(resourceId) }
            });
            //------------Assert Results-------------------------
            Assert.AreEqual(resourceId.ToGuid(), resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IResourceDefinationCleaner>();
            var mockCat = new Mock<IResourceCatalog>();
            mock.Setup(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
            var resource = XML.XmlResource.Fetch("Calculate_RecordSet_Subtract");
            mockCat.Setup(p => p.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(resource.ToString()));
            mock.Setup(p => p.GetResourceDefinition(true, It.IsAny<Guid>(), It.IsAny<StringBuilder>())).Returns(new StringBuilder(resource.ToString()));
            var fetchResourceDefinition = new FetchResourceDefinition();

            //------------Execute Test---------------------------
            var resourceId = Guid.NewGuid().ToString();
            var resId = fetchResourceDefinition.GetResourceID(new Dictionary<string, StringBuilder>()
            {
            });
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void DecryptAllPasswords_ShouldReturnVerifyCall()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IResourceDefinationCleaner>();
            var mockCat = new Mock<IResourceCatalog>();
            mock.Setup(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
            var resource = XML.XmlResource.Fetch("Calculate_RecordSet_Subtract");
            mock.Setup(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
            mockCat.Setup(p => p.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(resource.ToString()));
            mock.Setup(p => p.GetResourceDefinition(true, It.IsAny<Guid>(), It.IsAny<StringBuilder>())).Returns(new StringBuilder(resource.ToString()));
            var fetchResourceDefinition = new FetchResourceDefinition();
            fetchResourceDefinition.ResourceCat = mockCat.Object;
            fetchResourceDefinition.Cleaner = mock.Object;
            //------------Execute Test---------------------------
            fetchResourceDefinition.DecryptAllPasswords(new StringBuilder());
            //------------Assert Results-------------------------
            mock.Verify(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IResourceDefinationCleaner>();
            var mockCat = new Mock<IResourceCatalog>();
            mock.Setup(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
            var resource = XML.XmlResource.Fetch("Calculate_RecordSet_Subtract");
            mockCat.Setup(p => p.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(resource.ToString()));
            mock.Setup(p => p.GetResourceDefinition(true, It.IsAny<Guid>(), It.IsAny<StringBuilder>())).Returns(new StringBuilder(resource.ToString()));
            var fetchResourceDefinition = new FetchResourceDefinition();
            //------------Execute Test---------------------------
            var resId = fetchResourceDefinition.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.View, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DirectDeploy_HandlesType")]
        public void FetchResourceDefinition_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IResourceDefinationCleaner>();
            var mockCat = new Mock<IResourceCatalog>();
            mock.Setup(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
            var resource = XML.XmlResource.Fetch("Calculate_RecordSet_Subtract");
            mockCat.Setup(p => p.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(resource.ToString()));
            mock.Setup(p => p.GetResourceDefinition(true, It.IsAny<Guid>(), It.IsAny<StringBuilder>())).Returns(new StringBuilder(resource.ToString()));
            var fetchResourceDefinition = new FetchResourceDefinition();
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("FetchResourceDefinitionService", fetchResourceDefinition.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DirectDeploy_CreateServiceEntry")]
        public void CreateServiceEntry_PassThrough()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IResourceDefinationCleaner>();
            var mockCat = new Mock<IResourceCatalog>();
            mock.Setup(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
            var resource = XML.XmlResource.Fetch("Calculate_RecordSet_Subtract");
            mockCat.Setup(p => p.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(resource.ToString()));
            mock.Setup(p => p.GetResourceDefinition(true, It.IsAny<Guid>(), It.IsAny<StringBuilder>())).Returns(new StringBuilder(resource.ToString()));
            var fetchResourceDefinition = new FetchResourceDefinition();
            //------------Execute Test---------------------------
            var serviceEntry = fetchResourceDefinition.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.AreEqual("FetchResourceDefinitionService", serviceEntry.Actions.Single().Name);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_PassThrough()
        {
            //------------Setup for test--------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            mockWorkspace.Setup(workspace => workspace.ID).Returns(Guid.Empty);
            var resourceID = Guid.NewGuid();
            var dbSource = CreateDev2TestingDbSource(resourceID);
            var mock = new Mock<IResourceDefinationCleaner>();
            var mockCat = new Mock<IResourceCatalog>();
            mock.Setup(authorizer => authorizer.DecryptAllPasswords(It.IsAny<StringBuilder>()));
            var resource = XML.XmlResource.Fetch("Calculate_RecordSet_Subtract");
            mockCat.Setup(p => p.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(resource.ToString()));
            mock.Setup(p => p.GetResourceDefinition(true, It.IsAny<Guid>(), It.IsAny<StringBuilder>())).Returns(new StringBuilder(resource.ToString()));
            var fetchResourceDefinition = new FetchResourceDefinition();
            var values = new Dictionary<string, StringBuilder>
            {
                { "ResourceID",new StringBuilder(resourceID.ToString()) },
                { "PrepairForDeployment",new StringBuilder("true") }
            };
            fetchResourceDefinition.ResourceCat = mockCat.Object;
            fetchResourceDefinition.Cleaner = mock.Object;
            //------------Execute Test---------------------------
            var result = fetchResourceDefinition.Execute(values, mockWorkspace.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            mock.Verify(p => p.GetResourceDefinition(true, It.IsAny<Guid>(), It.IsAny<StringBuilder>()));
        }

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
