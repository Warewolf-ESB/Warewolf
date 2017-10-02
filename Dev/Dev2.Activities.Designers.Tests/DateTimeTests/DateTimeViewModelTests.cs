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
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Globalization;

namespace Dev2.Activities.Designers.Tests.DateTimeTests
{
    [TestClass]
    public class DateTimeViewModelTests
    {
        [TestMethod]
        [TestCategory("DateTimeActivityViewModel_SelectedTimeModifierTypeSelectedItem")]
        [Description("DateTime ViewModel clears the TimeModifierAmountDisplay property of the model item if the SelectedTimeModifierType property of the view model is set to a blank string")]
        [Owner("Ashley Lewis")]
        
        public void DateTimeActivityViewModel_SelectedTimeModifierTypeChange_SelectedTimeModifierTypeSetToABlankString_TimeModifierAmountDisplayCleared()

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
        
        public void DateTimeActivityViewModel_SelectedTimeModifierTypeChange_SelectedTimeModifierTypeSetToABlankString_TimeModifierAmountDisplayNotCleared()

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
        public void DateTimeDesignerViewModel_ShouldSetInputFormat_WhenNoInputFormat()
        {
            var modelItem = CreateModelItem();
            var viewModel = new DateTimeDesignerViewModel(modelItem);
            var expectedDefaultFormat = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern;
            var po = new PrivateObject(viewModel);
            Assert.AreEqual(expectedDefaultFormat, po.GetProperty("InputFormat"));
        }

        [TestMethod]
        public void DateTimeDesignerViewModel_ShouldNotSetInputFormat_WhenInputFormat()
        {
            var modelItem = CreateModelItemWithInputFormat("yyyy-mm-dd");
            var viewModel = new DateTimeDesignerViewModel(modelItem);
            var expectedDefaultFormat = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern;
            var po = new PrivateObject(viewModel);
            Assert.AreNotEqual(expectedDefaultFormat, po.GetProperty("InputFormat"));
            Assert.AreEqual("yyyy-mm-dd", po.GetProperty("InputFormat"));
        }

        [TestMethod]
        public void DateTimeDesignerViewModel_ShouldSetOutputFormat_WhenNoInputFormat()
        {
            var modelItem = CreateModelItem();
            var viewModel = new DateTimeDesignerViewModel(modelItem);
            var expectedDefaultFormat = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern;
            var po = new PrivateObject(viewModel);
            Assert.AreEqual(expectedDefaultFormat, po.GetProperty("OutputFormat"));
        }

        [TestMethod]
        public void DateTimeDesignerViewModel_ShouldNotSetOutputFormat_WhenOutputFormat()
        {
            var modelItem = CreateModelItemWithOutputFormat("yyyy-mm-dd");
            var viewModel = new DateTimeDesignerViewModel(modelItem);
            var expectedDefaultFormat = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern;
            var po = new PrivateObject(viewModel);
            Assert.AreNotEqual(expectedDefaultFormat, po.GetProperty("OutputFormat"));
            Assert.AreEqual("yyyy-mm-dd", po.GetProperty("OutputFormat"));
        }

        [TestMethod]
        public void DateTimeDesignerViewModel_ShouldNotSetInputOrOutputFormat_WhenInputAndOutputFormat()
        {
            var modelItem = CreateModelItemWithInputOutputFormat("yyyy-mm-dd","MM/dd/yyyy");
            var viewModel = new DateTimeDesignerViewModel(modelItem);
            var expectedDefaultFormat = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern;
            var po = new PrivateObject(viewModel);
            Assert.AreNotEqual(expectedDefaultFormat, po.GetProperty("InputFormat"));
            Assert.AreEqual("yyyy-mm-dd", po.GetProperty("InputFormat"));
            Assert.AreNotEqual(expectedDefaultFormat, po.GetProperty("OutputFormat"));
            Assert.AreEqual("MM/dd/yyyy", po.GetProperty("OutputFormat"));
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

            var mockMainViewModel = new Mock<IShellViewModel>();
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

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfDateTimeActivity());
        }

        static ModelItem CreateModelItemWithInputFormat(string dateTimeFormat)
        {
            return ModelItemUtils.CreateModelItem(new DsfDateTimeActivity
            {
                InputFormat = dateTimeFormat
            });
        }

        static ModelItem CreateModelItemWithOutputFormat(string dateTimeFormat)
        {
            return ModelItemUtils.CreateModelItem(new DsfDateTimeActivity
            {
                OutputFormat = dateTimeFormat
            });
        }

        static ModelItem CreateModelItemWithInputOutputFormat(string inputDateTimeFormat,string outputDateTimeFormat)
        {
            return ModelItemUtils.CreateModelItem(new DsfDateTimeActivity
            {
                InputFormat = inputDateTimeFormat,
                OutputFormat = outputDateTimeFormat
            });
        }
    }
}
