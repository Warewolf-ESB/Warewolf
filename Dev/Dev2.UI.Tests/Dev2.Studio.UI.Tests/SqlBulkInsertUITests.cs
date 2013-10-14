using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseSourceUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.EmailSourceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.PluginSourceMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.SaveDialogUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Point = System.Drawing.Point;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    ///     These are UI Tests for the SQL Bul insert tool
    /// </summary>
    [CodedUITest]
    public class SqlBulkInsertUITests
    {
        #region Fields

        const string RemoteServerName = "RemoteConnection";
        const string LocalHostServerName = "localhost";
        const string ExplorerTab = "Explorer";
        DatabaseServiceWizardUIMap _databaseServiceWizardUiMap;
        DatabaseSourceUIMap _databaseSourceUiMap;
        DocManagerUIMap _docManagerMap;
        EmailSourceWizardUIMap _emailSourceWizardUiMap;
        ExplorerUIMap _explorerUiMap;
        PluginServiceWizardUIMap _pluginServiceWizardUiMap;
        PluginSourceMap _pluginSourceMap;
        RibbonUIMap _ribbonUiMap;
        SaveDialogUIMap _saveDialogUiMap;
        TabManagerUIMap _tabManagerDesignerUiMap;
        ToolboxUIMap _toolboxUiMap;
        WorkflowDesignerUIMap _workflowDesignerUiMap;
        private ExternalUIMap _externalUiMap;

        #endregion

        #region Properties

        ExplorerUIMap ExplorerUiMap { get { return _explorerUiMap ?? (_explorerUiMap = new ExplorerUIMap()); } }
        TabManagerUIMap TabManagerUiMap { get { return _tabManagerDesignerUiMap ?? (_tabManagerDesignerUiMap = new TabManagerUIMap()); } }
        DocManagerUIMap DocManagerUiMap { get { return _docManagerMap ?? (_docManagerMap = new DocManagerUIMap()); } }
        WorkflowDesignerUIMap WorkflowDesignerUiMap { get { return _workflowDesignerUiMap ?? (_workflowDesignerUiMap = new WorkflowDesignerUIMap()); } }
        DatabaseSourceUIMap DatabaseSourceUiMap { get { return _databaseSourceUiMap ?? (_databaseSourceUiMap = new DatabaseSourceUIMap()); } }
        SaveDialogUIMap SaveDialogUiMap { get { return _saveDialogUiMap ?? (_saveDialogUiMap = new SaveDialogUIMap()); } }
        EmailSourceWizardUIMap EmailSourceWizardUiMap { get { return _emailSourceWizardUiMap ?? (_emailSourceWizardUiMap = new EmailSourceWizardUIMap()); } }
        PluginSourceMap PluginSourceMap { get { return _pluginSourceMap ?? (_pluginSourceMap = new PluginSourceMap()); } }
        DatabaseServiceWizardUIMap DatabaseServiceWizardUiMap { get { return _databaseServiceWizardUiMap ?? (_databaseServiceWizardUiMap = new DatabaseServiceWizardUIMap()); } }
        PluginServiceWizardUIMap PluginServiceWizardUiMap { get { return _pluginServiceWizardUiMap ?? (_pluginServiceWizardUiMap = new PluginServiceWizardUIMap()); } }
        ToolboxUIMap ToolboxUiMap { get { return _toolboxUiMap ?? (_toolboxUiMap = new ToolboxUIMap()); } }
        RibbonUIMap RibbonUiMap { get { return _ribbonUiMap ?? (_ribbonUiMap = new RibbonUIMap()); } }
        ExternalUIMap ExternalWizardUiMap { get { return _externalUiMap ?? (_externalUiMap = new ExternalUIMap()); } }
        
        #endregion

        #region Test Methods

        [TestCleanup]
        public void TestCleanup()
        {
            TabManagerUiMap.CloseAllTabs();
            DocManagerUIMap.ClickOpenTabPage(ExplorerTab);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_NoDatabaseIsSelected_GridHasNothing()
        {
            // Create the workflow
            RibbonUiMap.CreateNewWorkflow();

            // Get some variables
            Point startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            var smallDataGrid = GetControlById("SmallDataGrid") as WpfComboBox;
            Assert.IsTrue(smallDataGrid.GetChildren().Count > 0);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_OpenLargeViewAndEnterAnInvalidBatchAndTimeoutSizeAndClickDone_CorrectingErrorsAndClickDoneWillReturnToSmallView()
        {
            // Create the workflow
            RibbonUiMap.CreateNewWorkflow();

            // Get some variables
            Point startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID") as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            WpfListItem databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains("DBSource"));
            Mouse.Click(databaseName, new Point(5, 5));
            
            //Select a table
            var tableDropDown = GetControlById("UI__TableName_AutoID") as WpfComboBox;
            Mouse.Click(tableDropDown, new Point(10, 10));
            var listOfTableNames = tableDropDown.Items.Select(i => i as WpfListItem).ToList();
            Mouse.Click(listOfTableNames[6], new Point(5, 5));
            Playback.Wait(5000);

            //Open the large view
            var toggleButton = GetControlByFriendlyName("Open Large View");
            Mouse.Click(toggleButton, new Point(5, 5));
            Playback.Wait(5000);
            
            //Enter a few mappings
            Keyboard.SendKeys("record().id{TAB}");
            Keyboard.SendKeys("record().columnOne{TAB}");

            var batchSize = GetControlById("UI__BatchSize_AutoID");
            Mouse.Click(batchSize, new Point(5, 5));
            Keyboard.SendKeys("^a^x-2");

            var timeout = GetControlById("UI__Timeout_AutoID");
            Mouse.Click(timeout, new Point(5, 5));
            Keyboard.SendKeys("^a^x-2");

            var result = GetControlById("UI__Result_AutoID");
            Mouse.Click(result, new Point(5, 5));
            Keyboard.SendKeys("res");
            
            var done = GetControlById("DoneButton");
            Mouse.Click(done, new Point(5, 5));

            toggleButton = GetControlByFriendlyName("Open Large View");
            Assert.IsNull(toggleButton);

            UITestControl activeTab = TabManagerUiMap.GetActiveTab();
            var batchErrorMessage = WorkflowDesignerUiMap.FindControlByAutomationId(activeTab, "Batch size must be a number");
            Mouse.Click(batchErrorMessage.GetChildren()[0]);
            Keyboard.SendKeys("^a^x200");

            var timeoutErrorMessage = WorkflowDesignerUiMap.FindControlByAutomationId(activeTab, "Timeout must be a number");
            Mouse.Click(timeoutErrorMessage.GetChildren()[0]);
            Keyboard.SendKeys("^a^x200");

            Mouse.Click(done, new Point(5, 5));
            toggleButton = GetControlByFriendlyName("Open Large View");
            Assert.IsNotNull(toggleButton);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_SelectDatabaseAndTableName_GridHasColumnnames()
        {
            // Create the workflow
            RibbonUiMap.CreateNewWorkflow();
           
            Point startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID")  as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            WpfListItem databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains("DBSource"));
            Mouse.Click(databaseName, new Point(5, 5));
            
            //Select a table
             var tableDropDown = GetControlById("UI__TableName_AutoID")  as WpfComboBox;
            Mouse.Click(tableDropDown, new Point(10, 10));
            var listOfTableNames = tableDropDown.Items.Select(i => i as WpfListItem).ToList();
            Mouse.Click(listOfTableNames[1], new Point(5, 5));
            Playback.Wait(5000);

            //Assert that grid is not empty
            var smallDataGrid = GetControlById("SmallDataGrid");
            Assert.IsTrue(smallDataGrid.GetChildren().Count > 0);
        }


        #endregion

        #region Helpers
        private UITestControl GetControlById(string autoId)
        {
            var uiTestControls = GetChildrenUnderControl();
            return uiTestControls.FirstOrDefault(c => c.AutomationId.Equals(autoId));
        }

        private UITestControl GetControlByFriendlyName(string name)
        {
            var uiTestControls = GetChildrenUnderControl();
            return uiTestControls.FirstOrDefault(c => c.FriendlyName.Contains(name));
        }

        IEnumerable<WpfControl> GetChildrenUnderControl()
        {
            UITestControl sqlBulkInsert = WorkflowDesignerUiMap.FindControlByAutomationId(TabManagerUiMap.GetActiveTab(), "SqlBulkInsertDesigner");

            var uiTestControls = sqlBulkInsert
                .GetChildren()
                .Select(i => i as WpfControl)
                .ToList();

            uiTestControls.AddRange(sqlBulkInsert
                .GetChildren()
                .SelectMany(c => c.GetChildren())
                .Select(i => i as WpfControl)
                .ToList());

            return uiTestControls;
        }

        #endregion
    }
}