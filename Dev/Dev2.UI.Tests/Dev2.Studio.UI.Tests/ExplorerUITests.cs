using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
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
        [Owner("Ashley Lewis")]
        [TestCategory("RenameResource_WithDashes")]
        public void RenameResource_WithDashes_ResourceRenamed()
        {
            const string newTestResourceWithDashes = "New-Test-Resource-With-Dashes";
            const string oldResourceName = "OldResourceName";
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
            Playback.Wait(4000);
            ExplorerUIMap.RightClickRenameProject("localhost", "WORKFLOWS", "Unassigned", oldResourceName);
            Playback.Wait(2000);
            SendKeys.SendWait("New-Test-Resource-With-Dashes{ENTER}");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(newTestResourceWithDashes);
            Playback.Wait(3000);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes);
            Playback.Wait(2000);
            //Rename the resource back to the original name
            ExplorerUIMap.RightClickRenameProject("localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes);
            Playback.Wait(2000);
            SendKeys.SendWait("OldResourceName{ENTER}");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
            Playback.Wait(3000);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "Unassigned", oldResourceName);
        }
    }
}
