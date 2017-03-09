using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DBSource.DBSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Database.DatabaseToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Database
{
    [CodedUITest]
    public class OracleDatabaseTests
    {
        const string SourceName = "OracleSourceFromTool";

        [TestMethod]
        [TestCategory("Database Tools")]
        public void OracleDatabaseTool_Small_And_LargeView_Then_NewSource_UITest()
        {

            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.Exists, "Oracle database tool does not exist after dragging in from the toolbox");
            // Small View
            DatabaseToolsUIMap.OracleDatabaseTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.SmallView.Exists, "Oracle database connector tool small view does not exist after collapsing large view with a double click.");
            // Large View
            DatabaseToolsUIMap.OracleDatabaseTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.SourcesComboBox.Exists, "Sources combobox does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.EditSourceButton.Exists, "Edit Source button does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.NewSourceButton.Exists, "New Source button does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.ActionsComboBox.Exists, "Action Command combobox does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.GenerateOutputsButton.Exists, "Generate Outputs button does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.OutputsMappingDataGrTable.Exists, "Outputs Mapping table does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.RecordSetTextBoxEdit.Exists, "Recordset textbox does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.OnErrorCustom.Exists, "OnError panel does not exist on Oracle database connector tool large view.");
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.DoneButton.Exists, "Done button does not exist on Oracle database connector tool large view.");
            // New Source
            DatabaseToolsUIMap.Click_NewSourceButton_From_OracleTool();
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.Exists, "Oracle Source Tab does not exist");
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Oracle Server Address combobox is disabled new PostgreSQL Source wizard tab");
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsFalse(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is enabled.");
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Enabled, "Username textbox is not enabled.");
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Enabled, "Password textbos is not enabled.");
            DBSourceUIMap.Enter_Text_Into_DatabaseServer_Tab();
            DBSourceUIMap.IEnterRunAsUserTestUserOnDatabaseSource();
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is not enabled.");
            DBSourceUIMap.Click_DB_Source_Wizard_Test_Connection_Button();
            DBSourceUIMap.Select_HR_From_DB_Source_Wizard_Database_Combobox();
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            DBSourceUIMap.Click_Close_DB_Source_Wizard_Tab_Button();
            // Open Source
            DatabaseToolsUIMap.OracleDatabaseTool_ChangeView_With_DoubleClick();
            DatabaseToolsUIMap.Select_Source_From_OracleTool();
            Assert.IsTrue(DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.EditSourceButton.Enabled, "Edit Source Button is not enabled after selecting source.");
            DatabaseToolsUIMap.Click_EditSourceButton_On_OracleTool();
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.Exists, "Oracle Source Tab does not exist");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Oracle_Database_Onto_DesignSurface();
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

        DBSourceUIMap DBSourceUIMap
        {
            get
            {
                if (_DBSourceUIMap == null)
                {
                    _DBSourceUIMap = new DBSourceUIMap();
                }

                return _DBSourceUIMap;
            }
        }

        private DBSourceUIMap _DBSourceUIMap;

        DatabaseToolsUIMap DatabaseToolsUIMap
        {
            get
            {
                if (_DatabaseToolsUIMap == null)
                {
                    _DatabaseToolsUIMap = new DatabaseToolsUIMap();
                }

                return _DatabaseToolsUIMap;
            }
        }

        private DatabaseToolsUIMap _DatabaseToolsUIMap;

        #endregion
    }
}
