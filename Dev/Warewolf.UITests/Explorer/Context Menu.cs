using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Context_Menu
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void ExplorerWorkflowContextMenuItemsUITest()
        {
            UIMap.Filter_Explorer("Hello World");
            UIMap.RightClick_Explorer_Localhost_First_Item();
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Open.Exists, "Open Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewWorkflow.Exists, "New Workflow Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugInputsMenuItem.Exists, "DebugInputsMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugStudioMenuItem.Exists, "DebugStudioMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugBrowserMenuItem.Exists, "DebugBrowserMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Rename.Exists, "Rename Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Delete.Exists, "Delete Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate.Exists, "Duplicate Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "Tests Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.RunAllTestsMenuItem.Exists, "Run All Tests Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Deploy.Exists, "Deploy Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies.Exists, "Show Dependencies Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ViewSwagger.Exists, "View Swagger Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ScheduleMenuItem.Exists, "ScheduleMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ShowVersionHistory.Exists, "Show Version History Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ViewApisJsonMenuItem.Exists, "ViewApisJsonMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.MakeCurrentVersionMenuItem.Exists, "MakeCurrentVersionMenuItem Context menu item does not exist on explorer context menu for workflows.");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.OpenVersionMenuItem.Exists, "OpenVersionMenuItem Context menu item does not exist on explorer context menu for workflows.");
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
