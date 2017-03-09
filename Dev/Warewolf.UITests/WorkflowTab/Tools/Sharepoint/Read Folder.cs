using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.SharepointSource.SharepointSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Sharepoint.SharepointToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Sharepoint
{
    [CodedUITest]
    public class Read_Folder
    {
        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void SharepointReadFolderTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.Exists, "Sharepoint read folder tool does does not exist after dragging tool from toolbox.");
            //Small View
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.SmallView.Server.Exists, "Server Combobox does not exist on small view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.SmallView.EditSourceButton.Exists, "Edit Source Button does not exist on small view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.SmallView.ServerPathIntellisenseCombobox.Exists, "Server Path Combobox does not exist on small view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.SmallView.FilesRadioButton.Exists, "Files Radio Button does not exist on small view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.SmallView.FoldersRadioButton.Exists, "Folders Radio Button does not exist on small view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.SmallView.FilesAndFoldersRadioButton.Exists, "FilesAndFoldersRadioButton does not exist on small view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.SmallView.ResultIntellisenseCombobox.Exists, "Result Combobox does not exist on small view after dragging tool in from the toolbox.");
            //Large View
            SharepointToolsUIMap.Open_SharepointReadFolderTool_LargeView();
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.Server.Exists, "Server Combobox does not exist on large view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.EditSourceButton.Exists, "Edit Source Button does not exist on large view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.ServerPathIntellisenseCombobox.Exists, "Server Path Combobox does not exist on large view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.FilesRadioButton.Exists, "Files Radio Button does not exist on large view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.FoldersRadioButton.Exists, "Folders Radio Button does not exist on large view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.FilesAndFoldersRadioButton.Exists, "FilesAndFoldersRadioButton does not exist on large view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.ResultIntellisenseTextbox.Exists, "Result Textbox does not exist on large view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.OnErrorPane.Exists, "On Error Pane does not exist on large view after dragging tool in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.DoneButton.Exists, "Done Button does not exist on large view after dragging tool in from the toolbox.");
            //New Source
            SharepointToolsUIMap.Click_NewSource_From_SharepointReadFolderTool();
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
            WorkflowTabUIMap.Drag_Toolbox_Sharepoint_Read_Folder_Onto_DesignSurface();
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
