using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.Random
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            var expected = Dev2EnumConverter.ConvertEnumsTypeToStringList<enRandomType>();
            CollectionAssert.AreEqual(expected.ToList(), viewModel.RandomTypes.ToList());
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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RandomDesignerViewModel_SelectedRandomType")]
        public void RandomDesignerViewModel_SelectedRandomType_GUID_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            var viewModel = new TestRandomDesignerViewModel(modelItem);

            //------------Execute Test---------------------------
            viewModel.SelectedRandomType = "GUID";

            //------------Assert Results-------------------------
            Assert.AreEqual(enRandomType.Guid, viewModel.RandomType);
            Assert.AreEqual(false, viewModel.IsLengthPath);
            Assert.AreEqual(Visibility.Hidden, viewModel.Visibility);
            Assert.AreEqual("Length", viewModel.LengthContent);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RandomDesignerViewModel_SelectedRandomType")]
        public void RandomDesignerViewModel_SelectedRandomType_Numbers_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            var viewModel = new TestRandomDesignerViewModel(modelItem);

            //------------Execute Test---------------------------
            viewModel.SelectedRandomType = "Numbers";

            //------------Assert Results-------------------------
            Assert.AreEqual(enRandomType.Numbers, viewModel.RandomType);
            Assert.AreEqual(false, viewModel.IsLengthPath);
            Assert.AreEqual(Visibility.Visible, viewModel.Visibility);
            Assert.AreEqual("Range", viewModel.LengthContent);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RandomDesignerViewModel_SelectedRandomType")]
        public void RandomDesignerViewModel_SelectedRandomType_Other_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            var viewModel = new TestRandomDesignerViewModel(modelItem);

            //------------Execute Test---------------------------
            viewModel.SelectedRandomType = "Letters";

            //------------Assert Results-------------------------
            Assert.AreEqual(enRandomType.Letters, viewModel.RandomType);
            Assert.AreEqual(true, viewModel.IsLengthPath);
            Assert.AreEqual(Visibility.Hidden, viewModel.Visibility);
            Assert.AreEqual("Length", viewModel.LengthContent);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfRandomActivity());
        }
    }
}