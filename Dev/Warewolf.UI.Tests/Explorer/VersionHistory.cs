using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Drawing;

// ReSharper disable InconsistentNaming

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class VersionHistory
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void ShowVersionHistory_ForResource()
        {
            ExplorerUIMap.Filter_Explorer("ShowVersionsTestWorkflow");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            WorkflowTabUIMap.Make_Workflow_Savable();
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
            ExplorerUIMap.Select_ShowVersionHistory_From_ExplorerContextMenu();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void OpenVersionHistory_ForResource()
        {
            ExplorerUIMap.Filter_Explorer("OpenVersionsTestWorkflow");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.WaitForControlCondition(control => control is WpfEdit && ((WpfEdit)control).Text != string.Empty, 60000);
            UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.Text = "Bobby";
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
            ExplorerUIMap.Select_ShowVersionHistory_From_ExplorerContextMenu();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists, "No version history found for workflow after editting and saving it.");
            ExplorerUIMap.RightClick_Explorer_Localhost_First_Item_First_SubItem();
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.MakeCurrentVersionMenuItem.Enabled, "The make current version option is not enabled on the explorer context menu.");
            ExplorerUIMap.Select_Make_Current_Version();
            UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.WaitForControlCondition(control => control is WpfEdit && ((WpfEdit)control).Text != string.Empty, 60000);
            Assert.AreEqual("Trivial workflow for testing make current version in the explorer.", UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.Text, "Workflow did not roll back to older version.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void SetVersionHistory_ForResource()
        {
            ExplorerUIMap.Filter_Explorer("VersionWorkflow");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_Second_Item();
            UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.WaitForControlCondition(control => control is WpfEdit && ((WpfEdit)control).Text != string.Empty, 60000);
            UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.Text = "Bobby";
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
            ExplorerUIMap.Select_ShowVersionHistory_From_Explorer_SecondItem_ContextMenu();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem.Exists, "No version history found for workflow after editting and saving it.");
            ExplorerUIMap.RightClick_Explorer_Localhost_Second_Item_First_SubItem();
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.MakeCurrentVersionMenuItem.Enabled, "The make current version option is not enabled on the explorer context menu.");
            ExplorerUIMap.Select_Make_Current_Version();
            UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.WaitForControlCondition(control => control is WpfEdit && ((WpfEdit)control).Text != string.Empty, 60000);
            Assert.AreEqual("Trivial workflow for testing make current version in the explorer.", UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.Text, "Workflow did not roll back to older version.");
            ExplorerUIMap.Click_Explorer_Refresh_Button();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem.Exists, "No version history found for workflow after refreshing Explorer.");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_Second_Item();
            ExplorerUIMap.RightClick_Explorer_Localhost_Second_Item_First_SubItem();
            ExplorerUIMap.Select_Delete_Version();
            ExplorerUIMap.RightClick_Explorer_Localhost_Second_Item_First_SubItem();
            ExplorerUIMap.Select_Delete_Version();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Updating_Resource_With_Dependencies_Should_Show_Dependency_Popup()
        {
            ExplorerUIMap.Filter_Explorer("Main");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            ExplorerUIMap.Filter_Explorer("PerformOperations");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = !WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked;
            UIMap.Click_Save_RibbonButton();
            Assert.IsTrue(UIMap.MainStudioWindow.DependenciesOKButton.Exists, "The dependencies error window does not exists.");
            Mouse.Click(UIMap.MainStudioWindow.DependenciesOKButton);
            Assert.IsFalse(UIMap.MainStudioWindow.DependenciesOKButton.Exists, "The dependencies error window is showing multiple times.");
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

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;

        #endregion
    }
}
