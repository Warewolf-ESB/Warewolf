
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.Tests.VariableList
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class VariableListUITests : UIMapBase
    {

        #region Cleanup


        [TestInitialize]
        public void TestInit()
        {
            Init();

        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
            Halt();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableListTests")]
        public void VariableList_DeleteAColumnOffARecorset_DeleteAllButtonIsEnbaled()
        {
            var multiAssign = new DsfMultiAssignUiMap();

            multiAssign.EnterTextIntoVariable(0, "[[rec().a]]");
            multiAssign.EnterTextIntoVariable(1, "[[rec().b]]");
            multiAssign.EnterTextIntoVariable(2, "[[rec().c]]");
            multiAssign.EnterTextIntoVariable(1, "[[b]]");
            multiAssign.EnterTextIntoVariable(3, "[[c]]");

            Assert.IsTrue(VariablesUIMap.IsDeleteAllEnabled());
        }

        #endregion
    }
}
