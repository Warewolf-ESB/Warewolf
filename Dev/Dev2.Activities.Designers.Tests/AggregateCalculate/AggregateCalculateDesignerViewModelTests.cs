using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.AggregateCalculate;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.AggregateCalculate
{
    [TestClass]
    public class AggregateCalculateDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("AggregateCalculateDesignerViewModel_Constructor")]
        public void AggregateCalculateDesignerViewModel_Constructor_ModelItemIsValid_Result()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();

            //------------Execute Test---------------------------
            var aggregateCalculateDesignerViewModel = new AggregateCalculateDesignerViewModel(modelItem);

            //------------Assert Results-------------------------
            Assert.IsNotNull(aggregateCalculateDesignerViewModel);
            Assert.IsInstanceOfType(aggregateCalculateDesignerViewModel, typeof(ActivityDesignerViewModel));
            Assert.AreEqual("Aggregate Calculate", aggregateCalculateDesignerViewModel.ModelItem.GetProperty("DisplayName"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("AggregateCalculateDesignerViewModel_Constructor")]
        public void AggregateCalculateDesignerViewModel_Constructor_Constructed_HasHelpLargeViewToogle()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            //------------Execute Test---------------------------
            var aggregateCalculateDesignerViewModel = new AggregateCalculateDesignerViewModel(modelItem);
            //------------Assert Results-------------------------
            Assert.IsNotNull(aggregateCalculateDesignerViewModel);
            Assert.IsTrue(aggregateCalculateDesignerViewModel.HasLargeView);
            Assert.AreEqual(0, aggregateCalculateDesignerViewModel.TitleBarToggles.Count);
        }

        static ModelItem CreateModelItem()
        {
            var calculateActivity = new DsfCalculateActivity { DisplayName = "Aggregate Calculate" };
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
