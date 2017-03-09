using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.HTTPWebMethods.HTTPToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UITests.WebSource.WebSourceUIMapClasses;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebGet
    {
        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HTTPWebGETTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.Exists, "GET Web connector does not exist on the design surface after drag and drop from toolbox.");
            //Small View
            HTTPToolsUIMap.WebGetTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after collapsing the large view with a double click.");
            //Large View
            HTTPToolsUIMap.WebGetTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox.Exists, "Web GET large view sources combobox does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton.Exists, "Web GET large view generate inputs button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.HeadersTable.Exists, "Web GET large view headers table generate inputs button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton.Exists, "Web GET large view done does not exist.");
            //New Source
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.NewSourceButton.Exists, "New Source Button does not exist");
            HTTPToolsUIMap.Click_NewSourceButton_From_HttpWebGetTool();
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Enabled, "Web server address textbox not enabled.");
            Assert.IsFalse(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button is enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton.Enabled, "Anonymous Radio button is not enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User Radio button is not enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.DefaultQueryTextBox.Enabled, "Default Query Textbox is not enabled");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HTTPWebGETTool_GenerateOutputs_And_TestInputs_UITest()
        {
            HTTPToolsUIMap.Select_GETWebTool_Source_From_SourceCombobox();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton.Enabled, "Web GET tool large view generate outputs button is not enabled after selecting a source.");
            HTTPToolsUIMap.Click_GETWebTool_GenerateOutputsButton();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.TestButton.Exists, "Web GET large view generate outputs test button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton.Exists, "Web GET tool large view generate outputs done button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.CancelButton.Exists, "Web GET tool large view generate outputs cancel button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.PasteButton.Exists, "Web GET tool large view generate outputs paste button does not exist.");
            HTTPToolsUIMap.Click_GETWebTool_TestInputsButton();
            HTTPToolsUIMap.Click_GETWebTool_Outputs_DoneButton();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_HTTPGETWebTool_Onto_DesignSurface();
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

        WebSourceUIMap WebSourceUIMap
        {
            get
            {
                if (_WebSourceUIMap == null)
                {
                    _WebSourceUIMap = new WebSourceUIMap();
                }

                return _WebSourceUIMap;
            }
        }

        private WebSourceUIMap _WebSourceUIMap;

        HTTPToolsUIMap HTTPToolsUIMap
        {
            get
            {
                if (_HTTPToolsUIMap == null)
                {
                    _HTTPToolsUIMap = new HTTPToolsUIMap();
                }

                return _HTTPToolsUIMap;
            }
        }

        private HTTPToolsUIMap _HTTPToolsUIMap;

        #endregion
    }
}
