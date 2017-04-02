using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ResourceVersion
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void ShowVersionHistory_ForResource()
        {
            ExplorerUIMap.Filter_Explorer("ShowVersionsTestWorkflow");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.WaitForControlCondition(control => control is WpfEdit && ((WpfEdit)control).Text == "Bob", 60000);
            DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "Bobby";
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
            ExplorerUIMap.Select_ShowVersionHistory_From_ExplorerContextMenu();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_Second_Item();
            Assert.AreEqual(2, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.Tabs.Count);
            ExplorerUIMap.RightClick_Explorer_Localhost_SecondItem();
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Open.Exists, "The open option does not exist on the context menu");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Open.Enabled, "The open option is not enabled on the context menu");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Delete.Exists, "The delete option does not exist on the context menu");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Delete.Enabled, "The delete option is not enabled on the context menu");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.MakeCurrentVersionMenuItem.Exists, "The make current version option does not exist on the context menu");
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.MakeCurrentVersionMenuItem.Enabled, "The make current version option is not enabled on the context menu");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void OpenVersionHistory_ForResource()
        {
            ExplorerUIMap.Filter_Explorer("OpenVersionsTestWorkflow");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "Bobby";
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
            ExplorerUIMap.Select_ShowVersionHistory_From_ExplorerContextMenu();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists, "No version history found for workflow 'OpenVersionsTestWorkflow'. This UI test expects version history for this resource to start with.");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "Batman";
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
            ExplorerUIMap.RightClick_Explorer_Localhost_SecondItem();
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.MakeCurrentVersionMenuItem.Enabled, "The make current version option is not enabled on the context menu");
            ExplorerUIMap.Select_Make_Current_Version();
            UIMap.Click_Close_Workflow_Tab_Button();
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.AreEqual("Bobby", DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text);
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

        #endregion
    }
}
