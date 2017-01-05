/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Lookups;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Zip
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class ZipDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ZipDesignerViewModel_Constructor")]
        public void ZipDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = ZipViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("File or Folder", viewModel.InputPathLabel);
            Assert.AreEqual("Destination", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ZipDesignerViewModel_Handle")]
        public void ZipDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = ZipViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ZipDesignerViewModel_Validate")]
        public void ZipDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new ResourceModel(null));
            DataListSingleton.SetDataList(dataListViewModel);
            var viewModel = ZipViewModel();
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------            
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
            Assert.AreEqual(1, viewModel.ValidateDestinationUsernameAndPasswordHitCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ZipDesignerViewModel_Constructor")]
        public void ZipDesignerViewModel_Constructor_ModelItemIsValid_SelectedCompressionRatioIsInitialized()
        {
            var viewModel = ZipViewModel();
            Assert.AreEqual("Default", viewModel.CompressionRatio);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ZipDesignerViewModel_Constructor")]
        public void ZipDesignerViewModel_Constructor_ModelItemIsValid_CompressionRatiosHasThreeItems()
        {
            var viewModel = ZipViewModel();
            Assert.AreEqual(4, viewModel.CompressionRatioList.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ZipDesignerViewModel_SetSelectedCompressionRatio")]
        public void ZipDesignerViewModel_SetSelectedCompressionRatio_ValidCompressionRatio_CompressionRatioOnModelItemIsAlsoSet()
        {
            var viewModel = ZipViewModel();
            viewModel.SelectedCompressionRatioDescription = CompressionRatios.BestSpeed.GetDescription();
            Assert.AreEqual("BestSpeed", viewModel.CompressionRatio);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ZipDesignerViewModel_Constructor")]
        public void ZipDesignerViewModel_Constructor_InitList_ListMatchesEnumDescriptions()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var zipDesignerViewModel_Constructor = ZipViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual(CompressionRatios.None.GetDescription(), zipDesignerViewModel_Constructor.CompressionRatioList[0]);
            Assert.AreEqual(CompressionRatios.BestSpeed.GetDescription(), zipDesignerViewModel_Constructor.CompressionRatioList[1]);
            Assert.AreEqual(CompressionRatios.Default.GetDescription(), zipDesignerViewModel_Constructor.CompressionRatioList[2]);
            Assert.AreEqual(CompressionRatios.BestCompression.GetDescription(), zipDesignerViewModel_Constructor.CompressionRatioList[3]);
        }

        static TestZipDesignerViewModel ZipViewModel()
        {
            var viewModel = new TestZipDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfZip()));
            return viewModel;
        }
    }
}
