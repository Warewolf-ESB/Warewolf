using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
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
            Mouse.StopDragging(MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem7.MergeItemExpander.MergeButton, new Point(370, 13));
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


        /// <summary>
        /// RecordedMethod1
        /// </summary>
        public void ExpandDesignerSurface()
        {
            #region Variable Declarations
            var uIUI_GridSplitter_AutoIndicator = MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.UIUI_GridSplitter_AutoIndicator;
            var uIDev2iewModelsMergeTButton = MainWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.MergeTab.WorkSurfaceContext.ContentDockManager.MergeWorkflowView.ScrollViewerPane.ConflictsTree.MergeTreeItem5.MergeItemExpander.MergeButton;
            #endregion

            // Move 'UI_GridSplitter_AutoID' Indicator to 'Dev2.ViewModels.Merge.ToolConflictRow' button
            //System parameter 'Show window contents while dragging' is not set.This could lead to incorrect recording of drag actions.
            uIDev2iewModelsMergeTButton.EnsureClickable(new Point(10, 20));
            Mouse.StartDragging(uIUI_GridSplitter_AutoIndicator, new Point(2, 389));
            Mouse.StopDragging(uIDev2iewModelsMergeTButton, new Point(10, 20));
        }
    }
}
