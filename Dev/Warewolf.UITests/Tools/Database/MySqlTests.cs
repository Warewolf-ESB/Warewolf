using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DBSource.DBSourceUIMapClasses;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class MySQLTests
    {
        const string SourceName = "MySQLSourceFromTool";

        [TestMethod]
        [TestCategory("Database Tools")]
        public void MySQLDatabaseTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.Exists, "My SQL Connector tool does not exist on design surface.");
            //Small View
            ToolsUIMap.MySqlDatabaseTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.SmallView.Exists, "My SQL Connector tool small view does not exist after collapsing large view with a double click.");
            //Large View
            ToolsUIMap.MySqlDatabaseTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.SourcesComboBox.Exists, "Sources combobox does not exist on My SQL database connector tool large view.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.EditSourceButton.Exists, "Edit Source button does not exist on My SQL database connector tool large view.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.NewSourceButton.Exists, "New Source button does not exist on My SQL database connector tool large view.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.ActionsComboBox.Exists, "Actions combobox does not exist on My SQL database connector tool large view.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.GenerateOutputsButton.Exists, "Generate Outputs button does not exist on My SQL database connector tool large view.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.OutputsMappingDataGrTable.Exists, "Outputs Mapping table does not exist on My SQL database connector tool large view.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.RecordSetTextBoxEdit.Exists, "Recordset textbox does not exist on My SQL database connector tool large view.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.OnErrorCustom.Exists, "OnError panel does not exist on My SQL database connector tool large view.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.DoneButton.Exists, "Done button does not exist on My SQL database connector tool large view.");
            //New Source
            ToolsUIMap.Click_NewSourceButton_From_MySQLTool();
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.Exists, "MySQL Source Tab does not exist.");
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "MySQL Server Address combobox is disabled new MySQL Source wizard tab");
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsFalse(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is enabled.");
            DBSourceUIMap.Click_UserButton_On_DatabaseSource();
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Exists);
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Exists);
            DBSourceUIMap.Enter_Text_Into_DatabaseServer_Tab();
            DBSourceUIMap.IEnterRunAsUserRootOnDatabaseSource();
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is not enabled.");
            DBSourceUIMap.Click_DB_Source_Wizard_Test_Connection_Button();
            DBSourceUIMap.Select_mysql_From_DB_Source_Wizard_Database_Combobox();
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            DBSourceUIMap.Click_Close_DB_Source_Wizard_Tab_Button();
            //Edit Source
            ToolsUIMap.MySqlDatabaseTool_ChangeView_With_DoubleClick();
            ToolsUIMap.Select_Source_From_MySQLTool();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.EditSourceButton.Enabled, "Edit Source Button is not enabled after selecting source.");
            ToolsUIMap.Click_EditSourceButton_On_MySQLTool();
            Assert.IsTrue(DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.Exists, "MySql Source Tab does not exist");
            DBSourceUIMap.Select_test_From_DB_Source_Wizard_Database_Combobox();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            DBSourceUIMap.Click_Close_DB_Source_Wizard_Tab_Button();
            ToolsUIMap.MySqlDatabaseTool_ChangeView_With_DoubleClick();
            ToolsUIMap.Click_EditSourceButton_On_MySQLTool();
            Assert.AreEqual("test", DBSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.testText.DisplayText);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            ToolsUIMap.Drag_Toolbox_MySql_Database_Onto_DesignSurface();
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

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

        #endregion
    }
}
