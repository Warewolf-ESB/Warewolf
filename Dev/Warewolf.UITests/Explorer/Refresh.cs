using System;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Refresh
    {
        const string WorkflowName = "RefreshExplorerAfterDeletingResourceFromDiskUITest";

        [TestMethod]
        [TestCategory("Explorer")]
        public void RefreshExplorerAfterDeletingResourceFromDiskUITest()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Testing Resources";
            File.Delete(resourcesFolder + @"\" + WorkflowName + ".xml");
            UIMap.Filter_Explorer(WorkflowName);
            UIMap.WaitForControlVisible(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton);
            Mouse.Click(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem), "Workflow exists in explorer tree after deleting from disk.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
        }

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}
