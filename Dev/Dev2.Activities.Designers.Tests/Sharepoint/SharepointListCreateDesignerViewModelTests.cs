using Dev2.Activities.Designers2.SharepointListCreate;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Activities.Presentation.Model;

namespace Dev2.Activities.Designers.Tests.Sharepoint
{
    [TestClass]
    public class SharepointListCreateDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharepointListCreateDesignerViewModel))]
        public void SharepointListCreateDesignerViewModel_ShouldCall_UpdateHelpDescriptor()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);

            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();

            var sharepointCreateListItemActivity = new SharepointCreateListItemActivity();

            using (var viewModel = new SharepointListCreateDesignerViewModel(CreateModelItem(sharepointCreateListItemActivity), mockAsyncWorker.Object, mockServer.Object, mockMainViewModel.Object))
            {
                Assert.AreEqual("FilterCriteria", viewModel.CollectionName);
                //------------Execute Test---------------------------
                viewModel.UpdateHelpDescriptor("help");
                //------------Assert Results-------------------------
                mockHelpViewModel.Verify(model => model.UpdateHelpText("help"), Times.Once());
                Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Create_List_Item, viewModel.HelpText);
            }
        }

        static ModelItem CreateModelItem(SharepointCreateListItemActivity sharepointCreateListItemActivity)
        {
            return ModelItemUtils.CreateModelItem(sharepointCreateListItemActivity);
        }
    }
}
