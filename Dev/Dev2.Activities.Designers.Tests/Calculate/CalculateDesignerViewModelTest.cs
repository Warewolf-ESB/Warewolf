using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Calculate;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Calculate
{
    [TestClass]
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
            calculateDesignerViewModel.Validate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(calculateDesignerViewModel);
            Assert.IsTrue(calculateDesignerViewModel.HasLargeView);
            Assert.AreEqual(0, calculateDesignerViewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("CalculateDesignerViewModel_Handle")]
        public void CalculateDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new CalculateDesignerViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
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
