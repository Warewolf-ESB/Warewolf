/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Linq;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;
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
            Assert.IsTrue(viewModel.EscapeScript);
            var expected = Dev2EnumConverter.ConvertEnumsTypeToStringList<enScriptType>();
            CollectionAssert.AreEqual(expected.ToList(), viewModel.ScriptTypes.ToList());
            Assert.IsTrue(viewModel.HasLargeView);
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
            Assert.IsTrue(viewModel.EscapeScript);
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
            Assert.IsTrue(viewModel.EscapeScript);
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
            Assert.IsTrue(viewModel.EscapeScript);
            Assert.AreEqual("Ruby Syntax", viewModel.ScriptTypeDefaultText);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfScriptingActivity());
        }
    }
}
