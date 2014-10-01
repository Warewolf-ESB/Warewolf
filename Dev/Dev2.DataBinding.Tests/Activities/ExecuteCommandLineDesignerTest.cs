
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities.Designers2.CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.DataBinding.Tests.Activities
{
    [TestClass]
    public class ExecuteCommandLineDesignerTest : UiBindingTestBase
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ExecuteCommandLineDesigner_Bindings")]
        [Ignore]//21/01/2014 - Ashley: MSTest.exe cannot run this test
        public void ExecuteCommandLineDesigner_Bindings()
        {
            //------------Setup for test--------------------------
            var designer = new CommandLineDesigner();

            //------------Execute Test---------------------------
            var bindingList = GetBindings(designer);

            //------------Assert Results-------------------------
            Assert.IsTrue(bindingList.Count > 0);
            Assert.IsTrue(bindingList.ContainsBinding("ModelItem.DisplayName"));
        }
    }
}
