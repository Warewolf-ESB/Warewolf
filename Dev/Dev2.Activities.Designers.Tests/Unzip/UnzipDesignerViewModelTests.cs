using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Unzip
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class UnzipDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("UnzipDesignerViewModel_Constructor")]
        public void UnzipDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = UnzipViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("Zip Name", viewModel.InputPathLabel);
            Assert.AreEqual("Destination", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Help");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("UnzipDesignerViewModel_Validate")]
        public void UnzipDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var viewModel = UnzipViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
            Assert.AreEqual(1, viewModel.ValidateDestinationUsernameAndPasswordHitCount);
        }

        static TestUnzipDesignerViewModel UnzipViewModel()
        {
            var viewModel = new TestUnzipDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFileRead()));
            return viewModel;
        }
    }
}
