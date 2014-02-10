using System;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.Enums;
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
            ExplorerUIMap.RightClickRenameResource(oldResourceName, "Unassigned", ServiceType.Workflows, newTestResourceWithDashes);
            Assert.IsTrue(ExplorerUIMap.ValidateWorkflowExists(newTestResourceWithDashes, "Unassigned"));
            ExplorerUIMap.DoubleClickWorkflow(newTestResourceWithDashes, "Unassigned");
            //Rename the resource back to the original name
            ExplorerUIMap.RightClickRenameResource(newTestResourceWithDashes, "Unassigned", ServiceType.Workflows, oldResourceName);
            Assert.IsTrue(ExplorerUIMap.ValidateWorkflowExists(oldResourceName, "Unassigned"));
            ExplorerUIMap.DoubleClickWorkflow(oldResourceName, "Unassigned");
        }
    }
}
