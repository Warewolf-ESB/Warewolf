using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Runtime.ESB;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.Hosting;
using Dev2.Tests.Runtime.XML;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime
{
    /// <summary>
    /// Summary description for DynamicServicesInvokerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EsbServicesInvokerTest
    {
        static readonly Guid TestWorkspaceID = new Guid("B1890C86-95D8-4612-A7C3-953250ED237A");

        static readonly XElement TestWorkspaceItemXml = XmlResource.Fetch("WorkspaceItem");

        const int VersionNo = 9999;

        const string ServiceName = "Calculate_RecordSet_Subtract";

        const string ServiceNameUnsigned = "TestDecisionUnsigned";

        const string SourceName = "CitiesDatabase";

        readonly Guid _sourceID = Guid.NewGuid();

        readonly Guid _serviceID = Guid.NewGuid();

        readonly Guid _unsignedServiceID = Guid.NewGuid();

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
            ResourceCatalogTests.SaveResources(_workspaceID, VersionNo.ToString(CultureInfo.InvariantCulture), true, false,
                new[] { SourceName, ServerConnection1Name, ServerConnection2Name },
                new[] { ServiceName, ServiceNameUnsigned },
                out resources,
                new[] { _sourceID, Guid.Parse(ServerConnection1ID), Guid.Parse(ServerConnection2ID) },
                new[] { _serviceID, _unsignedServiceID });

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
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ItemXml"] = new StringBuilder();
            data["Roles"] = new StringBuilder();

            var result = endpoint.Execute(data, workspace.Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.IsTrue(obj.Message.Contains("Invalid workspace item definition"));
            Assert.IsTrue(obj.HasError);
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithInvalidItemXml()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ItemXml"] = new StringBuilder("<xxxx/>");
            data["Roles"] = null;

            var result = endpoint.Execute(data, workspace.Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.IsTrue(obj.Message.Contains("Error updating workspace item"));
            Assert.IsTrue(obj.HasError);
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithItemXmlFromAnotherWorkspace()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            var workspaceItem = new WorkspaceItem(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.Empty);
            var itemXml = workspaceItem.ToXml().ToString();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ItemXml"] = new StringBuilder(itemXml);
            data["Roles"] = new StringBuilder();

            var result = endpoint.Execute(data, workspace.Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.IsTrue(obj.Message.Contains("Cannot update a workspace item from another workspace"));
            Assert.IsTrue(obj.HasError);

        }

        [TestMethod]
        public void UpdateWorkspaceItemWithValidItemXml()
        {
            var workspaceItem = new WorkspaceItem(TestWorkspaceItemXml);

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);
            workspace.Setup(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<bool>(), It.IsAny<string>())).Verifiable();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ItemXml"] = new StringBuilder(TestWorkspaceItemXml.ToString());

            endpoint.Execute(data, workspace.Object);

            workspace.Verify(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<bool>(), It.IsAny<string>()), Times.Exactly(1));

        }

        #endregion UpdateWorkspaceItem

        #region FindResourcesByID

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void FindResourcesByID_With_NullTypeParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindResourcesByID();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
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
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["GuidCsv"] = new StringBuilder(string.Join(",", guids));
            data["ResourceType"] = new StringBuilder("Source");

            var resources = findResourcesEndPoint.Execute(data, workspace.Object);

            var resourcesObj = JsonConvert.DeserializeObject<List<Resource>>(resources.ToString());

            var actualCount = 0;
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var res in resourcesObj)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if(res.ResourceType == ResourceType.DbSource || res.ResourceType == ResourceType.PluginSource ||
                    res.ResourceType == ResourceType.WebSource || res.ResourceType == ResourceType.EmailSource || res.ResourceType == ResourceType.Server)
                {
                    actualCount++;
                }
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        #endregion

        #region FetchResourceDefinition

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenDefintionExist_ResourceDefinition()
        {

            //------------Setup for test--------------------------
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();

            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ResourceID"] = new StringBuilder("b2b0cc87-32ba-4504-8046-79edfb18d5fd");
            //data["ResourceType"] = new StringBuilder();

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------

            var expected = XmlResource.Fetch("Calculate_RecordSet_Subtract_Expected").ToString(SaveOptions.DisableFormatting);
            var obj = ConvertToMsg(xaml.ToString());

            Assert.AreEqual(expected.Replace(" ", ""), obj.Message.Replace(Environment.NewLine, "").Replace(" ", "").ToString());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenSourceDefintionExist_ResourceDefinition()
        {

            //------------Setup for test--------------------------

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            var sourceXML = XmlResource.Fetch(SourceName);
            var nameAttribute = sourceXML.Attribute("Name");

            var resourcePath = sourceXML.ElementSafe("Category");

            var serverIDAttribute = sourceXML.Attribute("ServerID");
            ResourceCatalog.Instance.SaveResource(_workspaceID, sourceXML.ToStringBuilder());
            var resource = ResourceCatalog.Instance.GetResource(_workspaceID, resourcePath + "\\" + SourceName);
            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();

            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ResourceID"] = new StringBuilder(resource.ResourceID.ToString());

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------
            var obj = ConvertToMsg(xaml.ToString());
            var actual = obj.Message.ToString();
            Assert.IsFalse(String.IsNullOrWhiteSpace(actual));
            Assert.IsNotNull(nameAttribute);
            StringAssert.Contains(actual, nameAttribute.ToString());
            StringAssert.Contains(actual, serverIDAttribute.ToString());
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenServiceHasUnlimitedNamespace_ResourceDefinition()
        {

            //------------Setup for test--------------------------
            const string serviceName = "Bug9304";

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            var xml = XmlResource.Fetch(serviceName);
            var categoryElement = xml.Element("Category");
            Assert.IsNotNull(categoryElement);
            var resourcePath = categoryElement.Value;
            ResourceCatalog.Instance.SaveResource(_workspaceID, xml.ToStringBuilder());
            var resource = ResourceCatalog.Instance.GetResource(_workspaceID, resourcePath + "\\" + serviceName);
            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();

            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ResourceID"] = new StringBuilder(resource.ResourceID.ToString());

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------
            var expected = XmlResource.Fetch("Bug9304_RemovedUnlimitedNamespace").ToString(SaveOptions.DisableFormatting);

            var obj = ConvertToMsg(xaml.ToString());
            var actual = obj.Message.ToString();
            Assert.IsFalse(String.IsNullOrWhiteSpace(actual));
            // There seems to be an extra " " somewhere ;(
            StringAssert.Contains(actual.Replace(" ", ""), expected.Replace(" ", ""));

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenDefintionDoesNotExist_ExpectNothing()
        {
            //------------Setup for test--------------------------
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ResourceID"] = new StringBuilder(Guid.NewGuid().ToString());

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------
            var obj = ConvertToMsg(xaml.ToString());
            Assert.AreEqual(string.Empty, obj.Message.ToString());
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
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
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
                "<DataList><Prefix>an</Prefix><Dev2System.Dev2Error>" + PreErrorMessage + "</Dev2System.Dev2Error></DataList>",
                "<ADL><Prefix></Prefix><Countries><CountryID></CountryID><CountryName></CountryName></Countries></ADL>", out errors);

            var workspaceID = Guid.NewGuid();
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(w => w.ID).Returns(workspaceID);

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.WorkspaceID).Returns(workspaceID);
            dataObj.Setup(d => d.DataListID).Returns(dataListID);
            dataObj.Setup(d => d.IsDebugMode()).Returns(true);

            var actualStates = new List<IDebugState>();

            var debugWriter = new Mock<IDebugWriter>();
            debugWriter.Setup(w => w.Write(It.IsAny<IDebugState>())).Callback<IDebugState>(actualStates.Add).Verifiable();

            DebugDispatcher.Instance.Add(workspaceID, debugWriter.Object);

            var dsi = new EsbServiceInvoker(new Mock<IEsbChannel>().Object, null, workspace.Object);
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
                        Assert.AreEqual(StateType.End, actualStates[i].StateType);
                        break;
                    default:
                        Assert.Fail("Too many DebugDispatcher.Write invocations");
                        break;
                }
            }
        }

        #endregion

        private ExecuteMessage ConvertToMsg(string payload)
        {
            return JsonConvert.DeserializeObject<ExecuteMessage>(payload);
        }

        // ReSharper restore InconsistentNaming
    }
}
