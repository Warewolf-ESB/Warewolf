using System.Windows.Controls;
using Dev2.Activities.Designers2.Core.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.DataBinding.Tests
{
    [TestClass]
    public class TestHelpAdorner : UiBindingTestBase
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("HelpAdorner_Bindings")]
        public void HelpAdorner_Bindings_GetBindings_HasBinding()
        {
            //------------Setup for test--------------------------
            var helpAdorner = new HelpAdorner(new TextBox());

            //------------Execute Test---------------------------
            var bindingList = GetBindings(helpAdorner);
            //------------Assert Results-------------------------
            Assert.IsTrue(bindingList.Count > 0);
            Assert.IsTrue(bindingList.ContainsBinding("ShowHelpToggleCommand"));
            Assert.IsTrue(bindingList.ContainsBinding("HelpText"));
            Assert.IsTrue(bindingList.ContainsBinding("ShowExampleWorkflowLink"));
        }

    }
}