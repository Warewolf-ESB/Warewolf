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
        [TestCategory("Merge Sequence Conflicts")]
        public void Open_Merge_For_MergeSequence_Select_Current_Variables_Radio_Button()
        {
            MergeDialogUIMap.MergeDialogWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.LeftVariablesRadio.Enabled);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.DifferenceVariableRadioButton.Enabled);
            Mouse.Click(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.MergeButton);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.UIUI_CurrentVariablesDCustom.UIUI_VariableTreeView_Tree.UIVariableTreeItem.UITreeItem1.UICustomerCSVPane.UIUI_NameTextBox_AutoIEdit.Exists);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.LeftVariablesRadio.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.LeftVariablesRadio.Selected);
        }

        [TestMethod]
        [TestCategory("Merge Sequence Conflicts")]
        public void Open_Merge_For_MergeSequence_Merge_With_Sequence_Expand_OrganizeCustomerTool_Has_Split_Names_On_Current()
        {
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.LeftVariablesRadio.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.DesignerView.ScrollViewerPane.ActivityBuilder.WorkflowItemPresenter.Flowchart.CreaqteExampleDataAssign.Exists, "Create Example Data Assign tool was not added to the design surface since it is not conflicting");
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem5.MergeItemExpander.MergeButton.CurrentCheckBox.Checked = false;
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem5.MergeItemExpander.MergeButton.DifferenceCheckBox.Checked = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.DesignerView.ScrollViewerPane.ActivityBuilder.WorkflowItemPresenter.Flowchart.SequenceActivityCustom.Exists, "Organize Customers Sequence tool was not added to the design surface after clicking Radio button.");
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem7.MergeItemExpander.MergeButton.CurrentCheckBox.Checked = false;
            Assert.IsFalse(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem6.Current.Enabled, "Current Sort Names Radio button is STILL Enabled.");
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem7.MergeItemExpander.MergeButton.DifferenceCheckBox.Checked = true;            
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem6.Difference.Enabled, "Sort Names Radio button is not Enabled.");
        }


        [TestMethod]
        [TestCategory("Merge Sequence Conflicts")]
        public void Open_Merge_ForMergeSequence_Loads_With_Current_Version_Selected_And_Tools_Onto_Design_Surface()
        {
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
            MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.VariablesExpander.VariablesHeader.LeftVariablesRadio.Selected = true;
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.DesignerView.ScrollViewerPane.ActivityBuilder.WorkflowItemPresenter.Flowchart.CreaqteExampleDataAssign.Exists, "Create Example Data Assign tool was not added to the design surface since it is not conflicting");
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem.MergeItemExpander.MergeButton.NoConflicts.Exists);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem5.MergeItemExpander.MergeButton.DifferenceCheckBox.Enabled);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem5.MergeItemExpander.MergeButton.CurrentCheckBox.Enabled);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem5.MergeItemExpander.MergeButton.CurrentCheckBox.Checked);
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem4.Current.Enabled, "Sort Names radio button did not enable after selecting Organze Customer");
            Assert.IsFalse(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem4.Difference.Enabled, "Sort Names radio button did not enable after selecting Organze Customer");
            Assert.IsTrue(MergeConflictsUIMap.MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.DesignerView.ScrollViewerPane.ActivityBuilder.WorkflowItemPresenter.Flowchart.SortRecordsActiviCustom.Exists, "Sort Names was not added to the designer surface.");
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
