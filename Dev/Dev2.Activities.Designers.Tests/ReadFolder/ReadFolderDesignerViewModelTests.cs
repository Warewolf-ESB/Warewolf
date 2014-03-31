using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.ReadFolder
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class ReadFolderDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ReadFolderDesignerViewModel_Constructor")]
        public void ReadFolderDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = ReadFolderViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("Directory", viewModel.InputPathLabel);
            Assert.AreEqual("", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Help");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ReadFolderDesignerViewModel_Validate")]
        public void ReadFolderDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var viewModel = ReadFolderViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(0, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ReadFolderDesignerViewModel_Constructor")]
        public void ReadFolderDesignerViewModel_Constructor_IsFilesSelectedIsSetToTrue()
        {
            //------------Setup for test-------------------------
            var viewModel = ReadFolderViewModel();
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsFilesSelected);
        }


        static TestReadFolderDesignerViewModel ReadFolderViewModel()
        {
            var viewModel = new TestReadFolderDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFolderRead()));
            return viewModel;
        }
    }
}
