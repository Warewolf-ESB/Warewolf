using System;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Communication;
using Dev2.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ServerProxyLayer.Test
{
    [TestClass]
    public class UpdateProxyTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_RabbitMqSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object,env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveRabbitMQServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage() { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveRabbitMQServiceSource(new Mock<IRabbitMQServiceSourceDefinition>().Object,Guid.NewGuid() );
            //------------Assert Results-------------------------

           controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_RabbitMqSource_Expectexception()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveRabbitMQServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage() { HasError = true, Message = new StringBuilder( "bob")});
            //------------Execute Test---------------------------

            updateProxyTest.SaveRabbitMQServiceSource(new Mock<IRabbitMQServiceSourceDefinition>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------

            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));

        }
    }
}
