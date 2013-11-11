using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Replace;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Replace
{
    [TestClass]
    public class ReplaceDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ReplaceDesignerViewModel_Constructor")]
        public void ReplaceDesignerViewModel_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new ReplaceDesignerViewModel(CreateModelItem());

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Help");
        }


        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfReplaceActivity());
        }

        static ReplaceDesignerViewModel CreateViewModel()
        {
            var viewModel = new ReplaceDesignerViewModel(ModelItemUtils.CreateModelItem(CreateModelItem()));
            return viewModel;
        }
    }
}
