using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.SharepointSource.SharepointSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Sharepoint.SharepointToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Sharepoint
{
    [CodedUITest]
    public class Upload_File
    {
        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void SharepointUploadFileTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.Exists, "Sharepoint upload file tool does does not exist after dragging tool from toolbox.");
            //Small View
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.Server.Exists, "Server Combobox does not exist on tool small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.EditSourceButton.Exists, "Edit source button does not exist on tool small view after dragging in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.LocalPathFromIntellisenseCombobox.Exists, "Local Path From Intellisense Combobox does not exist on tool small view after dragging in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.ServerPathToIntellisenseCombobox.Exists, "Server Path To Intellisense Combobox does not exist on tool small view after dragging in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.ResultsIntellisenseCombobox.Exists, "Result Combobox does not exist on tool small view after dragging in from the toolbox.");
            //Large View
            SharepointToolsUIMap.Open_SharepointUploadTool_LargeView();
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.Server.Exists, "Server Combobox does not exist on tool large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.EditSourceButton.Exists, "Edit source button does not exist on tool large view after dragging in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.LocalPathFromIntellisenseCombobox.Exists, "Local Path From Intellisense Combobox does not exist on tool large view after dragging in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.ServerPathToIntellisenseCombobox.Exists, "Server Path To Intellisense Combobox does not exist on tool large view after dragging in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.ResultsIntellisenseCombobox.Exists, "Result Combobox does not exist on tool large view after dragging in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.OnErrorPane.Exists, "On Error Pane does not exist on tool large view after dragging in from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.DoneButton.Exists, "Done Button does not exist on tool large view after dragging in from the toolbox.");
            //New Source
            SharepointToolsUIMap.Click_NewSource_From_SharepointUploadFileTool();
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
            WorkflowTabUIMap.Drag_Toolbox_Sharepoint_UploadFile_Onto_DesignSurface();
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
