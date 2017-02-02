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
using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.DateTimeDifference
{
    [TestClass]
    public class DateTimeDifferenceDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DateTimeDifferenceDesignerViewModel_Constructor")]
        public void DateTimeDifferenceDesignerViewModel_Constructor_ModelItemIsValid_SelectedOutputTypeIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestDateTimeDifferenceDesignerViewModel(modelItem);
            Assert.AreEqual("Years", viewModel.OutputType);
            Assert.AreEqual("Years", viewModel.SelectedOutputType);
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DateTimeDifferenceDesignerViewModel_Constructor")]
        public void DateTimeDifferenceDesignerViewModel_Constructor_ModelItemIsValid_SelectedOutputTypeAreInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestDateTimeDifferenceDesignerViewModel(modelItem);
            var expected = new List<string>(DateTimeComparer.OutputFormatTypes);
            CollectionAssert.AreEqual(expected, viewModel.OutputTypes);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DateTimeDifferenceDesignerViewModel_SetSelectedOutputType")]
        public void DateTimeDifferenceDesignerViewModel_SetSelectedOutputType_ValidType_OutputTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestDateTimeDifferenceDesignerViewModel(modelItem);
            viewModel.Validate();
            const string ExpectedValue = "Normal";
            viewModel.SelectedOutputType = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.OutputType);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DateTimeDifferenceDesignerViewModel_Handle")]
        public void DateTimeDifferenceDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new TestDateTimeDifferenceDesignerViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfDateTimeDifferenceActivity());
        }
    }
}
