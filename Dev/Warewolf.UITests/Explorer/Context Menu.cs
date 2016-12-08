using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Context_Menu
    {
        //[TestMethod]
        //[TestCategory("Explorer")]
        //public void ExplorerWorkflowContextMenuItemsUITest()
        //{
        //    UIMap.Filter_Explorer("Hello World");
        //    UIMap.RightClick_Explorer_Localhost_First_Item();
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Open.Exists, "Open Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewWorkflow.Exists, "New Workflow Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewDatabaseSource.Exists, "New Database Source Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewDropboxSource.Exists, "New Dropbox Source Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewEmailSource.Exists, "New Email Source Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewWebServiceSource.Exists, "New Web Service Source Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewFolder.Exists, "New Folder Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Delete.Exists, "Delete Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Deploy.Exists, "Deploy Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate.Exists, "Duplicate Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugBrowserMenuItem.Exists, "Debug Browser Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugInputsMenuItem.Exists, "Debug Inputs Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugStudioMenuItem.Exists, "Debug Studio Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.DebugBrowserMenuItem.Exists, "Debug Browser Menu Item Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewFolderMenuItem.Exists, "New Folder Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewPluginSource.Exists, "New Plugin Source Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewServerSource.Exists, "New Server Source Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.NewSharepointSource.Exists, "New Sharepoint Source Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.RunAllTestsMenuItem.Exists, "Run All Tests Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies.Exists, "Show Dependencies Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "Tests Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ShowVersionHistory.Exists, "Show Version History Context menu item does not exist on explorer context menu for workflows.");
        //    Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.ViewSwagger.Exists, "View Swagger Context menu item does not exist on explorer context menu for workflows.");
        //}

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
