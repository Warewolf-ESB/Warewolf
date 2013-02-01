using Dev2.Runtime.InterfaceImplementors;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
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

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            var result = invoker.UpdateWorkspaceItem(null, null);
            Assert.IsTrue(result.Contains("<Error>Invalid workspace item definition</Error>"));
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithInvalidItemXml()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            var result = invoker.UpdateWorkspaceItem("<xxxx/>", null);
            Assert.IsTrue(result.Contains("<Error>Error updating workspace item</Error>"));
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithItemXmlFromAnotherWorkspace()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

            var workspaceItem = new WorkspaceItem(Guid.NewGuid(), Guid.NewGuid());
            var itemXml = workspaceItem.ToXml().ToString();

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            var result = invoker.UpdateWorkspaceItem(itemXml, null);
            Assert.IsTrue(result.Contains("<Error>Cannot update a workspace item from another workspace</Error>"));
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithValidItemXml()
        {
            var workspaceItem = new WorkspaceItem(WorkspaceTest.TestWorkspaceItemXml);

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);
            workspace.Setup(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<string>())).Verifiable();

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            invoker.UpdateWorkspaceItem(WorkspaceTest.TestWorkspaceItemXml.ToString(), null);

            workspace.Verify(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<string>()), Times.Exactly(1));
        }

        #endregion UpdateWorkspaceItem

        #region FindResourcesByID

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindResourcesByID_With_NullGuidCsvParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            var result = invoker.FindResourcesByID(null, "xx");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindResourcesByID_With_NullTypeParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(WorkspaceTest.TestWorkspaceID);

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            var result = invoker.FindResourcesByID("xx", null);
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

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            var resources = invoker.FindResourcesByID(string.Join(",", guids), "Source");

            var resourcesObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(resources);
            var actualCount = 0;
            if(resourcesObj.Source != null)
            {
                foreach(var source in resourcesObj.Source)
                {
                    var sourceObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(source.XmlString);
                    if(guids.Contains(sourceObj.ID as string))
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

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            invoker.FindSourcesByType(null);
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

            var invoker = new DynamicServicesInvoker(null, null, false, workspace.Object);
            var resources = invoker.FindSourcesByType(sourceType.ToString());

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

    }
}
