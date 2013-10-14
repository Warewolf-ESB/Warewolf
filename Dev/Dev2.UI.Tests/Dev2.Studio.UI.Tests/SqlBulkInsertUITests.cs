using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    ///     These are UI Tests for the SQL Bul insert tool
    /// </summary>
    [CodedUITest]
    public class SqlBulkInsertUiTests
    {
        #region Fields
        RibbonUIMap _ribbonUiMap;
        TabManagerUIMap _tabManagerDesignerUiMap;
        ToolboxUIMap _toolboxUiMap;
        WorkflowDesignerUIMap _workflowDesignerUiMap;
        #endregion

        #region Properties
        TabManagerUIMap TabManagerUiMap { get { return _tabManagerDesignerUiMap ?? (_tabManagerDesignerUiMap = new TabManagerUIMap()); } }
        WorkflowDesignerUIMap WorkflowDesignerUiMap { get { return _workflowDesignerUiMap ?? (_workflowDesignerUiMap = new WorkflowDesignerUIMap()); } }
        ToolboxUIMap ToolboxUiMap { get { return _toolboxUiMap ?? (_toolboxUiMap = new ToolboxUIMap()); } }
        RibbonUIMap RibbonUiMap { get { return _ribbonUiMap ?? (_ribbonUiMap = new RibbonUIMap()); } }
        #endregion

        #region Test Methods
        [TestCleanup]
        public void TestCleanup()
        {
            TabManagerUiMap.CloseAllTabs();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_NoDatabaseIsSelected_GridHasNothing()
        {
            // Create the workflow
            RibbonUiMap.CreateNewWorkflow();

            // Get some variables
            var startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            var smallDataGrid = GetControlById("SmallDataGrid");
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
            var startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID") as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            var databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains("DBSource"));
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
            Keyboard.SendKeys("^a^xrecord().id{TAB}");
            Keyboard.SendKeys("^a^xrecord().name{TAB}");
            Keyboard.SendKeys("^a^xrecord().mail{TAB}");

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

            var activeTab = TabManagerUiMap.GetActiveTab();
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
        public void SqlBulkInsertTest_OpenQuickVariableInputAndCloseItImmediately_ReturnsToSmallView()
        {
            // Create the workflow
            RibbonUiMap.CreateNewWorkflow();

            // Get some variables
            var startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            //Open the quick variable input view
            var toggleButton = GetControlByFriendlyName("Open Quick Variable Input");
            Mouse.Click(toggleButton, new Point(5, 5));
            Playback.Wait(5000);

            var quickVarInputContent = GetControlByFriendlyName("QuickVariableInputContent");
            Assert.IsNotNull(quickVarInputContent);

            //Close the quick variable input view
            toggleButton = GetControlByFriendlyName("Close Quick Variable Input");
            Mouse.Click(toggleButton, new Point(5, 5));
            Playback.Wait(5000);

            var smallDataGrid = GetControlById("SmallDataGrid");
            Assert.IsNotNull(smallDataGrid);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_SelectDatabaseAndTableName_GridHasColumnnames()
        {
            // Create the workflow
            RibbonUiMap.CreateNewWorkflow();

            var startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID") as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            var databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains("DBSource"));
            Mouse.Click(databaseName, new Point(5, 5));

            //Select a table
            var tableDropDown = GetControlById("UI__TableName_AutoID") as WpfComboBox;
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

        UITestControl GetControlById(string autoId)
        {
            var uiTestControls = GetChildrenUnderControl();
            return uiTestControls.FirstOrDefault(c => c.AutomationId.Equals(autoId));
        }

        UITestControl GetControlByFriendlyName(string name)
        {
            var uiTestControls = GetChildrenUnderControl();
            return uiTestControls.FirstOrDefault(c => c.FriendlyName.Contains(name));
        }

        IEnumerable<WpfControl> GetChildrenUnderControl()
        {
            var sqlBulkInsert = WorkflowDesignerUiMap.FindControlByAutomationId(TabManagerUiMap.GetActiveTab(), "SqlBulkInsertDesigner");

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