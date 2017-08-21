using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DropboxSource.DropboxSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.StorageDropbox.DropboxToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Storage_Dropbox
{
    [CodedUITest]
    public class List_Contents
    {
        [TestMethod]
        [TestCategory("Dropbox Tools")]
        public void DropboxListContentsTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.Exists, "Dropbox list contents tool does not exist on design surface after dragging in from the toolbox.");
            //Small View
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.SmallViewContent.DropboxPathIntellisenseCombobox.Textbox.Exists, "Dropbox file path textbox does not exist on Dropbox FileList tool small view after dragging in from the toolbox.");
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.SmallViewContent.ResultsIntellisenseCombobox.Textbox.Exists, "Result textbox does not exist on Dropbox FileList tool small view after dragging in from the toolbox.");
            //Large View
            DropboxToolsUIMap.DropboxListContentsTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.SourceCombobox.Exists, "Source combobox does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.EditSourceButton.Exists, "Edit Source Button does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.NewSourceButton.Exists, "New Source Button does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.DropboxPathIntellisenseCombobox.Textbox.Exists, "Dropbox file path textbox does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.ResultIntellisenseCombobox.Textbox.Exists, "Result textbox does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.SourceCombobox.Exists, "Source combobox does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.OnErrorPane.OnErrorGroup.Exists, "OnError Group does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            Assert.IsTrue(DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.DoneButton.Exists, "Done Button does not exist on Dropbox FileList tool large view after openning the large view with a double click.");
            //New Source
            DropboxToolsUIMap.Click_NewSourceButton_From_DropboxListContentsTool();
            Assert.IsFalse(DropboxSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.AuthoriseButton.Enabled, "Authorise button is enabled");
            Assert.IsTrue(DropboxSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.ServerTypeComboBox.Enabled, "Server Type Combobox is not enabled");
            Assert.IsTrue(DropboxSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.OAuthKeyTextBox.Enabled, "OAuth Key Textbox is not enabled");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Dropbox_FileList_Onto_DesignSurface();
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

        DropboxSourceUIMap DropboxSourceUIMap
        {
            get
            {
                if (_DropboxSourceUIMap == null)
                {
                    _DropboxSourceUIMap = new DropboxSourceUIMap();
                }

                return _DropboxSourceUIMap;
            }
        }

        private DropboxSourceUIMap _DropboxSourceUIMap;

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
