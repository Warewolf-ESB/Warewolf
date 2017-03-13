using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.HTTPWebMethods.HTTPToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UITests.WebSource.WebSourceUIMapClasses;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebDelete
    {
        const string SourceName = "WebSourceFromTool";

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HTTPWebDELETETool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.Exists, "Delete Web Tool does not exist on the design surface after drag and drop from toolbox.");
            //Small View
            HTTPToolsUIMap.WebDeleteTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.SmallView.Exists, "Web DELETE small view does not exist after collapsing the large view with a double click.");
            //Large View
            HTTPToolsUIMap.WebDeleteTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.Exists, "Web DELETE large view sources combobox does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton.Exists, "Web DELETE large view generate inputs button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Table.Exists, "Web DELETE large view headers table generate inputs button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.DoneButton.Exists, "Web DELETE large view done does not exist.");
            //New Source
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.NewSourceButton.Exists, "New Source Button does not exist");
            HTTPToolsUIMap.Click_NewSourceButton_From_HttpWebDeleteTool();
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Enabled, "Web server address textbox not enabled.");
            Assert.IsFalse(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button is enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton.Enabled, "Anonymous Radio button is not enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User Radio button is not enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.DefaultQueryTextBox.Enabled, "Default Query Textbox is not enabled");
            WebSourceUIMap.Click_UserButton_On_WebServiceSourceTab();
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserNameTextBox.Enabled, "Username Textbox not enabled");
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.PasswordTextBox.Enabled, "Password Textbox not enabled");
            WebSourceUIMap.Enter_TextIntoAddress_On_WebServiceSourceTab();
            WebSourceUIMap.Enter_RunAsUser_On_WebServiceSourceTab();
            WebSourceUIMap.Enter_DefaultQuery_On_WebServiceSourceTab();
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button not enabled");
            WebSourceUIMap.Click_NewWebSource_TestConnectionButton();
            UIMap.MainStudioWindow.SideMenuBar.SaveButton.WaitForControlEnabled(60000);
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save Ribbon Button is not enabled after entering valid web source details, clicking test in web source wizard and waiting one minute (6000ms).");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            WebSourceUIMap.Click_Close_Web_Source_Wizard_Tab_Button();
            //Edit Source
            HTTPToolsUIMap.WebDeleteTool_ChangeView_With_DoubleClick();
            HTTPToolsUIMap.Select_Source_From_DELETEWebTool();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.EditSourceButton.Enabled, "Edit Source Button is not enabled after selecting source.");
            HTTPToolsUIMap.Click_EditSourceButton_On_DELETEWebTool();
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Enabled, "Web server address textbox not enabled.");
            WebSourceUIMap.Click_AnonymousButton_On_WebServiceSourceTab();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            WebSourceUIMap.Click_Close_Web_Source_Wizard_Tab_Button();
            HTTPToolsUIMap.WebDeleteTool_ChangeView_With_DoubleClick();
            HTTPToolsUIMap.Click_EditSourceButton_On_DELETEWebTool();
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton.Selected);
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HTTPWebDELETETool_GenerateOutputs_And_TestInputs_UITest()
        {
            HTTPToolsUIMap.Select_DELETEWebTool_Source_From_SourceCombobox();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton.Enabled, "Web DELETE tool large view generate outputs button is not enabled after selecting a source.");
            HTTPToolsUIMap.Click_DELETEWebTool_GenerateOutputsButton();
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.TestButton.Exists, "Web DELETE tool large view generate outputs test button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.DoneButton.Exists, "Web DELETE tool large view generate outputs done button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.CancelButton.Exists, "Web DELETE tool large view generate outputs cancel button does not exist.");
            Assert.IsTrue(HTTPToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.PasteButton.Exists, "Web DELETE tool large view generate outputs paste button does not exist.");
            HTTPToolsUIMap.Click_DELETEWebTool_TestInputsButton();
            HTTPToolsUIMap.Click_DELETEWebTool_Outputs_DoneButton();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_HTTPDELETEWebTool_Onto_DesignSurface();
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
