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
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.FormatNumber
{
    [TestClass]
    public class FormatNumberDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FormatNumberDesignerViewModel_Constructor")]
        public void FormatNumberDesignerViewModel_Constructor_ModelItemIsValid_SelectedRoundingTypeIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFormatNumberDesignerViewModel(modelItem);
            Assert.AreEqual("None", viewModel.SelectedRoundingType);
            Assert.AreEqual("None", viewModel.RoundingType);
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FormatNumberDesignerViewModel_Constructor")]
        public void FormatNumberDesignerViewModel_Constructor_ModelItemIsValid_RoundingTypesHasFourItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFormatNumberDesignerViewModel(modelItem);
            viewModel.Validate();
            Assert.AreEqual(4, viewModel.RoundingTypes.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("FormatNumberDesignerViewModel_Handle")]
        public void FormatNumberDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new TestFormatNumberDesignerViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FormatNumberDesignerViewModel_SetSelectedRoundingType")]
        public void FormatNumberDesignerViewModel_SetSelectedSelectedSort_ValidOrderType_RoundingTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFormatNumberDesignerViewModel(modelItem);
            const string ExpectedValue = "Normal";
            viewModel.SelectedRoundingType = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.RoundingType);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfNumberFormatActivity());
        }
    }
}
