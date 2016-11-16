using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Sharepoint
{
    [CodedUITest]
    public class Download_File
    {
        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void Sharepoint_Download_File_Small_View_UITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.SmallView.SourceCombobox.Exists, "Source combobox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.SmallView.EditSourceButton.Exists, "Edit Source Button does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.SmallView.LocalPathToIntellisenseCombobox.Textbox.Exists, "Local Path To textbox does not exist on small view after tool has been dragged from the toolbox.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.SmallView.ServerPathFromIntellisenseCombobox.Textbox.Exists, "Server Path From textbox does not exist on small view after tool has been dragged from the toolbox.");
        }

        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void Sharepoint_Download_File_Large_View_UITest()
        {
            UIMap.Open_Sharepoint_Download_File_Tool_Large_View_With_Double_Click();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.LargeView.SourceCombobox.Exists, "Source combobox does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.LargeView.LocalPathToIntellisenseCombobox.Textbox.Exists, "Local Path To textbox does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.LargeView.ServerPathFromIntellisenseTextbox.Textbox.Exists, "Server Path From textbox does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.LargeView.OnErrorPane.Exists, "OnError Pane does not exist on tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.DoneButton.Exists, "Done Button does not exist on tool large view after openning the large view with a double click.");
        }

       #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Sharepoint_Download_File_Onto_DesignSurface();
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

        #endregion
    }
}
