using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.SharePointDeleteFile;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Threading;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;

namespace Dev2.Activities.Designers.Tests.Sharepoint
{
    [TestClass]
    public class SharePointDeleteFileDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointDeleteFileDesignerViewModel))]
        public void SharePointDeleteFileDesignerViewModel_ShouldCall_UpdateHelpDescriptor()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);

            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();

            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity();

            using (var viewModel = new SharePointDeleteFileDesignerViewModel(CreateModelItem(sharepointDeleteFileActivity), mockAsyncWorker.Object, mockServer.Object, mockMainViewModel.Object))
            {
                Assert.AreEqual("FilterCriteria", viewModel.CollectionName);
                //------------Execute Test---------------------------
                viewModel.UpdateHelpDescriptor("help");
                //------------Assert Results-------------------------
                mockHelpViewModel.Verify(model => model.UpdateHelpText("help"), Times.Once());
                Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Delete_File, viewModel.HelpText);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointDeleteFileDesignerViewModel))]
        public void SharePointDeleteFileDesignerViewModel_ValidateThis_ClearErrors()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);

            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();

            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity
            {
                SharepointServerResourceId = Guid.NewGuid(),
                ServerInputPath = "Path"
            };

            using (var viewModel = new SharePointDeleteFileDesignerViewModel(CreateModelItem(sharepointDeleteFileActivity), mockAsyncWorker.Object, mockServer.Object, mockMainViewModel.Object))
            {
                viewModel.Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo
                    {
                        Message = "Empty Error"
                    }
                };
                //------------Execute Test---------------------------
                viewModel.Validate();
                //------------Assert Results-------------------------
                Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Delete_File, viewModel.HelpText);
                Assert.IsNull(viewModel.Errors);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointDeleteFileDesignerViewModel))]
        public void SharePointDeleteFileDesignerViewModel_ValidateThis_SharepointServerRequired()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);

            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();

            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity();

            using (var viewModel = new SharePointDeleteFileDesignerViewModel(CreateModelItem(sharepointDeleteFileActivity), mockAsyncWorker.Object, mockServer.Object, mockMainViewModel.Object))
            {
                viewModel.SharepointServerResourceId = Guid.Empty;
                //------------Execute Test---------------------------
                viewModel.Validate();
                //------------Assert Results-------------------------
                Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Delete_File, viewModel.HelpText);
                Assert.AreEqual(1, viewModel.Errors.Count);
                Assert.AreEqual(ErrorResource.SharepointServerRequired, viewModel.Errors[0].Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointDeleteFileDesignerViewModel))]
        public void SharePointDeleteFileDesignerViewModel_ValidateThis_SharepointServerPathRequired()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);

            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();

            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity
            {
                SharepointServerResourceId = Guid.NewGuid()
            };

            using (var viewModel = new SharePointDeleteFileDesignerViewModel(CreateModelItem(sharepointDeleteFileActivity), mockAsyncWorker.Object, mockServer.Object, mockMainViewModel.Object))
            {
                //------------Execute Test---------------------------
                viewModel.Validate();
                //------------Assert Results-------------------------
                Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Delete_File, viewModel.HelpText);
                Assert.AreEqual(1, viewModel.Errors.Count);
                Assert.AreEqual(ErrorResource.SharepointServerPathRequired, viewModel.Errors[0].Message);
            }
        }

        static ModelItem CreateModelItem(SharepointDeleteFileActivity sharepointDeleteFileActivity)
        {
            return ModelItemUtils.CreateModelItem(sharepointDeleteFileActivity);
        }
    }
}
