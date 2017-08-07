using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.Recordset.RecordsetToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Recordset
{
    [CodedUITest]
    public class CountTest
    {
        [TestMethod]
        [TestCategory("Recordset Tools")]
        public void CountTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.Exists, "Count tool on the design surface does not exist");
            //Small View
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.SmallViewContentCustom.RecorsetComboBox.Exists, "Recordset Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.SmallViewContentCustom.ResultComboBox.Exists, "Result Combobox does not exist on design surface");
            //Large View
            RecordsetToolsUIMap.Open_CountRecords_LargeView();
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.LargeViewContentCustom.RecorsetComboBox.Exists, "Result Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.LargeViewContentCustom.NullAsZeroCheckBoxCheckBox.Exists, "NullAsZero Checkbox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.LargeViewContentCustom.ResultComboBox.Exists, "Result Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.LargeViewContentCustom.OnErrorCustom.Exists, "Result Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.DoneButton.Exists, "Done Button does not exist on design surface");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Count_Records_Onto_DesignSurface();
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

        RecordsetToolsUIMap RecordsetToolsUIMap
        {
            get
            {
                if (_RecordsetToolsUIMap == null)
                {
                    _RecordsetToolsUIMap = new RecordsetToolsUIMap();
                }

                return _RecordsetToolsUIMap;
            }
        }

        private RecordsetToolsUIMap _RecordsetToolsUIMap;

        #endregion
    }
}
