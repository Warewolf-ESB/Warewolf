using System.Activities.Presentation.Model;
using Dev2.Common.Enums;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Activities.Designers.Tests.Script
{
    [TestClass][ExcludeFromCodeCoverage]
    public class ScriptDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ScriptDesignerViewModel_Constructor")]
        public void ScriptDesignerViewModel_Constructor_ModelItemIsValid_SelectedScriptTypeIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestScriptDesignerViewModel(modelItem);
            Assert.AreEqual(enScriptType.JavaScript, viewModel.ScriptType);
            Assert.AreEqual("JavaScript", viewModel.SelectedScriptType);
            Assert.AreEqual("JavaScript Syntax", viewModel.ScriptTypeDefaultText); 
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ScriptDesignerViewModel_Constructor")]
        public void ScriptDesignerViewModel_Constructor_ModelItemIsValid_ScriptTypesHasThreeItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestScriptDesignerViewModel(modelItem);
            Assert.AreEqual(3, viewModel.ScriptTypes.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ScriptDesignerViewModel_SetSelectedScriptType")]
        public void ScriptDesignerViewModel_SetSelectedScriptType_ValidScriptType_ScriptTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestScriptDesignerViewModel(modelItem);
            const string ExpectedValue = "Python";
            viewModel.SelectedScriptType = ExpectedValue;
            Assert.AreEqual(enScriptType.Python, viewModel.ScriptType);
            Assert.AreEqual("Python Syntax", viewModel.ScriptTypeDefaultText); 
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfScriptingActivity());
        }
    }
}