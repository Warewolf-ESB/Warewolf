using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.SharepointFolderRead;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Sharepoint
{
    [TestClass]
    public class SharePointReadFolderViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "SharePoint";

        private ModelItem CreateModelItem()
        {
            var folderActivity = new SharepointReadFolderItemActivity();
            folderActivity.ServerInputPath = "TestFolder";
            folderActivity.IsFilesAndFoldersSelected = true;
            folderActivity.IsFilesSelected = true;
            folderActivity.IsFoldersSelected = true;

            return ModelItemUtils.CreateModelItem(folderActivity);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointReadFolderDesignerViewModel_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointReadFolderDesignerViewModel = new SharePointReadFolderDesignerViewModel(CreateModelItem());
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointReadFolderDesignerViewModel);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointReadFolderDesignerViewModel_Constructor_NullAsyncWorker_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            
            new SharePointReadFolderDesignerViewModel(CreateModelItem(), null, new Mock<IServer>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointReadFolderDesignerViewModel_Constructor_NullEnvironmentModel_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            
            new SharePointReadFolderDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharepointReadFolderDesignerViewModel_InitilizeProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointReadFolderDesignerViewModel = new SharePointReadFolderDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), new Mock<IServer>().Object);

            sharepointReadFolderDesignerViewModel.UpdateHelpDescriptor("Test");

            Assert.IsNotNull(sharepointReadFolderDesignerViewModel);
            Assert.IsNotNull(sharepointReadFolderDesignerViewModel.CollectionName);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharepointReadFolderDesignerViewModel_GetProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
            //------------Execute Test---------------------------
            var sharepointReadFolderDesignerViewModel = new SharePointReadFolderDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointReadFolderDesignerViewModel.Validate();
           var isFileandFolders = modelItem.GetProperty<bool>("IsFilesAndFoldersSelected");
            var isFiles = modelItem.GetProperty<bool>("IsFilesSelected");
            var isFolders = modelItem.GetProperty<bool>("IsFoldersSelected");
           var inputPath = modelItem.GetProperty<string>("ServerInputPath");

            Assert.IsNotNull(inputPath);
            Assert.IsTrue(isFileandFolders);
            Assert.IsTrue(isFiles);
            Assert.IsTrue(isFolders);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharepointReadFolderDesignerViewModel_ValidInputs_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
            //------------Execute Test---------------------------
            var sharepointReadFolderDesignerViewModel = new SharePointReadFolderDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointReadFolderDesignerViewModel.Validate();

            sharepointReadFolderDesignerViewModel.IsFilesAndFoldersSelected = true;
            sharepointReadFolderDesignerViewModel.IsFilesSelected = true;
            sharepointReadFolderDesignerViewModel.IsFoldersSelected = true;

            Assert.IsTrue(sharepointReadFolderDesignerViewModel.IsFilesAndFoldersSelected);
            Assert.IsTrue(sharepointReadFolderDesignerViewModel.IsFilesSelected);
            Assert.IsTrue(sharepointReadFolderDesignerViewModel.IsFoldersSelected);
            Assert.IsNotNull(sharepointReadFolderDesignerViewModel.ServerInputPath);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharepointReadFolderDesignerViewModel_SetProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
            modelItem.SetProperty("SharepointServerResourceId",Guid.NewGuid());
            //------------Execute Test---------------------------
            var sharepointReadFolderDesignerViewModel = new SharePointReadFolderDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointReadFolderDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointReadFolderDesignerViewModel.Validate();
            var isFileandFolders = modelItem.GetProperty<bool>("IsFilesAndFoldersSelected");
            var isFiles = modelItem.GetProperty<bool>("IsFilesSelected");
            var isFolders = modelItem.GetProperty<bool>("IsFoldersSelected");
            var inputPath = modelItem.GetProperty<string>("ServerInputPath");
            var sourceId = modelItem.GetProperty<Guid>("SharepointServerResourceId");

            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            sharepointReadFolderDesignerViewModel.UpdateHelpDescriptor("Test");

            Assert.IsNotNull(inputPath);
            Assert.IsTrue(isFileandFolders);
            Assert.IsTrue(isFiles);
            Assert.IsTrue(isFolders);
            Assert.AreNotEqual(sourceId,Guid.Empty);
            //------------Assert Results-------------------------
        }
    }
}
