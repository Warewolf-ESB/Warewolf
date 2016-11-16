using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Find_Records
    {
        [TestMethod]
        [TestCategory("Recordset Tools")]
        public void FindRecordsTool__OpenLargeViewUITest()
        {
            UIMap.Open_Find_Record_Index_Tool_Large_View();
        }

        [TestMethod]
        [TestCategory("Recordset Tools")]
        public void ToolDesigners_FindRecordslargeView_TabbingToResultBox_FocusIsSetToResultBox_UITest()
        {
            UIMap.Open_Find_Record_Index_Tool_Large_View();
            UIMap.Click_RequireAllFieldsToMatch_CheckBox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.ResultComboBox.TextEdit.HasFocus, "Focus is not set to Result combobox after tabbing from RequireAllFieldsToMatchCheckBox");
        }

        [TestMethod]
        [TestCategory("Recordset Tools")]
        public void ToolDesigners_FindRecordsSmallView_SelectingOptionInDopdownWithKeyboard_MatchesBoxEnabled_UITest()
        {
            Keyboard.SendKeys(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.FieldsToSearchComboBox.TextEdit, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchTypeCell.SearchTypeComboBox, "{Down}", ModifierKeys.None);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.SmallViewContentCustom.SmallDataGridTable.Row1.SearchCriteriaCell.SearchCriteriaComboBox.Enabled);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Find_Record_Index_Onto_DesignSurface();
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

        #endregion
    }
}
