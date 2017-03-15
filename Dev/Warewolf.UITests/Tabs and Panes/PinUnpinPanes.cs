using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DBSource.DBSourceUIMapClasses;
using Warewolf.UITests.DependencyGraph.DependencyGraphUIMapClasses;
using Warewolf.UITests.Deploy.DeployUIMapClasses;
using Warewolf.UITests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.Scheduler.SchedulerUIMapClasses;
using Warewolf.UITests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UITests.Settings.SettingsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UITests.WebSource.WebSourceUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;

namespace Warewolf.UITests.Tabs
{
    [CodedUITest]
    public class PinUnpinPanes
    {
        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinAndRepinNewWorkflowWizardTabByDraggingOnly()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            UIMap.Unpin_Tab_With_Drag(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.UnpinnedTab), "Unpinned pane still exists after being dragged onto the central dock indicator.");
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists, "Workflow tab did not dock into it's default position after being dragged onto the central dock indicator.");
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinnedPaneContextMenuItems()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            UIMap.Unpin_Tab_With_Drag(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);            
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
        public void UnpinSettingsWizardTab()
        {
            UIMap.Click_Settings_RibbonButton();
            UIMap.Unpin_Tab_With_Drag(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinServerSourceWizardTab()
        {
            ExplorerUIMap.Select_NewRemoteServer_From_Explorer_Server_Dropdownlist();
            UIMap.Unpin_Tab_With_Drag(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinDBSourceWizardTab()
        {
            ExplorerUIMap.Click_NewSQLServerSource_From_ExplorerContextMenu();
            UIMap.Unpin_Tab_With_Drag(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinDotNetPluginSourceWizardTab()
        {
            ExplorerUIMap.Click_NewDotNetPluginSource_From_ExplorerContextMenu();
            UIMap.Unpin_Tab_With_Drag(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinWebSourceWizardTab()
        {
            ExplorerUIMap.Click_NewWebSource_From_ExplorerContextMenu();
            UIMap.Unpin_Tab_With_Drag(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinDeployWizardTab()
        {
            UIMap.Click_Deploy_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinDependencyGraphWizardTab()
        {
            ExplorerUIMap.Select_ShowDependencies_In_ExplorerContextMenu("Hello World");
            UIMap.Unpin_Tab_With_Drag(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinTestsWizardTab()
        {
            ExplorerUIMap.Filter_Explorer("Hello World");
            ExplorerUIMap.Open_ExplorerFirstItemTests_With_ExplorerContextMenu();
            UIMap.Unpin_Tab_With_Drag(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void UnpinSchedulerWizardTab()
        {
            UIMap.Click_Scheduler_RibbonButton();
            UIMap.Unpin_Tab_With_Drag(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab);
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void AssignToolInUnpinnedWorkflowWizardTabDebugOutputUITest()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            DataToolsUIMap.Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1();
            UIMap.Unpin_Tab_With_Drag(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            UIMap.Press_F6_On_UnPinnedTab();
            Assert.AreEqual("[[SomeVariable]]", UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.VariableTextbox2.DisplayText, "Variable name does not exist in unpinned debug output.");
            Assert.AreEqual("50", UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.ValueTextbox5.DisplayText, "Variable value does not exist in unpinned debug output.");
        }

        [TestMethod]
        [TestCategory("Tabs and Panes")]
        public void AssignToolInUnpinnedWorkflowWizardTabAddVariableUITest()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            UIMap.Unpin_Tab_With_Drag(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            const string Variable1Name = "SomeVariable";
            UIMap.Enter_Variable_Into_Assign_Row1_On_Unpinned_Tab("[[" + Variable1Name + "]]");
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.Exists, "Scalar variable list not found.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

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

        SchedulerUIMap SchedulerUIMap
        {
            get
            {
                if (_SchedulerUIMap == null)
                {
                    _SchedulerUIMap = new SchedulerUIMap();
                }

                return _SchedulerUIMap;
            }
        }

        private  SchedulerUIMap _SchedulerUIMap;

        SettingsUIMap SettingsUIMap
        {
            get
            {
                if (_SettingsUIMap == null)
                {
                    _SettingsUIMap = new SettingsUIMap();
                }

                return _SettingsUIMap;
            }
        }

        private SettingsUIMap _SettingsUIMap;

        DBSourceUIMap DBSourceUIMap
        {
            get
            {
                if (_DBSourceUIMap == null)
                {
                    _DBSourceUIMap = new DBSourceUIMap();
                }

                return _DBSourceUIMap;
            }
        }

        private DBSourceUIMap _DBSourceUIMap;

        ServerSourceUIMap ServerSourceUIMap
        {
            get
            {
                if (_ServerSourceUIMap == null)
                {
                    _ServerSourceUIMap = new ServerSourceUIMap();
                }

                return _ServerSourceUIMap;
            }
        }

        private ServerSourceUIMap _ServerSourceUIMap;

        DotNetPluginSourceUIMap DotNetPluginSourceUIMap
        {
            get
            {
                if (_DotNetPluginSourceUIMap == null)
                {
                    _DotNetPluginSourceUIMap = new DotNetPluginSourceUIMap();
                }

                return _DotNetPluginSourceUIMap;
            }
        }

        private DotNetPluginSourceUIMap _DotNetPluginSourceUIMap;

        WebSourceUIMap WebSourceUIMap
        {
            get
            {
                if (_WebSourceUIMap == null)
                {
                    _WebSourceUIMap = new WebSourceUIMap();
                }

                return _WebSourceUIMap;
            }
        }

        private WebSourceUIMap _WebSourceUIMap;

        DeployUIMap DeployUIMap
        {
            get
            {
                if (_DeployUIMap == null)
                {
                    _DeployUIMap = new DeployUIMap();
                }

                return _DeployUIMap;
            }
        }

        private DeployUIMap _DeployUIMap;

        DataToolsUIMap DataToolsUIMap
        {
            get
            {
                if (_DataToolsUIMap == null)
                {
                    _DataToolsUIMap = new DataToolsUIMap();
                }

                return _DataToolsUIMap;
            }
        }

        private DataToolsUIMap _DataToolsUIMap;

        DependencyGraphUIMap DependencyGraphUIMap
        {
            get
            {
                if (_DependencyGraphUIMap == null)
                {
                    _DependencyGraphUIMap = new DependencyGraphUIMap();
                }

                return _DependencyGraphUIMap;
            }
        }

        private DependencyGraphUIMap _DependencyGraphUIMap;

        #endregion
    }
}
