using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses;
using Warewolf.UI.Tests.Merge.MergeDialogUIMapClasses;

namespace Warewolf.UI.Tests.Merge
{
    [CodedUITest]
    public class MergeSequenceConflictsTest
    {
        public const string MergeSequence = "MergeSequence";

        [TestMethod]
        [TestCategory("Merge")]
        public void RightClick_On_Merge_With_Sequence_Has_Merge_Option()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Merge.Exists, "Merge option does not show after Right cliking " + MergeSequence);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Open_Merge_For_Merge_With_Sequence_Has_Assign_OnDesign_Surface_Since_The_Are_No_Differences()
        {
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeDialogUIMap.MergeDialogWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.DifferenceVariableRadioButton.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.DesignerView.ScrollViewerPane.ActivityBuilder.WorkflowItemPresenter.Flowchart.MergeSequenceAssign.Exists);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Open_Merge_For_Merge_With_Sequence_Select_Current_On_OrganizeCustomerTool_Enables_SortNames_Radio_Button()
        {
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            MergeDialogUIMap.MergeDialogWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.MergeButton.KeepCurrentCheckbox.Selected = true;
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Current.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Difference.Enabled);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Current.Enabled);
        }

        [TestMethod]
        [TestCategory("Merge")]
        public void Open_Merge_For_Merge_With_Sequence_Expand_OrganizeCustomerTool_Has_Split_Names_On_Current()
        {
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Assert.IsTrue(MergeDialogUIMap.MergeDialogWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.MergeSequence.Exists);
            MergeDialogUIMap.MergeDialogWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.DifferenceVariableRadioButton.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.DesignerView.ScrollViewerPane.ActivityBuilder.WorkflowItemPresenter.Flowchart.CreaqteExampleDataAssign.Exists, "Create Example Data Assign tool was not added to the design surface since it is not conflicting");
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.NoConflicts.Exists);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Difference.Enabled);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Difference.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.DesignerView.ScrollViewerPane.ActivityBuilder.WorkflowItemPresenter.Flowchart.SequenceActivityCustom.Exists, "Organize Customers Sequence tool was not added to the design surface after clicking Radio button.");
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Difference.Enabled, "Sort Names radio button did not enable after selecting Organze Customer");
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem3.MergeItemExpander.MergeButton.ThirdAssign_Difference.Selected = true;
        }


        [TestMethod]
        [TestCategory("Merge")]
        public void Open_Merge_For_Merge_With_Sequence_Select_All_Radio_Buttons_On_Difference_Adds_Tool_Onto_Design_Surface()
        {
            ExplorerUIMap.Click_Merge_From_Context_Menu();
            Assert.IsTrue(MergeDialogUIMap.MergeDialogWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.MergeSequence.Exists);
            MergeDialogUIMap.MergeDialogWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
            Mouse.Click(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.MergeButton.OrganizeCustomers_Current);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem2.MergeItemExpander.ChildrenConflictsTree.SplitNames.MergeExpander.MergeButton.NoConflicts.Exists);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            ExplorerUIMap.Open_Context_Menu_For_Service(MergeSequence);
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

        public MergeDialogUIMap MergeDialogUIMap
        {
            get
            {
                if (_MergeDialogUIMap == null)
                {
                    _MergeDialogUIMap = new MergeDialogUIMap();
                }

                return _MergeDialogUIMap;
            }
        }

        private MergeDialogUIMap _MergeDialogUIMap;

        #endregion
    }
}
