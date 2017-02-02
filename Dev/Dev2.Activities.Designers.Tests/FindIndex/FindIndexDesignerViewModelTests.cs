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
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.FindIndex
{
    [TestClass]
    public class FindIndexDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindIndexDesignerViewModel_Constructor")]
        public void FindIndexDesignerViewModel_Constructor_ModelItemIsValid_SelectedIndexIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            Assert.AreEqual("First Occurrence", viewModel.Index);
            Assert.AreEqual("First Occurrence", viewModel.SelectedIndex);
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindIndexDesignerViewModel_Constructor")]
        public void FindIndexDesignerViewModel_Constructor_ModelItemIsValid_IndexListHasThreeItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);

            var expectedIndices = new List<string> { "First Occurrence", "Last Occurrence", "All Occurrences" };
            var expectedDirections = new List<string> { "Left to Right", "Right to Left" };

            CollectionAssert.AreEqual(expectedIndices, viewModel.IndexList.ToList());
            CollectionAssert.AreEqual(expectedDirections, viewModel.DirectionList.ToList());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindIndexDesignerViewModel_SetSelectedIndex")]
        public void FindIndexDesignerViewModel_SetSelectedIndex_ValidIndex_IndexOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            const string ExpectedValue = "Last Occurrence";
            viewModel.SelectedIndex = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.Index);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindDirectionDesignerViewModel_Constructor")]
        public void FindDirectionDesignerViewModel_Constructor_ModelItemIsValid_SelectedDirectionIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            Assert.AreEqual("Left to Right", viewModel.Direction);
            Assert.AreEqual("Left to Right", viewModel.SelectedDirection);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindDirectionDesignerViewModel_Constructor")]
        public void FindDirectionDesignerViewModel_Constructor_ModelItemIsValid_DirectionListHasTowItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            viewModel.Validate();
            Assert.AreEqual(2, viewModel.DirectionList.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("FindDirectionDesignerViewModel_Handle")]
        public void FindDirectionDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindDirectionDesignerViewModel_SetSelectedDirection")]
        public void FindDirectionDesignerViewModel_SetSelectedDirection_ValidDirection_DirectionOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            const string ExpectedValue = "Right to Left";
            viewModel.SelectedDirection = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.Direction);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfIndexActivity());
        }
    }
}
