using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Converters.DateAndTime;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            const string ExpectedValue = "Normal";
            viewModel.SelectedOutputType = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.OutputType);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfDateTimeDifferenceActivity());
        }
    }
}