using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest, System.Runtime.InteropServices.GuidAttribute("DAA88B10-98C4-488E-ACB2-1256C95CE8F0")]
    public class ExplorerUITests : UIMapBase
    {
        #region Cleanup

        private static TabManagerUIMap _tabManager = new TabManagerUIMap();

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        //[ClassCleanup]
        //public static void MyTestCleanup()
        //{
        //    _tabManager.CloseAllTabs();
        //}

        #endregion

        [TestMethod]
        public void SearchAndRefresh_AttemptToSearch_ExpectedSearchFilteredByAllItems()
        {

            DockManagerUIMap.ClickOpenTabPage("Explorer");

            // Now count
            ExplorerUIMap.ClearExplorerSearchText();
            var items = ExplorerUIMap.GetCategoryItems();
            var itemCount = items.Count;
            Assert.IsTrue(itemCount > 1, "Cannot find any items in the explorer tree, cannot test explorer filter without any explorer items");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Integration");

            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            items[0].WaitForControlReady();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;

            items = ExplorerUIMap.GetCategoryItems();
            int allResourcesAfterSearch = items.Count;
            Assert.IsTrue(itemCount > allResourcesAfterSearch, "Cannot filter explorer tree");

        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("RenameResource_WithDashes")]
        public void RenameResource_WithDashes_ResourceRenamed()
        {

            const string newTestResourceWithDashes = "New-Test-Resource-With-Dashes";
            const string oldResourceName = "OldResourceName";
            RibbonUIMap.CreateNewWorkflow();
            SendKeys.SendWait("^s");
            WizardsUIMap.WaitForWizard();
            SaveDialogUIMap.ClickAndTypeInNameTextbox(oldResourceName);
            //wait for save tab switch
            Playback.Wait(2000);
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
            ExplorerUIMap.RightClickRenameProject("Localhost", "WORKFLOWS", "Unassigned", oldResourceName);
            SendKeys.SendWait("New-Test-Resource-With-Dashes{ENTER}");
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(newTestResourceWithDashes);
            ExplorerUIMap.DoubleClickOpenProject("Localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes);
            
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        #endregion
    }
}
