using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    [CodedUITest]
    public class Data_Merge
    {
        [TestMethod]
        [TestCategory("Data Tools")]
        public void DataMergeUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.UISmallDataGridTable.Row.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.UISmallDataGridTable.Row2.Exists);
        }

        [TestMethod]
        [TestCategory("Data Tools")]
        public void DataMergeTool_OpenLargeViewUITest()
        {
            UIMap.Open_DataMerge_LargeView();
        }

        [TestMethod]
        [TestCategory("Data Tools")]
        public void DataMergeTool_OpenQVIUITest()
        {
            UIMap.Open_DataMergeToolQvi_LargeView();
        }

        [TestMethod]
        [TestCategory("Data Tools")]
        public void DataMerge_ScrollingUITest()
        {
            UIMap.Open_DataMerge_LargeView();
            UIMap.Enter_Values_Into_Data_Merge_Tool_Large_View();
            UIMap.Close_Data_Merge_LargeView();
            UIMap.Scroll_Down_Then_Up_On_The_DataMerge_SmallView();
            Assert.AreEqual("1", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.UISmallDataGridTable.Row.UsingCell.Row1UsingDComboBox.TextEdit.Text);
            Assert.AreEqual("2", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.UISmallDataGridTable.Row2.UsingCell.Row2UsingComboBox.TextEdit.Text);
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
