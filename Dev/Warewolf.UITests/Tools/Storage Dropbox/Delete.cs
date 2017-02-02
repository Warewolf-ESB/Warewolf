using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Storage_Dropbox
{
    [CodedUITest]
    public class Delete
    {
        [TestMethod]
        [TestCategory("Dropbox Tools")]
        public void DropboxDeleteTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            //Small View
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.SmallViewContent.DropboxFileIntellisenseCombobox.Textbox.Exists, "Local file path textbox does not exist on Dropbox Delete tool small view after dragging in from the toolbox.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.SmallViewContent.DropboxFileIntellisenseCombobox.Textbox.Exists, "Dropbox file path textbox does not exist on Dropbox Delete tool small view after dragging in from the toolbox.");
            //Large View
            UIMap.DropboxDeleteTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContent.SourceCombobox.Exists, "Source combobox does not exist on Dropbox Delete tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContent.EditSourceButton.Exists, "Edit Source Button does not exist on Dropbox Delete tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContent.NewSourceButton.Exists, "New Source Button does not exist on Dropbox Delete tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContent.DropboxFileIntellisenseCombobox.Textbox.Exists, "Local file path textbox does not exist on Dropbox Delete tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContent.DropboxFileIntellisenseCombobox.Textbox.Exists, "Dropbox file path textbox does not exist on Dropbox Delete tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContent.SourceCombobox.Exists, "Source combobox does not exist on Dropbox Delete tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContent.OnErrorPane.OnErrorGroup.Exists, "OnError Group does not exist on Dropbox Delete tool large view after openning the large view with a double click.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.DoneButton.Exists, "Done Button does not exist on Dropbox Delete tool large view after openning the large view with a double click.");
            //New Source
            UIMap.Click_NewSourceButton_From_DropboxDeleteTool();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.AuthoriseButton.Enabled, "Authorise button is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.ServerTypeComboBox.Enabled, "Server Type Combobox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.OAuthKeyTextBox.Enabled, "OAuth Key Textbox is not enabled");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            UIMap.Drag_Toolbox_Dropbox_Delete_Onto_DesignSurface();
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
