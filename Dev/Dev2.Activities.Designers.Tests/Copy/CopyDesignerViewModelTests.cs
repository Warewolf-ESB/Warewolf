/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Copy
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class CopyDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CopyDesignerViewModel_Constructor")]
        public void CopyDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = CopyViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("File or Folder", viewModel.InputPathLabel);
            Assert.AreEqual("Destination", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CopyDesignerViewModel_Validate")]
        public void CopyDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><a></a></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            var viewModel = CopyViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
            Assert.AreEqual(1, viewModel.ValidateDestinationUsernameAndPasswordHitCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("CopyDesignerViewModel_Handle")]
        public void CopyDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = CopyViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        static TestCopyDesignerViewModel CopyViewModel()
        {
            var viewModel = new TestCopyDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfPathCopy()));
            return viewModel;
        }
    }
}
