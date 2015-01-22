using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerItemViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullShellViewModel_ExpectException()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            new ExplorerItemViewModel(null,null,null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Constructor")]
        public void Constructor_SetsUpOpenCommand()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            shellViewModelMock.Setup(model => model.AddService(It.IsAny<IResource>())).Verifiable();
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object,new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object);
            //------------Assert Results-------------------------
            explorerViewModel.OpenCommand.Execute(null);
            shellViewModelMock.Verify(model => model.AddService(It.IsAny<IResource>()),Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemViewModel_Constructor")]
        public void ExplorerItemViewModel_Constructor_NewCommandHasResourceTypeParameter()
        {
            //------------Setup for test--------------------------
            ResourceType? resourceTypeParameter = null;
            var shellViewModelMock = new Mock<IShellViewModel>();
            shellViewModelMock.Setup(model => model.NewResource(It.IsAny<ResourceType?>())).Callback((ResourceType? resourceType) => resourceTypeParameter = resourceType);
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object);
            //------------Assert Results-------------------------
            explorerViewModel.NewCommand.Execute(ResourceType.DbService);
            shellViewModelMock.Verify(model => model.NewResource(It.IsAny<ResourceType>()), Times.Once());
            Assert.IsNotNull(resourceTypeParameter);
            Assert.AreEqual(ResourceType.DbService,resourceTypeParameter);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemViewModel_PermissionsUpdated")]
        public void ExplorerItemViewModel_PermissionsUpdated_HasResourcePermissionShouldUpdateItem()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var mockServer = new Mock<IServer>();
            var resourceId = Guid.NewGuid();
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, mockServer.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object) { ResourceId = resourceId };
            //------------Execute Test---------------------------
            mockServer.Raise(server => server.PermissionsChanged += null, new PermissionsChangedArgs(new List<IWindowsGroupPermission>{new WindowsGroupPermission
            {
                ResourceID = resourceId,
                Contribute = false,
                View = true,
                Execute = false,
            }}));
            //------------Assert Results-------------------------
            Assert.IsTrue(explorerViewModel.CanView);
            Assert.IsFalse(explorerViewModel.CanEdit);
            Assert.IsFalse(explorerViewModel.CanExecute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemViewModel_PermissionsUpdated")]
        public void ExplorerItemViewModel_PermissionsUpdated_HasResourcePermissionContributeShouldUpdateItem()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var mockServer = new Mock<IServer>();
            var resourceId = Guid.NewGuid();
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, mockServer.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object) { ResourceId = resourceId };
            //------------Execute Test---------------------------
            mockServer.Raise(server => server.PermissionsChanged += null, new PermissionsChangedArgs(new List<IWindowsGroupPermission>{new WindowsGroupPermission
            {
                ResourceID = resourceId,
                View = false,
                Execute = false,
                Contribute = true,
            }}));
            //------------Assert Results-------------------------
            Assert.IsTrue(explorerViewModel.CanView);
            Assert.IsTrue(explorerViewModel.CanEdit);
            Assert.IsTrue(explorerViewModel.CanExecute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemViewModel_PermissionsUpdated")]
        public void ExplorerItemViewModel_PermissionsUpdated_HasNoResourcePermissionShouldUpdateItemWithServerPermission()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var mockServer = new Mock<IServer>();
            var resourceId = Guid.NewGuid();
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, mockServer.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object) { ResourceId = resourceId };
            //------------Execute Test---------------------------
            mockServer.Raise(server => server.PermissionsChanged += null, new PermissionsChangedArgs(new List<IWindowsGroupPermission>{
                new WindowsGroupPermission
            {
                ResourceID = Guid.NewGuid(),
                View = false,
                Execute = false,
                Contribute = true,
            },
            new WindowsGroupPermission
            {
                ResourceID = Guid.Empty,
                IsServer = true,
                View = false,
                Administrator = false,
                Contribute = false,
                Execute = true,
            }}));
            //------------Assert Results-------------------------
            Assert.IsFalse(explorerViewModel.CanView);
            Assert.IsFalse(explorerViewModel.CanEdit);
            Assert.IsTrue(explorerViewModel.CanExecute);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Renaming")]
        public void ExplorerItemViewModel_IsRenaming_NotRenaming()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
                    //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object);
            //------------Assert Results-------------------------
            explorerViewModel.IsRenaming = true;
            Assert.IsTrue(explorerViewModel.IsRenaming);
            Assert.IsFalse(explorerViewModel.IsNotRenaming);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Renaming")]
        public void ExplorerItemViewModel_UpdateName_RenamesToFalse()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();
           
            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
                        //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object,server.Object , new Mock<IExplorerHelpDescriptorBuilder>().Object);
            expRepo.Setup(a => a.Rename(explorerViewModel, "bob")).Returns(true);
            //------------Assert Results-------------------------
            explorerViewModel.IsRenaming = true;
            explorerViewModel.ResourceName = "bob";
            Assert.IsFalse(explorerViewModel.IsRenaming);
         }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Renaming")]
        public void ExplorerItemViewModel_UpdateName_RenamesToFalse_ErrorOnCall()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object);
            explorerViewModel.ResourceName = "dave";
            expRepo.Setup(a => a.Rename(explorerViewModel, "bob")).Throws(new Exception());
            //------------Assert Results-------------------------
            explorerViewModel.IsRenaming = true;
            explorerViewModel.ResourceName = "dave";
            Assert.IsFalse(explorerViewModel.IsRenaming);
        }
    }
}
