using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ESB;
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

        Guid _workspaceID;

        #region TestInitialize/Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            _workspaceID = Guid.NewGuid();

            List<IResource> resources;
            ResourceCatalogTests.SaveResources(_workspaceID, VersionNo.ToString(), true, false,
                new[] { SourceName, ServerConnection1Name, ServerConnection2Name },
                new[] { ServiceName, ServiceNameUnsigned },
                out resources);

            ResourceCatalog.Instance.LoadWorkspace(_workspaceID);
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

            var workspaceItem = new WorkspaceItem(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);
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
            workspace.Setup(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<bool>(), It.IsAny<string>())).Verifiable();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = TestWorkspaceItemXml.ToString();
            data["Roles"] = string.Empty;

            var result = endpoint.Execute(data, workspace.Object);

            workspace.Verify(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<bool>(), It.IsAny<string>()), Times.Exactly(1));

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
                    if(guids.Any(g => g.Equals(sourceObj.ID as string, StringComparison.InvariantCultureIgnoreCase)))
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

        #region Invoke

        // BUG 9706 - 2013.06.22 - TWR : added
        [TestMethod]
        public void DynamicServicesInvokerInvokeWithErrorsExpectedInvokesDebugDispatcherBeforeAndAfterExecution()
        {
            const string PreErrorMessage = "There was an pre error.";

            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                "<DataList><Prefix>an</Prefix><Dev2System.Error>" + PreErrorMessage + "</Dev2System.Error></DataList>",
                "<ADL><Prefix></Prefix><Countries><CountryID></CountryID><CountryName></CountryName></Countries></ADL>", out errors);

            var workspaceID = Guid.NewGuid();
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(w => w.ID).Returns(workspaceID);

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.WorkspaceID).Returns(workspaceID);
            dataObj.Setup(d => d.DataListID).Returns(dataListID);
            dataObj.Setup(d => d.IsDebug).Returns(true);

            var actualStates = new List<IDebugState>();

            var debugWriter = new Mock<IDebugWriter>();
            debugWriter.Setup(w => w.Write(It.IsAny<IDebugState>())).Callback<IDebugState>(actualStates.Add).Verifiable();

            DebugDispatcher.Instance.Add(workspaceID, debugWriter.Object);

            var dsi = new DynamicServicesInvoker(new Mock<IEsbChannel>().Object, null, workspace.Object);
            dsi.Invoke(dataObj.Object, out errors);

            Thread.Sleep(3000);  // wait for DebugDispatcher Write Queue 

            // Clean up
            DebugDispatcher.Instance.Remove(workspaceID);

            // Will get called twice once for pre and once for post
            debugWriter.Verify(w => w.Write(It.IsAny<IDebugState>()), Times.Exactly(2));

            for(var i = 0; i < actualStates.Count; i++)
            {
                Assert.IsNotNull(actualStates[i]);
                Assert.IsTrue(actualStates[i].HasError);
                Assert.AreEqual(ActivityType.Workflow, actualStates[i].ActivityType);
                switch(i)
                {
                    case 0:
                        Assert.AreEqual(PreErrorMessage, actualStates[i].ErrorMessage);
                        Assert.AreEqual(StateType.Before, actualStates[i].StateType);
                        break;
                    case 1:
                        Assert.AreEqual("Error: Service was not specified", actualStates[i].ErrorMessage);
                        Assert.AreEqual(StateType.After, actualStates[i].StateType);
                        break;
                    default:
                        Assert.Fail("Too many DebugDispatcher.Write invocations");
                        break;
                }
            }
        }

        #endregion

    }
}
