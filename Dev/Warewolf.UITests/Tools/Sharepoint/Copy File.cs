using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.SharepointSource.SharepointSourceUIMapClasses;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

namespace Warewolf.UITests.Tools.Sharepoint
{
    [CodedUITest]
    public class Copy_File
    {

        const string SourceName = "SharepointSourceFromTool";

        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void SharepointCopyFileTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.Exists, "Sharepoint Copy tool does does not exist after dragging tool from toolbox.");
            //Small View
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server.Exists, "Server Combobox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.EditSourceButton.Exists, "Edit Source Button does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.FromDirectoryComboBox.Exists, "From Directory Combobox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.PathDirectoryComboBox.Exists, "Path Directory Combobox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.ResultComboBox.Exists, "Result Combobox does not exist on small view after tool has been dragged from the toolbox.");
            //Large View
            ToolsUIMap.Open_SharepointCopyTool_LargeView();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server.Exists, "Server Combobox does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.EditSourceButton.Exists, "Edit Source Button does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.FromDirectoryComboBox.Exists, "From Directory Combobox does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.PathDirectoryComboBox.Exists, "Path Directory Combobox does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.OverwriteCheckBox.Exists, "Overwrite Checkbox does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.ResultComboBox.Exists, "Result Combobox does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.OnErrorPane.Exists, "On Error Pane does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.DoneButton.Exists, "Done Button does not exist on tool large view after openning the large view with a double click.");
            //New Source
            ToolsUIMap.Click_NewSource_From_SharepointCopyFileTool();
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.Exists, "Sharepoint Source Tab does not exist.");
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Enabled, "Server Name Textbox is not enabled.");
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.WindowsRadioButton.Enabled, "Windows Radio button is not enabled.");
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserRadioButton.Enabled, "User Radio button is not enabled.");
            Assert.IsFalse(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton.Enabled, "Test Connection button is enabled.");
            Assert.IsFalse(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.CancelTestButton.Enabled, "Cancel Test button is  enabled.");
            SharepointSourceUIMap.Enter_TextIntoAddress_In_SharepointServiceSourceTab();
            SharepointSourceUIMap.Click_UserButton_On_SharepointSource();
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserNameTextBox.Exists);
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox.Exists);
            SharepointSourceUIMap.Enter_Sharepoint_ServerSource_User_Credentials();
            SharepointSourceUIMap.Click_Sharepoint_Server_Source_TestConnection();
            SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton.DrawHighlight();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new source.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            SharepointSourceUIMap.Click_Close_SharepointSource_Tab_Button();
            //Edit Source
            ToolsUIMap.Open_SharepointCopyTool_LargeView();
            ToolsUIMap.Select_Source_From_SharepointCopyFileTool();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.EditSourceButton.Enabled, "Edit Source Button is not enabled after selecting source.");
            ToolsUIMap.Click_EditSourceButton_On_SharepointCopyFileTool();
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.Exists, "Sharepoint Source Tab does not exist.");
            SharepointSourceUIMap.Click_WindowsButton_On_SharepointSource();
            SharepointSourceUIMap.Click_Sharepoint_Server_Source_TestConnection();
            SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton.DrawHighlight();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            SharepointSourceUIMap.Click_Close_SharepointSource_Tab_Button();
            ToolsUIMap.Open_SharepointCopyTool_LargeView();
            ToolsUIMap.Click_EditSourceButton_On_SharepointCopyFileTool();
            Assert.IsTrue(SharepointSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.WindowsRadioButton.Selected);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            ToolsUIMap.Drag_Toolbox_Sharepoint_CopyFile_Onto_DesignSurface();
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

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

        #endregion
    }
}
