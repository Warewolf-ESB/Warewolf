using System.Activities.Presentation.Model;
using Dev2.Common.Enums;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Activities.Designers.Tests.Random
{
    [TestClass][ExcludeFromCodeCoverage]
    public class RandomDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RandomDesignerViewModel_Constructor")]
        public void RandomDesignerViewModel_Constructor_ModelItemIsValid_SelectedRandomTypeIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestRandomDesignerViewModel(modelItem);
            Assert.AreEqual(enRandomType.Numbers, viewModel.RandomType);
            Assert.AreEqual("Numbers", viewModel.SelectedRandomType);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RandomDesignerViewModel_Constructor")]
        public void RandomDesignerViewModel_Constructor_ModelItemIsValid_RandomTypesHasThreeItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestRandomDesignerViewModel(modelItem);
            Assert.AreEqual(4, viewModel.RandomTypes.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RandomDesignerViewModel_SetSelectedRandomType")]
        public void RandomDesignerViewModel_SetSelectedRandomType_ValidRandomType_RandomTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestRandomDesignerViewModel(modelItem);
            const string ExpectedValue = "GUID";
            viewModel.SelectedRandomType = ExpectedValue;
            Assert.AreEqual(enRandomType.Guid, viewModel.RandomType);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfRandomActivity());
        }
    }
}