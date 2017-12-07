using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Data
{
    [CodedUITest]
    public class Case_Convert
    {
        [TestMethod]
		[TestCategory("Data Tools")]
        public void CaseConvertTool_Small_And_Large_Then_QVIView_UITest()
        {
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.Exists, "Case Conversion on the design surface does not exist");
            //Small View
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.SmallViewContentCustom.SmallDataGridTable.Exists, "Data Grid does not exist on tool Case Covert Small View");
            //Large View
            DataToolsUIMap.Open_CaseConvertTool_LargeView();
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.DoneButton.Exists, "Done Button does not exist after opening Case Convert large view");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.SmallDataGridTable.Exists, "Data Grid does not exist on tool Case Convert Large View");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.OnErrorCustom.Exists, "On Error Pane does not exist on tool Case Convert Large View");
            //QVI View
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.OpenQuickVariableInpToggleButton.Exists, "QVI Toggle Button does not exist");
            DataToolsUIMap.Open_CaseConvertTool_QVIView();
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.QuickVariableInputContent.Exists, "QVI on CaseConvert is not open");
        }

        [TestMethod]
        [TestCategory("Data Tools")]
        public void CaseConvertTool_Small_And_Large_Then_ShowErrorBox_UITest()
        {
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.Exists, "Case Conversion on the design surface does not exist");
            //Small View
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.SmallViewContentCustom.SmallDataGridTable.Exists, "Data Grid does not exist on tool Case Covert Small View");
            //Large View
            DataToolsUIMap.Open_CaseConvertTool_LargeView();
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.DoneButton.Exists, "Done Button does not exist after opening Case Convert large view");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.SmallDataGridTable.Exists, "Data Grid does not exist on tool Case Convert Large View");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.OnErrorCustom.Exists, "On Error Pane does not exist on tool Case Convert Large View");
            DataToolsUIMap.Enter_Values_Into_Case_Conversion_Large_Tool("[[1]]");
            //Done
            Mouse.Click(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.DoneButton);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.Exists, "Case Convert large view is closed after clicking Done on an incorrect variable entered");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvertError.Exists, "Validation box did not show after puting Invalid Variable.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Case_Conversion_Onto_DesignSurface();
        }

        UIMap UIMap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

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

        DataToolsUIMap DataToolsUIMap
        {
            get
            {
                if (_DataToolsUIMap == null)
                {
                    _DataToolsUIMap = new DataToolsUIMap();
                }

                return _DataToolsUIMap;
            }
        }

        private DataToolsUIMap _DataToolsUIMap;

        #endregion
    }
}
