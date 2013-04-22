using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for DynamicServicesInvokerTest
    /// </summary>
    [TestClass]
    public class DynamicServicesInvokerTest
    {
        static readonly Guid TestWorkspaceID = new Guid("B1890C86-95D8-4612-A7C3-953250ED237A");

        static readonly XElement TestWorkspaceItemXml = XmlResource.Fetch("WorkspaceItem");


        const int VersionNo = 9999;

        const string ServiceName = "Calculate_RecordSet_Subtract";

        const string ServiceNameUnsigned = "TestDecisionUnsigned";

        const string SourceName = "CitiesDatabase";

        public const string ServerConnection1Name = "ServerConnection1";

        public const string ServerConnection1ResourceName = "MyDevServer";

        public const string ServerConnection1ID = "68F5B4FE-4573-442A-BA0C-5303F828344F";

        public const string ServerConnection2Name = "ServerConnection2";

        public const string ServerConnection2ResourceName = "MySecondDevServer";

        public const string ServerConnection2ID = "70238921-FDC7-4F7A-9651-3104EEDA1211";

        static string _workspacesDir;

        Guid _workspaceID;

        #region ClassInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _workspacesDir = Path.Combine(testContext.TestDir, "Workspaces");
            Directory.SetCurrentDirectory(testContext.TestDir);
        }

        #endregion

        #region TestInitialize/Cleanup

        static readonly object TestLock = new object();

        [TestInitialize]
        public void TestInitialize()
        {
            Monitor.Enter(TestLock);

            _workspaceID = Guid.NewGuid();

            List<IResource> resources;
            ResourceCatalogTests.SaveResources(_workspaceID, VersionNo.ToString(), true, false,
                new[] { SourceName, ServerConnection1Name, ServerConnection2Name },
                new[] { ServiceName, ServiceNameUnsigned },
                out resources);

            ResourceCatalog.Instance.LoadWorkspace(_workspaceID);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DirectoryHelper.CleanUp(_workspacesDir);
            Monitor.Exit(TestLock);
        }

        #endregion

        #region UpdateWorkspaceItem

        [TestMethod]
        public void UpdateWorkspaceItemWithNull()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = string.Empty;
            data["Roles"] = string.Empty;

            var result = endpoint.Execute(data, workspace.Object);

            Assert.IsTrue(result.Contains("<Error>Invalid workspace item definition</Error>"));

        }

        [TestMethod]
        public void UpdateWorkspaceItemWithInvalidItemXml()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = "<xxxx/>";
            data["Roles"] = null;

            var result = endpoint.Execute(data, workspace.Object);

            Assert.IsTrue(result.Contains("<Error>Error updating workspace item</Error>"));
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithItemXmlFromAnotherWorkspace()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            var workspaceItem = new WorkspaceItem(Guid.NewGuid(), Guid.NewGuid());
            var itemXml = workspaceItem.ToXml().ToString();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = itemXml;
            data["Roles"] = string.Empty;

            var result = endpoint.Execute(data, workspace.Object);

            Assert.IsTrue(result.Contains("<Error>Cannot update a workspace item from another workspace</Error>"));

        }

        [TestMethod]
        public void UpdateWorkspaceItemWithValidItemXml()
        {
            var workspaceItem = new WorkspaceItem(TestWorkspaceItemXml);

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);
            workspace.Setup(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<string>())).Verifiable();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = TestWorkspaceItemXml.ToString();
            data["Roles"] = string.Empty;

            var result = endpoint.Execute(data, workspace.Object);

            workspace.Verify(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<string>()), Times.Exactly(1));

        }

        #endregion UpdateWorkspaceItem

        #region FindResourcesByID

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindResourcesByID_With_NullTypeParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindResourcesByID();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["GuidCsv"] = null;
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
        }

        [TestMethod]
        public void FindResourcesByID_With_EmptyServerGuid_Expected_FindsZeroServers()
        {
            FindResourcesByID(0);
        }

        [TestMethod]
        public void FindResourcesByID_With_InvalidServerGuids_Expected_FindsZeroServers()
        {
            FindResourcesByID(0, Guid.NewGuid().ToString(), "xxx");
        }

        [TestMethod]
        public void FindResourcesByID_With_OneValidServerGuidAndOneInvalidServerGuiD_Expected_FindsOneServer()
        {
            FindResourcesByID(1, ServerConnection1ID, Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void FindResourcesByID_With_TwoValidServerGuidAndOneInvalidServerGuiD_Expected_FindsTwoServers()
        {
            FindResourcesByID(2, ServerConnection1ID, ServerConnection2ID, Guid.NewGuid().ToString());
        }

        void FindResourcesByID(int expectedCount, params string[] guids)
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint findResourcesEndPoint = new FindResourcesByID();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["GuidCsv"] = string.Join(",", guids);
            data["Type"] = "Source";

            var resources = findResourcesEndPoint.Execute(data, workspace.Object);

            var resourcesObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(resources);
            var actualCount = 0;
            if(resourcesObj.Source != null)
            {
                foreach(var source in resourcesObj.Source)
                {
                    var sourceObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(source.XmlString);
                    if (guids.Any(g => g.Equals(sourceObj.ID as string, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        actualCount++;
                    }
                }
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        #endregion

        #region FindSourcesByType

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindSourcesByType_With_NullTypeParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindSourcesByType();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
        }

        [TestMethod]
        [Ignore] // Weird test
        public void FindSourcesByType_With_SourceTypeParameter_Expected_ReturnsLoadedCount()
        {
            FindSourcesByType(enSourceType.Dev2Server);
        }

        void FindSourcesByType(enSourceType sourceType)
        {
            var expectedCount = 2;

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint endpoint = new FindSourcesByType();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["Type"] = sourceType.ToString();

            var resources = endpoint.Execute(data, workspace.Object);

            var resourcesObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(resources);
            var actualCount = 0;
            if(resourcesObj.Source != null)
            {
                foreach(var source in resourcesObj.Source)
                {
                    actualCount++;
                }
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        #endregion


        static void Initialize()
        {
        }
    }
}
