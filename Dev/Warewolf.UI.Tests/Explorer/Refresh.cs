using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;

namespace Warewolf.UI.Tests
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
            var path = resourcesFolder + @"\" + WorkflowName + ".xml";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            ExplorerUIMap.Filter_Explorer(WorkflowName);
            UIMap.WaitForControlVisible(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton);
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            UIMap.WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Assert.IsFalse(UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem) ? ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ItemEdit.Text.Contains(WorkflowName) : UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem), "Workflow exists in explorer tree after deleting from disk.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void RefreshExplorerAfterConnectingToRemoteDoesNotRefreshLocalhost()
        {
            try
            {
                ExplorerUIMap.Collapse_Localhost();
                ExplorerUIMap.Select_RemoteConnectionIntegration_From_Explorer();
                Point point;
                Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
                Assert.IsFalse(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Spinner.TryGetClickablePoint(out point), "Localhost spinner is showing while refreshing remote.");
                ExplorerUIMap.Expand_Localhost();
            }
            finally
            {
                ExplorerUIMap.Expand_Localhost();
            }
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        #endregion
    }
}
