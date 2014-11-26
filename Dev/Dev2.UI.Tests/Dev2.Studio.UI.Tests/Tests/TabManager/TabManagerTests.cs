
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Tests.TabManager
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [CodedUITest]
    public class TabManagerTests : UIMapBase
    {

        #region Setup
        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            RestartStudioOnFailure();
            TabManagerUIMap.CloseAllTabs();
        }
        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TabManagerTests_CodedUI")]
        [Description("For bug 10086 - Switching tabs does not flicker unsaved status")]
        //This test could fail because of the really long time it takes to save a workflow and close the old tab
        public void TabManagerTests_CodedUI_CreateTwoWorkflowsSwitchBetween_ExpectStarNotShowingInName()
        {

            const string firstName = "Bug_10528";
            const string secondName = "Bug_10528_InnerWorkFlow";

            ExplorerUIMap.EnterExplorerSearchText("Bug_10528");

            ExplorerUIMap.DoubleClickOpenProject("localhost", "INTEGRATION TEST SERVICES", firstName);

            var tab1 = TabManagerUIMap.GetActiveTab();

            ExplorerUIMap.DoubleClickOpenProject("localhost", "INTEGRATION TEST SERVICES", secondName);

            var tab2 = TabManagerUIMap.GetActiveTab();

            //Switch tabs a couple of times 
            TabManagerUIMap.Click(tab1);
            TabManagerUIMap.Click(tab2);
            TabManagerUIMap.Click(tab1);

            //Check that the tabs names dont have stars in them

            var tabCount = TabManagerUIMap.GetTabCount();

            Assert.IsTrue(tabCount >= 2);
            for(int i = 0; i < tabCount; i++)
            {
                Assert.IsFalse(TabManagerUIMap.GetTabNameAtPosition(i).Contains("*"));
            }
        }
    }
}
