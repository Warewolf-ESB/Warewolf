using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.Recordset.RecordsetToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Recordset
{
    [CodedUITest]
    public class DeleteRecordTest
    {
        [TestMethod]
        [TestCategory("Recordset Tools")]
        public void DeleteTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.Exists, "Delete Record tool does not exist on design surface after dragging in from the toolbox.");
            //Small View
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.SmallViewContentCustom.RecordsetComboBox.Exists, "Recordset Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.SmallViewContentCustom.ResultComboBox.Exists, "Result Combobox does not exist on design surface");
            //Large View
            RecordsetToolsUIMap.Open_DeleteRecords_LargeView();
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.LargeViewContentCustom.RecordsetComboBox.Exists, "Result Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.LargeViewContentCustom.NullAsZeroCheckBoxCheckBox.Exists, "NullAsZero Checkbox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.LargeViewContentCustom.ResultComboBox.Exists, "Result Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.LargeViewContentCustom.OnErrorCustom.Exists, "Result Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.DoneButton.Exists, "Done Button does not exist on design surface");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Delete_Record_Onto_DesignSurface();
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
