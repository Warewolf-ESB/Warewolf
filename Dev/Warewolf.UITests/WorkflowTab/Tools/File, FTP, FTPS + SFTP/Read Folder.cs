using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.FileFTPFTPSSFTP.FileToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;


namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Read_Folder
    {
        [TestMethod]
		[TestCategory("File Tools")]
        public void ReadFolderTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.Exists, "Read folder tool on the design surface does not exist");
            // Small View
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.SmallViewContentCustom.DirectoryComboBox.Exists, "Directory Combobox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.SmallViewContentCustom.ResultComboBox.Exists, "Result Combobox does not exist on the design surface");
            // Large View
            FileToolsUIMap.Open_ReadFolderTool_LargeView();
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.DirectoryComboBox.Exists, "Directory ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FilesRadioButton.Exists, "Files RadioButton does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FoldersRadioButton.Exists, "Folders RadioButton does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FilesFoldersRadioButton.Exists, "Files&Folders RadioButton does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.UserNameComboBox.Exists, "Username ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.PasswordEdit.Exists, "Password TextBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.PrivateKeyComboBox.Exists, "Private Key ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.ResultComboBox.Exists, "Result ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.OnErrorCustom.Exists, "OnError Pane does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.DoneButton.Exists, "Done Button does not exist on the design surface");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Read_Folder_Onto_DesignSurface();
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
