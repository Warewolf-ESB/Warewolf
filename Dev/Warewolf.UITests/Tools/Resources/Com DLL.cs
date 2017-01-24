using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Resources
{
    [CodedUITest]
    public class Com_DLL
    {
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void ComDLLTool_LargeViewUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.SourcesCombobox.Exists, "Sources combobox does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.ActionsCombobox.Exists, "Actions Combobox does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.GenerateOutputsButton.Exists, "Generate Outputs Button does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.InputsTable.Exists, "Inputs Table does not exist on Com DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.DoneButton.Exists, "Done button does not exist on Com DLL tool large view after openning it by double clicking the small view.");
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void ComTool_SmallViewUITest()
        {
            UIMap.Collapse_Com_DLL_Connector_Tool_Large_View_to_Small_View_With_Double_Click();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.SmallView.Exists, "Com DLL tool small view does not exist after double clicking tool large view.");
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Click_ComDLLTool_LargeView_NewSourceButton_UITests()
        {
            UIMap.Click_NewSourceButton_From_COMDLLPluginTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.SearchTextBox.Enabled);
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.RefreshButton.RefreshSpinner);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree.Enabled);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.RefreshButton.Enabled);
        }
        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Com_DLL_Connector_Onto_DesignSurface();
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
