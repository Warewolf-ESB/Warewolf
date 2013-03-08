using System.Collections.Generic;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Runtime.ESB;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Unlimited.Framework;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for DynamicServicesInvokerTest
    /// </summary>
    [TestClass]
    public class DynamicServicesInvokerTest
    {
        #region Class Members

        public static object _testGuard = new object();

        #endregion Class Members

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
           
            DynamicServicesHostTests.ClassInitialize(testContext);
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {
            
        }

        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(_testGuard); 
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #region UpdateWorkspaceItem

        [TestMethod]
        public void UpdateWorkspaceItemWithNull()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

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
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

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
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

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
            var workspaceItem = new WorkspaceItem(WorkspaceTest.TestWorkspaceItemXml);

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);
            workspace.Setup(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<string>())).Verifiable();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = WorkspaceTest.TestWorkspaceItemXml.ToString();
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
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

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
            FindResourcesByID(1, DynamicServicesHostTests.ServerConnection1ID, Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void FindResourcesByID_With_TwoValidServerGuidAndOneInvalidServerGuiD_Expected_FindsTwoServers()
        {
            FindResourcesByID(2, DynamicServicesHostTests.ServerConnection1ID, DynamicServicesHostTests.ServerConnection2ID, Guid.NewGuid().ToString());
        }

        void FindResourcesByID(int expectedCount, params string[] guids)
        {
            var host = new DynamicServicesHost();

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);
            workspace.Setup(m => m.Host).Returns(host);

            IEsbManagementEndpoint findResourcesEndPoint = new FindResourcesByID();
            IDictionary<string,string> data = new Dictionary<string, string>();
            data["GuidCsv"] = string.Join(",", guids);
            data["Type"] = "Source";

            var resources = findResourcesEndPoint.Execute(data, workspace.Object);

            var resourcesObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(resources);
            var actualCount = 0;
            if (resourcesObj.Source != null)
            {
                foreach (var source in resourcesObj.Source)
                {
                    var sourceObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(source.XmlString);
                    if (guids.Contains(sourceObj.ID as string))
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
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindSourcesByType();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
        }

        [TestMethod]
        public void FindSourcesByType_With_SourceTypeParameter_Expected_ReturnsLoadedCount()
        {
            FindSourcesByType(enSourceType.Dev2Server);
        }

        static void FindSourcesByType(enSourceType sourceType)
        {
            var host = new DynamicServicesHost();
            var expectedCount = host.Sources.Count(s => s.Type == sourceType);

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);
            workspace.Setup(m => m.Host).Returns(host);

            IEsbManagementEndpoint endpoint = new FindSourcesByType();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["Type"] = sourceType.ToString();

            var resources = endpoint.Execute(data, workspace.Object);

            var resourcesObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(resources);
            var actualCount = 0;
            if (resourcesObj.Source != null)
            {
                foreach (var source in resourcesObj.Source)
                {
                    actualCount++;
                }
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        #endregion

    }
}
