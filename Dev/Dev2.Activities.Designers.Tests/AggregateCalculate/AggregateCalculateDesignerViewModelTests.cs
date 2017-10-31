using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.AggregateCalculate;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System;

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
            var x = Guid.NewGuid();
            var y = Guid.NewGuid();
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            //------------Execute Test---------------------------
            var aggregateCalculateDesignerViewModel = new AggregateCalculateDesignerViewModel(modelItem);
            aggregateCalculateDesignerViewModel.Validate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(aggregateCalculateDesignerViewModel);
            Assert.IsTrue(aggregateCalculateDesignerViewModel.HasLargeView);
            Assert.AreEqual(0, aggregateCalculateDesignerViewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("AggregateCalculateDesignerViewModel_Handle")]
        public void AggregateCalculateDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------            
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new AggregateCalculateDesignerViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
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
