using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Context_Menu
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void Explorer_Workflow_And_Folder_ContextMenuItems_UITest()
        {
            //Workflow Context Menu
            UIMap.Filter_Explorer("Hello World");
            UIMap.RightClick_Explorer_Localhost_FirstItem();
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Open.Exists, "Open does not exist in explorer context menu.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugInputsMenuItem.Exists, "DebugInputsMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugStudioMenuItem.Exists, "DebugStudioMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugBrowserMenuItem.Exists, "DebugBrowserMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ScheduleMenuItem.Exists, "ScheduleMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Rename.Exists, "Rename Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Delete.Exists, "Delete Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate.Exists, "Duplicate Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "Tests Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.RunAllTestsMenuItem.Exists, "Run All Tests Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DeployItem.Exists, "Deploy Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies.Exists, "Show Dependencies Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ViewSwagger.Exists, "View Swagger Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ViewApisJsonMenuItem.Exists, "ViewApisJsonMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ShowVersionHistory.Exists, "Show Version History Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.MakeCurrentVersionMenuItem.Exists, "MakeCurrentVersionMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.OpenVersionMenuItem.Exists, "OpenVersionMenuItem Context menu item does not exist on explorer context menu for workflows.");
            //Folder Context Menu
            UIMap.Filter_Explorer("Examples");
            UIMap.RightClick_Explorer_Localhost_FirstItem();
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewWorkflowItem.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewFolderMenuItem.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Rename.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Delete.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DeployItem.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ViewApisJsonMenuItem.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.Exists);
            //Server Context Menu
            UIMap.RightClick_Localhost();
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.Exists, "Sources Context menu item does not exist on explorer context menu for workflows.");
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

        #endregion
    }
}
