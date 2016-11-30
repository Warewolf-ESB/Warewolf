using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tabs
{
    [CodedUITest]
    public class PinUnpinPanes
    {
        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinNewWorkflowTabByDraggingOnly()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.UnpinnedTab), "Unpinned pane still exists after being dragged onto the central dock indicator.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists, "Workflow tab did not dock into it's default position after being dragged onto the central dock indicator.");
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinnedPaneContextMenuItems()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Mouse.Click(UIMap.MainStudioWindow.UnpinnedTab, MouseButtons.Right, ModifierKeys.None, new Point(14, 12));
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTabContextMenu.Floating.Exists, "Menu item as floating does not exist after openning unpinned tab context menu with a right click.");
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTabContextMenu.Dockable.Exists, "Menu item as dockable does not exist after openning unpinned tab context menu with a right click.");
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTabContextMenu.TabbedDocument.Exists, "Menu item as tabbed document does not exist after openning unpinned tab context menu with a right click.");
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTabContextMenu.AutoHide.Exists, "Menu item as auto hide does not exist after openning unpinned tab context menu with a right click.");
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTabContextMenu.Hide.Exists, "Menu item as hide does not exist after openning unpinned tab context menu with a right click.");
            UIMap.MainStudioWindow.UnpinnedTabContextMenu.TabbedDocument.Checked = true;
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinSettingsTab()
        {
            UIMap.Click_Settings_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinServerSourceWizardTab()
        {
            UIMap.Select_NewRemoteServer_From_Explorer_Server_Dropdownlist();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinDBSourceWizardTab()
        {
            UIMap.Click_New_Database_Source_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinDotNetPluginSourceWizardTab()
        {
            UIMap.Click_NewPluginSource_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinWebSourceWizardTab()
        {
            UIMap.Click_New_Web_Source_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinDeployTab()
        {
            UIMap.Click_Deploy_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinDependencyGraphTab()
        {
            UIMap.Select_Show_Dependencies_In_Explorer_Context_Menu("Hello World");
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinTestsTabPage()
        {
            UIMap.Filter_Explorer("Hello World");
            UIMap.Open_Explorer_First_Item_Tests_With_Context_Menu();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinSchedulerTab()
        {
            UIMap.Click_Scheduler_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab);
            UIMap.Restore_Unpinned_Tab_Using_Context_Menu();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
        }

        UIMap UIMap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
