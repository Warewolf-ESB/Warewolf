using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Create
{
    [TestClass]    
    // ReSharper disable InconsistentNaming
    public class CreateDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CreateDesignerViewModel_Constructor")]
        public void CreateDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("", viewModel.InputPathLabel);
            Assert.AreEqual("File or Folder", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Help");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CreateDesignerViewModel_Validate")]
        public void CreateDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var viewModel = CreateViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(0, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
        }

        static TestCreateDesignerViewModel CreateViewModel()
        {
            var viewModel = new TestCreateDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfPathCreate()));
            return viewModel;
        }
    }
}
