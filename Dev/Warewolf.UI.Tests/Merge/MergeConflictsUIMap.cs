using Microsoft.VisualStudio.TestTools.UITesting;
using System.Drawing;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Merge.MergeDialogUIMapClasses;

namespace Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses
{
    public partial class MergeConflictsUIMap

    {

        public void Expand_Designer()
        {
            Mouse.StartDragging(MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUI_GridSplitter_AutoIndicator, new Point(3, 395));
            Mouse.StopDragging(MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem8.MergeItemExpander.MergeButton, new Point(370, 13));
        }

        [Given(@"I have merge conflicts tab open for service ""(.*)""")]
        [When(@"I open merge conflicts tab open for service ""(.*)""")]
        public void OpenMerge_For_Workflow(string workflow)
        {
            ExplorerUIMap.Open_Context_Menu_For_Service(workflow);
            MergeDialogUIMap.MergeDialogWindow.MergeResourceVersionList.WarewolfStudioViewMoListItem.ItemRadioButton.Selected = true;
            Mouse.Click(MergeDialogUIMap.MergeDialogWindow.MergeButton);
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

    }
}
