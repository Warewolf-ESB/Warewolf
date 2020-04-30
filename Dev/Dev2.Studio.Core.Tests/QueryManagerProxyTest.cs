using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Windows;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Explorer;
using System.Threading;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class QueryManagerProxyTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        public void QueryManagerProxy_Ctor_ValidValues_ExpectCreated()
        {
            //------------Setup for test--------------------------
            var queryManagerProxy = new QueryManagerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(queryManagerProxy.CommunicationControllerFactory);
            var p = new PrivateObject(queryManagerProxy);
            Assert.IsNotNull(p.GetField("Connection"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources_ShowServerDisconnected")]
        public void QueryManagerProxy_Fetch_DBSources_ShowServerDisconnected()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            ErrorRunTest("FetchDbSources", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count), a => a.FetchDbSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        public void QueryManagerProxy_Fetch_DBSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchDbSources", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count), a => a.FetchDbSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchDependantsFromServerService")]
        public void QueryManagerProxy_FetchDependantsFromServerService()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            var msg = new ExecuteMessage { HasError = false, Message = res };
            var id = Guid.NewGuid();
            RunTestStringArgs("FindDependencyService", msg, new List<Tuple<string, object>> { new Tuple<string, object>("ResourceId", id) }, a => Assert.AreEqual(a, msg), a => a.FetchDependencies(id));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchDependantsFromServerService")]
        public void QueryManagerProxy_FetchDependenciesFromServerService()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            var msg = new ExecuteMessage { HasError = false, Message = res };
            var id = Guid.NewGuid();
            RunTestStringArgs("FindDependencyService", msg, new List<Tuple<string, object>> { new Tuple<string, object>("ResourceId", id) }, a => Assert.AreEqual(a, msg), a => a.FetchDependencies(id));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        public void QueryManagerProxy_GetComputerNamesService()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("GetComputerNamesService", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count), a => a.GetComputerNames());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_GetComputerNamesService_Error()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("GetComputerNamesService", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count), a => a.GetComputerNames());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchFiles")]
        public void QueryManagerProxy_FetchFiles()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetFiles", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("fileListing", new FileListing()) }, a => Assert.AreEqual(0, a.Count), a => a.FetchFiles(new FileListing()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchFiles")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_FetchFiles_error()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetFiles", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("fileListing", new FileListing()) }, a => Assert.AreEqual(0, a.Count), a => a.FetchFiles(new FileListing()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchFiles")]
        public void QueryManagerProxy_FetchFilesroot()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetFiles", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count), a => a.FetchFiles());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchFiles")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_FetchFilesroot_error()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetFiles", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count), a => a.FetchFiles());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchNamespaces")]
        public void QueryManagerProxy_FetchNamespaces()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchPluginNameSpaces", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new PluginSourceDefinition()) }, a => Assert.AreEqual(0, a.Count), a => a.FetchNamespaces(new PluginSourceDefinition()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_FetchConstructors")]
        public void QueryManagerProxy_FetchConstructors()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginConstructor>());
            var ns = new Mock<INamespaceItem>();
            RunTest("FetchPluginConstructors", new ExecuteMessage
            {
                HasError = false,
                Message = res
            }
            , new List<Tuple<string, object>>
            {
                new Tuple<string, object>("source", new PluginSourceDefinition())
            }, a => Assert.AreEqual(0, a.Count), a => a.PluginConstructors(new PluginSourceDefinition(), ns.Object));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_FetchConstructors")]
        public void QueryManagerProxy_FetchConstructors_GivenEnvHasObjectVariablesAddsvariables()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginConstructor>());
            var aggr = new Mock<IEventAggregator>();
            var dataListViewModel = new DataListViewModel(aggr.Object);
            dataListViewModel.Add(new ComplexObjectItemModel("Name", null, enDev2ColumnArgumentDirection.Both));
            DataListSingleton.SetDataList(dataListViewModel);
            var ns = new Mock<INamespaceItem>();
            RunTest("FetchPluginConstructors", new ExecuteMessage
            {
                HasError = false,
                Message = res
            }
            , new List<Tuple<string, object>>
            {
                new Tuple<string, object>("source", new PluginSourceDefinition())
            }, a => Assert.AreEqual(1, a.Count), a => a.PluginConstructors(new PluginSourceDefinition(), ns.Object));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchNamespaces")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_FetchNamespaces_error()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchPluginNameSpaces", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new PluginSourceDefinition()) }, a => Assert.AreEqual(0, a.Count), a => a.FetchNamespaces(new PluginSourceDefinition()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_GetDllListings")]
        public void QueryManagerProxy_Fetch_GetDllListings()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetDllListingsService", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("currentDllListing", new FileListing()) }, a => Assert.AreEqual(0, a.Count), a => a.GetDllListings(new FileListing()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_GetDllListings")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_GetDllListings_Error()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetDllListingsService", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("currentDllListing", new FileListing()) }, a => Assert.AreEqual(0, a.Count), a => a.GetDllListings(new FileListing()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_Fetch_GetComDllListings")]
        public void QueryManagerProxy_Fetch_GetCOMDllListings()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetComDllListingsService", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("currentDllListing", new FileListing()) }
            , a => Assert.AreEqual(0, a.Count), a => a.GetComDllListings(new FileListing()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_Fetch_GetComDllListings")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_GetComDllListings_Error()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetComDllListingsService", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("currentDllListing", new FileListing()) }
            , a => Assert.AreEqual(0, a.Count), a => a.GetComDllListings(new FileListing()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBActions")]
        public void QueryManagerProxy_Fetch_DBActions()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchDbActions", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new DbSourceDefinition()) }, a => Assert.AreEqual(0, a.Count), a => a.FetchDbActions(new Mock<IDbSource>().Object));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBActions")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_DBActionsError()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchDbActions", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new DbSourceDefinition()) }, a => Assert.AreEqual(0, a.Count), a => a.FetchDbActions(new Mock<IDbSource>().Object));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_DBSources_HasError()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchDbSources", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count), a => a.FetchDbSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_WebSources")]
        public void QueryManagerProxy_Fetch_WebSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchWebServiceSources", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchWebServiceSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_WebSources")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_WebSourcesError()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchWebServiceSources", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchWebServiceSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_ResourceXML")]
        public void QueryManagerProxy_Fetch_ResourceXML()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginSource>());
            RunTestStringArgs("FetchResourceDefinitionService", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("ResourceID", Guid.NewGuid()) }, a => Assert.IsNotNull(a), a => a.FetchResourceXaml(Guid.NewGuid()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        public void QueryManagerProxy_Fetch_PlugintSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginSource>());
            RunTest("FetchPluginSources", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchPluginSources());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_Ctor")]
        public void QueryManagerProxy_Fetch_ComPlugintSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IComPluginSource>());
            RunTest("FetchComPluginSources", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchComPluginSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchPlugin")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_PluginSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginSource>());
            RunTest("FetchPluginSources", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchPluginSources());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_FetchComPlugin")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_ComPluginSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IComPluginSource>());
            RunTest("FetchComPluginSources", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchComPluginSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        public void QueryManagerProxy_Fetch_RabbitSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchRabbitMQServiceSources", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchRabbitMQServiceSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_RabbitSourcesError()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchRabbitMQServiceSources", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchRabbitMQServiceSources());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(QueryManagerProxy))]
        public void QueryManagerProxy_Fetch_FetchElasticsearchSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IElasticsearchSourceDefinition>());
            RunTest("FetchElasticsearchServiceSources", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchElasticsearchServiceSources());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QueryManagerProxy_FetchTools")]
        public void QueryManagerProxy_FetchTools()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IToolDescriptor>());
            RunTest("FetchToolsService", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchTools());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QueryManagerProxy_FetchNamespaces")]
        public void QueryManagerProxy_FetchComNamespaces()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IComPluginSource>());
            RunTest("FetchComPluginNameSpaces", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new ComPluginSourceDefinition()) }, a => Assert.AreEqual(0, a.Count), a => a.FetchNamespaces(new ComPluginSourceDefinition()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QueryManagerProxy_FetchNamespaces")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_FetchComNamespaces_error()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IComPluginSource>());
            RunTest("FetchComPluginNameSpaces", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new ComPluginSourceDefinition()) }, a => Assert.AreEqual(0, a.Count), a => a.FetchNamespaces(new ComPluginSourceDefinition()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QueryManagerProxy_FetchExchangeSources")]
        public void QueryManagerProxy_FetchExchangeSources()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IExchangeSource>());
            RunTest("FetchExchangeSources", new ExecuteMessage { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchExchangeSources());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QueryManagerProxy_FetchExchangeSources")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_FetchExchangeSources_error()
        {
            var ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IExchangeSource>());
            RunTest("FetchExchangeSources", new ExecuteMessage { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(0, a.Count()), a => a.FetchExchangeSources());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [DoNotParallelize]
        public void QueryManagerProxy_LoadExplorer_WhenLongerThan30Sec_ShouldLoadExplorerItemsShowPopup()
        {
            //------------Setup for test--------------------------
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            env.Setup(a => a.DisplayName).Returns("localhost");
            env.Setup(a => a.IsConnected).Returns(true);
            comms.Setup(a => a.CreateController("FetchExplorerItemsService")).Returns(controller.Object);
            var queryManagerProxy = new QueryManagerProxy(comms.Object, env.Object);
            controller.Setup(a => a.ExecuteCompressedCommandAsync<IExplorerItem>(env.Object, It.IsAny<Guid>())).Returns(Task.Delay(70000).ContinueWith(t => new Mock<IExplorerItem>().Object));
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popup => popup.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var item = queryManagerProxy.Load(false).Result;
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            mockPopupController.Verify(popup => popup.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false, false, false), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [DoNotParallelize]
        public void QueryManagerProxy_LoadExplorer_WhenLongerThan30Sec__Localhost_ShouldLoadExplorerItemsNotShowPopup()
        {
            //------------Setup for test--------------------------
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            env.Setup(a => a.DisplayName).Returns("localhost");
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(e => e.IsLocalHost).Returns(true);
            comms.Setup(a => a.CreateController("FetchExplorerItemsService")).Returns(controller.Object);
            var queryManagerProxy = new QueryManagerProxy(comms.Object, env.Object);
            controller.Setup(a => a.ExecuteCompressedCommandAsync<IExplorerItem>(env.Object, It.IsAny<Guid>())).Returns(Task.Delay(70000).ContinueWith(t => new Mock<IExplorerItem>().Object));
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popup => popup.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var item = queryManagerProxy.Load(false).Result;
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            mockPopupController.Verify(popup => popup.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false, false, false), Times.Never);

        }
        public void ErrorRunTest<T>(string svcName, ExecuteMessage message, IList<Tuple<string, Object>> args, Action<T> resultAction, Func<IQueryManager, T> action)
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            env.Setup(a => a.DisplayName).Returns("localhost");
            env.Setup(a => a.IsConnected).Returns(false);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController(svcName)).Returns(controller.Object);
            var queryManagerProxy = new QueryManagerProxy(comms.Object, env.Object);
            controller.Setup(a => a.ExecuteCommand<ExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(message);
            //------------Execute Test---------------------------
            var res = action(queryManagerProxy);
            //------------Assert Results-------------------------

            foreach (var tuple in args)
            {

                controller.Verify(a => a.AddPayloadArgument(tuple.Item1, It.IsAny<StringBuilder>()));
            }
            resultAction(res);
        }

        public void RunTest<T>(string svcName, ExecuteMessage message, IList<Tuple<string, Object>> args, Action<T> resultAction, Func<IQueryManager, T> action)
        {
            //------------Setup for test--------------------------
            CustomContainer.Register<IPopupController>(new PopupController());
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            env.Setup(a => a.DisplayName).Returns("localhost");
            env.Setup(a => a.IsConnected).Returns(true);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController(svcName)).Returns(controller.Object);
            var queryManagerProxy = new QueryManagerProxy(comms.Object, env.Object);
            controller.Setup(a => a.ExecuteCommand<ExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(message);
            //------------Execute Test---------------------------
            var res = action(queryManagerProxy);
            //------------Assert Results-------------------------

            foreach (var tuple in args)
            {

                controller.Verify(a => a.AddPayloadArgument(tuple.Item1, It.IsAny<StringBuilder>()));
            }
            resultAction(res);
        }

        public void RunTestStringArgs<T>(string svcName, ExecuteMessage message, IList<Tuple<string, Object>> args, Action<T> resultAction, Func<IQueryManager, T> action)
        {
            //------------Setup for test--------------------------
            CustomContainer.Register<IPopupController>(new PopupController());
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            env.Setup(a => a.DisplayName).Returns("localhost");
            env.Setup(a => a.IsConnected).Returns(true);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController(svcName)).Returns(controller.Object);
            var queryManagerProxy = new QueryManagerProxy(comms.Object, env.Object);
            controller.Setup(a => a.ExecuteCommand<ExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(message);
            //------------Execute Test---------------------------
            var res = action(queryManagerProxy);
            //------------Assert Results-------------------------

            foreach (var tuple in args)
            {

                controller.Verify(a => a.AddPayloadArgument(tuple.Item1, It.IsAny<string>()));
            }
            resultAction(res);
        }
    }
}