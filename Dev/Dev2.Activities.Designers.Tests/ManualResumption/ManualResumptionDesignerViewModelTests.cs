/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.ManualResumption;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.ManualResumption
{
    [TestClass]
    public class ManualResumptionViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionViewModel))]
        public void ManualResumptionViewModel_Constructor_ModelItemIsValid_Initialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new ManualResumptionViewModel(modelItem);
            viewModel.Validate();
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionViewModel))]
        public void ManualResumptionViewModel_UpdateHelp_ShouldCallToHelpViewModel()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new ManualResumptionViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        static ModelItem CreateModelItem()
        {
           return ModelItemUtils.CreateModelItem(new ManualResumptionActivity());
        }
    }
}
