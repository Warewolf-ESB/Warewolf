using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.SharepointSource.SharepointSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Sharepoint.SharepointToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Sharepoint
{
    [CodedUITest]
    public class Move_File
    {
        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void SharepointMoveFileTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.Exists, "Sharepoint move file tool does does not exist after dragging tool from toolbox.");
            //Small View
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.Server.Exists, "Server Combobox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.EditSourceButton.Exists, "Edit Source Button does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.FromDirectoryComboBox.Exists, "From Directory Combobox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.PathDirectoryComboBox.Exists, "Path Directory Combobox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.ResultComboBox.Exists, "Result Combobox does not exist on small view after tool has been dragged from the toolbox.");
            //Large View
            SharepointToolsUIMap.Open_SharepointMoveFileTool_LargeView();
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.Server.Exists, "Server combobox does not exist on tool large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.FromDirectoryComboBox.TextEdit.Exists, "From Directory textbox does not exist on tool large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.PathDirectoryComboBox.TextEdit.Exists, "Path Directory textbox does not exist on tool large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.OverwriteCheckBox.Exists, "Overwrite Checkbox does not exist on tool large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.ResultComboBox.Exists, "Result Combobox does not exist on tool large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.OnErrorPane.Exists, "OnError Pane does not exist on tool large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.DoneButton.Exists, "Done Button does not exist on tool large view after tool has been dragged from the toolbox.");
            //New Source
            SharepointToolsUIMap.Click_NewSource_From_SharepointMoveFileTool();
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Enabled, "Server Name Textbox is not enabled.");
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.WindowsRadioButton.Enabled, "Windows Radio button is not enabled.");
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserRadioButton.Enabled, "User Radio button is not enabled.");
            Assert.IsFalse(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton.Enabled, "Test Connection button is enabled.");
            Assert.IsFalse(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.CancelTestButton.Enabled, "Cancel Test button is  enabled.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Sharepoint_MoveFile_Onto_DesignSurface();
        }
        
        UIMap UIMap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

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

        SharepointSourceUIMap SharepointSourceUIMap
        {
            get
            {
                if (_SharepointSourceUIMap == null)
                {
                    _SharepointSourceUIMap = new SharepointSourceUIMap();
                }

                return _SharepointSourceUIMap;
            }
        }

        private SharepointSourceUIMap _SharepointSourceUIMap;

        SharepointToolsUIMap SharepointToolsUIMap
        {
            get
            {
                if (_SharepointToolsUIMap == null)
                {
                    _SharepointToolsUIMap = new SharepointToolsUIMap();
                }

                return _SharepointToolsUIMap;
            }
        }

        private SharepointToolsUIMap _SharepointToolsUIMap;

        #endregion
    }
}
