using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.Script
{
    [TestClass]    
    public class ScriptDesignerViewModelTests
    {

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ScriptDesignerViewModel_Constructor")]
        public void ScriptDesignerViewModel_Constructor_PropertiesInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestScriptDesignerViewModel(modelItem);
            var expected = Dev2EnumConverter.ConvertEnumsTypeToStringList<enScriptType>();
            CollectionAssert.AreEqual(expected.ToList(), viewModel.ScriptTypes.ToList());
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ScriptDesignerViewModel_SelectedScriptType")]
        public void ScriptDesignerViewModel_SelectedScriptType_JavaScript_PropertiesInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestScriptDesignerViewModel(modelItem);
            Assert.AreEqual(enScriptType.JavaScript, viewModel.ScriptType);
            Assert.AreEqual("JavaScript", viewModel.SelectedScriptType);
            Assert.AreEqual("JavaScript Syntax", viewModel.ScriptTypeDefaultText);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ScriptDesignerViewModel_SelectedScriptType")]
        public void ScriptDesignerViewModel_SelectedScriptType_Python_PropertiesInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestScriptDesignerViewModel(modelItem);
            const string ExpectedValue = "Python";
            viewModel.SelectedScriptType = ExpectedValue;
            Assert.AreEqual(enScriptType.Python, viewModel.ScriptType);
            Assert.AreEqual("Python Syntax", viewModel.ScriptTypeDefaultText);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ScriptDesignerViewModel_SelectedScriptType")]
        public void ScriptDesignerViewModel_SelectedScriptType_Ruby_PropertiesInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestScriptDesignerViewModel(modelItem);
            const string ExpectedValue = "Ruby";
            viewModel.SelectedScriptType = ExpectedValue;
            Assert.AreEqual(enScriptType.Ruby, viewModel.ScriptType);
            Assert.AreEqual("Ruby Syntax", viewModel.ScriptTypeDefaultText);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfScriptingActivity());
        }
    }
}