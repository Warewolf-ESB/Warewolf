using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.Tools.Recordset.RecordsetToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Recordset
{
    [CodedUITest]
    public class Find_Records
    {
        [TestMethod]
        [TestCategory("Recordset Tools")]
        public void FindRecordsTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.Exists, "Find Records tool on the design surface does not exist");
            //Small View
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.FieldsToSearchComboBox.Exists, "SearchFields Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Exists, "DataGrid does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.ResultComboBox.Exists, "Result Combobox does not exist on design surface");
            //Large View
            RecordsetToolsUIMap.Open_FindRecordIndexTool_LargeView();
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.FieldsToSearchComboBox.Exists, "SearchFields Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.LargeDataGridTable.Exists, "DataGrid does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.RequireAllMatchesTruCheckBox.Exists, "AllMatchesBeTrue Checkbox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.RequireAllFieldsToMatchCheckBox.Exists, "AllFieldsMatch Checkbox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.ResultComboBox.Exists, "Result Combobox does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.OnErrorCustom.Exists, "On Error Pane does not exist on design surface");
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.DoneButton.Exists, "Done Button does not exist on design surface");
        }

        [TestMethod]
        [TestCategory("Recordset Tools")]
        public void Selecting_OptionInDropdown_With_Keyboard_UITest()
        {
            Keyboard.SendKeys(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.FieldsToSearchComboBox.TextEdit, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(RecordsetToolsUIMap.MainStudioWindow.TabManSplitPane.TabManager.WorkflowTab.WorkflowSurfaceContext.UIContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchTypeCell.SearchTypeComboBox, "{Down}", ModifierKeys.None);
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.TabManSplitPane.TabManager.WorkflowTab.WorkflowSurfaceContext.UIContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchCriteriaCell.SearchCriteriaComboBox.Enabled);
        }

        [TestMethod]
        [TestCategory("Recordset Tools")]
        public void Selecting_OptionInDropdown_Changed_Fires_EventChanged()
        {
            Keyboard.SendKeys(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.FieldsToSearchComboBox.TextEdit, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(RecordsetToolsUIMap.MainStudioWindow.TabManSplitPane.TabManager.WorkflowTab.WorkflowSurfaceContext.UIContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchTypeCell.SearchTypeComboBox, "{Down}", ModifierKeys.None);
            Assert.IsTrue(RecordsetToolsUIMap.MainStudioWindow.TabManSplitPane.TabManager.WorkflowTab.WorkflowSurfaceContext.UIContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchCriteriaCell.SearchCriteriaComboBox.Enabled);
            RecordsetToolsUIMap.MainStudioWindow.TabManSplitPane.TabManager.WorkflowTab.WorkflowSurfaceContext.UIContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchCriteriaCell.SearchCriteriaComboBox.TextEdit.Text = "1";
            UIMap.Press_F6();
            Assert.AreEqual("1", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.FindRecordIndexTreeItem.InputsItem1Text.DisplayText);
            Assert.AreEqual("=", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.FindRecordIndexTreeItem.InputsSearchTypeItemText.DisplayText);
            Assert.AreEqual("1", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.FindRecordIndexTreeItem.InputsItem2Text.DisplayText);
            RecordsetToolsUIMap.MainStudioWindow.TabManSplitPane.TabManager.WorkflowTab.WorkflowSurfaceContext.UIContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchCriteriaCell.SearchCriteriaComboBox.TextEdit.Text = "2";
            Keyboard.SendKeys(RecordsetToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.FieldsToSearchComboBox.TextEdit, "{Tab}", ModifierKeys.Shift);
            Keyboard.SendKeys(RecordsetToolsUIMap.MainStudioWindow.TabManSplitPane.TabManager.WorkflowTab.WorkflowSurfaceContext.UIContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchTypeCell.SearchTypeComboBox, "{Down}", ModifierKeys.None);
            UIMap.Press_F6();
            Assert.AreEqual("1", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.FindRecordIndexTreeItem.InputsItem1Text.DisplayText);
            Assert.AreEqual(">", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.FindRecordIndexTreeItem.InputsSearchTypeItemText.DisplayText);
            Assert.AreEqual("2", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.FindRecordIndexTreeItem.InputsItem2Text.DisplayText);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Find_Record_Index_Onto_DesignSurface();
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
