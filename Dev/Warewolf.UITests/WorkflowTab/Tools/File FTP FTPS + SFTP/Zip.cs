using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.FileFTPFTPSSFTP.FileToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Zip
    {
        [TestMethod]
		[TestCategory("File Tools")]
        public void ZipTool_OpenLargeViewUITest()
        {
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.Exists, "Zip tool does not exist after dragging in from the toolbox");
            // Small View
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.SmallViewContentCustom.FileOrFolderComboBox.Exists, "FileOrFolder ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.SmallViewContentCustom.DestinationComboBox.Exists, "Destination ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.SmallViewContentCustom.ResultComboBox.Exists, "Result ComboBox does not exist on the design surface");
            // Large View
            FileToolsUIMap.Open_ZipTool_LargeView();
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.FileOrFolderComboBox.Exists, "FileOrFolder ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.UserNameComboBox.Exists, "Username ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.PasswordEdit.Exists, "Password TextBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.PrivateKeyComboBox.Exists, "PrivateKey ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.DestinationComboBox.Exists, "Destination ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ZipDestinationUsernComboBox.Exists, "ZipUsername ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ZipDestinationPasswEdit.Exists, "ZipPassword TextBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ZipPrivateKeyComboBox.Exists, "ZipPrivateKey ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OverwriteCheckBox.Exists, "Overwrite CheckBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ArchivePasswordtEdit.Exists, "ArchivePassword TextBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox.Exists, "SelectedCompress ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ResultComboBox.Exists, "Result ComboBox does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.IsTrue(FileToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.DoneButton.Exists, "Done Button does not exist on the design surface");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Zip_Onto_DesignSurface();
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
