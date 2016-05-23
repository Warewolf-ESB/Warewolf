using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.Calculate;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Calculate
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CalculateDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("CalculateDesignerViewModel_Constructor")]
        public void CalculateDesignerViewModel_Constructor_ModelItemIsValid_Result()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            
            //------------Execute Test---------------------------
            var calculateDesignerViewModel = new CalculateDesignerViewModel(modelItem);

            //------------Assert Results-------------------------
            Assert.IsNotNull(calculateDesignerViewModel);
            Assert.IsInstanceOfType(calculateDesignerViewModel, typeof(ActivityDesignerViewModel));
            Assert.AreEqual("Calculate", calculateDesignerViewModel.ModelItem.GetProperty("DisplayName"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("CalculateDesignerViewModel_Constructor")]
        public void CalculateDesignerViewModel_Constructor_Constructed_HasHelpLargeViewToogle()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            //------------Execute Test---------------------------
            var calculateDesignerViewModel = new CalculateDesignerViewModel(modelItem);
            //------------Assert Results-------------------------
            Assert.IsNotNull(calculateDesignerViewModel);
            Assert.IsTrue(calculateDesignerViewModel.HasLargeView);
            Assert.AreEqual(0, calculateDesignerViewModel.TitleBarToggles.Count);
        }

        static ModelItem CreateModelItem()
        {
            var calculateActivity = new DsfCalculateActivity { DisplayName = "Calculate" };
            var modelItem = CreateModelItem(calculateActivity);
            return modelItem;
        }

        static ModelItem CreateModelItem(DsfCalculateActivity calculateActivity)
        {
            var modelItem = ModelItemUtils.CreateModelItem(calculateActivity);
            return modelItem;
        }
    }
}
