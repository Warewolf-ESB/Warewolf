using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebGet
    {
        [TestMethod]
		[TestCategory("HTTP Tools")]
        public void HttpWebGetToolClickLargeViewUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox.Exists, "Web GET large view sources combobox does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton.Exists, "Web GET large view generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.HeadersTable.Exists, "Web GET large view headers table generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton.Exists, "Web GET large view done does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebGetToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Click_AddNew_Web_Source_From_tool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Exists, "Address Textbox does not exist on new DB source wizard tab after openning it from the Web GET tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Exists, "Test Connection Button does not exist on new DB source wizard tab after openning it from the Web GET tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.Spinner.Exists, "Spinner does not exist on new DB source wizard tab after openning it from the Web GET tool.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebGetToolEnableGenerateOutputsButton()
        {
            UIMap.Select_Test_Source_From_GET_Web_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton.Enabled, "Web GET tool large view generate outputs button is not enabled after selecting a source.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebGetToolClickGenerateOutputsButton()
        {
            UIMap.Select_Test_Source_From_GET_Web_Large_View_Source_Combobox();
            UIMap.Click_GetWeb_GenerateOutputs_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton.Exists, "Web GET tool large view generate outputs done button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.CancelButton.Exists, "Web GET tool large view generate outputs cancel button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.PasteButton.Exists, "Web GET tool large view generate outputs paste button does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebGetToolClickTestInputsDoneButton()
        {
            UIMap.Select_Test_Source_From_GET_Web_Large_View_Source_Combobox();
            UIMap.Click_GET_Web_Large_View_Generate_Outputs();
            UIMap.Click_GET_Web_Large_View_Test_Inputs_Button();
            UIMap.Click_GET_Web_Large_View_Test_Inputs_Done_Button();
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebGetToolSmallView()
        {
            UIMap.Collapse_GET_Web_Connector_Tool_Large_View_to_Small_View();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after collapsing the large view with a double click.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.InitializeABlankWorkflow();
            UIMap.Drag_GET_Web_Connector_Onto_DesignSurface();
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
