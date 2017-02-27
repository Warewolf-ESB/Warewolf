using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.AntiCorruptionLayer;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerDeleteProviderTests
    {

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Delete_WhenResource_ShouldDeleteResource()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IExplorerRepository>();
            mock.Setup(proxy => proxy.HasDependencies(It.IsAny<IExplorerItemViewModel>(), It.IsAny<IDependencyGraphGenerator>(), It.IsAny<IExecuteMessage>()))
               .Returns(() => new DeletedFileMetadata()
               {
                   ApplyToAll = true,
                   DeleteAnyway = true
               });

            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mock.SetupGet(repository => repository.QueryManagerProxy).Returns(mockQueryManager.Object);
            mock.SetupGet(repository => repository.VersionManager).Returns(mockVersionManager.Object);
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.DeleteResource(It.IsAny<Guid>()));
            mock.SetupGet(repository => repository.UpdateManagerProxy).Returns(updateManagerProxy.Object);
            mock.Setup(repository => repository.HasDependencies(It.IsAny<IExplorerItemViewModel>(), It.IsAny<IDependencyGraphGenerator>(), It.IsAny<IExecuteMessage>()))
                .Returns(new DeletedFileMetadata() { IsDeleted = true, DeleteAnyway = true });
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            var child = new Mock<IExplorerItemViewModel>();

            child.SetupGet(model => model.ResourceType).Returns("Resourse");
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("resource");
            mockExplorerItemModel.Setup(model => model.ResourcePath).Returns("path");
            mockExplorerItemModel.Setup(model => model.AsList())
                                 .Returns(new List<IExplorerItemViewModel>()
                                 {
                                     child.Object
                                 });
            var explorerDeleteProvider = new ExplorerDeleteProvider(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(explorerDeleteProvider);
            //---------------Execute Test ----------------------
            explorerDeleteProvider.Delete(mockExplorerItemModel.Object);
            //---------------Test Result -----------------------
            updateManagerProxy.Verify(manager => manager.DeleteResource(It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Delete_WhenVersion_ShouldDeleteVersion()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IExplorerRepository>();
            mock.Setup(proxy => proxy.HasDependencies(It.IsAny<IExplorerItemViewModel>(), It.IsAny<IDependencyGraphGenerator>(), It.IsAny<IExecuteMessage>()))
               .Returns(() => new DeletedFileMetadata()
               {
                   ApplyToAll = true,
                   DeleteAnyway = true
               });

            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mock.SetupGet(repository => repository.QueryManagerProxy).Returns(mockQueryManager.Object);
            mock.SetupGet(repository => repository.VersionManager).Returns(mockVersionManager.Object);
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            mock.SetupGet(repository => repository.UpdateManagerProxy).Returns(updateManagerProxy.Object);
            mock.Setup(repository => repository.HasDependencies(It.IsAny<IExplorerItemViewModel>(), It.IsAny<IDependencyGraphGenerator>(), It.IsAny<IExecuteMessage>()))
                .Returns(new DeletedFileMetadata() { IsDeleted = true, DeleteAnyway = true });
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            var child = new Mock<IExplorerItemViewModel>();

            child.SetupGet(model => model.ResourceType).Returns("Version");
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Version");
            mockExplorerItemModel.Setup(model => model.ResourcePath).Returns("path");
            mockExplorerItemModel.Setup(model => model.AsList())
                                 .Returns(new List<IExplorerItemViewModel>()
                                 {
                                     child.Object
                                 });
            var explorerDeleteProvider = new ExplorerDeleteProvider(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(explorerDeleteProvider);
            //---------------Execute Test ----------------------
            var deletedFileMetadata = explorerDeleteProvider.Delete(mockExplorerItemModel.Object);
            //---------------Test Result -----------------------
            mockVersionManager.Verify(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()));
            Assert.IsTrue(deletedFileMetadata.IsDeleted);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Delete_WhenFolderDeleteAnywayApplyToAll_ShouldShowDependenciesAndDeleteFolder()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IExplorerRepository>();
            mock.Setup(proxy => proxy.HasDependencies(It.IsAny<IExplorerItemViewModel>(), It.IsAny<IDependencyGraphGenerator>(), It.IsAny<IExecuteMessage>()))
               .Returns(() => new DeletedFileMetadata()
               {
                   ApplyToAll = true,
                   DeleteAnyway = true
               });

            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mock.SetupGet(repository => repository.QueryManagerProxy).Returns(mockQueryManager.Object);
            mock.SetupGet(repository => repository.VersionManager).Returns(mockVersionManager.Object);
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueFunction(ExecStatus.Fail));
            mock.SetupGet(repository => repository.UpdateManagerProxy).Returns(updateManagerProxy.Object);
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            var child = new Mock<IExplorerItemViewModel>();

            child.SetupGet(model => model.ResourceType).Returns("Resourse");
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Folder");
            mockExplorerItemModel.Setup(model => model.ResourcePath).Returns("path");
            mockExplorerItemModel.Setup(model => model.AsList())
                                 .Returns(new List<IExplorerItemViewModel>()
                                 {
                                     child.Object
                                 });
            var explorerDeleteProvider = new ExplorerDeleteProvider(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(explorerDeleteProvider);
            //---------------Execute Test ----------------------
            var item = explorerDeleteProvider.Delete(mockExplorerItemModel.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(item);
            mockQueryManager.Verify(manager => manager.FetchDependants(It.IsAny<Guid>()));
            updateManagerProxy.Verify(manager => manager.DeleteFolder(It.IsAny<string>()));
            Assert.AreEqual(true, item.IsDeleted);
            Assert.AreEqual(Guid.Empty, item.ResourceId);
        }

        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Delete_WhenFolderDeleteAnywayNotApplyToAll_ShouldShowDependenciesAndDeleteResource()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IExplorerRepository>();
            mock.Setup(proxy => proxy.HasDependencies(It.IsAny<IExplorerItemViewModel>(), It.IsAny<IDependencyGraphGenerator>(), It.IsAny<IExecuteMessage>()))
               .Returns(() => new DeletedFileMetadata()
               {
                   ApplyToAll = false,
                   DeleteAnyway = true,
                   IsDeleted = false
               });

            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mock.SetupGet(repository => repository.QueryManagerProxy).Returns(mockQueryManager.Object);
            mock.SetupGet(repository => repository.VersionManager).Returns(mockVersionManager.Object);
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueFunction(ExecStatus.Fail));
            mock.SetupGet(repository => repository.UpdateManagerProxy).Returns(updateManagerProxy.Object);
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            var child = new Mock<IExplorerItemViewModel>();

            child.SetupGet(model => model.ResourceType).Returns("Resourse");
            mockExplorerItemModel.Setup(model => model.RemoveChild(It.IsAny<IExplorerItemViewModel>()));
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Folder");
            mockExplorerItemModel.Setup(model => model.ResourcePath).Returns("path");
            mockExplorerItemModel.Setup(model => model.AsList())
                                 .Returns(new List<IExplorerItemViewModel>()
                                 {
                                     child.Object
                                 });
            var explorerDeleteProvider = new ExplorerDeleteProvider(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(explorerDeleteProvider);
            //---------------Execute Test ----------------------
            var item = explorerDeleteProvider.Delete(mockExplorerItemModel.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(item);
            mockQueryManager.Verify(manager => manager.FetchDependants(It.IsAny<Guid>()), Times.Once);
            updateManagerProxy.Verify(manager => manager.DeleteResource(It.IsAny<Guid>()), Times.Once);
            mockExplorerItemModel.Verify(model => model.RemoveChild(It.IsAny<IExplorerItemViewModel>()), Times.Once);
            Assert.AreEqual(false, item.IsDeleted);
            Assert.AreEqual(Guid.Empty, item.ResourceId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Delete_WhenException_ShouldNotDelete()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IExplorerRepository>();
            mock.Setup(proxy => proxy.HasDependencies(It.IsAny<IExplorerItemViewModel>(), It.IsAny<IDependencyGraphGenerator>(), It.IsAny<IExecuteMessage>()))
               .Returns(() => new DeletedFileMetadata()
               {
                   ApplyToAll = false,
                   DeleteAnyway = true,
                   IsDeleted = false,
                   ShowDependencies = true
               });

            var mockQueryManager = new Mock<IQueryManager>();
            var mockVersionManager = new Mock<IVersionManager>();
            mockQueryManager.Setup(manager => manager.FetchDependants(It.IsAny<Guid>())).Throws(new Exception("err"));
            mockVersionManager.Setup(manager => manager.DeleteVersion(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mock.SetupGet(repository => repository.QueryManagerProxy).Returns(mockQueryManager.Object);
            mock.SetupGet(repository => repository.VersionManager).Returns(mockVersionManager.Object);
            var updateManagerProxy = new Mock<IExplorerUpdateManager>();
            updateManagerProxy.Setup(manager => manager.MoveItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueFunction(ExecStatus.Fail));
            mock.SetupGet(repository => repository.UpdateManagerProxy).Returns(updateManagerProxy.Object);
            var mockExplorerItemModel = new Mock<IExplorerItemViewModel>();
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(model => model.ResourceType).Returns("Resourse");
            var child1 = new Mock<IExplorerItemViewModel>();
            child1.SetupGet(model => model.ResourceType).Returns("Resourse");
            mockExplorerItemModel.Setup(model => model.RemoveChild(It.IsAny<IExplorerItemViewModel>()));
            mockExplorerItemModel.Setup(model => model.ResourceType).Returns("Folder");
            mockExplorerItemModel.Setup(model => model.ResourcePath).Returns("path");
            mockExplorerItemModel.Setup(model => model.AsList())
                                 .Returns(new List<IExplorerItemViewModel>()
                                 {
                                     child.Object, child1.Object
                                 });
            var explorerDeleteProvider = new ExplorerDeleteProvider(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(explorerDeleteProvider);
            //---------------Execute Test ----------------------
            var item = explorerDeleteProvider.Delete(mockExplorerItemModel.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(item);
            mockQueryManager.Verify(manager => manager.FetchDependants(It.IsAny<Guid>()), Times.Once);
            updateManagerProxy.Verify(manager => manager.DeleteResource(It.IsAny<Guid>()), Times.Never);
            mockExplorerItemModel.Verify(model => model.RemoveChild(It.IsAny<IExplorerItemViewModel>()), Times.Never);
            Assert.AreEqual(false, item.IsDeleted);
        }

        private Task<IExplorerRepositoryResult> ValueFunction(ExecStatus status)
        {
            IExplorerRepositoryResult result = new ExplorerRepositoryResult(status, "");
            return Task.FromResult(result);
        }
    }
}
