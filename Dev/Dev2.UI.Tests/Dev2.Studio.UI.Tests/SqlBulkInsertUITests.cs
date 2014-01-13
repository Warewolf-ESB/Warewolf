using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
            var startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("DsfSqlBulkInsertActivity", point, "Sql Bulk");

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
            var startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("DsfSqlBulkInsertActivity", point, "Sql Bulk");

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID", theTab) as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            Playback.Wait(2000);
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            var databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains(TestingDB));
            Mouse.Click(databaseName, new Point(5, 5));
            Playback.Wait(2000);

            //Select a table
            var tableDropDown = GetControlById("UI__TableName_AutoID", theTab) as WpfComboBox;
            Mouse.Click(tableDropDown, new Point(10, 10));
            Playback.Wait(2000);
            var listOfTableNames = tableDropDown.Items.Select(i => i as WpfListItem).ToList();
            Playback.Wait(2000);
            Mouse.Click(listOfTableNames[TableIndex], new Point(5, 5));
            Playback.Wait(2000);

            //Open the large view
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "DsfSqlBulkInsertActivity");
            Mouse.Move(controlOnWorkflow, new Point(5, 5));
            var toggleButton = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "DsfSqlBulkInsertActivity", "Open Large View") as WpfToggleButton;

            if(toggleButton == null)
            {
                Assert.Fail("Could not find mapping button");
            }

            Mouse.Click(toggleButton);
            Playback.Wait(2000);

            //Enter a few mappings

            // THIS IS FAULTY LOGIC!!!!
            var getFirstTextbox = WorkflowDesignerUIMap.GetSqlBulkInsertLargeViewFirstInputTextbox(controlOnWorkflow);
            Mouse.Click(getFirstTextbox);
            SendKeys.SendWait("^a^xrecord().id{TAB}");
            SendKeys.SendWait("^a^xrecord().name{TAB}");
            SendKeys.SendWait("^a^xrecord().mail{TAB}");
            Playback.Wait(2000);

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

            Playback.Wait(1000);

            var batchErrorMessage = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Batch size must be a number");
            Mouse.Move(new Point(batchErrorMessage.GetChildren()[0].BoundingRectangle.X + 5, batchErrorMessage.GetChildren()[0].BoundingRectangle.Y + 5));
            Mouse.Click();
            SendKeys.SendWait("^a^x200");

            var timeoutErrorMessage = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Timeout must be a number");
            Mouse.Move(new Point(timeoutErrorMessage.GetChildren()[0].BoundingRectangle.X + 5, timeoutErrorMessage.GetChildren()[0].BoundingRectangle.Y + 5));
            Mouse.Click();
            SendKeys.SendWait("^a^x200");

            Mouse.Click(done, new Point(5, 5));
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
            var startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("DsfSqlBulkInsertActivity", point, "Sql Bulk");

            //Open the quick variable input view
            var toggleButton = GetControlByFriendlyName("Open Quick Variable Input");
            Mouse.Click(toggleButton, new Point(5, 5));
            Playback.Wait(2000);

            var quickVarInputContent = GetControlByFriendlyName("QuickVariableInputContent");
            Assert.IsNotNull(quickVarInputContent);

            //Close the quick variable input view
            toggleButton = GetControlByFriendlyName("Close Quick Variable Input");
            Mouse.Click(toggleButton, new Point(5, 5));
            Playback.Wait(2000);

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

            var startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            var point = new Point(startPoint.X, startPoint.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("DsfSqlBulkInsertActivity", point, "Sql Bulk");

            //Select a database
            var dbDropDown = GetControlById("UI__Database_AutoID", theTab) as WpfComboBox;
            Mouse.Click(dbDropDown, new Point(10, 10));
            Playback.Wait(1000);
            var listOfDbNames = dbDropDown.Items.Select(i => i as WpfListItem).ToList();
            var databaseName = listOfDbNames.SingleOrDefault(i => i.DisplayText.Contains(TestingDB));
            Mouse.Click(databaseName, new Point(5, 5));

            //Select a table
            var tableDropDown = GetControlById("UI__TableName_AutoID", theTab) as WpfComboBox;
            Mouse.Click(tableDropDown, new Point(10, 10));
            Playback.Wait(1000);
            var listOfTableNames = tableDropDown.Items.Select(i => i as WpfListItem).ToList();
            Playback.Wait(1000);
            Mouse.Click(listOfTableNames[TableIndex], new Point(5, 5));
            Playback.Wait(2000);

            //Assert that grid is not empty
            var smallDataGrid = GetControlById("SmallDataGrid", theTab);
            Assert.IsTrue(smallDataGrid.GetChildren().Count > 0);
        }

        #endregion

        #region Helpers

        UITestControl GetControlById(string autoId, UITestControl theTab)
        {
            var sqlBulkInsert = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "SqlBulkInsertDesigner");
            var uiTestControls = WorkflowDesignerUIMap.GetSqlBulkInsertChildren(sqlBulkInsert);
            return uiTestControls.FirstOrDefault(c => c.AutomationId.Equals(autoId));
        }

        UITestControl GetControlByFriendlyName(string name)
        {
            var sqlBulkInsert = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "SqlBulkInsertDesigner");
            var uiTestControls = WorkflowDesignerUIMap.GetSqlBulkInsertChildren(sqlBulkInsert);
            return uiTestControls.FirstOrDefault(c => c.FriendlyName.Contains(name));
        }

        #endregion
    }
}