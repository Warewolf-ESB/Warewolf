/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Interfaces;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.ServerProxyLayer;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Deploy;

namespace Dev2.Core.Tests
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
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveRabbitMQServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveRabbitMQServiceSource(new Mock<IRabbitMQServiceSourceDefinition>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Test_RabbitMqSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestRabbitMQServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.TestRabbitMQServiceSource(new Mock<IRabbitMQServiceSourceDefinition>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_RabbitMqSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveRabbitMQServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveRabbitMQServiceSource(new Mock<IRabbitMQServiceSourceDefinition>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_RabbitMqSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestRabbitMQServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("") });
            //------------Execute Test---------------------------

            updateProxyTest.TestRabbitMQServiceSource(new Mock<IRabbitMQServiceSourceDefinition>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_ExchangeSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveExchangeServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveExchangeSource(new Mock<IExchangeSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Test_ExchangeSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestExchangeServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.TestExchangeServiceSource(new Mock<IExchangeSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_ExchangeSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveExchangeServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveExchangeSource(new Mock<IExchangeSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_ExchangeSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestExchangeServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("") });
            //------------Execute Test---------------------------

            updateProxyTest.TestExchangeServiceSource(new Mock<IExchangeSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_EmailServiceSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveEmailServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveEmailServiceSource(new Mock<IEmailServiceSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Test_EmailServiceSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestEmailServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.TestEmailServiceSource(new Mock<IEmailServiceSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_EmailServiceSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveEmailServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveEmailServiceSource(new Mock<IEmailServiceSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_EmailServiceSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestEmailServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.TestEmailServiceSource(new Mock<IEmailServiceSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_OAuthSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveOAuthSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveOAuthSource(new Mock<IOAuthSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_OAuthSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveOAuthSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveOAuthSource(new Mock<IOAuthSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_ComPluginSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveComPluginSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveComPluginSource(new Mock<IComPluginSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Test_ComPluginSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestComPluginService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.TestComPluginService(new Mock<IComPluginService>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_ComPluginSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveComPluginSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveComPluginSource(new Mock<IComPluginSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_ComPluginSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestComPluginService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.TestComPluginService(new Mock<IComPluginService>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_PluginSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SavePluginSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SavePluginSource(new Mock<IPluginSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Test_PluginSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestPluginService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.TestPluginService(new Mock<IPluginService>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_PluginSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SavePluginSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SavePluginSource(new Mock<IPluginSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_PluginSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestPluginService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.TestPluginService(new Mock<IPluginService>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_SharepointServer_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveSharepointServerService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveSharePointServiceSource(new Mock<ISharepointServerSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_SharepointServer_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveSharepointServerService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveSharePointServiceSource(new Mock<ISharepointServerSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_SharepointServer_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestSharepointServerService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.TestConnection(new Mock<ISharepointServerSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_WebserviceSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveWebserviceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveWebserviceSource(new Mock<IWebServiceSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_WebserviceSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveWebserviceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveWebserviceSource(new Mock<IWebServiceSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_RedisSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveRedisSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveRedisServiceSource(new Mock<IRedisServiceSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_RedisSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveRedisSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveRedisServiceSource(new Mock<IRedisServiceSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_DbSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveDbSourceService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveDbSource(new Mock<IDbSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Test_DbSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestDbSourceService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.TestDbConnection(new Mock<IDbSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_DbSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveDbSourceService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveDbSource(new Mock<IDbSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_DbSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestDbSourceService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.TestDbConnection(new Mock<IDbSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_ServerSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveServerSourceService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveServerSource(new Mock<IServerSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Test_ServerSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestConnectionService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.TestConnection(new Mock<IServerSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_ServerSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveServerSourceService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveServerSource(new Mock<IServerSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_ServerSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestConnectionService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.TestConnection(new Mock<IServerSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_WcfSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveWcfServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveWcfSource(new Mock<IWcfServerSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Test_WcfSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestWcfServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.TestWcfServiceSource(new Mock<IWcfServerSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_WcfSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveWcfServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveWcfSource(new Mock<IWcfServerSource>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxyTest_Test_WcfSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("TestWcfServiceSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.TestWcfServiceSource(new Mock<IWcfServerSource>().Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("UpdateProxyTest_Test")]
        public void UpdateProxyTest_Deploy()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("DirectDeploy")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<List<IDeployResult>>(env.Object, It.IsAny<Guid>())).Returns(new List<IDeployResult>());
            //------------Execute Test---------------------------

            var mockConnection = new Mock<IConnection>();
            mockConnection.Setup(con => con.Address).Returns("localhost");
            mockConnection.Setup(con => con.AuthenticationType).Returns(Runtime.ServiceModel.Data.AuthenticationType.Public);
            mockConnection.Setup(con => con.UserName).Returns("username");
            mockConnection.Setup(con => con.Password).Returns("password");

            updateProxyTest.Deploy(new List<Guid> { Guid.NewGuid() }, false, false, mockConnection.Object);
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<List<IDeployResult>>(env.Object, It.IsAny<Guid>()));
        }
        
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("UpdateProxyTest_Save")]
        public void UpdateProxyTest_Save_ElasticsearchSource_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveElasticsearchSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = false });
            //------------Execute Test---------------------------

            updateProxyTest.SaveElasticsearchServiceSource(new Mock<IElasticsearchSourceDefinition>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("UpdateProxyTest_Save")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxyTest_Save_ElasticsearchSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            var updateProxyTest = new UpdateProxy(comms.Object, env.Object);
            var controller = new Mock<ICommunicationController>();
            comms.Setup(a => a.CreateController("SaveElasticsearchSource")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>())).Returns(new ExecuteMessage { HasError = true, Message = new StringBuilder("bob") });
            //------------Execute Test---------------------------

            updateProxyTest.SaveElasticsearchServiceSource(new Mock<IElasticsearchSourceDefinition>().Object, Guid.NewGuid());
            //------------Assert Results-------------------------
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(env.Object, It.IsAny<Guid>()));
        }
    }
}
