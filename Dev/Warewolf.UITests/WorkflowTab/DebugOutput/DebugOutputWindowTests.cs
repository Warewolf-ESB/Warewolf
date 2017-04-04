﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.LoopConstructs.LoopConstructToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.StorageDropbox.DropboxToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.DebugOutputTests
{
    [CodedUITest]
    public class DebugOutputWindowTests
    {
        const string SelectionHighlightWf = "SelectionHighlightWf";
        const string DropboxSelectionHighlightWf = "DropboxSelectionHighlightWf";
        [TestMethod]
        [TestCategory("Debug Input")]
        // ReSharper disable once InconsistentNaming
        public void WorkFlowSelection_Validation_UITest()
        {
            ExplorerUIMap.Filter_Explorer(SelectionHighlightWf);
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.Press_F6();
            ExplorerUIMap.Click_AssignStep_InDebugOutput();
            var assignFocus = DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(assignFocus);
            ExplorerUIMap.Click_DesicionStep_InDebugOutput();
            var assignHasNoFocus = DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=False IsSelection=False");
            Assert.IsTrue(assignHasNoFocus);
        }

        [TestMethod]
        [TestCategory("Debug Input")]
        public void DropboxWorkFlowSelection_Validation_UITest()
        {
            ExplorerUIMap.Filter_Explorer(DropboxSelectionHighlightWf);
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.Press_F6();
            ExplorerUIMap.Click_AssignStep_InDebugOutput();
            var assignFocus = DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(assignFocus);
            ExplorerUIMap.Click_DesicionStep_InDebugOutput();
            var assignHasNoFocus = DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=False IsSelection=False");
            Assert.IsTrue(assignHasNoFocus);
        }

        [TestMethod]
        [TestCategory("Debug Input")]
        public void SelectAndApplyWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_SelectAndApply_Onto_DesignSurface();
            UIMap.Press_F6();
            var selectAndApplyFocus = LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(selectAndApplyFocus);

        }

        [TestMethod]
        [TestCategory("Debug Input")]
        public void DropboxDownloadWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Dropbox_Download_Onto_DesignSurface();
            UIMap.Press_F6();
            var dropboxDownloadHasFocus = DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(dropboxDownloadHasFocus);

        }

        [TestMethod]
        [TestCategory("Debug Input")]
        public void DropboxDeleteWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Dropbox_Delete_Onto_DesignSurface();
            UIMap.Press_F6();
            var dropboxDeleteHasFocus = DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(dropboxDeleteHasFocus);


        }
        [TestMethod]
        [TestCategory("Debug Input")]
        public void DropboxUploadWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            DropboxToolsUIMap.Drag_Toolbox_Dropbox_Upload_Onto_DesignSurface();
            UIMap.Press_F6();
            var dropboxUploadHasFocus = DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(dropboxUploadHasFocus);


        }
        [TestMethod]
        [TestCategory("Debug Input")]
        public void DropboxListWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Dropbox_FileList_Onto_DesignSurface();
            UIMap.Press_F6();
            var dropboxListHasFocus = DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(dropboxListHasFocus);

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

        LoopConstructToolsUIMap LoopConstructToolsUIMap
        {
            get
            {
                if (_LoopConstructToolsUIMap == null)
                {
                    _LoopConstructToolsUIMap = new LoopConstructToolsUIMap();
                }

                return _LoopConstructToolsUIMap;
            }
        }

        private LoopConstructToolsUIMap _LoopConstructToolsUIMap;

        DropboxToolsUIMap DropboxToolsUIMap
        {
            get
            {
                if (_DropboxToolsUIMap == null)
                {
                    _DropboxToolsUIMap = new DropboxToolsUIMap();
                }

                return _DropboxToolsUIMap;
            }
        }

        private DropboxToolsUIMap _DropboxToolsUIMap;

        #endregion
    }
}