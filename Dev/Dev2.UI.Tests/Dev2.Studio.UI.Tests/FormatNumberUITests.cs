using Dev2.CodedUI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class FormatNumberUITests : UIMapBase
    {
        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        #endregion

        #region Context Init

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;
        
        #endregion

        #region Format Number Inputs Tests

        // BUG 8876 : This test ensure that the input box is enabled when selecting any rounding
        //           type (except None) in the Format Tool
        [TestMethod][Ignore]//Ashleys weekend \:D/
        public void SetRoundingType_Normal_ExpectedRoundingInputIsEnabled()
        {
            RibbonUIMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("NumberFormat");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            UITestControl ctrl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "NumberFormat");
            FormatNumberUIMap.InputAllFormatNumberValues(ctrl, "1234.56", "Normal", "1", "3", "[[Result]]");
            Assert.IsTrue(FormatNumberUIMap.IsRoundingInputEnabled());

            TabManagerUIMap.CloseAllTabs();
        }

        // BUG 8876 : This test ensure that the input box is empty when selecting none in the Format Tool
        [TestMethod][Ignore]//Ashleys weekend \:D/
        public void SetRoundingType_None_ExpectedRoundingInputIsDisabled()
        {

            RibbonUIMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("NumberFormat");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            UITestControl ctrl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "NumberFormat");
            FormatNumberUIMap.InputAllFormatNumberValues(ctrl, "1234.56", "None", "1", "3", "[[Result]]");
            Assert.IsFalse(FormatNumberUIMap.IsRoundingInputEnabled());

            TabManagerUIMap.CloseAllTabs();
        }

        // BUG 8876 : This test ensure that the input box is disabled and cleared when changing the rounding
        //            type to none.
        [TestMethod][Ignore]//Ashleys weekend \:D/
        public void ChangeRoundingType_None_Expected_RoundingInputBecomesDisabledAndEmpty()
        {
            RibbonUIMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("NumberFormat");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            UITestControl ctrl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "NumberFormat");
            FormatNumberUIMap.InputAllFormatNumberValues(ctrl, "1234.56", "Normal", "1", "3", "[[Result]]");
            FormatNumberUIMap.SelectRoundingType("None");
            WpfEdit inputControl = FormatNumberUIMap.GetRoudingInputBoxControl();
            //Assert.IsFalse(ctrl.Enabled);
            Assert.IsFalse(inputControl.Enabled);
            Assert.AreEqual(inputControl.GetProperty("Text").ToString(), string.Empty);

        }

        #endregion Format Number Inputs Tests

        #region Private Test Methods

        //private void CreateWorkflow(string workflowName)
        //{
        //    RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
        //    while (!WorkflowWizardUIMap.IsWindowOpen())
        //        Thread.Sleep(500);
        //    Thread.Sleep(1000);
        //    WorkflowWizardUIMap.EnterWorkflowName(workflowName);
        //    WorkflowWizardUIMap.EnterWorkflowCategory("CodedUITestCategory");
        //    WorkflowWizardUIMap.DoneButtonClick();
        //}

        #endregion Private Test Methods
    }
}
