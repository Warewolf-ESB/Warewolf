/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.SharepointListDelete;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Sharepoint
{
    [TestClass]
    public class SharepointListDeleteDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharepointListDeleteDesignerViewModel))]
        public void SharepointListDeleteDesignerViewModel_ShouldCall_UpdateHelpDescriptor()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);

            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();

            var sharepointDeleteListItemActivity = new SharepointDeleteListItemActivity();

            using (var viewModel = new SharepointListDeleteDesignerViewModel(CreateModelItem(sharepointDeleteListItemActivity), mockAsyncWorker.Object, mockServer.Object, mockMainViewModel.Object))
            {
                Assert.AreEqual("FilterCriteria", viewModel.CollectionName);
                Assert.AreEqual(9, viewModel.WhereOptions.Count);
                //------------Execute Test---------------------------
                viewModel.UpdateHelpDescriptor("help");
                //------------Assert Results-------------------------
                mockHelpViewModel.Verify(model => model.UpdateHelpText("help"), Times.Once());
                Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Delete_List_Item, viewModel.HelpText);
            }
        }

        static ModelItem CreateModelItem(SharepointDeleteListItemActivity sharepointDeleteListItemActivity)
        {
            return ModelItemUtils.CreateModelItem(sharepointDeleteListItemActivity);
        }
    }
}
