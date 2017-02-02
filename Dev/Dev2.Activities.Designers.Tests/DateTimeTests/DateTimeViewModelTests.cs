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
using Dev2.Activities.Designers2.DateTime;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable CollectionNeverQueried.Local

namespace Dev2.Activities.Designers.Tests.DateTimeTests
{
    [TestClass]
    public class DateTimeViewModelTests
    {
        [TestMethod]
        [TestCategory("DateTimeActivityViewModel_SelectedTimeModifierTypeSelectedItem")]
        [Description("DateTime ViewModel clears the TimeModifierAmountDisplay property of the model item if the SelectedTimeModifierType property of the view model is set to a blank string")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void DateTimeActivityViewModel_SelectedTimeModifierTypeChange_SelectedTimeModifierTypeSetToABlankString_TimeModifierAmountDisplayCleared()
        // ReSharper restore InconsistentNaming
        {
            //init
            var expected = string.Empty;
            const string TimeModifierAmountDisplay = "TimeModifierAmountDisplay";

            var prop = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            prop.Setup(p => p.SetValue(expected)).Verifiable();
            properties.Add(TimeModifierAmountDisplay, prop);
            propertyCollection.Protected().Setup<ModelProperty>("Find", TimeModifierAmountDisplay, true).Returns(prop.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            //exe
            var viewModel = new DateTimeDesignerViewModel(mockModel.Object) { SelectedTimeModifierType = expected };
            viewModel.Validate();
            //assert
            prop.Verify(c => c.SetValue(expected), Times.Once(), "Find Records ViewModel does not clear the match data property of the model item when it's no longer needed");
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [TestCategory("DateTimeActivityViewModel_SelectedTimeModifierTypeSelectedItem")]
        [Description("DateTime ViewModel does not clear the TimeModifierAmountDisplay property of the model item if the SelectedTimeModifierType property of the view model is set to some string")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void DateTimeActivityViewModel_SelectedTimeModifierTypeChange_SelectedTimeModifierTypeSetToABlankString_TimeModifierAmountDisplayNotCleared()
        // ReSharper restore InconsistentNaming
        {
            //init
            var expected = "Some Data";
            const string MatchDataPropertyName = "TimeModifierAmountDisplay";

            var prop = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            prop.Setup(p => p.SetValue(expected)).Verifiable();
            properties.Add(MatchDataPropertyName, prop);
            propertyCollection.Protected().Setup<ModelProperty>("Find", MatchDataPropertyName, true).Returns(prop.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            //exe
            new DateTimeDesignerViewModel(mockModel.Object) { SelectedTimeModifierType = expected };

            //assert
            prop.Verify(c => c.SetValue(expected), Times.Never(), "Find Records ViewModel does not clear the match data property of the model item when it's no longer needed");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DateTimeActivityViewModel_Handle")]
        public void DateTimeActivityViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var expected = string.Empty;
            const string TimeModifierAmountDisplay = "TimeModifierAmountDisplay";

            var prop = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            prop.Setup(p => p.SetValue(expected)).Verifiable();
            properties.Add(TimeModifierAmountDisplay, prop);
            propertyCollection.Protected().Setup<ModelProperty>("Find", TimeModifierAmountDisplay, true).Returns(prop.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var viewModel = new DateTimeDesignerViewModel(mockModel.Object) { SelectedTimeModifierType = expected };
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }
    }
}
