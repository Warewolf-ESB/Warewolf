using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Storage_Dropbox
{
    [CodedUITest]
    public class List_Contents
    {
        [TestMethod]
        [TestCategory("Dropbox Tools")]
        public void Dropbox_FileList_Tool_Small_View()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.SmallViewContent.DropboxPathIntellisenseCombobox.Textbox.Exists, "Dropbox file path textbox does not exist on Dropbox FileList tool small view after dragging in from the toolbox.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.SmallViewContent.ResultsIntellisenseCombobox.Textbox.Exists, "Result textbox does not exist on Dropbox FileList tool small view after dragging in from the toolbox.");
        }

        [TestMethod]
        [TestCategory("Dropbox Tools")]
        public void Dropbox_FileList_Tool_Large_View()
        {
            UIMap.Open_Dropbox_List_Contents_Tool_Large_View_With_Double_Click();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.SourceCombobox.Exists, "Source combobox does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.EditSourceButton.Exists, "Edit Source Button does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.NewSourceButton.Exists, "New Source Button does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.DropboxPathIntellisenseCombobox.Textbox.Exists, "Dropbox file path textbox does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.ResultIntellisenseCombobox.Textbox.Exists, "Result textbox does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.SourceCombobox.Exists, "Source combobox does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.OnErrorPane.OnErrorGroup.Exists, "OnError Group does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.DoneButton.Exists, "Done Button does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Dropbox_FileList_Onto_DesignSurface();
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
