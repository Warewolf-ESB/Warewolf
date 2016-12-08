using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.ServerProxyLayer;

// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class StudioServerProxyTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudioServerProxy_Constructor_WhenNullComControllerFactory_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new StudioServerProxy(null, new Mock<IEnvironmentConnection>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudioServerProxy_Constructor_WhenNullEnvironmentConnection_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_Constructor")]
        public void StudioServerProxy_Constructor_WhenValidArgs_ShouldConstuct()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(studioServerProxy);
            Assert.IsNotNull(studioServerProxy.QueryManagerProxy);
            Assert.IsNotNull(studioServerProxy.UpdateManagerProxy);
            Assert.IsNotNull(studioServerProxy.VersionManager);
            Assert.IsNotNull(studioServerProxy.AdminManagerProxy);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_LoadExplorer")]
        public void StudioServerProxy_LoadExplorer_Called_ShouldLoadExplorerItems()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            mockQueryManager.Setup(manager => manager.Load(false)).Returns(Task.FromResult(new Mock<IExplorerItem>().Object));
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            //------------Execute Test---------------------------
            var item = studioServerProxy.LoadExplorer().Result;
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_LoadExplorer")]
        public void StudioServerProxy_LoadExplorerDuplicates_Called_ShouldLoadDuplicates()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            mockQueryManager.Setup(manager => manager.LoadDuplicates()).Returns(Task.FromResult(new List<string>()));
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            //------------Execute Test---------------------------
            var item = studioServerProxy.LoadExplorerDuplicates().Result;
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_LoadExplorer")]
        public void StudioServerProxy_Delete_WhenResource_ShouldDeleteResource()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockUpdateManagerProxy = new Mock<IExplorerUpdateManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockUpdateManagerProxy.Setup(manager => manager.DeleteResource(It.IsAny<Guid>())).Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.UpdateManagerProxy = mockUpdateManagerProxy.Object;
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("WorkflowService");
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(),It.IsAny<string>(),MessageBoxButton.OK, MessageBoxImage.Error, "false",true,true,false,false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var item = studioServerProxy.Delete(mockExplorerItemModel.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            mockUpdateManagerProxy.Verify(manager => manager.DeleteResource(It.IsAny<Guid>()),Times.Once);
            mockQueryManager.Verify(manager => manager.FetchDependants(It.IsAny<Guid>()),Times.Once);
            Assert.IsTrue(item.IsDeleted);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_LoadExplorer")]
        public void StudioServerProxy_Delete_WhenVersion_ShouldDeleteVersion()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(),It.IsAny<string>(),It.IsAny<string>())).Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Version");
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(),It.IsAny<string>(),MessageBoxButton.OK, MessageBoxImage.Error, "false",true,true,false,false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var item = studioServerProxy.Delete(mockExplorerItemModel.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            mockVersionManager.Verify(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()),Times.Once);
            mockQueryManager.Verify(manager => manager.FetchDependants(It.IsAny<Guid>()),Times.Never);
            Assert.IsTrue(item.IsDeleted);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioServerProxy_VersionManager")]
        public void StudioServerProxy_VersionManager_GetVersions_ServerDown_ShowPopup()
        {
            //------------Setup for test--------------------------
            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(a => a.DisplayName).Returns("localhost");
            var versionManagerProxy = new VersionManagerProxy(new CommunicationControllerFactory(), environmentConnection.Object);
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);

            var versions = versionManagerProxy.GetVersions(It.IsAny<Guid>());

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(versions);
        }
    }
}