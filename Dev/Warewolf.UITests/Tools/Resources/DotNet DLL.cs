﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace Warewolf.UITests.Tools.Resources
{
    [CodedUITest]
    public class DotNet_DLL
    {
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void DotNetDLLTool_LargeViewUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Exists, "Class name textbox does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Exists, "Actions Combobox does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.GenerateOutputsButton.Exists, "Generate Outputs Button does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.InputsTable.Exists, "Inputs Table does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.DoneButton.Exists, "Done button does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void DotNetDLLTool_SmallViewUITest()
        {
            UIMap.Collapse_DotNet_DLL_Connector_Tool_Large_View_to_Small_View_With_Double_Click();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.SmallView.Exists, "DotNet DLL tool small view does not exist after double clicking tool large view.");
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Click_DotNetDLLTool_LargeView_NewSourceButton_UITests()
        {
            UIMap.Click_NewSourceButton_From_DotNetDLLPluginTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.Enabled, "Assembly Combobox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyDirectoryButton.Enabled, "Assembly Combobox Button is not enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File Combobox is enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileDirectoryButton.Enabled, "Config File Combobox Button is enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyComboBox.Enabled, "GAC Assembly Combobox is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyDirectoryButton.Enabled, "GAC Assembly Combobox Button is not enabled");
        }



        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_DotNet_DLL_Connector_Onto_DesignSurface();
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
