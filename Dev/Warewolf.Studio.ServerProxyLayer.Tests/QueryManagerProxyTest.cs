
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Data.Binary_Objects;
// ReSharper disable AccessToForEachVariableInClosure
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedVariable

namespace Warewolf.Studio.ServerProxyLayer.Test
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
            PrivateObject p = new PrivateObject(queryManagerProxy);
            Assert.IsNotNull(p.GetField("Connection"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources_ShowServerDisconnected")]
        public void QueryManagerProxy_Fetch_DBSources_ShowServerDisconnected()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            ErrorRunTest("FetchDbSources", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count, 0), a => a.FetchDbSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        public void QueryManagerProxy_Fetch_DBSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchDbSources", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count, 0), a => a.FetchDbSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchDependantsFromServerService")]
        public void QueryManagerProxy_FetchDependantsFromServerService()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            var msg = new ExecuteMessage() { HasError = false, Message = res };
            var id = Guid.NewGuid();
            RunTestStringArgs("FindDependencyService", msg, new List<Tuple<string, object>> { new Tuple<string, object>("ResourceId", id) }, a => Assert.AreEqual(a, msg), a => a.FetchDependencies(id));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchDependantsFromServerService")]
        public void QueryManagerProxy_FetchDependenciesFromServerService()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            var msg = new ExecuteMessage() { HasError = false, Message = res };
            var id = Guid.NewGuid();
            RunTestStringArgs("FindDependencyService", msg, new List<Tuple<string, object>> { new Tuple<string, object>("ResourceId", id) }, a => Assert.AreEqual(a, msg), a => a.FetchDependencies(id));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        public void QueryManagerProxy_GetComputerNamesService()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("GetComputerNamesService", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count, 0), a => a.GetComputerNames());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_GetComputerNamesService_Error()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("GetComputerNamesService", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count, 0), a => a.GetComputerNames());
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchFiles")]
        public void QueryManagerProxy_FetchFiles()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetFiles", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("fileListing", new FileListing()) }, a => Assert.AreEqual(a.Count, 0), a => a.FetchFiles(new FileListing()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchFiles")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_FetchFiles_error()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetFiles", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("fileListing", new FileListing()) }, a => Assert.AreEqual(a.Count, 0), a => a.FetchFiles(new FileListing()));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchFiles")]
        public void QueryManagerProxy_FetchFilesroot()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetFiles", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>> { }, a => Assert.AreEqual(a.Count, 0), a => a.FetchFiles());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchFiles")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_FetchFilesroot_error()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetFiles", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>> { }, a => Assert.AreEqual(a.Count, 0), a => a.FetchFiles());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchNamespaces")]
        public void QueryManagerProxy_FetchNamespaces()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchPluginNameSpaces", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new PluginSourceDefinition()) }, a => Assert.AreEqual(a.Count, 0), a => a.FetchNamespaces(new PluginSourceDefinition()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_FetchConstructors")]
        public void QueryManagerProxy_FetchConstructors()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginConstructor>());
            var ns = new Mock<INamespaceItem>();
            RunTest("FetchPluginConstructors", new ExecuteMessage()
            {
                HasError = false,
                Message = res
            }
            , new List<Tuple<string, object>>
            {
                new Tuple<string, object>("source", new PluginSourceDefinition())
            }, a => Assert.AreEqual(a.Count, 0), a => a.PluginConstructors(new PluginSourceDefinition(), ns.Object));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_FetchConstructors")]
        public void QueryManagerProxy_FetchConstructors_GivenEnvHasObjectVariablesAddsvariables()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginConstructor>());
            var aggr = new Mock<IEventAggregator>();
            var dataListViewModel = new DataListViewModel(aggr.Object);
            dataListViewModel.ComplexObjectCollection.Add(new ComplexObjectItemModel("Name", null, enDev2ColumnArgumentDirection.Both));
            DataListSingleton.SetDataList(dataListViewModel);
            var ns = new Mock<INamespaceItem>();
            RunTest("FetchPluginConstructors", new ExecuteMessage()
            {
                HasError = false,
                Message = res
            }
            , new List<Tuple<string, object>>
            {
                new Tuple<string, object>("source", new PluginSourceDefinition())
            }, a => Assert.AreEqual(a.Count, 1), a => a.PluginConstructors(new PluginSourceDefinition(), ns.Object));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchNamespaces")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_FetchNamespaces_error()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchPluginNameSpaces", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new PluginSourceDefinition()) }, a => Assert.AreEqual(a.Count, 0), a => a.FetchNamespaces(new PluginSourceDefinition()));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_GetDllListings")]
        public void QueryManagerProxy_Fetch_GetDllListings()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetDllListingsService", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("currentDllListing", new FileListing()) }, a => Assert.AreEqual(a.Count, 0), a => a.GetDllListings(new FileListing()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_GetDllListings")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_GetDllListings_Error()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetDllListingsService", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("currentDllListing", new FileListing()) }, a => Assert.AreEqual(a.Count, 0), a => a.GetDllListings(new FileListing()));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_Fetch_GetComDllListings")]
        public void QueryManagerProxy_Fetch_GetCOMDllListings()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetComDllListingsService", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("currentDllListing", new FileListing()) }
            , a => Assert.AreEqual(a.Count, 0), a => a.GetComDllListings(new FileListing()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_Fetch_GetComDllListings")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_GetComDllListings_Error()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTestStringArgs("GetComDllListingsService", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("currentDllListing", new FileListing()) }
            , a => Assert.AreEqual(a.Count, 0), a => a.GetComDllListings(new FileListing()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBActions")]
        public void QueryManagerProxy_Fetch_DBActions()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchDbActions", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new DbSourceDefinition()) }, a => Assert.AreEqual(a.Count, 0), a => a.FetchDbActions(new Mock<IDbSource>().Object));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBActions")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_DBActionsError()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchDbActions", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("source", new DbSourceDefinition()) }, a => Assert.AreEqual(a.Count, 0), a => a.FetchDbActions(new Mock<IDbSource>().Object));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_DBSources_HasError()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchDbSources", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count, 0), a => a.FetchDbSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_WebSources")]
        public void QueryManagerProxy_Fetch_WebSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchWebServiceSources", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchWebServiceSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_WebSources")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_WebSourcesError()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchWebServiceSources", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchWebServiceSources());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_ResourceXML")]
        public void QueryManagerProxy_Fetch_ResourceXML()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginSource>());
            RunTestStringArgs("FetchResourceDefinitionService", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>> { new Tuple<string, object>("ResourceID", Guid.NewGuid()) }, a => Assert.IsNotNull(a), a => a.FetchResourceXaml(Guid.NewGuid()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        public void QueryManagerProxy_Fetch_PlugintSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginSource>());
            RunTest("FetchPluginSources", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchPluginSources());
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_Ctor")]
        public void QueryManagerProxy_Fetch_ComPlugintSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IComPluginSource>());
            RunTest("FetchComPluginSources", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchComPluginSources());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchPlugin")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_PluginSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IPluginSource>());
            RunTest("FetchPluginSources", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchPluginSources());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("QueryManagerProxy_FetchComPlugin")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_ComPluginSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IComPluginSource>());
            RunTest("FetchComPluginSources", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchComPluginSources());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        public void QueryManagerProxy_Fetch_RabbitSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest("FetchRabbitMQServiceSources", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchRabbitMQServiceSources());
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        [ExpectedException(typeof(WarewolfSupportServiceException))]
        public void QueryManagerProxy_Fetch_RabbitSourcesError()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            RunTest<IEnumerable<IRabbitMQServiceSourceDefinition>>("FetchRabbitMQServiceSources", new ExecuteMessage() { HasError = true, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchRabbitMQServiceSources());
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
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            controller.Setup(a => a.ExecuteCommand<ExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(message);
            //------------Execute Test---------------------------
            T res = action(queryManagerProxy);
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
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            controller.Setup(a => a.ExecuteCommand<ExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(message);
            //------------Execute Test---------------------------
            T res = action(queryManagerProxy);
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
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            controller.Setup(a => a.ExecuteCommand<ExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(message);
            //------------Execute Test---------------------------
            T res = action(queryManagerProxy);
            //------------Assert Results-------------------------

            foreach (var tuple in args)
            {

                controller.Verify(a => a.AddPayloadArgument(tuple.Item1, It.IsAny<string>()));
            }
            resultAction(res);
        }

    }
}
