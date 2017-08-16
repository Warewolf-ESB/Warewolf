using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.SharePointMoveFile;
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
    public class SharePointFileMoveViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "SharePoint";

        private ModelItem CreateModelItem()
        {
            var fileUploadactivity = new SharepointMoveFileActivity();
           
            return ModelItemUtils.CreateModelItem(fileUploadactivity);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointMoveFileDesignerViewModel_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointFileMoveDesignerViewModel = new SharePointMoveFileDesignerViewModel(CreateModelItem());
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointFileMoveDesignerViewModel);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointMoveFileDesignerViewModel_Constructor_NullAsyncWorker_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            
            new SharePointMoveFileDesignerViewModel(CreateModelItem(), null, new Mock<IServer>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharePointMoveFileDesignerViewModel_Constructor_NullEnvironmentModel_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            
            new SharePointMoveFileDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointMoveFileDesignerViewModel_InitilizeProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointFileMoveDesignerViewModel = new SharePointMoveFileDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), new Mock<IServer>().Object);

            sharepointFileMoveDesignerViewModel.UpdateHelpDescriptor("Test");

            Assert.IsNotNull(sharepointFileMoveDesignerViewModel);
            Assert.IsNotNull(sharepointFileMoveDesignerViewModel.CollectionName);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointMoveFileDesignerViewModel_SetProperties_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
            modelItem.SetProperty("ServerInputPathFrom", "TestFolder");
            modelItem.SetProperty("ServerInputPathTo", "TestFolder");
            modelItem.SetProperty("SharepointServerResourceId", Guid.NewGuid());
            //------------Execute Test---------------------------
            var sharepointFileMoveDesignerViewModel = new SharePointMoveFileDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointFileMoveDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointFileMoveDesignerViewModel.Validate();
            var inputPathfrom = modelItem.GetProperty<string>("ServerInputPathFrom");
            var inputPathTo = modelItem.GetProperty<string>("ServerInputPathTo");
            var sourceId = modelItem.GetProperty<Guid>("SharepointServerResourceId");


            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            sharepointFileMoveDesignerViewModel.UpdateHelpDescriptor("Test");

            Assert.IsNotNull(inputPathfrom);
            Assert.IsNotNull(inputPathTo);
            Assert.IsNotNull(sharepointFileMoveDesignerViewModel.ServerInputPathFrom);
            Assert.IsNotNull(sharepointFileMoveDesignerViewModel.ServerInputPathTo);
            Assert.AreNotEqual(sourceId, Guid.Empty);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointMoveFileDesignerViewModel_SetPropertiesNullSource_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();
            modelItem.SetProperty("ServerInputPathFrom", "TestFolder");
            modelItem.SetProperty("ServerInputPathTo", "TestFolder");

            //------------Execute Test---------------------------
            var sharepointFileMoveDesignerViewModel = new SharePointMoveFileDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointFileMoveDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointFileMoveDesignerViewModel.Validate();
            var inputPathfrom = modelItem.GetProperty<string>("ServerInputPathFrom");
            var inputPathTo = modelItem.GetProperty<string>("ServerInputPathTo");
  

            Assert.IsNotNull(inputPathfrom);
            Assert.IsNotNull(inputPathTo);
            Assert.IsNotNull(sharepointFileMoveDesignerViewModel.ServerInputPathFrom);
            Assert.IsNotNull(sharepointFileMoveDesignerViewModel.ServerInputPathTo);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointMoveFileDesignerViewModel_SetPropertiesNullLocalPathFrom_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();

            modelItem.SetProperty("ServerInputPathTo", "TestFolder");
            modelItem.SetProperty("SharepointServerResourceId", Guid.NewGuid());
            //------------Execute Test---------------------------
            var sharepointFileMoveDesignerViewModel = new SharePointMoveFileDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointFileMoveDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointFileMoveDesignerViewModel.Validate();

            var inputPathTo = modelItem.GetProperty<string>("ServerInputPathTo");

            Assert.IsNotNull(inputPathTo);
            Assert.IsNotNull(sharepointFileMoveDesignerViewModel.ServerInputPathTo);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void SharePointMoveFileDesignerViewModel_SetPropertiesNullLocalPathTo_ReturnsSuccess()
        {
            //------------Setup for test--------------------------

            var modelItem = CreateModelItem();

            modelItem.SetProperty("ServerInputPathFrom", "TestFolder");
            modelItem.SetProperty("SharepointServerResourceId", Guid.NewGuid());
            //------------Execute Test---------------------------
            var sharepointFileMoveDesignerViewModel = new SharePointMoveFileDesignerViewModel(modelItem, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            sharepointFileMoveDesignerViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please Select a SharePoint Server" } };
            sharepointFileMoveDesignerViewModel.Validate();
        
            var inputPathfrom = modelItem.GetProperty<string>("ServerInputPathFrom");

            Assert.IsNotNull(inputPathfrom);
            Assert.IsNotNull(sharepointFileMoveDesignerViewModel.ServerInputPathTo);
            //------------Assert Results-------------------------
        }
    }
}
