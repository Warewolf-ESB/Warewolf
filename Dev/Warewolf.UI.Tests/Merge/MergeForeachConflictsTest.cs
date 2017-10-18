using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses;

namespace Warewolf.UI.Tests.Merge
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class MergeForeachConflictsTest
    {
        public const string MergeForeach = "MergeForeach";


        [TestMethod]
        [TestCategory("Merge")]
        public void RightClick_On_MergeForeach_Has_Merge_Option()
        {
            MergeConflictsUIMap.RightClick_On_MergeWorkflow(MergeForeach);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeForeach);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_MergeForeach_Has_Conflicts()
        {
            MergeConflictsUIMap.RightClick_On_MergeWorkflow(MergeForeach);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeForeach);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Assert.IsTrue(MergeConflictsUIMap.MergeDialogViewWindow.ServerSource.Exists);
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.Exists);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_OrganizeCustomers_Current_Removes_OrganizeCustomers_From_Design_Surface()
        {
            MergeConflictsUIMap.RightClick_On_MergeWorkflow(MergeForeach);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.DifferenceVariableRadioButton.Selected = true;
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.FirstAssign_Difference.Selected = true;
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Difference.Selected = true;
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Current.Selected = true;
            Assert.IsFalse(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.MergeSequenceAssign.Exists);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_MergeForeach_Difference_Foreach_Add_Foreach_On_Design_Surface()
        {
            MergeConflictsUIMap.RightClick_On_MergeWorkflow(MergeForeach);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.DifferenceVariableRadioButton.Selected = true;
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.FirstAssign_Difference.Selected = true;
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Difference.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Difference.Enabled, "Foreach radio button did not enable after selecting previous tool");
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Difference.Selected = true;
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Expanding_Foreach_Has_Convert_Tool_Inside()
        {
            MergeConflictsUIMap.RightClick_On_MergeWorkflow(MergeForeach);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Mouse.Click(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton, new Point(10,10));
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeSubTreeItem.Exists, "Foreach does not contain any tools inside it.");
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeSubTreeItem.SubTreeItem.ChildrenConflictsTree.FirstTool.MergeExpander.MergeButton.NoConflicts.Exists);
        }


        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }
        public MergeConflictsUIMap MergeConflictsUIMap
        {
            get
            {
                if (_MergeConflictsUIMap == null)
                {
                    _MergeConflictsUIMap = new MergeConflictsUIMap();
                }

                return _MergeConflictsUIMap;
            }
        }

        private MergeConflictsUIMap _MergeConflictsUIMap;

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

        public ExplorerUIMap ExplorerUIMap
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
