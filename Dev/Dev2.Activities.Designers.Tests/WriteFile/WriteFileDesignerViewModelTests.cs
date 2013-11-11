using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.WriteFile
{
    [TestClass]
    public class WriteFileDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WriteFileDesignerViewModel_Constructor")]
        public void WriteFileDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = WriteFileViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("File Name", viewModel.InputPathLabel);
            Assert.AreEqual("", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(2, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Large");
            StringAssert.Contains(viewModel.TitleBarToggles[1].ExpandToolTip, "Help");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WriteFileDesignerViewModel_Validate")]
        public void WriteFileDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var viewModel = WriteFileViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(0, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
            Assert.IsFalse(viewModel.ValidateOutputPathIsRequired);
            Assert.IsTrue(viewModel.ValidateInputPathIsRequired);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WriteFileDesignerViewModel_Constructor")]
        public void WriteFileDesignerViewModel_Contructor_OverwriteIsSetToTrue()
        {
            //------------Setup for test-------------------------
            var viewModel = WriteFileViewModel();
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.Overwrite);
        }

        static TestWriteFileDesignerViewModel WriteFileViewModel()
        {
            var viewModel = new TestWriteFileDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFileWrite()));
            return viewModel;
        }
    }
}
