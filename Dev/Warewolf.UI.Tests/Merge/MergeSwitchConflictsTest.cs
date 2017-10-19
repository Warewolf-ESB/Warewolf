using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses;

namespace Warewolf.UI.Tests.Merge
{
    [CodedUITest]
    public class MergeSwitchConflictsTest
    {
        public const string MergeSwitch = "MergeSwitch";

        [TestMethod]
        [TestCategory("Merge")]
        public void RightClick_On_Merge_With_Switch_Has_Merge_Option()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeSwitch);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_Merge_With_Switch_Has_Decision_And_Children()
        {
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Assert.IsTrue(MergeConflictsUIMap.MergeDialogViewWindow.ServerSource.Exists);
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.DecisionMergeTreeItem.Exists);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_MergeSwitch_With_Difference_Between_Decision_Add_Decision_On_Design_Surface()
        {
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.DecisionMergeTreeItem.DecisionSubTreeItem.MergeButton.Decision_Diff_RadioButton.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.Difference_Decision.Exists, "Dicision from difference was not added to the design surface After checking Radio Button");
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Expand_Merge_With_Switch_Difference_Has_2_Assigns()
        {
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Mouse.Click(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.UIIfNameIsText);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.ChildrenConflictsTree.FisrtAssign.Exists);
        }
        
        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            MergeConflictsUIMap.OpenMerge_For_Workflow(MergeSwitch);
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
