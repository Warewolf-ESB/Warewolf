using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.SharePointFileUpload;
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
    public class SharePointFileUploadViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "SharePoint";

        private ModelItem CreateModelItem()
        {
            var fileUploadactivity = new SharepointFileUploadActivity();
           
            return ModelItemUtils.CreateModelItem(fileUploadactivity);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointFileUploadDesignerViewModel_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointFileUploadDesignerViewModel = new SharePointFileUploadDesignerViewModel(CreateModelItem());
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointFileUploadDesignerViewModel);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointFileUploadDesignerViewModel_Constructor_NullAsyncWorker_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            
            new SharePointFileUploadDesignerViewModel(CreateModelItem(), null, new Mock<IServer>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointFileUploadDesignerViewModel_Constructor_NullEnvironmentModel_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            
            new SharePointFileUploadDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointFileUploadDesignerViewModel_InitilizeProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointFileUploadDesignerViewModel = new SharePointFileUploadDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), new Mock<IServer>().Object);

            sharepointFileUploadDesignerViewModel.UpdateHelpDescriptor("Test");

            Assert.IsNotNull(sharepointFileUploadDesignerViewModel);
            Assert.IsNotNull(sharepointFileUploadDesignerViewModel.CollectionName);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointFileUploadDesignerViewModel_SetProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
            modelItem.SetProperty("LocalInputPath", "TestFolder");
            modelItem.SetProperty("SharepointServerResourceId", Guid.NewGuid());
            //------------Execute Test---------------------------
            var sharepointReadFolderDesignerViewModel = new SharePointFileUploadDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointReadFolderDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointReadFolderDesignerViewModel.Validate();
            var inputPath = modelItem.GetProperty<string>("LocalInputPath");
            var sourceId = modelItem.GetProperty<Guid>("SharepointServerResourceId");

            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            sharepointReadFolderDesignerViewModel.UpdateHelpDescriptor("Test");

            Assert.IsNotNull(inputPath);
            Assert.IsNotNull(sharepointReadFolderDesignerViewModel.LocalInputPath);
            Assert.AreNotEqual(sourceId, Guid.Empty);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointFileUploadDesignerViewModel_SetPropertiesNullSource_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
           modelItem.SetProperty("LocalInputPath","TestFolder");
            //------------Execute Test---------------------------
            var sharepointReadFolderDesignerViewModel = new SharePointFileUploadDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointReadFolderDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointReadFolderDesignerViewModel.Validate();
            var inputPath = modelItem.GetProperty<string>("LocalInputPath");
            modelItem.GetProperty<Guid>("SharepointServerResourceId");

            Assert.IsNotNull(inputPath);
            Assert.IsNotNull(sharepointReadFolderDesignerViewModel.LocalInputPath);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointFileUploadDesignerViewModel_SetPropertiesNullLocalPath_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
            modelItem.SetProperty("SharepointServerResourceId", Guid.NewGuid());

            //------------Execute Test---------------------------
            var sharepointReadFolderDesignerViewModel = new SharePointFileUploadDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointReadFolderDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointReadFolderDesignerViewModel.Validate();
         

            Assert.IsNotNull(sharepointReadFolderDesignerViewModel.LocalInputPath);
            //------------Assert Results-------------------------
        }
    }
}
