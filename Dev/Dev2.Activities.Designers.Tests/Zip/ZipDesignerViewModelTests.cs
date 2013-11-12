using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Zip
{
    [TestClass][ExcludeFromCodeCoverage]
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
            Assert.AreEqual(2, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Large");
            StringAssert.Contains(viewModel.TitleBarToggles[1].ExpandToolTip, "Help");
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
            Assert.AreEqual(0, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ZipDesignerViewModel_Constructor")]
        public void ZipDesignerViewModel_Constructor_ModelItemIsValid_SelectedCompressionRatioIsInitialized()
        {
            var viewModel = ZipViewModel();
            Assert.AreEqual("No Compression", viewModel.CompressionRatio);
            Assert.AreEqual(viewModel.CompressionTypes[0], viewModel.SelectedCompressionRatio);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ZipDesignerViewModel_Constructor")]
        public void ZipDesignerViewModel_Constructor_ModelItemIsValid_CompressionRatiosHasThreeItems()
        {
            var viewModel = ZipViewModel();
            Assert.AreEqual(4, viewModel.CompressionTypes.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ZipDesignerViewModel_SetSelectedCompressionRatio")]
        public void ZipDesignerViewModel_SetSelectedCompressionRatio_ValidCompressionRatio_CompressionRatioOnModelItemIsAlsoSet()
        {
            var viewModel = ZipViewModel();
            viewModel.SelectedCompressionRatio = viewModel.CompressionTypes[1];
            Assert.AreEqual("Best Speed", viewModel.CompressionRatio);
        }

        static TestZipDesignerViewModel ZipViewModel()
        {
            var viewModel = new TestZipDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfZip()));
            return viewModel;
        }
    }
}
