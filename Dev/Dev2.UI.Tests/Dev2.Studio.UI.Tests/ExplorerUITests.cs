using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class ExplorerUITests : UIMapBase
    {
        #region Cleanup

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }
        #endregion

        [TestMethod]
        public void SearchAndRefresh_AttemptToSearch_ExpectedSearchFilteredByAllItems()
        {

            // Now count
            ExplorerUIMap.ClearExplorerSearchText();
            Playback.Wait(2000);
            var items = ExplorerUIMap.GetServiceItems();
            var itemCount = items.Count;

            ExplorerUIMap.EnterExplorerSearchText("Integration");

            Playback.Wait(5000);

            items = ExplorerUIMap.GetServiceItems();
            int allResourcesAfterSearch = items.Count;
            Assert.IsTrue(itemCount > allResourcesAfterSearch, "Cannot filter explorer tree");

        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("RenameResource_WithDashes")]
        public void RenameResource_WithDashes_ResourceRenamed()
        {
            try
            {
                TabManagerUIMap.CloseAllTabs();
                const string newTestResourceWithDashes = "New-Test-Resource-With-Dashes";
                const string oldResourceName = "OldResourceName";
                ExplorerUIMap.ClearExplorerSearchText();
                ExplorerUIMap.EnterExplorerSearchText(newTestResourceWithDashes);
                if(ExplorerUIMap.ServiceExists("Localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes))
                {
                    ExplorerUIMap.RightClickDeleteProject("Localhost", "WORKFLOWS", "Unassigned",
                                                          newTestResourceWithDashes);
                }
                ExplorerUIMap.ClearExplorerSearchText();
                ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
                if(ExplorerUIMap.ServiceExists("Localhost", "WORKFLOWS", "Unassigned", oldResourceName))
                {
                    ExplorerUIMap.RightClickDeleteProject("Localhost", "WORKFLOWS", "Unassigned", oldResourceName);
                }
                RibbonUIMap.CreateNewWorkflow();
                SendKeys.SendWait("^s");
                WizardsUIMap.WaitForWizard();
                SaveDialogUIMap.ClickAndTypeInNameTextbox(oldResourceName);
                //wait for save tab switch
                Playback.Wait(2000);
                TabManagerUIMap.CloseAllTabs();
                ExplorerUIMap.ClearExplorerSearchText();
                ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
                ExplorerUIMap.RightClickRenameProject("localhost", "WORKFLOWS", "Unassigned", oldResourceName);
                SendKeys.SendWait("New-Test-Resource-With-Dashes{ENTER}");
                ExplorerUIMap.DoRefresh();
                ExplorerUIMap.ClearExplorerSearchText();
                ExplorerUIMap.EnterExplorerSearchText(newTestResourceWithDashes);
                ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes);
                SendKeys.SendWait("^s");

                RibbonUIMap.ClickRibbonMenuItem("Debug");
                if(DebugUIMap.WaitForDebugWindow(5000))
                {
                    SendKeys.SendWait("{F5}");
                    Playback.Wait(1000);
                }
            }
            finally
            {
                //close any open wizards
                var tryFindDialog = StudioWindow.GetChildren()[0];
                if(tryFindDialog.GetType() == typeof(WpfWindow))
                {
                    Mouse.Click(tryFindDialog);
                    SendKeys.SendWait("{ESCAPE}");
                    Assert.Fail("Dialog hanging after test, might not have rendered properly");
                }
                //close any open tabs
                TabManagerUIMap.CloseAllTabs();
                ExplorerUIMap.ClearExplorerSearchText();
            }
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
