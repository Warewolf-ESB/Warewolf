using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ESB
{
    [TestClass]
    public class WebServiceContainerTests
    {
        static readonly XElement WebSourceXml = XmlResource.Fetch("WebSource");
        static readonly XElement WebServiceXml = XmlResource.Fetch("WebService");
        static readonly XElement WebServiceResponseXml = XmlResource.Fetch("WebServiceResponse");

        #region Execute

        [TestMethod]
        public void WebServiceContainerExecuteWithValidServiceExpectedExecutesService()
        {
            var container = CreateWebServiceContainer();

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);
            var compiler = DataListFactory.CreateDataListCompiler();

            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);

            var resultXml = XElement.Parse(result);

            var expectedRoot = (XElement)WebServiceResponseXml.FirstNode;
            foreach(var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if(!actualName.StartsWith("Dev2System"))
                {
                    var expectedNode = expectedRoot.Element(actualName);
                    Assert.AreEqual(expectedNode.Value, actualNode.Value);
                }
            }
        }

        #endregion

        #region CreateWebServiceContainer

        static WebServiceContainerMock CreateWebServiceContainer()
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "", "<DataList></DataList>", out errors);

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.DataListID).Returns(dataListID);

            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();

            var sa = CreateServiceAction();
            var container = new WebServiceContainerMock(sa, dataObj.Object, workspace.Object, esbChannel.Object)
            {
                WebRequestRespsonse = WebServiceResponseXml.ToString()
            };
            return container;
        }

        #endregion

        #region CreateServiceAction

        static ServiceAction CreateServiceAction()
        {
            var graph = DynamicObjectHelper.GenerateObjectGraphFromString(WebServiceXml.ToString());

            var ds = (DynamicService)graph[0];
            var sa = ds.Actions[0];
            sa.Source = new Source { ResourceDefinition = WebSourceXml.ToString() };
            return sa;
        }

        #endregion

    }
}
