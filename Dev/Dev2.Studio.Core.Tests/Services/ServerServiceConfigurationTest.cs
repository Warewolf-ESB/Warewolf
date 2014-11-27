
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Services;
using Dev2.Studio.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Services
{
    /// <summary>
    /// Summary description for ServerServiceConfigurationTest
    /// </summary>
    [TestClass]
    public class ServerServiceConfigurationTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        // ReSharper disable InconsistentNaming
        
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_PromptUserToStartService")]
        [ExpectedException(typeof(Exception))]
        public void ServerServiceConfiguration_PromptUserToStartService_WhenNullServiceConfiguration_ExpectException()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = new ServerServiceConfiguration(null);

            //------------Execute Test---------------------------
            serverServiceConfiguration.PromptUserToStartService();

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_PromptUserToStartService")]
        [ExpectedException(typeof(Exception))]
        public void ServerServiceConfiguration_PromptUserToStartService_WhenServiceNotRunningAndNullPopupController_ExpectException()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = CreateServerServiceConfiguration(false);

            //------------Execute Test---------------------------
            serverServiceConfiguration.PromptUserToStartService();

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_IsServiceRunning")]
        public void ServerServiceConfiguration_IsServiceRunning_WhenNotRunning_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = CreateServerServiceConfiguration(false);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.IsServiceRunning();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_IsServiceRunning")]
        public void ServerServiceConfiguration_IsServiceRunning_WhenRunning_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = CreateServerServiceConfiguration(true);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.IsServiceRunning();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);

        }
        

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_PromptUserToStartService")]
        public void ServerServiceConfiguration_PromptUserToStartService_WhenServiceRunning_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = CreateServerServiceConfiguration(true);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.PromptUserToStartService();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_PromptUserToStartService")]
        public void ServerServiceConfiguration_PromptUserToStartService_WhenServiceNotRunningAndNoneSelected_ExpectFalse()
        {
            //------------Setup for test--------------------------
            Mock<IPopupController> controller;
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, MessageBoxResult.None, out controller);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.PromptUserToStartService();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            controller.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_PromptUserToStartService")]
        public void ServerServiceConfiguration_PromptUserToStartService_WhenServiceNotRunningAndNoSelected_ExpectFalse()
        {
            //------------Setup for test--------------------------
            Mock<IPopupController> controller;
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, MessageBoxResult.No, out controller);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.PromptUserToStartService();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            controller.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_PromptUserToStartService")]
        public void ServerServiceConfiguration_PromptUserToStartService_WhenServiceNotRunningAndCancelSelected_ExpectFalse()
        {
            //------------Setup for test--------------------------
            Mock<IPopupController> controller;
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, MessageBoxResult.Cancel, out controller);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.PromptUserToStartService();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            controller.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_PromptUserToStartService")]
        public void ServerServiceConfiguration_PromptUserToStartService_WhenServiceNotRunningAndOkSelected_ExpectTrue()
        {
            //------------Setup for test--------------------------
            Mock<IPopupController> controller;
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, MessageBoxResult.OK, out controller);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.PromptUserToStartService();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            controller.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_PromptUserToStartService")]
        public void ServerServiceConfiguration_PromptUserToStartService_WhenServiceNotRunningAndYesSelected_ExpectTrue()
        {
            //------------Setup for test--------------------------
            Mock<IPopupController> controller;
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, MessageBoxResult.Yes, out controller);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.PromptUserToStartService();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            controller.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_StartService")]
        [ExpectedException(typeof(Exception))]
        public void ServerServiceConfiguration_StartService_WhenServiceConfigurationNull_ExpectException()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = new ServerServiceConfiguration(null);

            //------------Execute Test---------------------------
            serverServiceConfiguration.StartService();

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_StartService")]
        public void ServerServiceConfiguration_StartService_WhenServiceRunning_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = CreateServerServiceConfiguration(true);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.StartService();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_StartService")]
        public void ServerServiceConfiguration_StartService_WhenServiceNotRunning_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, true);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.StartService();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_StartService")]
        public void ServerServiceConfiguration_StartService_WhenServiceNotRunningAndProblemsStarting_ExpectFalse()
        {
            //------------Setup for test--------------------------
            Mock<IPopupController> controller;
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, false, true, out controller);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.StartService();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            controller.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_StartService")]
        public void ServerServiceConfiguration_StartService_WhenServiceNotRunningAndDoesNotExist_ExpectFalse()
        {
            //------------Setup for test--------------------------
            Mock<IPopupController> controller;
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, false, false, out controller);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.StartService();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            controller.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_DoesServiceExist")]
        public void ServerServiceConfiguration_DoesServiceExist_WhenServiceExist_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, false);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.DoesServiceExist();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerServiceConfiguration_DoesServiceExist")]
        public void ServerServiceConfiguration_DoesServiceExist_WhenServiceDoesNotExist_ExpectFalse()
        {
            //------------Setup for test--------------------------
            Mock<IPopupController> controller;
            var serverServiceConfiguration = CreateServerServiceConfiguration(false, false, false, out controller);

            //------------Execute Test---------------------------
            var result = serverServiceConfiguration.DoesServiceExist();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            controller.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
        }

        #region Helpers

        static ServerServiceConfiguration CreateServerServiceConfiguration(bool isServiceRunning)
        {
            Mock<IWindowsServiceManager> serviceManager = new Mock<IWindowsServiceManager>();
            serviceManager.Setup(sm => sm.IsRunning()).Returns(isServiceRunning);

            var serverServiceConfiguration = new ServerServiceConfiguration(serviceManager.Object);
            return serverServiceConfiguration;
        }

        static ServerServiceConfiguration CreateServerServiceConfiguration(bool isServiceRunning, bool startServiceResult)
        {
            Mock<IWindowsServiceManager> serviceManager = new Mock<IWindowsServiceManager>();
            serviceManager.Setup(sm => sm.IsRunning()).Returns(isServiceRunning);
            serviceManager.Setup(sm => sm.Start()).Returns(startServiceResult);
            serviceManager.Setup(sm => sm.Exists()).Returns(true);

            var serverServiceConfiguration = new ServerServiceConfiguration(serviceManager.Object);
            return serverServiceConfiguration;
        }

        static ServerServiceConfiguration CreateServerServiceConfiguration(bool isServiceRunning, bool startServiceResult, bool serviceExist, out Mock<IPopupController> ctrl)
        {
            Mock<IWindowsServiceManager> serviceManager = new Mock<IWindowsServiceManager>();
            serviceManager.Setup(sm => sm.IsRunning()).Returns(isServiceRunning);
            serviceManager.Setup(sm => sm.Start()).Returns(startServiceResult);
            serviceManager.Setup(sm => sm.Exists()).Returns(serviceExist);

            Mock<IPopupController> controller = new Mock<IPopupController>();
            controller.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Verifiable();

            // set for out arg
            ctrl = controller;

            var serverServiceConfiguration = new ServerServiceConfiguration(serviceManager.Object, controller.Object);
            return serverServiceConfiguration;
        }

        static ServerServiceConfiguration CreateServerServiceConfiguration(bool isServiceRunning, MessageBoxResult promptResult, out Mock<IPopupController> ctrl)
        {
            Mock<IWindowsServiceManager> serviceManager = new Mock<IWindowsServiceManager>();
            serviceManager.Setup(sm => sm.IsRunning()).Returns(isServiceRunning);

            Mock<IPopupController> controller = new Mock<IPopupController>();
            controller.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Verifiable();
            controller.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Returns(promptResult);

            ctrl = controller;

            var serverServiceConfiguration = new ServerServiceConfiguration(serviceManager.Object, controller.Object);

            return serverServiceConfiguration;

        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}
