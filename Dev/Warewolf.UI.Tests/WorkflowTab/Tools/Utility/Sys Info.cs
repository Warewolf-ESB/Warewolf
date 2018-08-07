using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Utility
{
    [CodedUITest]
    public class Sys_Info
    {
        [TestMethod]
		[TestCategory("Utility Tools")]
        public void SysInfoTool_Small_And_LargeView_Then_QVIView_UITest()
        {
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.Exists, "GatherSysInfo Tool does not exist on the design surface.");
            //Small View
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.SmallViewContentCustom.SmallDataGridTable.Exists, "Variable Grid does not exist on the design surface.");
            //Large View
            UtilityToolsUIMap.Open_SystemInformationTool_LargeView();
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.SmallDataGridTable.Exists, "Variable Grid does not exist on the design surface.");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.Exists, "OnError group  does not exist on the design surface.");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.DoneButton.Exists, "Done button does not exist on the design surface.");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.OpenQuickVariableInpToggleButton.Exists, "QVIToggle button does not exist on the design surface.");
            //QVI View
            UtilityToolsUIMap.Open_SystemInformationTool_QVIView();
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.QuickVariableInputContent.Exists, "System Info QVI window does not exist on the design surface");
        }

        [TestMethod]
        [TestCategory("Data Tools")]
        public void SysInfo_Small_And_Large_Then_ShowErrorBox_UITest()
        {
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.Exists, "Sys Info on the design surface does not exist");
            //Small View
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.SmallViewContentCustom.SmallDataGridTable.Exists, "Data Grid does not exist on tool Case Covert Small View");
            //Large View
            UtilityToolsUIMap.Open_SystemInformationTool_LargeView();
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.DoneButton.Exists, "Done Button does not exist after opening Sys Info large view");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.SmallDataGridTable.Exists, "Data Grid does not exist on tool Case Convert Large View");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.OnErrorCustom.Exists, "On Error Pane does not exist on tool Case Convert Large View");
            UtilityToolsUIMap.Enter_Values_Into_SysInfo_Large_Tool("[[1]]");
            //Done
            Mouse.Click(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.DoneButton);
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.Exists, "Sys Info large view is closed after clicking Done on an incorrect variable entered");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfoError.Exists, "Validation box did not show after puting Invalid Variable.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_System_Information_Onto_DesignSurface();
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

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;

        #endregion
    }
}
