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

namespace Dev2.Activities.Designers.Tests.Delete
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DeleteDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DeleteDesignerViewModel_Constructor")]
        public void DeleteDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = DeleteViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("File or Folder", viewModel.InputPathLabel);
            Assert.AreEqual("", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.TitleBarToggles.Count);
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DeleteDesignerViewModel_Handle")]
        public void DeleteDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = DeleteViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DeleteDesignerViewModel_Validate")]
        public void DeleteDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><a></a></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            var viewModel = DeleteViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(0, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
        }

        static TestDeleteDesignerViewModel DeleteViewModel()
        {
            var viewModel = new TestDeleteDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfPathDelete()));
            return viewModel;
        }
    }
}
