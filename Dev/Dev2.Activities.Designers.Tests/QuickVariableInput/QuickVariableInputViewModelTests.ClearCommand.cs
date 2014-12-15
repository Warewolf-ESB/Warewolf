
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.QuickVariableInput
{
    public partial class QuickVariableInputViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ClearCommand")]
        public void QuickVariableInputViewModel_ClearCommand_WiredUpCorrectly()
        {
            var qviViewModel = new QuickVariableInputViewModelMock();

            qviViewModel.ClearCommand.Execute(null);

            Assert.AreEqual(1, qviViewModel.DoClearHitCount);

            Assert.IsTrue(qviViewModel.ClearCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_ClearCommand")]
        public void QuickVariableInputViewModel_ClearCommand_ClearsFields()
        {
            var qviViewModel = new QuickVariableInputViewModelMock
            {
                Suffix = "xxx",
                Prefix = "xxx",
                VariableListString = "xxx",
                SplitType = "Index",
                SplitToken = "xxx",
                Overwrite = true                
            };
            qviViewModel.PreviewViewModel.Output = "TestString";
            qviViewModel.ClearCommand.Execute(null);

            Assert.AreEqual("Chars", qviViewModel.SplitType);
            Assert.AreEqual(string.Empty, qviViewModel.SplitToken);
            Assert.AreEqual(string.Empty, qviViewModel.VariableListString);
            Assert.AreEqual(string.Empty, qviViewModel.Prefix);
            Assert.AreEqual(string.Empty, qviViewModel.Suffix);
            Assert.AreEqual(string.Empty, qviViewModel.PreviewViewModel.Output);
            Assert.IsFalse(qviViewModel.Overwrite);
        }
    }
}
