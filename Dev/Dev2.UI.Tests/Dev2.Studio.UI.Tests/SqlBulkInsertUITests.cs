using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
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
    public class SqlBulkInsertUiTests : UIMapBase
    {
        #region Fields
        RibbonUIMap _ribbonUiMap;
        TabManagerUIMap _tabManagerDesignerUiMap;
        ToolboxUIMap _toolboxUiMap;
        WorkflowDesignerUIMap _workflowDesignerUiMap;
        const string TestingDB = "GetCities";
        const int TableIndex = 1;
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
            var theTab = TabManagerUiMap.GetActiveTab();

            // Get some variables
            var startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            var theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            var smallDataGrid = GetControlById("SmallDataGrid", theTab);
            Assert.IsTrue(smallDataGrid.GetChildren().Count == 0);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_OpenLargeViewAndEnterAnInvalidBatchAndTimeoutSizeAndClickDone_CorrectingErrorsAndClickDoneWillReturnToSmallView()
        {
            // Create the workflow
            RibbonUiMap.CreateNewWorkflow();
            var theTab = TabManagerUiMap.GetActiveTab();

            // Get some variables
            var startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            var theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID", theTab) as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            var databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains(TestingDB));
            Mouse.Click(databaseName, new Point(5, 5));
            Playback.Wait(1000);

            //Select a table
            var tableDropDown = GetControlById("UI__TableName_AutoID", theTab) as WpfComboBox;
            Mouse.Click(tableDropDown, new Point(10, 10));
            var listOfTableNames = tableDropDown.Items.Select(i => i as WpfListItem).ToList();
            Mouse.Click(listOfTableNames[TableIndex], new Point(5, 5));
            Playback.Wait(5000);

            //Open the large view
            var toggleButton = GetControlByFriendlyName("Open Large View");
            Mouse.Click(toggleButton, new Point(5, 5));
            Playback.Wait(2000);

            //Enter a few mappings
            SendKeys.SendWait("^a^xrecord().id{TAB}");
            SendKeys.SendWait("^a^xrecord().name{TAB}");
            SendKeys.SendWait("^a^xrecord().mail{TAB}");

            var batchSize = GetControlById("UI__BatchSize_AutoID", theTab);
            Mouse.Click(batchSize, new Point(5, 5));
            SendKeys.SendWait("^a^x-2");

            var timeout = GetControlById("UI__Timeout_AutoID", theTab);
            Mouse.Click(timeout, new Point(5, 5));
            SendKeys.SendWait("^a^x-2");

            var result = GetControlById("UI__Result_AutoID", theTab);
            Mouse.Click(result, new Point(5, 5));
            SendKeys.SendWait("res");

            var done = GetControlById("DoneButton", theTab);
            Mouse.Click(done, new Point(5, 5));

            toggleButton = GetControlByFriendlyName("Open Large View");
            Assert.IsNull(toggleButton);

            var batchErrorMessage = WorkflowDesignerUiMap.FindControlByAutomationId(theTab, "Batch size must be a number");
            Mouse.Move(new Point(batchErrorMessage.GetChildren()[0].BoundingRectangle.X + 5, batchErrorMessage.GetChildren()[0].BoundingRectangle.Y + 5));
            Mouse.Click();
            SendKeys.SendWait("^a^x200");

            var timeoutErrorMessage = WorkflowDesignerUiMap.FindControlByAutomationId(theTab, "Timeout must be a number");
            Mouse.Move(new Point(timeoutErrorMessage.GetChildren()[0].BoundingRectangle.X + 5, timeoutErrorMessage.GetChildren()[0].BoundingRectangle.Y + 5));
            Mouse.Click();
            SendKeys.SendWait("^a^x200");

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
            var theTab = TabManagerUiMap.GetActiveTab();

            // Get some variables
            var startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
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

            var smallDataGrid = GetControlById("SmallDataGrid", theTab);
            Assert.IsNotNull(smallDataGrid);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_SelectDatabaseAndTableName_GridHasColumnnames()
        {
            // Create the workflow
            RibbonUiMap.CreateNewWorkflow();
            var theTab = TabManagerUiMap.GetActiveTab();

            var startPoint = WorkflowDesignerUiMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            var theControl = ToolboxUiMap.FindControl("DsfSqlBulkInsertActivity");
            ToolboxUiMap.DragControlToWorkflowDesigner(theControl, point);

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID", theTab) as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            var databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains(TestingDB));
            Mouse.Click(databaseName, new Point(5, 5));

            //Select a table
            var tableDropDown = GetControlById("UI__TableName_AutoID", theTab) as WpfComboBox;
            Mouse.Click(tableDropDown, new Point(10, 10));
            var listOfTableNames = tableDropDown.Items.Select(i => i as WpfListItem).ToList();
            Mouse.Click(listOfTableNames[TableIndex], new Point(5, 5));
            Playback.Wait(5000);

            //Assert that grid is not empty
            var smallDataGrid = GetControlById("SmallDataGrid", theTab);
            Assert.IsTrue(smallDataGrid.GetChildren().Count > 0);
        }

        #endregion

        #region Helpers

        UITestControl GetControlById(string autoId, UITestControl theTab)
        {
            var sqlBulkInsert = WorkflowDesignerUiMap.FindControlByAutomationId(theTab, "SqlBulkInsertDesigner");
            var uiTestControls = WorkflowDesignerUiMap.GetSqlBulkInsertChildren(sqlBulkInsert);
            return uiTestControls.FirstOrDefault(c => c.AutomationId.Equals(autoId));
        }

        UITestControl GetControlByFriendlyName(string name)
        {
            var sqlBulkInsert = WorkflowDesignerUiMap.FindControlByAutomationId(TabManagerUiMap.GetActiveTab(), "SqlBulkInsertDesigner");
            var uiTestControls = WorkflowDesignerUiMap.GetSqlBulkInsertChildren(sqlBulkInsert);
            return uiTestControls.FirstOrDefault(c => c.FriendlyName.Contains(name));
        }

        #endregion
    }
}