using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.FileFTPFTPSSFTP.FileToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Copy
    {
        [TestMethod]
		[TestCategory("File Tools")]
        public void PathCopyTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.Exists, "Copy on the design surface does not exist");
            // Small View
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.SmallViewContentCustom.FileOrFolderComboBox.Exists, "File/Folder Combobox does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.SmallViewContentCustom.DestinationComboBox.Exists, "Destination Combobox does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.SmallViewContentCustom.ResultComboBox.Exists, "Result Combobox does not exist");
            // Large View
            FileToolsUIMap.Open_CopyTool_LargeView();
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.FileOrFolderComboBox.Exists, "FileOrFolder ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.UserNameComboBox.Exists, "Username Combobox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.PasswordEdit.Exists, "Password Textbox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.PrivateKeyComboBox.Exists, "Private Key ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationComboBox.Exists, "Destination ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationUserNameComboBox.Exists, "Destination Username ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationPasswordEdit.Exists, "Destination Password Textbox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationPrivateKeyComboBox.Exists, "Destination Private Key ComboBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.OverwriteCheckBox.Exists, "Overwrite CheckBox on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.OnErrorCustom.Exists, "OnError Custom group on the design surface does not exist");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.DoneButton.Exists, "DoneButton on the design surface does not exist");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Copy_Onto_DesignSurface();
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
