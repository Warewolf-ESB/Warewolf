using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses;

namespace Warewolf.UI.Tests.Merge
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class MergeConflictsTests
    {
        public const string MergeWfWithVersion = "MergeWfWithVersion";
        public const string MergeHelloWorldWithVersion = "MergeHelloWorldWithVersion";
        public const string MergeSequence = "MergeSequence";

        [TestMethod]
        [TestCategory("Merge")]
        public void RightClick_On_MergeSequence_Has_Merge_Option()
        {
            RightClick_On_MergeWfWithVersion(MergeSequence);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeSequence);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Open_Merge_For_MergeSequence_Has_Assign_OnDesign_Surface_Since_The_Are_No_Differences()
        {
            RightClick_On_MergeWfWithVersion(MergeSequence);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeSequence);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.MergeSequenceAssign.Exists);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Open_Merge_For_MergeSequence_Select_Current_On_OrganizeCustomerTool_Enables_SortNames_Radio_Button()
        {
            RightClick_On_MergeWfWithVersion(MergeSequence);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Current.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Difference.Enabled);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Current.Enabled);
        }
        [TestMethod]
        [TestCategory("Merge")]
        public void Open_Merge_For_MergeSequence_Expand_OrganizeCustomerTool_Has_Split_Names_On_Current()
        {
            RightClick_On_MergeWfWithVersion(MergeSequence);            
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Assert.IsTrue(MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.MergeSequence.Exists);
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;            
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Mouse.Click(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Current);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.ChildrenConflictsTree.SplitNames.MergeExpander.MergeButton.NoConflicts.Exists);
        }

        
        [TestMethod]
        [TestCategory("Merge")]
        public void RightClick_On_MergeWfWithVersion_Has_Merge_Option()
        {
            RightClick_On_MergeWfWithVersion(MergeWfWithVersion);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeWfWithVersion);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_MergeHelloWorldWithVersion_Has_Decision_And_Children()
        {
            RightClick_On_MergeWfWithVersion(MergeHelloWorldWithVersion);
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeWfWithVersion);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Assert.IsTrue(MergeConflictsUIMap.MergeDialogViewWindow.ServerSource.Exists);
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.DecisionMergeTreeItem.Exists);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_MergeHelloWorldWithVersion__Difference_Decision_Add_Decision_On_Design_Surface()
        {
            RightClick_On_MergeWfWithVersion(MergeHelloWorldWithVersion);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.DecisionMergeTreeItem.DecisionSubTreeItem.MergeButton.Decision_Diff_RadioButton.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.Difference_Decision.Exists, "Dicision from difference was not added to the design surface After checking Radio Button");
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_Merge_From_MergeWfWithVersion_ContextMenu_Show_Merge_PopUp()
        {
            RightClick_On_MergeWfWithVersion(MergeWfWithVersion);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Assert.IsTrue(MergeConflictsUIMap.MergeDialogViewWindow.ServerSource.Exists);
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton.Enabled);

            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.Exists);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.Exists, "Workflow surface did not open.");
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Select_All_Radio_Buttons_Enables_Save()
        {
            OpenMerge_For_MergWfWithVersion();
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.FirstAssign_Difference.Selected = true;
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Current.Selected = true;
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_Assign_From_The_Difference_List_Adds_Assign_To_The_Workflow()
        {
            OpenMerge_For_MergWfWithVersion();
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.NoConflicts.Exists);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.FirstAssign_Difference.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.FirstAssign_Diff_On_Surface.Exists, "Assig tool from difference was not added to the design surface.");
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.SecondAssign_Curr_On_Surface.Exists, "Auto Add did not add the 2nd Assig tool Since there are not conflits.");
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Click_On_Assign_From_The_Current_List_Removes_Assign_On_The_Workflow()
        {
            OpenMerge_For_MergWfWithVersion();
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.FirstAssign_Difference.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.FirstAssign_Diff_On_Surface.Exists, "Assig tool from difference was not added to the design surface.");
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.FirstAssign_Current.Selected = true;
            Assert.IsFalse(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.FirstAssign_Difference.Selected);
        }

        private void OpenMerge_For_MergWfWithVersion()
        {
            RightClick_On_MergeWfWithVersion(MergeWfWithVersion);
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeConflictsUIMap.MergeDialogViewWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeConflictsUIMap.MergeDialogViewWindow.MergeButton);
        }

        public void RightClick_On_MergeWfWithVersion(string workflow)
        {
            ExplorerUIMap.Filter_Explorer(workflow);
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
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
