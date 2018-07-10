using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using System.Drawing;
using Warewolf.UI.Tests.WorkflowTab.Tools.Resources.ResourcesToolsUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class RecordsetTests
    {
        const string workflow = "F6ExecuteOnFocusLostTest";
        const string SetDeclareVarToRecordset = "SetDeclareVarToRecordset";

        [TestMethod]
        [TestCategory("Recordset")]
        public void F6_Execute_OnFocustLost_Thus_Persisting_Textbox_Changes()
        {
            ExplorerUIMap.Filter_Explorer(workflow);

            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            WorkflowTabUIMap.Drag_Explorer_Item_To_Design_Surface();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
            Assert.AreEqual("[[person().name]]", ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Flowchart1.Service.ServiceToolLargeView.UIOutputsDataGridTable.UIUI_ActivityGridRow_0Row.UIItemDev2StudioViewMoCell.UIItemComboBox.UITextEdit.Text);
            UIMap.Press_F6();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.PersonOutputButton.Exists);
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.PersonOutputButton);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.FirstRowOfRecordset.Exists);
            Assert.IsFalse(UIMap.ControlExistsNow(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.ThirdRowOfRecordset));
            ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Flowchart1.Service.ServiceToolLargeView.UIOutputsDataGridTable.UIUI_ActivityGridRow_0Row.UIItemDev2StudioViewMoCell.UIItemComboBox.UITextEdit.Text = "[[person(*).name]]";
            UIMap.Press_F6();
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.PersonOutputButton);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.FirstRowOfRecordset.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.ThirdRowOfRecordset.Exists);

        }

        [TestMethod]
        [TestCategory("Recordset")]
        public void Assigning_Declared_Variable_To_A_Recordset_Shows_The_Equal_Sign()
        {
            ExplorerUIMap.Filter_Explorer(SetDeclareVarToRecordset);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIIDText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIIDText.DisplayText.Contains("="));
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIPersonidExpander.Exists);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
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

        ResourcesToolsUIMap ResourcesToolsUIMap
        {
            get
            {
                if (_ResourcesToolsUIMap == null)
                {
                    _ResourcesToolsUIMap = new ResourcesToolsUIMap();
                }

                return _ResourcesToolsUIMap;
            }
        }

        private ResourcesToolsUIMap _ResourcesToolsUIMap;

        #endregion
    }
}
