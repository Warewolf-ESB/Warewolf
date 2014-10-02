
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
        [Ignore]//21/01/2014 - Ashley: MSTest.exe cannot run this test
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
