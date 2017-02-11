using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Database
{
    [CodedUITest]
    public class ODBCTests
    {
        const string SourceName = "ODBCSourceFromTool";

        [TestMethod]
        [TestCategory("Database Tools")]
        public void ODBCDatabaseTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.Exists, "ODBC database tool does not exist after dragging in from the toolbox");
            // Small View
            UIMap.ODBCDatabaseTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.SmallView.Exists, "ODBC database connector tool small view does not exist after collapsing large view with a double click.");
            // Large View
            UIMap.ODBCDatabaseTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.SourcesComboBox.Exists, "Sources combobox does not exist on ODBC database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.EdistSourceButton.Exists, "Edit Source button does not exist on ODBC database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.NewSourceButton.Exists, "New Source button does not exist on ODBC database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.ActionCommandComboBox.Exists, "Action Command combobox does not exist on ODBC database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.GenerateOutputsButton.Exists, "Generate Outputs button does not exist on ODBC database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.OutputsMappingDataGrTable.Exists, "Outputs Mapping table does not exist on ODBC database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.RecordSetTextBoxEdit.Exists, "Recordset textbox does not exist on ODBC database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.OnErrorCustom.Exists, "OnError panel does not exist on ODBC database connector tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.DoneButton.Exists, "Done button does not exist on ODBC database connector tool large view.");
            // New Source
            UIMap.Click_NewSourceButton_From_ODBCTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.Exists, "ODBC Source Tab does not exist");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "ODBC server combobox is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection Button is not enabled.");
            UIMap.Click_DB_Source_Wizard_Test_Connection_Button();
            UIMap.Select_ExcelFiles_From_DB_Source_Wizard_Database_Combobox();
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            UIMap.Click_Close_DB_Source_Wizard_Tab_Button();
            //Edit Source
            UIMap.ODBCDatabaseTool_ChangeView_With_DoubleClick();
            UIMap.Select_Source_From_ODBCTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.EdistSourceButton.Enabled, "Edit Source Button is not enabled after selecting source.");
            UIMap.Click_EditSourceButton_On_ODBCTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.Exists, "ODBC Source Tab does not exist");
            UIMap.Select_MSAccess_From_DB_Source_Wizard_Database_Combobox();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_Close_DB_Source_Wizard_Tab_Button();
            UIMap.ODBCDatabaseTool_ChangeView_With_DoubleClick();
            UIMap.Click_EditSourceButton_On_ODBCTool();
            Assert.AreEqual("MS Access Database", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.MSAccessDatabaseText.DisplayText);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            UIMap.Drag_Toolbox_ODBC_Dtatbase_Onto_DesignSurface();
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
