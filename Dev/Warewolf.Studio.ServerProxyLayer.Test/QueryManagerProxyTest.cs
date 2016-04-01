
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable AccessToForEachVariableInClosure

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
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_DBSources")]
        public void QueryManagerProxy_Fetch_DBSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new  List<IRabbitMQServiceSourceDefinition>());
            QueryManagerProxy_Ctor_FetchRabbitMqSources("FetchDbSources", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count, 0), a => a.FetchDbSources());
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Fetch_WebSources")]
        public void QueryManagerProxy_Fetch_WebSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            QueryManagerProxy_Ctor_FetchRabbitMqSources("FetchWebServiceSources", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchWebServiceSources());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        public void QueryManagerProxy_Fetch_RabbitSources()
        {
            var message = new ExecuteMessage();
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res = ser.SerializeToBuilder(new List<IRabbitMQServiceSourceDefinition>());
            QueryManagerProxy_Ctor_FetchRabbitMqSources<IEnumerable<IRabbitMQServiceSourceDefinition>>("FetchRabbitMQServiceSources", new ExecuteMessage() { HasError = false, Message = res }, new List<Tuple<string, object>>(), a => Assert.AreEqual(a.Count(), 0), a => a.FetchRabbitMQServiceSources());
        }



        public void QueryManagerProxy_Ctor_FetchRabbitMqSources<T>(string svcName, ExecuteMessage message, IList<Tuple<string, Object>> args, Action<T> resultAction, Func<IQueryManager,T> action)
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController(svcName)).Returns(controller.Object);
            var queryManagerProxy = new QueryManagerProxy(comms.Object, env.Object);
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            controller.Setup(a => a.ExecuteCommand<ExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(message);
            //------------Execute Test---------------------------
            T res = action(queryManagerProxy);
            //------------Assert Results-------------------------

            foreach(var tuple in args)
            {

                controller.Verify(a=>a.AddPayloadArgument(tuple.Item1,ser.SerializeToBuilder(tuple.Item2)));
            }
            resultAction(res);
        }

    }
}
