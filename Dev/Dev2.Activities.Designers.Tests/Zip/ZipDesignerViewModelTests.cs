using System.Diagnostics.CodeAnalysis;
using Dev2.Common.ExtMethods;
using Dev2.Common.Lookups;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Zip
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Help");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ZipDesignerViewModel_Validate")]
        public void ZipDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
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
            Assert.AreEqual(CompressionRatios.NoCompression.GetDescription(), zipDesignerViewModel_Constructor.CompressionRatioList[0]);
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
