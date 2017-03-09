using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.FileFTPFTPSSFTP.FileToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Delete
    {
        [TestMethod]
		[TestCategory("File Tools")]
        public void PathDeleteTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.Exists, "Delete tool on the design surface does not exist");
            // Small View
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.SmallViewContentCustom.FileOrFolderComboBox.Exists, "File/Folder ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.SmallViewContentCustom.ResultComboBox.Exists, "Result ComboBox on the design surface does not exist");
            // Large View
            FileToolsUIMap.Open_DeleteTool_LargeView();
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.FileOrFolderComboBox.Exists, "File/Folder ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.UserNameComboBox.Exists, "UserName ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.PasswordEdit.Exists, "Password TextBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.PrivateKeyComboBox.Exists, "Private Key ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.ResultComboBox.Exists, "Result ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.OnErrorCustom.Exists, "OnError Pane on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.DoneButton.Exists, "Done Button on the design surface does not exist");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Delete_Onto_DesignSurface();
        }

        UIMap UIMap
        {
            get
            {
                if ((_UIMap == null))
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

        FileToolsUIMap FileToolsUIMap
        {
            get
            {
                if (_FileToolsUIMap == null)
                {
                    _FileToolsUIMap = new FileToolsUIMap();
                }

                return _FileToolsUIMap;
            }
        }

        private FileToolsUIMap _FileToolsUIMap;

        #endregion
    }
}
