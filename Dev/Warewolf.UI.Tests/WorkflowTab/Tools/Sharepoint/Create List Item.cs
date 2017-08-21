using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.SharepointSource.SharepointSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Sharepoint.SharepointToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Sharepoint
{
    [CodedUITest]
    public class Create_List_Item
    {
        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void SharepointCreateListTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.Exists, "Sharepoint Create List Item tool does does not exist after dragging tool from toolbox.");
            //Small View
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server.Exists, "Server Combobox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.EditSourceButton.Exists, "Edit Source Button does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.Exists, "Method List does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.RefreshButton.Exists, "Refresh Button does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.ResultComboBox.Exists, "Result Combobox does not exist on small view after tool has been dragged from the toolbox.");
            //Large View
            SharepointToolsUIMap.Open_SharepointCreateTool_LargeView();
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.Server.Exists, "Server Combobox does not exist on large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.EditSourceButton.Exists, "Edit Source Button does not exist on large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.MethodList.Exists, "Method List does not exist on large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.RefreshButton.Exists, "Refresh Button does not exist on large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.DataGridTable.Exists, "DataGrid Table does not exist on large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.ResultComboBox.Exists, "Result Combobox does not exist on large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.OnErrorPane.Exists, "On Error Pane does not exist on large view after tool has been dragged from the toolbox.");
            Assert.IsTrue(SharepointToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.DoneButton.Exists, "Done Button does not exist on large view after tool has been dragged from the toolbox.");
            //New Source
            SharepointToolsUIMap.Click_NewSource_From_SharepointCreateListItemTool();
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
            WorkflowTabUIMap.Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface();
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
