using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Data_Merge
    {
        [TestMethod]
        [TestCategory("Data Tools")]
        public void DataMergeTool_Small_And_Large_Then_QVIView_UITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.Exists, "Data Merge on the design surface does not exist");
            //Small View
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Exists, "Data Grid does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.ResultTextbox.Exists, "Results Textbox does not exist");
            //Large View
            UIMap.Open_DataMerge_LargeView();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Exists, "Data Grid does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.ResultTextbox.Exists, "Result Textbox does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.OnErrorGroup.Exists, "On Error Pane does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.DoneButton.Exists, "Done Button does not exist");
            //QVI View
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.QVIToggleButton.Exists, "QVI Toggle Button on DataMerge Tool does not exist");
            UIMap.Open_DataMergeTool_QVIView();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.QuickVariableInputContent.Exists, "QVI on DataMerge Tool is not open");
        }

        [TestMethod]
        [TestCategory("Data Tools")]
        public void DataMerge_ScrollingUITest()
        {
            UIMap.Open_DataMerge_LargeView();
            UIMap.Enter_Values_Into_DataMergeTool();
            UIMap.Open_DataMergeTool_SmallView();
            UIMap.Scroll_DownThenUp_On_DataMergeTool_SmallView();
            Assert.AreEqual("1", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row.UsingCell.Row1UsingDComboBox.TextEdit.Text);
            Assert.AreEqual("2", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row2.UsingCell.Row2UsingComboBox.TextEdit.Text);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Data_Merge_Onto_DesignSurface();
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

        #endregion
    }
}
