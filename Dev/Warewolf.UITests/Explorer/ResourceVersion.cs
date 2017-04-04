using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

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
            UIMap.Click_Close_Workflow_Tab_Button();
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            var originalText = "Trivial workflow for testing make current version in the explorer.";
            UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.WaitForControlCondition(control => control is WpfEdit && ((WpfEdit)control).Text == originalText, 60000);
            Assert.AreEqual(originalText, UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.Text, "Workflow did not roll back to older version.");
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
