using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.SharePointCopyFile;
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
    public class SharePointFileCopyViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "SharePoint";

        private ModelItem CreateModelItem()
        {
            var fileUploadactivity = new SharepointCopyFileActivity();
           
            return ModelItemUtils.CreateModelItem(fileUploadactivity);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointCopyFileDesignerViewModel_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointFileCopyDesignerViewModel = new SharePointCopyFileDesignerViewModel(CreateModelItem());
          
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointFileCopyDesignerViewModel);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointCopyFileDesignerViewModel_Constructor_NullAsyncWorker_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            
            new SharePointCopyFileDesignerViewModel(CreateModelItem(), null, new Mock<IServer>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointCopyFileDesignerViewModel_Constructor_NullEnvironmentModel_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            
            new SharePointCopyFileDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointCopyFileDesignerViewModel_InitilizeProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointFileCopyDesignerViewModel = new SharePointCopyFileDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), new Mock<IServer>().Object);

            sharepointFileCopyDesignerViewModel.UpdateHelpDescriptor("Test");

            Assert.IsNotNull(sharepointFileCopyDesignerViewModel);
            Assert.IsNotNull(sharepointFileCopyDesignerViewModel.CollectionName);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointCopyFileDesignerViewModel_SetProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
            modelItem.SetProperty("ServerInputPathFrom", "TestFolder");
            modelItem.SetProperty("ServerInputPathTo", "TestFolder");
            modelItem.SetProperty("SharepointServerResourceId", Guid.NewGuid());
            //------------Execute Test---------------------------
            var sharepointFileCopyDesignerViewModel = new SharePointCopyFileDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointFileCopyDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointFileCopyDesignerViewModel.Validate();
            var inputPathfrom = modelItem.GetProperty<string>("ServerInputPathFrom");
            var inputPathTo = modelItem.GetProperty<string>("ServerInputPathTo");
            var sourceId = modelItem.GetProperty<Guid>("SharepointServerResourceId");

            Assert.IsNotNull(inputPathfrom);
            Assert.IsNotNull(inputPathTo);
            Assert.IsNotNull(sharepointFileCopyDesignerViewModel.ServerInputPathFrom);
            Assert.IsNotNull(sharepointFileCopyDesignerViewModel.ServerInputPathTo);
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
            modelItem.SetProperty("ServerInputPathFrom", "TestFolder");
            modelItem.SetProperty("ServerInputPathTo", "TestFolder");

            //------------Execute Test---------------------------
            var sharepointFileCopyDesignerViewModel = new SharePointCopyFileDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointFileCopyDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointFileCopyDesignerViewModel.Validate();
            var inputPathfrom = modelItem.GetProperty<string>("ServerInputPathFrom");
            var inputPathTo = modelItem.GetProperty<string>("ServerInputPathTo");
  

            Assert.IsNotNull(inputPathfrom);
            Assert.IsNotNull(inputPathTo);
            Assert.IsNotNull(sharepointFileCopyDesignerViewModel.ServerInputPathFrom);
            Assert.IsNotNull(sharepointFileCopyDesignerViewModel.ServerInputPathTo);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointFileUploadDesignerViewModel_SetPropertiesNullLocalPathFrom_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();

            modelItem.SetProperty("ServerInputPathTo", "TestFolder");
            modelItem.SetProperty("SharepointServerResourceId", Guid.NewGuid());
            //------------Execute Test---------------------------
            var sharepointFileCopyDesignerViewModel = new SharePointCopyFileDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointFileCopyDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointFileCopyDesignerViewModel.Validate();

            var inputPathTo = modelItem.GetProperty<string>("ServerInputPathTo");

            Assert.IsNotNull(inputPathTo);
            Assert.IsNotNull(sharepointFileCopyDesignerViewModel.ServerInputPathTo);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointFileUploadDesignerViewModel_SetPropertiesNullLocalPathTo_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();

            modelItem.SetProperty("ServerInputPathFrom", "TestFolder");
            modelItem.SetProperty("SharepointServerResourceId", Guid.NewGuid());
            //------------Execute Test---------------------------
            var sharepointFileCopyDesignerViewModel = new SharePointCopyFileDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointFileCopyDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointFileCopyDesignerViewModel.Validate();
        
            var inputPathfrom = modelItem.GetProperty<string>("ServerInputPathFrom");
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            sharepointFileCopyDesignerViewModel.UpdateHelpDescriptor("Test");
            Assert.IsNotNull(inputPathfrom);
            Assert.IsNotNull(sharepointFileCopyDesignerViewModel.ServerInputPathTo);
            //------------Assert Results-------------------------
        }
    }
}
