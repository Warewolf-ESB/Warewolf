using System;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ESB
{
    // BUG 9619 - 2013.06.05 - TWR - Created
    [TestClass]
    public class PluginServiceContainerTests
    {
        #region Execute

        [TestMethod]
        [Ignore]
        public void PluginServiceContainerExecuteWithValidServiceHavingInputsExpectedExecutesService()
        {
            var container = CreatePluginServiceContainer(true);

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);
            var compiler = DataListFactory.CreateDataListCompiler();

            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);

            var resultXml = XElement.Parse(result);

            foreach (var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if (!actualName.StartsWith("Dev2System"))
                {
                    switch (actualName)
                    {
                        case "text":
                            Assert.AreEqual("hello", actualNode.Value);
                            break;
                        case "echo":
                            Assert.AreEqual("", actualNode.Value);
                            break;
                        default:
                            Assert.Fail("Invalid result");
                            break;
                    }
                }
            }
        }

        [TestMethod]
        [Ignore]
        public void PluginServiceContainerExecuteWithValidServiceHavingNoInputsExpectedExecutesService()
        {
            var container = CreatePluginServiceContainer(false);

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);
            var compiler = DataListFactory.CreateDataListCompiler();

            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);

            var resultXml = XElement.Parse(result);

            foreach (var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if (!actualName.StartsWith("Dev2System"))
                {
                    switch (actualName)
                    {
                        case "text":
                            Assert.AreEqual("hello", actualNode.Value);
                            break;
                        case "echo":
                            Assert.AreEqual("", actualNode.Value);
                            break;
                        default:
                            Assert.Fail("Invalid result");
                            break;
                    }
                }
            }
        }

        #endregion

        #region CreatePluginServiceContainer

        static PluginServiceContainer CreatePluginServiceContainer(bool withParameters)
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                "<DataList><text>hello</text><echo></echo></DataList>",
                "<DataList><text></text><echo></echo></DataList>", out errors);

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.DataListID).Returns(dataListID);

            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();

            var sa = CreateServiceAction(withParameters);
            var container = new PluginServiceContainer(sa, dataObj.Object, workspace.Object, esbChannel.Object);
            return container;
        }

        #endregion

        #region CreateServiceAction

        static ServiceAction CreateServiceAction(bool withParameters)
        {
            var type = typeof(DummyClassForPluginTest);
            var assembly = type.Assembly;
            var source = new PluginSource
            {
                AssemblyLocation = assembly.Location,
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dummy",
                ResourceType = ResourceType.PluginSource,
                ResourcePath = "Test",
            };

            var serviceXml = XmlResource.Fetch("PluginService");
            var service = new PluginService(serviceXml) { Source = source, Namespace = "DummyNamespaceForTest.DummyClassForPluginTest" };
            if (!withParameters)
            {
                service.Method.Name = "NoEcho";
                service.Method.Parameters.Clear();
            }

            serviceXml = service.ToXml();
            var graph = DynamicObjectHelper.GenerateObjectGraphFromString(serviceXml.ToString());

            var ds = (DynamicService)graph[0];
            var sa = ds.Actions[0];
            sa.Source = new Source { ResourceDefinition = service.Source.ToXml().ToString() };
            return sa;
        }

        #endregion

    }
}
