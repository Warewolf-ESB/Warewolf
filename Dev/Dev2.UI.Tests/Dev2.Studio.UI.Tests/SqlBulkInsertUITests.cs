using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
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
        const string TestingDB = "GetCities";
        const int TableIndex = 1;
        #endregion

        #region Cleanup

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion


        #region Helpers

        UITestControl GetControlById(string autoId, UITestControl theTab)
        {
            var sqlBulkInsert = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "SqlBulkInsertDesigner");
            var uiTestControls = WorkflowDesignerUIMap.GetSqlBulkInsertChildren(sqlBulkInsert);
            var result = uiTestControls.FirstOrDefault(c => c.AutomationId.Equals(autoId));
            if(result != null)
            {
                result.WaitForControlReady();
            }

            return result;
        }

        UITestControl GetControlByFriendlyName(string name)
        {
            var sqlBulkInsert = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "SqlBulkInsertDesigner");
            var uiTestControls = WorkflowDesignerUIMap.GetSqlBulkInsertChildren(sqlBulkInsert);
            return uiTestControls.FirstOrDefault(c => c.FriendlyName.Contains(name));
        }

        private void WaitForControlLoad(int waitAmt = 2000)
        {
            Playback.Wait(waitAmt);
        }

        #endregion

        #region Test Methods


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_NoDatabaseIsSelected_GridHasNothing()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();

            // Get some variables
            var startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(theTab);
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.SqlBulkInsert, point, "Sql Bulk");

            var smallDataGrid = GetControlById("SmallDataGrid", theTab);
            Assert.IsTrue(smallDataGrid.GetChildren().Count == 0);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        // 05/11 - Failure is Intermittent - Problems finding LargeView button ;)
        public void SqlBulkInsertTest_OpenLargeViewAndEnterAnInvalidBatchAndTimeoutSizeAndClickDone_CorrectingErrorsAndClickDoneWillReturnToSmallView()
        {
            //// Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();

            // Get some variables
            var startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(theTab);
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.SqlBulkInsert, point, "Sql Bulk");

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID", theTab) as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            WaitForControlLoad();
            if(dbDropDown != null)
            {
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            var databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains(TestingDB));
            Mouse.Click(databaseName, new Point(5, 5));
            }
            WaitForControlLoad();

            //Select a table
            var tableDropDown = GetControlById("UI__TableName_AutoID", theTab) as WpfComboBox;
            Mouse.Click(tableDropDown, new Point(10, 10));
            WaitForControlLoad();
            if(tableDropDown != null)
            {
            var listOfTableNames = tableDropDown.Items.Select(i => i as WpfListItem).ToList();
                WaitForControlLoad();
            Mouse.Click(listOfTableNames[TableIndex], new Point(5, 5));
            }
            WaitForControlLoad();

            //Open the large view
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "DsfSqlBulkInsertActivity");
            Mouse.Move(controlOnWorkflow, new Point(5, 5));
            var toggleButton = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "DsfSqlBulkInsertActivity", "Open Large View") as WpfToggleButton;

            if(toggleButton == null)
            {
                Assert.Fail("Could not find mapping button");
            }

            Mouse.Click(toggleButton);
            WaitForControlLoad();

            //Enter a few mappings

            // THIS IS FAULTY LOGIC!!!!
            var getFirstTextbox = WorkflowDesignerUIMap.GetSqlBulkInsertLargeViewFirstInputTextbox(controlOnWorkflow);
            Mouse.Click(getFirstTextbox);

            KeyboardCommands.SendKey("^a^xrecord().id");
            KeyboardCommands.SendTab();
            KeyboardCommands.SendKey("^a^xrecord().name");
            KeyboardCommands.SendTab();
            KeyboardCommands.SendKey("^a^xrecord().mail");
            KeyboardCommands.SendTab();
            WaitForControlLoad();

            var batchSize = GetControlById("UI__BatchSize_AutoID", theTab);

            MouseCommands.ClickControlAtPoint(batchSize, new Point(5, 5));
            KeyboardCommands.SendKey("^a^x-2");

            var timeout = GetControlById("UI__Timeout_AutoID", theTab);
            MouseCommands.ClickControlAtPoint(timeout, new Point(5, 5));
            KeyboardCommands.SendKey("^a^x-2");

            var result = GetControlById("UI__Result_AutoID", theTab);
            MouseCommands.ClickControlAtPoint(result, new Point(5, 5));
            KeyboardCommands.SendKey("^a^x-2");

            var done = GetControlById("DoneButton", theTab);
            MouseCommands.ClickControlAtPoint(done, new Point(5, 5));

            WaitForControlLoad();

            var batchErrorMessage = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Batch size must be a number");
            MouseCommands.MoveAndClick(new Point(batchErrorMessage.GetChildren()[0].BoundingRectangle.X + 5, batchErrorMessage.GetChildren()[0].BoundingRectangle.Y + 5));
            KeyboardCommands.SendKey("^a^x200");

            var timeoutErrorMessage = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Timeout must be a number");
            MouseCommands.MoveAndClick(new Point(timeoutErrorMessage.GetChildren()[0].BoundingRectangle.X + 5, timeoutErrorMessage.GetChildren()[0].BoundingRectangle.Y + 5));
            KeyboardCommands.SendKey("^a^x200");

            MouseCommands.ClickControlAtPoint(done, new Point(5, 5));
            toggleButton = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "DsfSqlBulkInsertActivity", "Open Large View") as WpfToggleButton;

            Assert.IsNotNull(toggleButton);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        public void SqlBulkInsertTest_OpenQuickVariableInputAndCloseItImmediately_ReturnsToSmallView()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();

            // Get some variables
            var startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(theTab);
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.SqlBulkInsert, point, "Sql Bulk");

            //Open the quick variable input view
            var toggleButton = GetControlByFriendlyName("Open Quick Variable Input");
            MouseCommands.ClickControlAtPoint(toggleButton, new Point(5, 5));
            WaitForControlLoad();

            var quickVarInputContent = GetControlByFriendlyName("QuickVariableInputContent");
            Assert.IsNotNull(quickVarInputContent);

            //Close the quick variable input view
            toggleButton = GetControlByFriendlyName("Close Quick Variable Input");
            MouseCommands.ClickControlAtPoint(toggleButton, new Point(5, 5));
            WaitForControlLoad();

            var smallDataGrid = GetControlById("SmallDataGrid", theTab);
            Assert.IsNotNull(smallDataGrid);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SqlBulkInsertUITests")]
        // 05/11 - Failure is Intermittent - Problems finding LargeView button ;)
        public void SqlBulkInsertTest_SelectDatabaseAndTableName_GridHasColumnnames()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();

            var startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(theTab);
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.SqlBulkInsert, point, "Sql Bulk");

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID", theTab) as WpfComboBox;
            MouseCommands.ClickControlAtPoint(dbDropDown, new Point(10, 10));
            WaitForControlLoad(5000);
            if(dbDropDown != null)
            {
                var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
                var databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains(TestingDB));
                MouseCommands.ClickControlAtPoint(databaseName, new Point(5, 5));
            }

            //Select a table
            var tableDropDown = GetControlById("UI__TableName_AutoID", theTab) as WpfComboBox;
            MouseCommands.ClickControlAtPoint(tableDropDown, new Point(10, 10));
            WaitForControlLoad(2500);
            if(tableDropDown != null)
            {
                var listOfTableNames = tableDropDown.Items.Select(i => i as WpfListItem).ToList();
                WaitForControlLoad(2500);
                MouseCommands.ClickControlAtPoint(listOfTableNames[TableIndex], new Point(5, 5));
            }

            WaitForControlLoad(3000);

            //Assert that grid is not empty
            var smallDataGrid = GetControlById("SmallDataGrid", theTab);
            Assert.IsTrue(smallDataGrid.GetChildren().Count > 0);
        }

        #endregion
    }
}