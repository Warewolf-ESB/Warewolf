﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebDelete
    {
        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolClickLargeViewUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.Exists, "Web Delete large view sources combobox does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton.Exists, "Web Delete large view generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Table.Exists, "Web Delete large view headers table generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.DoneButton.Exists, "Web Delete large view done does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Click_HTTP_Delete_Web_Tool_New_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.Exists, "Manage DatabaseSource Control does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Exists, "Test Connection Button does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ErrorText.Exists, "Error Text does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.Exists, "Password TextBox does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.Exists, "UserName TextBox does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Exists, "UserRadio Button does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Exists, "Windows Radio Button does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.CancelTestButton.Exists, "Cancel Test Button does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ConnectionPassedImage.Exists, "Connection Passed Image does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.Spinner.Exists, "Spinner does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolClickTestInputsDoneButton()
        {
            UIMap.Select_UITestingSource_From_Web_Server_Large_View_Source_Combobox();
            UIMap.Click_DeleteWeb_Generate_Outputs();
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolSmallView()
        {
            UIMap.Open_DeleteWeb_Tool_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.InitializeABlankWorkflow();
            UIMap.Drag_DeleteWeb_Toolbox_Onto_Workflow_Surface();
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
