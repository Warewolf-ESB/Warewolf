/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.SortRecords
{
    [TestClass]
    public class SortRecordsDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SortRecordsDesignerViewModel_Constructor")]

        public void SortRecordsDesignerViewModel_Constructor_ModelItemIsValid_SelectedSortIsInitialized()

        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            Assert.AreEqual("Forward", viewModel.SelectedSort);
            Assert.AreEqual("Forward", viewModel.SelectedSelectedSort);
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SortRecordsDesignerViewModel_Handle")]
        public void SortRecordsDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new TestSortRecordsDesignerViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SortRecordsDesignerViewModel_Constructor")]

        public void SortRecordsDesignerViewModel_Constructor_ModelItemIsValid_SortOrderTypesHasTwoItems()

        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            Assert.AreEqual(2, viewModel.SortOrderTypes.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SortRecordsDesignerViewModel_SetSelectedSelectedSort")]

        public void SortRecordsDesignerViewModel_SetSelectedSelectedSort_ValidOrderType_SelectedOrderTypeOnModelItemIsAlsoSet()

        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Backwards";
            viewModel.SelectedSelectedSort = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_RehydratesSortOrder")]

        public void SortRecordsDesignerViewModel_RehydratesSortOrder_ValidOrderType_ExpectUnderlyingValueBackwards()

        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort","Backwards");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Backwards";
          
            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_Validate")]

        public void SortRecordsDesignerViewModel_Validate_ValidatesSingleRegion_ExpectNoErrors()

        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "Backwards");
            modelItem.SetProperty("SortField","[[rec().a]]");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            viewModel.Validate();

            Assert.IsNull(viewModel.Errors);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_Validate")]

        public void SortRecordsDesignerViewModel_Validate_ValidatesSingleRegion_ExpectErrors()

        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "Backwards");
            modelItem.SetProperty("SortField", "[[rec.a()]],[[rec.b()]]");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            viewModel.Validate();
            Assert.AreEqual(1, viewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_Validate")]

        public void SortRecordsDesignerViewModel_Validate_ValidatesSingleRegion_NoErrors()

        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "Backwards");
            modelItem.SetProperty("SortField", "[[rec([[rec.b()]]).a]]");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            viewModel.Validate();
            Assert.IsNull(viewModel.Errors);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_RehydratesSortOrder")]

        public void SortRecordsDesignerViewModel_RehydratesSortOrder_ValidOrderType_ExpectUnderlyingValueForwards()

        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "Forward");

            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Forward";

            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_RehydratesSortOrder")]

        public void SortRecordsDesignerViewModel_RehydratesSortOrder_ValidOrderType_ExpectUnderlyingValueEmpty()

        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Forward";

            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }
        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSortRecordsActivity());
        }
    }
}
