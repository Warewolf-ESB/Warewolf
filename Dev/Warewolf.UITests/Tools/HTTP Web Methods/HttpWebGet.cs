using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebGet
    {
        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HTTPWebGetTool_LargeView_UITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox.Exists, "Web GET large view sources combobox does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton.Exists, "Web GET large view generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.HeadersTable.Exists, "Web GET large view headers table generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton.Exists, "Web GET large view done does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HTTPWebGetTool_SmallView_UITest()
        {
            UIMap.Collapse_GETWebTool_LargeView_To_SmallView();
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void NewSource_From_HTTPWebGetTool_UITest()
        {
            UIMap.Click_NewSourceButton_From_HttpWebGetTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Enabled, "Web server address textbox not enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton.Enabled, "Anonymous Radio button is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User Radio button is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.DefaultQueryTextBox.Enabled, "Default Query Textbox is not enabled");
            UIMap.Click_Close_Web_Source_Wizard_Tab_Button();
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void Click_HTTPWebGetTool_GenerateOutputsButton_UITest()
        {
            UIMap.Select_GETWebTool_Source_From_SourceCombobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton.Enabled, "Web GET tool large view generate outputs button is not enabled after selecting a source.");
            UIMap.Click_GETWebTool_GenerateOutputsButton();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton.Exists, "Web GET tool large view generate outputs done button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.CancelButton.Exists, "Web GET tool large view generate outputs cancel button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.PasteButton.Exists, "Web GET tool large view generate outputs paste button does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void Click_HTTPWebGetTool_TestInputs_DoneButton_UITest()
        {
            UIMap.Select_GETWebTool_Source_From_SourceCombobox();
            UIMap.Click_GETWebTool_GenerateOutputsButton();
            UIMap.Click_GETWebTool_TestInputsButton();
            UIMap.Click_GETWebTool_Outputs_DoneButton();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
            UIMap.Drag_GETWebTool_Onto_DesignSurface();
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

        #endregion
    }
}
