using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Explorer;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerItemViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Constructor")]
    
        // ReSharper disable InconsistentNaming
        public void Constructor_NullShellViewModel_ExpectException()
      
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            NullArgumentConstructorHelper.AssertNullConstructor( new object[]{new Mock<IShellViewModel>().Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object},typeof(ExplorerItemViewModel));
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
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object,new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object,null, new Mock<IExplorerViewModel>().Object);
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
            shellViewModelMock.Setup(model => model.NewResource(It.IsAny<ResourceType?>(), Guid.Empty)).Callback((ResourceType? resourceType) => resourceTypeParameter = resourceType);
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object);
            //------------Assert Results-------------------------
            explorerViewModel.NewCommand.Execute(ResourceType.DbService);
            shellViewModelMock.Verify(model => model.NewResource(It.IsAny<ResourceType>(), Guid.Empty), Times.Once());
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
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, mockServer.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object) { ResourceId = resourceId };
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
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, mockServer.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object) { ResourceId = resourceId };
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
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, mockServer.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object) { ResourceId = resourceId };
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
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object) { IsRenaming = true };
            //------------Assert Results-------------------------
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
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object);
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
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object) { ResourceName = "dave" };
            expRepo.Setup(a => a.Rename(explorerViewModel, "bob")).Throws(new Exception());
            //------------Assert Results-------------------------
            explorerViewModel.IsRenaming = true;
            explorerViewModel.ResourceName = "dave";
            Assert.IsFalse(explorerViewModel.IsRenaming);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Move")]
        public void ExplorerItemViewModel_Move_CallsCorrectModel_NoErrorOnCall()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();
            var expMovedInto = new Mock<IExplorerItemViewModel>().Object;

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object);
            explorerViewModel.Move(expMovedInto);
            expRepo.Verify(a=>a.Move(explorerViewModel,expMovedInto));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Move")]
        public void ExplorerItemViewModel_Move_CallsCorrectModel_ErrorOnCall()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();
            var expMovedInto = new Mock<IExplorerItemViewModel>().Object;

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
          
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object);
            expRepo.Setup(a => a.Move(explorerViewModel, expMovedInto)).Throws(new Exception());
            explorerViewModel.Move(expMovedInto);
            shellViewModelMock.Verify(a=>a.Handle(It.IsAny<Exception>()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Move")]
        public void ExplorerItemViewModel_HistoryAvailable()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);

            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object) { ResourceType = ResourceType.DbService };
            Assert.IsFalse(explorerViewModel.CanShowVersions);
            Assert.IsFalse(explorerViewModel.AreVersionsVisible);
            Assert.AreEqual("Show Version History",explorerViewModel.VersionHeader);
            explorerViewModel.ResourceType = ResourceType.WorkflowService;
            Assert.IsTrue(explorerViewModel.CanShowVersions);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Rollback")]
        public void ExplorerItemViewModel_Rollback_Execute()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
            
            //------------Execute Test---------------------------
            var parent = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object);
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, parent, new Mock<IExplorerViewModel>().Object)
            {
                VersionNumber = "3",ResourceId = Guid.NewGuid()
            
            };
            expRepo.Setup(a => a.Rollback(explorerViewModel.ResourceId, "3")).Returns(new RollbackResult {DisplayName = "bob", VersionHistory = new List<IExplorerItem>()});
            explorerViewModel.RollbackCommand.Execute(null);
            Assert.AreEqual(parent.ResourceName,"bob");
            expRepo.Verify(a => a.Rollback(explorerViewModel.ResourceId, "3"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_History")]
        public void ExplorerItemViewModel_History_Show()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);

            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object) { ResourceId = Guid.NewGuid(), ResourceType = ResourceType.DbService };
            Assert.IsFalse(explorerViewModel.CanShowVersions);
            Assert.IsFalse(explorerViewModel.AreVersionsVisible);
            Assert.AreEqual("Show Version History", explorerViewModel.VersionHeader);
            explorerViewModel.ResourceType = ResourceType.WorkflowService;
            Assert.IsTrue(explorerViewModel.CanShowVersions);
            var mockVersion = new Mock<IVersionInfo>();
            mockVersion.Setup(a => a.VersionNumber).Returns("1");
            mockVersion.Setup(a => a.DateTimeStamp).Returns(DateTime.MaxValue);
            mockVersion.Setup(a => a.Reason).Returns("bob");
            expRepo.Setup(a => a.GetVersions(explorerViewModel.ResourceId)).Returns(new List<IVersionInfo> { mockVersion.Object });
            explorerViewModel.AreVersionsVisible = true;
            Assert.IsTrue(explorerViewModel.Children.Count==1);
            Assert.AreEqual("1" + " " + DateTime.MaxValue.ToString(CultureInfo.InvariantCulture) + " " + "bob", explorerViewModel.Children.First().ResourceName);
            explorerViewModel.AreVersionsVisible = false;
            Assert.AreEqual(0,explorerViewModel.Children.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Delete")]
        public void ExplorerItemViewModel_Delete_CallsCorrectMethod()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            shellViewModelMock.Setup(a => a.ShowPopup(It.IsAny<IPopupMessage>())).Returns(true);
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object);
            explorerViewModel.DeleteCommand.Execute(null);
            expRepo.Verify(a => a.Delete(explorerViewModel));
            shellViewModelMock.Verify(a=>a.RemoveServiceFromExplorer(explorerViewModel));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Filter")]
        public void ExplorerItemViewModel_FilterSetChildrenInvisible_CallsCorrectMethod()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            shellViewModelMock.Setup(a => a.ShowPopup(It.IsAny<IPopupMessage>())).Returns(true);
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();
         

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object) 
            { ResourceName = "mat",
                Children = new ObservableCollection<IExplorerItemViewModel>
                {
                     new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object,null, new Mock<IExplorerViewModel>().Object)
                     {
                         ResourceName = "bob",
                         Children = new ObservableCollection<IExplorerItemViewModel>
                         {
                              new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object,null, new Mock<IExplorerViewModel>().Object) {ResourceName = "The"},
                               new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object,null, new Mock<IExplorerViewModel>().Object) {ResourceName = "Builder"}

                         }
                     },
                      new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object,null, new Mock<IExplorerViewModel>().Object){ResourceName = "moot"},
                       new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object,null, new Mock<IExplorerViewModel>().Object){ResourceName = "boot"}
                }
            };
            Assert.AreEqual(explorerViewModel.Children.Count,3);
            explorerViewModel.Filter("bob");
            Assert.AreEqual(explorerViewModel.Children.Count,1);
            Assert.AreEqual(explorerViewModel.Children.First().Children.Count,0);
            explorerViewModel.Filter("The");
            Assert.AreEqual(explorerViewModel.Children.Count, 1);
            Assert.AreEqual(explorerViewModel.Children.First().Children.Count, 1);
            explorerViewModel.Filter("Sasuke");
            Assert.AreEqual(explorerViewModel.Children.Count, 0);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_Delete")]
        public void ExplorerItemViewModel_Delete_DoesNotOccurIfPopupIsFalse()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            shellViewModelMock.Setup(a => a.ShowPopup(It.IsAny<IPopupMessage>())).Returns(false);
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object);
            explorerViewModel.DeleteCommand.Execute(null);
            expRepo.Verify(a => a.Delete(explorerViewModel),Times.Never());
            shellViewModelMock.Verify(a => a.RemoveServiceFromExplorer(explorerViewModel),Times.Never());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_UpdatePermissions")]
        public void ExplorerItemViewModel_UpdatePermissions_CanCreateNewFolder()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();

            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);

            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object)
            {
                ResourceType = ResourceType.Folder,
                CanCreateFolder = false
                ,ResourceId = Guid.NewGuid()
            };
            explorerViewModel.UpdatePermissions( new PermissionsChangedArgs(new List<IWindowsGroupPermission>{new WindowsGroupPermission {Administrator = true , ResourceID = Guid.Empty ,IsServer = true}}));
            Assert.IsTrue(explorerViewModel.CanCreateFolder);
            explorerViewModel.UpdatePermissions(new PermissionsChangedArgs(new List<IWindowsGroupPermission> { new WindowsGroupPermission { Administrator = false, Contribute = false, ResourceID = Guid.Empty, IsServer = true } }));
            Assert.IsFalse(explorerViewModel.CanCreateFolder);
            explorerViewModel.UpdatePermissions(new PermissionsChangedArgs(new List<IWindowsGroupPermission> { new WindowsGroupPermission { Contribute = true, ResourceID = Guid.Empty, IsServer = true } }));
            Assert.IsTrue(explorerViewModel.CanCreateFolder);
            explorerViewModel.ResourceType = ResourceType.DbService;
            Assert.IsFalse(explorerViewModel.CanCreateFolder);
            explorerViewModel.ResourceType = ResourceType.Server;
            Assert.IsTrue(explorerViewModel.CanCreateFolder);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemViewModel_UpdatePermissions")]
        public void ExplorerItemViewModel_CreateNewFolderCommand()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var expRepo = new Mock<IExplorerRepository>();
           
            var id = Guid.NewGuid();
            server.Setup(a => a.ExplorerRepository).Returns(expRepo.Object);
            server.Setup(a => a.Permissions).Returns(new List<IWindowsGroupPermission>() { new WindowsGroupPermission() { Contribute = true,IsServer = true} });
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object, server.Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null, new Mock<IExplorerViewModel>().Object)
            {
                ResourceType = ResourceType.Folder,
                CanCreateFolder = true,
                ResourceId = id,
                Children =  new ObservableCollection<IExplorerItemViewModel>()
            };
           
            explorerViewModel.UpdatePermissions(new PermissionsChangedArgs(new List<IWindowsGroupPermission> { new WindowsGroupPermission { Administrator = true } }));
            Assert.IsTrue(explorerViewModel.CanCreateFolder);
            var result = new Mock<IExplorerItem>();
            result.Setup(a => a.DisplayName).Returns("New Folder");
         
            // ReSharper disable MaximumChainedReferences
            expRepo.Setup(a => a.CreateFolder(id, "New Folder",It.IsAny<Guid>()));//.Returns(result.Object);
            // ReSharper restore MaximumChainedReferences
            // execute command expect repo called and child added
            explorerViewModel.CreateFolderCommand.Execute(null);
            Assert.IsTrue(explorerViewModel.Children.Count == 1);
            Assert.AreEqual("New Folder",explorerViewModel.Children.First().ResourceName);
            Assert.IsTrue(explorerViewModel.Children.First().CanCreateFolder);
        }

    }



}
// ReSharper restore InconsistentNaming