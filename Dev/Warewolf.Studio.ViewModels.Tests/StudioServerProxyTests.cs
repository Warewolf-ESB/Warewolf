using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common;
using Dev2.Common.DependencyVisualization;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Explorer;
using System.Linq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class StudioServerProxyTests
    {
        [TestMethod]
        [Timeout(100)]
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
        [Timeout(100)]
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
        [Timeout(100)]
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
        [Timeout(250)]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_LoadExplorer")]
        public void StudioServerProxy_LoadExplorer_Called_ShouldLoadExplorerItems()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            mockQueryManager.Setup(manager => manager.Load(false, It.IsAny<IPopupController>())).Returns(Task.FromResult(new Mock<IExplorerItem>().Object));
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            //------------Execute Test---------------------------
            var item = studioServerProxy.LoadExplorer().Result;
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
        }

        [TestMethod]
        [Timeout(100)]
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
        [Timeout(2000)]
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
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "false", true, true, false, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var item = studioServerProxy.TryDelete(mockExplorerItemModel.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            mockUpdateManagerProxy.Verify(manager => manager.DeleteResource(It.IsAny<Guid>()), Times.Once);
            mockQueryManager.Verify(manager => manager.FetchDependants(It.IsAny<Guid>()), Times.Once);
            Assert.IsTrue(item.IsDeleted);
        }

        [TestMethod]
        [Timeout(2000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioServerProxy_LoadExplorer")]
        public void StudioServerProxy_Delete_WhenVersion_ShouldDeleteVersion()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;

            var mockExplorerItemModelParent = new Mock<IExplorerItemViewModel>();
            mockExplorerItemModelParent.Setup(model => model.ResourceType).Returns("Folder");
            mockExplorerItemModelParent.Setup(model => model.ResourcePath).Returns("folderPath");

            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Version");
            mockExplorerItemModel.Setup(model => model.Parent).Returns(mockExplorerItemModelParent.Object);
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "false", true, true, false, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var item = studioServerProxy.TryDelete(mockExplorerItemModel.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            mockVersionManager.Verify(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockQueryManager.Verify(manager => manager.FetchDependants(It.IsAny<Guid>()), Times.Never);
            Assert.IsTrue(item.IsDeleted);
        }

        [TestMethod]
        [Timeout(2000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_RenameFolder")]
        public void StudioServerProxy_Rename_WhenFolder_ShouldRenameFolder()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.RenameFolder(It.IsAny<string>(), It.IsAny<string>()));
            studioServerProxy.UpdateManagerProxy = updateManagerProxy.Object;
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemModel.SetupAllProperties();
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Folder");
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "false", true, true, false, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var item = studioServerProxy.Rename(mockExplorerItemModel.Object, It.IsAny<string>());
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            updateManagerProxy.Setup(manager => manager.RenameFolder(It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        [Timeout(2000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_Rename")]
        public void StudioServerProxy_Rename_WhenNotFolder_ShouldRenameResourceId()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.Rename(It.IsAny<Guid>(), It.IsAny<string>()));
            studioServerProxy.UpdateManagerProxy = updateManagerProxy.Object;
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemModel.SetupAllProperties();
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Resource");
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "false", true, true, false, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var item = studioServerProxy.Rename(mockExplorerItemModel.Object, It.IsAny<string>());
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            updateManagerProxy.Setup(manager => manager.Rename(It.IsAny<Guid>(), It.IsAny<string>()));
        }

        [TestMethod]
        [Timeout(2000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_Move")]
        public void StudioServerProxy_Move_WhenSucces_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueFunction(ExecStatus.Success));
            studioServerProxy.UpdateManagerProxy = updateManagerProxy.Object;
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemModel.SetupAllProperties();
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Resource");
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "false", true, true, false, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var treeItem = new Mock<IExplorerTreeItem>();
            treeItem.Setup(explorerTreeItem => explorerTreeItem.ResourcePath);
            var item = studioServerProxy.Move(mockExplorerItemModel.Object, treeItem.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Result);
            updateManagerProxy.Verify(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        [Timeout(2000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_Move")]
        public void StudioServerProxy_Move_WhenFaulty_ShouldReturnfalse()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueFunction(ExecStatus.Fail));
            studioServerProxy.UpdateManagerProxy = updateManagerProxy.Object;
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemModel.SetupAllProperties();
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Resource");
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "false", true, true, false, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);
            //------------Execute Test---------------------------
            var treeItem = new Mock<IExplorerTreeItem>();
            treeItem.Setup(explorerTreeItem => explorerTreeItem.ResourcePath);
            var item = studioServerProxy.Move(mockExplorerItemModel.Object, treeItem.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            Assert.IsFalse(item.Result);
            updateManagerProxy.Verify(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()));
        }      

        [TestMethod]
        [Timeout(1000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_GetVersion")]
        public void StudioServerProxy_GetVersion_WhenGivenVersion_ShouldLoadUsingVersion()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            var versionInfo = new VersionInfo();
            mockVersionManager.Setup(manager => manager.GetVersion(versionInfo, It.IsAny<Guid>()))
                .Returns(new StringBuilder())
                .Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;
            //------------Execute Test---------------------------
            var treeItem = new Mock<IExplorerTreeItem>();
            treeItem.Setup(explorerTreeItem => explorerTreeItem.ResourcePath);
            var item = studioServerProxy.GetVersion(versionInfo, It.IsAny<Guid>());
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);

            mockVersionManager.Verify(manager => manager.GetVersion(versionInfo, It.IsAny<Guid>()));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("StudioServerProxy_GetVersions")]
        public void StudioServerProxy_GetVersions_WhenGivenVersionId_ShouldLoadUsingVersionId()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.IsConnected).Returns(true);

            var resourceId = Guid.NewGuid();
            var mockCommunicationControllerFactory = new Mock<ICommunicationControllerFactory>();
            var mockGetVersionsController = new Mock<ICommunicationController>();
            mockGetVersionsController.Setup(o => o.AddPayloadArgument("resourceId", resourceId.ToString())).Verifiable();


            var explorerItem = new Mock<IExplorerItem>
            {
                Name = "Item1",
            };
            var v1 = new VersionInfo
            {
                VersionNumber = "v1.0.0"
            };
            explorerItem.Setup(o => o.VersionInfo).Returns(v1);
            var items = new List<IExplorerItem>
            {
                explorerItem.Object
            };
            mockGetVersionsController.Setup(o => o.ExecuteCommand<IList<IExplorerItem>>(mockEnvironmentConnection.Object, It.IsAny<Guid>())).Returns(items);

            mockCommunicationControllerFactory.Setup(o => o.CreateController("GetVersions")).Returns(mockGetVersionsController.Object);
            

            var communicationControllerFactory = mockCommunicationControllerFactory.Object;
            var studioServerProxy = new StudioServerProxy(communicationControllerFactory, mockEnvironmentConnection.Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var versionManager = new VersionManagerProxy(communicationControllerFactory, mockEnvironmentConnection.Object);
            
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = versionManager;
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueFunction(ExecStatus.Fail));
            studioServerProxy.UpdateManagerProxy = updateManagerProxy.Object;
            //------------Execute Test---------------------------
            var treeItem = new Mock<IExplorerTreeItem>();
            treeItem.Setup(explorerTreeItem => explorerTreeItem.ResourcePath);
            var item = studioServerProxy.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            Assert.AreEqual(v1.VersionNumber, item.FirstOrDefault().VersionNumber);
            //mockVersionManager.Verify(manager => manager.GetVersions(It.IsAny<Guid>()));
            mockGetVersionsController.Verify();
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_Rollback")]
        public void StudioServerProxy_Rollback_GivenVersionIdAndResourceId_ShouldLoadVersionIdAndResourceId()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            var mock = new Mock<IRollbackResult>();
            mockVersionManager.Setup(manager => manager.RollbackTo(It.IsAny<Guid>(), It.IsAny<string>()))
                                .Returns(mock.Object)
                                .Verifiable();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueFunction(ExecStatus.Fail));
            studioServerProxy.UpdateManagerProxy = updateManagerProxy.Object;
            //------------Execute Test---------------------------
            var treeItem = new Mock<IExplorerTreeItem>();
            treeItem.Setup(explorerTreeItem => explorerTreeItem.ResourcePath);
            var item = studioServerProxy.Rollback(It.IsAny<Guid>(), It.IsAny<string>());
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);

            mockVersionManager.Verify(manager => manager.RollbackTo(It.IsAny<Guid>(), It.IsAny<string>()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_CreateFolder")]
        public void StudioServerProxy_CreateFolder_VerifyFolderCreated()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.AddFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()));
            studioServerProxy.UpdateManagerProxy = updateManagerProxy.Object;
            //------------Execute Test---------------------------
            var treeItem = new Mock<IExplorerTreeItem>();
            treeItem.Setup(explorerTreeItem => explorerTreeItem.ResourcePath);
            studioServerProxy.CreateFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>());
            //------------Assert Results-------------------------
            updateManagerProxy.Verify(manager => manager.AddFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioServerProxy_CreateFolder")]
        public void StudioServerProxy_CreateFolder_VerifyFolderIsTrue()
        {
            //------------Setup for test--------------------------
            var commControllerFatoryMock = new Mock<ICommunicationControllerFactory>();
            var commController = new Mock<ICommunicationController>();
            commController.Setup(controller => controller.ExecuteCommand<IExplorerRepositoryResult>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>())).Returns(new ExplorerRepositoryResult(ExecStatus.Success,""));            
            ServerExplorerItem sendItem = null;
            var serialiser = new Dev2JsonSerializer();
            commController.Setup(controller => controller.AddPayloadArgument(It.IsAny<string>(), It.IsAny<StringBuilder>())).Callback((string key, StringBuilder builder) => {
                sendItem = serialiser.Deserialize<ServerExplorerItem>(builder);
            });
            commControllerFatoryMock.Setup(controller => controller.CreateController(It.IsAny<string>())).Returns(commController.Object);
            var studioServerProxy = new StudioServerProxy(commControllerFatoryMock.Object, new Mock<IEnvironmentConnection>().Object);
            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            studioServerProxy.QueryManagerProxy = mockQueryManager.Object;
            studioServerProxy.VersionManager = mockVersionManager.Object;

            var resID = Guid.NewGuid();
            var resName = "TestFolder";
            var resPath = "TestFolderPath";

            //------------Execute Test---------------------------
            studioServerProxy.CreateFolder(resPath, resName, resID);
            //------------Assert Results-------------------------
            Assert.AreEqual(resID, sendItem.ResourceId);
            Assert.AreEqual(resName, sendItem.DisplayName);
            Assert.AreEqual(resPath + "\\" + resName, sendItem.ResourcePath);
            Assert.AreEqual("Folder", sendItem.ResourceType);
            Assert.AreEqual(true, sendItem.IsFolder);
        }

        [TestMethod]
        [Timeout(1000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_HasDependencies")]
        public void StudioServerProxy_HasDependencies_GivenNoNodes_ShowDepenciesIsFalse()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mock = new Mock<IExplorerItemViewModel>();
            var dependencyGraphGenerator = new Mock<IDependencyGraphGenerator>();
            dependencyGraphGenerator.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(new Graph("myGraph"));
            var msgMock = new Mock<IExecuteMessage>();
            //------------Execute Test---------------------------
            var metaData = studioServerProxy.HasDependencies(mock.Object, dependencyGraphGenerator.Object, msgMock.Object, new Mock<IPopupController>().Object);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(metaData, typeof(IDeletedFileMetadata));
            var deletedFileMetadata = metaData;
            Assert.IsNotNull(deletedFileMetadata);
            Assert.AreEqual(true, deletedFileMetadata.IsDeleted);
            Assert.AreEqual(false, deletedFileMetadata.ShowDependencies);
            Assert.AreEqual(mock.Object.ResourceId, deletedFileMetadata.ResourceId);
        }

        [TestMethod]
        [Timeout(2000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_HasDependencies")]
        public void StudioServerProxy_HasDependencies_GivenNodesAndDeleteAnyway_ShowDepenciesIsFalse()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mock1 = new Mock<IPopupController>();
            mock1.Setup(controller => controller.DeleteAnyway).Returns(true);
            CustomContainer.Register(mock1.Object);
            var mock = new Mock<IExplorerItemViewModel>();
            var dependencyGraphGenerator = new Mock<IDependencyGraphGenerator>();
            var value = new Graph("myGraph");
            value.Nodes.Add(new DependencyVisualizationNode("a", 2, 2, false, false));
            value.Nodes.Add(new DependencyVisualizationNode("b", 2, 2, false, false));
            dependencyGraphGenerator.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            var msgMock = new Mock<IExecuteMessage>();
            //------------Execute Test---------------------------
            var metaData = studioServerProxy.HasDependencies(mock.Object, dependencyGraphGenerator.Object, msgMock.Object, mock1.Object);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(metaData, typeof(IDeletedFileMetadata));
            var deletedFileMetadata = metaData;
            Assert.IsNotNull(deletedFileMetadata);
            Assert.AreEqual(false, deletedFileMetadata.IsDeleted);
            Assert.AreEqual(false, deletedFileMetadata.ShowDependencies);
            Assert.AreEqual(mock1.Object.ApplyToAll, deletedFileMetadata.ApplyToAll);
            Assert.AreEqual(mock1.Object.DeleteAnyway, deletedFileMetadata.DeleteAnyway);
            Assert.AreEqual(mock.Object.ResourceId, deletedFileMetadata.ResourceId);

        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_HasDependencies")]
        public void StudioServerProxy_HasDependencies_GivenNodesAndOkClicked_ShowSetupCorrectly()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.DeleteAnyway).Returns(false);
            mockPopupController.Setup(controller => controller.Show(
                                          It.IsAny<string>()
                                        , It.IsAny<string>()
                                        , It.IsAny<MessageBoxButton>()  
                                        , It.IsAny<MessageBoxImage>()                
                                        , "false"
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()))
                .Returns(MessageBoxResult.OK);
            var mock = new Mock<IExplorerItemViewModel>();
            var dependencyGraphGenerator = new Mock<IDependencyGraphGenerator>();
            var value = new Graph("myGraph");
            value.Nodes.Add(new DependencyVisualizationNode("a", 2, 2, false, false));
            value.Nodes.Add(new DependencyVisualizationNode("b", 2, 2, false, false));
            dependencyGraphGenerator.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            var msgMock = new Mock<IExecuteMessage>();
            //------------Execute Test---------------------------
            var metaData = studioServerProxy.HasDependencies(mock.Object, dependencyGraphGenerator.Object, msgMock.Object, mockPopupController.Object);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(metaData, typeof(IDeletedFileMetadata));
            var deletedFileMetadata = metaData;
            Assert.IsNotNull(deletedFileMetadata);
            Assert.IsFalse(deletedFileMetadata.IsDeleted);
            Assert.IsFalse(deletedFileMetadata.ShowDependencies);
            Assert.AreEqual(mockPopupController.Object.ApplyToAll, deletedFileMetadata.ApplyToAll);
            Assert.AreEqual(mockPopupController.Object.DeleteAnyway, deletedFileMetadata.DeleteAnyway);
            Assert.AreEqual(mock.Object.ResourceId, deletedFileMetadata.ResourceId);

        }

        [TestMethod]
        [Timeout(2000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_HasDependencies")]
        public void StudioServerProxy_HasDependencies_GivenNodesAndCanceClicked_ShowsDependencies()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.DeleteAnyway).Returns(false);
            mockPopupController.Setup(controller => controller.Show(
                                          It.IsAny<string>()
                                        , It.IsAny<string>()
                                        , It.IsAny<MessageBoxButton>()  
                                        , It.IsAny<MessageBoxImage>()                
                                        , "false"
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()
                                        , It.IsAny<bool>()))
                .Returns(MessageBoxResult.Cancel);
            CustomContainer.Register(mockPopupController.Object);
            var mock = new Mock<IExplorerItemViewModel>();
            mock.Setup(model => model.ShowDependencies()).Verifiable();
            var dependencyGraphGenerator = new Mock<IDependencyGraphGenerator>();
            var value = new Graph("myGraph");
            value.Nodes.Add(new DependencyVisualizationNode("a", 2, 2, false, false));
            value.Nodes.Add(new DependencyVisualizationNode("b", 2, 2, false, false));
            dependencyGraphGenerator.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            var msgMock = new Mock<IExecuteMessage>();
            //------------Execute Test---------------------------
            var metaData = studioServerProxy.HasDependencies(mock.Object, dependencyGraphGenerator.Object, msgMock.Object, mockPopupController.Object);
            //------------Assert Results-------------------------
            mock.Verify(model => model.ShowDependencies());
            Assert.IsInstanceOfType(metaData, typeof(IDeletedFileMetadata));
            var deletedFileMetadata = metaData;
            Assert.IsNotNull(deletedFileMetadata);
            Assert.AreEqual(false, deletedFileMetadata.IsDeleted);
            Assert.AreEqual(true, deletedFileMetadata.ShowDependencies);
            Assert.AreEqual(mockPopupController.Object.ApplyToAll, deletedFileMetadata.ApplyToAll);
            Assert.AreEqual(mockPopupController.Object.DeleteAnyway, deletedFileMetadata.DeleteAnyway);
            Assert.AreEqual(mock.Object.ResourceId, deletedFileMetadata.ResourceId);
        }

        Task<IExplorerRepositoryResult> ValueFunction(ExecStatus status)
        {
            IExplorerRepositoryResult result = new ExplorerRepositoryResult(status, "");
            return Task.FromResult(result);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("StudioServerProxy_LoadExplorer")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudioServerProxy_Rename_WhenNullItem_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            //------------Execute Test---------------------------
            studioServerProxy.Rename(default(IExplorerItemViewModel), It.IsAny<string>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Pieter Terblanche")]
        [TestCategory("StudioServerProxy_VersionManager")]
        public void StudioServerProxy_VersionManager_GetVersions_ServerDown_ShowPopup()
        {
            //------------Setup for test--------------------------
            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(a => a.DisplayName).Returns("localhost");
            var versionManagerProxy = new VersionManagerProxy(new CommunicationControllerFactory(), environmentConnection.Object);
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);

            var versions = versionManagerProxy.GetVersions(It.IsAny<Guid>());

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(versions);
        }


    }
}