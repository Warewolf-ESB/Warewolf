using System;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ESB
{
    [TestClass]
    public class RemoteWorkflowExecutionContainerTests
    {
        static XElement _connectionXml;
        static Connection _connection;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _connectionXml = XmlResource.Fetch("ServerConnection2");
            _connection = new Connection(_connectionXml);
        }

        #region CTOR

        [TestMethod]
        [TestCategory("RemoteWorkflowExecutionContainer_Constructor")]
        [Description("RemoteWorkflowExecutionContainer cannot be constructed without a resource catalog.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoteWorkflowExecutionContainer_UnitTest_ConstructorWithNullResourceCatalog_ArgumentNullException()
        {
            var sa = new ServiceAction();
            var dataObj = new Mock<IDSFDataObject>();
            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();
            var container = new RemoteWorkflowExecutionContainerMock(sa, dataObj.Object, workspace.Object, esbChannel.Object, null);
        }

        #endregion

        #region Execute

        [TestMethod]
        [TestCategory("RemoteWorkflowExecutionContainer_Execute")]
        [Description("RemoteWorkflowExecutionContainer execute must fetch the connection uri from the resource catalog.")]
        [Owner("Trevor Williams-Ros")]
        public void RemoteWorkflowExecutionContainer_UnitTest_ExecuteWithConnectionInResourceCatalog_ConnectionRetrieved()
        {
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(c => c.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).Returns(_connectionXml.ToString);

            var container = CreateExecutionContainer(resourceCatalog.Object);

            ErrorResultTO errors;
            container.Execute(out errors);

            Assert.AreEqual(_connection.WebAddress, container.GetRequestUri, "ExecuteGetRequest did not fetch web address from resource catalog connection.");
            Assert.AreEqual(_connection.WebAddress, container.FetchRemoteDebugItemsUri, "FetchRemoteDebugItems did not fetch web address from resource catalog connection.");
        }

        [TestMethod]
        [TestCategory("RemoteWorkflowExecutionContainer_Execute")]
        [Description("RemoteWorkflowExecutionContainer execute must return an error when the connection cannot be retrieved from the resource catalog.")]
        [Owner("Trevor Williams-Ros")]
        public void RemoteWorkflowExecutionContainer_UnitTest_ExecuteWithoutConnectionInCatalog_ConnectionNotRetrieved()
        {
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(c => c.GetResourceContents(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).Returns("");

            var container = CreateExecutionContainer(resourceCatalog.Object);

            ErrorResultTO errors;
            container.Execute(out errors);

            Assert.AreEqual("Server source not found.", errors.MakeDisplayReady(), "Execute did not return an error for a non-existent resource catalog connection.");
        }
        #endregion

        #region CreateExecutionContainer

        static RemoteWorkflowExecutionContainerMock CreateExecutionContainer(IResourceCatalog resourceCatalog)
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "", "<DataList></DataList>", out errors);

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.DataListID).Returns(dataListID);
            dataObj.Setup(d => d.EnvironmentID).Returns(_connection.ResourceID);
            dataObj.Setup(d => d.ServiceName).Returns("Test");
            dataObj.Setup(d => d.RemoteInvokeResultShape).Returns("<ADL><NumericGUID></NumericGUID></ADL>");

            var sa = new ServiceAction();
            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();

            var container = new RemoteWorkflowExecutionContainerMock(sa, dataObj.Object, workspace.Object, esbChannel.Object, resourceCatalog)
            {
                GetRequestRespsonse = "<DataList><NumericGUID>74272317-2264-4564-3988-700350008298</NumericGUID></DataList>"
            };
            return container;
        }

        #endregion
    }
}
