using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class OracleDatabaseTests
    {
        [TestMethod]
        [TestCategory("Database Tools")]
        public void OracleDatabaseTool_LargeViewUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.SourcesComboBoxComboBox.Exists, "Sources combobox does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.EditSourceButton.Exists, "Edit Source button does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.NewSourceButton.Exists, "New Source button does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.ActionsComboBox.Exists, "Action Command combobox does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.GenerateOutputsButton.Exists, "Generate Outputs button does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.OutputsMappingDataGrTable.Exists, "Outputs Mapping table does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.RecordSetTextBoxEdit.Exists, "Recordset textbox does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.OnErrorCustom.Exists, "OnError panel does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.DoneButton.Exists, "Done button does not exist on Oracle database connector tool large view.");
        }

        [TestMethod]
        [TestCategory("Database Tools")]
        public void OracleDatabaseTool_SmallViewUITest()
        {
            UIMap.Collapse_Oracle_Tool_Large_View_to_Small_View_With_Double_Click();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.SmallView.Exists, "Oracle database connector tool small view does not exist after collapsing large view with a double click.");
        }

        [TestMethod]
        [TestCategory("Database Tools")]
        public void Click_OracleDatabaseTool_LargeView_NewSourceButton_UITest()
        {
            UIMap.Click_NewSourceButton_From_OracleTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "PostgreSQL Server Address combobox is disabled new PostgreSQL Source wizard tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Enabled, "Username textbox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Enabled, "Password textbos is not enabled.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Oracle_Database_Onto_DesignSurface();
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
