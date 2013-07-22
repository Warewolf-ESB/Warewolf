using System;
using System.IO;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Execution;
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
        static string _testDir;
        static string _pluginAssemblyPath;

        enum ServiceActionType
        {
            WithInputs,
            WithoutInputs,
            ReturnsClass
        }

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
            _pluginAssemblyPath = Path.Combine(_testDir, "Dev2.PluginTester.dll");
        }

        #endregion

        [TestMethod]
        public void PluginServiceContainer_UnitTest_ExecuteWhereHasPluginServiceExecution_Guid()
        {
            //------------Setup for test--------------------------
            var mockServiceExecution = new Mock<IServiceExecution>();
            ErrorResultTO errors;
            Guid expected = Guid.NewGuid();
            mockServiceExecution.Setup(execution => execution.Execute(out errors)).Returns(expected);
            PluginServiceContainer pluginServiceContainer = new PluginServiceContainer(mockServiceExecution.Object);
            //------------Execute Test---------------------------
            Guid actual = pluginServiceContainer.Execute(out errors);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected,actual,"Execute should return the Guid from the service execution");
        }

        #region HandlesOutputFormatting

        [TestMethod]
        [Ignore]
        public void PluginServiceContainerHandlesOutputFormattingExpectedReturnsFalse()
        {
            var sa = CreateServiceAction(ServiceActionType.WithoutInputs);
            var container = new PluginServiceContainer(sa, null, null, null);
            //Assert.IsFalse(container.HandlesOutputFormatting);
        }

        #endregion

        #region Execute

        [TestMethod]
        [Ignore]
        public void PluginServiceContainerExecuteWithValidServiceHavingInputsExpectedExecutesService()
        {
            var container = CreatePluginServiceContainer(ServiceActionType.WithInputs);

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);
            var compiler = DataListFactory.CreateDataListCompiler();

            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);

            var resultXml = XElement.Parse(result);

            foreach(var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if(!actualName.StartsWith("Dev2System"))
                {
                    switch(actualName)
                    {
                        case "text":
                        case "reverb":
                            Assert.AreEqual("hello", actualNode.Value);
                            break;
                        case "hacked":
                            Assert.AreEqual("wtf", actualNode.Value);
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
            var container = CreatePluginServiceContainer(ServiceActionType.WithoutInputs);

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);
            var compiler = DataListFactory.CreateDataListCompiler();

            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);

            var resultXml = XElement.Parse(result);

            foreach(var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if(!actualName.StartsWith("Dev2System"))
                {
                    switch(actualName)
                    {
                        case "reverb":
                            Assert.AreEqual("None", actualNode.Value);
                            break;
                        case "hacked":
                            Assert.AreEqual("wtf", actualNode.Value);
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
        public void PluginServiceContainerExecuteWithValidServiceReturningClassExpectedExecutesService()
        {
            var container = CreatePluginServiceContainer(ServiceActionType.ReturnsClass);

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);
            var compiler = DataListFactory.CreateDataListCompiler();

            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);

            var resultXml = XElement.Parse(result);

            foreach(var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if(!actualName.StartsWith("Dev2System"))
                {
                    switch(actualName)
                    {
                        case "Name":
                        case "MountainName":
                            Assert.AreEqual("berg", actualNode.Value);
                            break;
                        case "Height":
                        case "MountainHeight":
                            Assert.AreEqual("1000", actualNode.Value);
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

        static PluginServiceContainer CreatePluginServiceContainer(ServiceActionType actionType)
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var dataListID = Guid.Empty;
            switch(actionType)
            {
                case ServiceActionType.WithInputs:
                    dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                        "<DataList><text>hello</text><reverb></reverb><hacked></hacked></DataList>", "<DataList><text></text><reverb></reverb><hacked></hacked></DataList>", out errors);
                    break;
                case ServiceActionType.WithoutInputs:
                    dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                        "<DataList><reverb></reverb><hacked></hacked></DataList>", "<DataList><reverb></reverb><hacked></hacked></DataList>", out errors);
                    break;
                case ServiceActionType.ReturnsClass:
                    dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                        "<DataList><Name>berg</Name><Height>1000</Height><MountainName></MountainName><MountainHeight></MountainHeight></DataList>",
                        "<DataList><Name></Name><Height></Height><MountainName></MountainName><MountainHeight></MountainHeight></DataList>", out errors);
                    break;
            }

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.DataListID).Returns(dataListID);

            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();

            var sa = CreateServiceAction(actionType);
            var container = new PluginServiceContainer(sa, dataObj.Object, workspace.Object, esbChannel.Object);
            return container;
        }

        #endregion

        #region CreateServiceAction

        static ServiceAction CreateServiceAction(ServiceActionType actionType)
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

            XElement serviceXml;
            var service = new PluginService();
            switch(actionType)
            {
                case ServiceActionType.WithInputs:
                    serviceXml = XmlResource.Fetch("PluginService");
                    service = new PluginService(serviceXml)
                    {
                        Source = source,
                        Namespace = "DummyNamespaceForTest.DummyClassForPluginTest",
                        Method = { Name = "Echo" }
                    };
                    break;
                case ServiceActionType.WithoutInputs:
                    serviceXml = XmlResource.Fetch("PluginService");
                    service = new PluginService(serviceXml)
                    {
                        Source = source,
                        Namespace = "DummyNamespaceForTest.DummyClassForPluginTest",
                        Method = { Name = "NoEcho" }
                    };
                    service.Method.Parameters.Clear();
                    break;
                case ServiceActionType.ReturnsClass:
                    serviceXml = XmlResource.Fetch("PluginServiceTester");
                    service = new PluginService(serviceXml)
                    {
                        Source = new PluginSource
                        {
                            AssemblyLocation = _pluginAssemblyPath,
                            AssemblyName = "Dev2.PluginTester",
                            ResourceID = Guid.NewGuid(),
                            ResourceName = "PluginTester",
                            ResourceType = ResourceType.PluginSource,
                            ResourcePath = "Test",
                        }
                    };
                    break;
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
