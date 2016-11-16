using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Format_Number
    {
        [TestMethod]
        [TestCategory("Utility Tools")]
        public void FormatNumberTool_OpenLargeViewUITest()
        {
            UIMap.Open_NumberFormat_Toolbox_Large_View();
        }

        [TestMethod]
		[TestCategory("Utility Tools")]
        public void FormatNumberTool_SetRoundingType_None_ExpectedRoundingInputIsDisabled_UITest()
        {            
            UIMap.Open_NumberFormat_Toolbox_Large_View();
            Assert.AreEqual("None", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.SelectedItem);
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingInputComboBox.Enabled);
        }

        [TestMethod]
		[TestCategory("Utility Tools")]
        public void FormatNumberTool_ChangeRoundingType_None_Expected_RoundingInputBecomesDisabledAndEmpty_UITest()
        {            
            UIMap.Open_NumberFormat_Toolbox_Large_View();
            UIMap.Select_RoundingType_Normal();
            Assert.AreEqual("Normal", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.SelectedItem);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingInputComboBox.Enabled);
            UIMap.Select_RoundingType_None();
            Assert.AreEqual("None", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.SelectedItem);
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingInputComboBox.Enabled);
        }        

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Format_Number_Onto_DesignSurface();
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
