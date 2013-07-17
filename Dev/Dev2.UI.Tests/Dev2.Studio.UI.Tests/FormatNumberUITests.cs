using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Threading;


namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for FormatNumberUITests
    /// </summary>
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

        public FormatNumberUITests()
        {
        }

        #region Format Number Inputs Tests

        // BUG 8876 : This test ensure that the input box is enabled when selecting any rounding
        //           type (except None) in the Format Tool
        [TestMethod]
        public void SetRoundingType_Normal_ExpectedRoundingInputIsEnabled()
        {

            new TestBase().CreateWorkflow();

            UITestControl theTab = TabManagerUIMap.FindTabByName("Unsaved 1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            new DocManagerUIMap().ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("NumberFormat");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            UITestControl ctrl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "NumberFormat");
            FormatNumberUIMap.InputAllFormatNumberValues(ctrl, "1234.56", "Normal", "1", "3", "[[Result]]");
            Assert.IsTrue(FormatNumberUIMap.IsRoundingInputEnabled());

            new TestBase().DoCleanup("Unsaved 1", true);

            //string workflowName = "NumberFormatRoundingNormalWorkflowTest";
            //CreateWorkflow(workflowName);
            //DockManagerUIMap.ClickOpenTabPage("Toolbox");
            //UITestControl workflowTab = TabManagerUIMap.FindTabByName("NumberFormatRoundingNormalWorkflowTest");
            //Point pointUnderStartPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(workflowTab);
            //DockManagerUIMap.ClickOpenTabPage("Toolbox");
            //ToolboxUIMap.DragControlToWorkflowDesigner("NumberFormat", pointUnderStartPoint);
            //UITestControl ctrl = WorkflowDesignerUIMap.FindControlByAutomationId(workflowTab, "NumberFormat");

            //FormatNumberUIMap.InputAllFormatNumberValues(ctrl, "1234.56", "Normal", "1", "3", "[[Result]]");
            //Assert.IsTrue(FormatNumberUIMap.IsRoundingInputEnabled());

            //Assert.Inconclusive("Workflow not in repo!!!");
        }


        // BUG 8876 : This test ensure that the input box is empty when selecting none in the Format Tool
        [TestMethod]
        public void SetRoundingType_None_ExpectedRoundingInputIsDisabled()
        {

            new TestBase().CreateWorkflow();

            UITestControl theTab = TabManagerUIMap.FindTabByName("Unsaved 1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            new DocManagerUIMap().ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("NumberFormat");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            UITestControl ctrl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "NumberFormat");
            FormatNumberUIMap.InputAllFormatNumberValues(ctrl, "1234.56", "None", "1", "3", "[[Result]]");
            Assert.IsFalse(FormatNumberUIMap.IsRoundingInputEnabled());

            new TestBase().DoCleanup("Unsaved 1", true);
        }

        // BUG 8876 : This test ensure that the input box is disabled and cleared when changing the rounding
        //            type to none.
        [TestMethod]
        public void ChangeRoundingType_None_Expected_RoundingInputBecomesDisabledAndEmpty()
        {

            new TestBase().CreateWorkflow();

            UITestControl theTab = TabManagerUIMap.FindTabByName("Unsaved 1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            new DocManagerUIMap().ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("NumberFormat");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            UITestControl ctrl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "NumberFormat");
            FormatNumberUIMap.InputAllFormatNumberValues(ctrl, "1234.56", "Normal", "1", "3", "[[Result]]");
            FormatNumberUIMap.SelectRoundingType("None");
            WpfEdit inputControl = FormatNumberUIMap.GetRoudingInputBoxControl();
            //Assert.IsFalse(ctrl.Enabled);
            Assert.IsFalse(inputControl.Enabled);
            Assert.AreEqual(inputControl.GetProperty("Text").ToString(), string.Empty);

            new TestBase().DoCleanup("Unsaved 1", true);

        }

        #region Deprecated Test

        // BUG 8876 : This test ensure that the debug output window does not display any rounding information
        //            when no rounding is selected.
        [TestMethod]
        [Ignore] // Covered in the Tool Testing Workflow ;)
        public void ChangeRoundingType_None_And_Execute_Expected_DebugOutputContainsNorReferenceToPreviousValue()
        {

            new TestBase().CreateWorkflow();

            UITestControl theTab = TabManagerUIMap.FindTabByName("Unsaved 1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            new DocManagerUIMap().ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("NumberFormat");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            UITestControl ctrl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "NumberFormat");
            FormatNumberUIMap.InputAllFormatNumberValues(ctrl, "1234.56", "Normal", "10", "3", "[[Result]]");
            FormatNumberUIMap.SelectRoundingType("None");

            WpfEdit inputControl = FormatNumberUIMap.GetRoudingInputBoxControl();
            WorkflowDesignerUIMap.SetStartNode(theTab, "NumberFormat");

            DockManagerUIMap.ClickOpenTabPage("Variables");
            //Massimo.Guerrera - 6/3/2013 - Removed because variables are now auto added to the list.
            //VariablesUIMap.UpdateDataList();

            RibbonUIMap.ClickRibbonMenuItem("Home", "Debug");
            DebugUIMap.ExecuteDebug();
            DockManagerUIMap.ClickOpenTabPage("Output");
            UITestControlCollection outputWindow = OutputUIMap.GetOutputWindow();
            UITestControlCollection formatNumberStep = OutputUIMap.GetInputDetailsDetails(outputWindow[1]);
            WpfText decimalPlaces = new WpfText();
            for (int i = 0; i <= formatNumberStep.Count; i++)
            {
                if (formatNumberStep[i].Name.Equals("Rounding Decimal Places"))
                {
                    decimalPlaces = (WpfText)formatNumberStep[i + 1];
                    break;
                }
            }

            Assert.IsFalse(decimalPlaces.DisplayText.Contains("10"));

            new TestBase().DoCleanup("Unsaved 1", true);

        }

        #endregion

        #endregion Format Number Inputs Tests

        #region Private Test Methods

        private void CreateWorkflow(string workflowName)
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
            while (!WorkflowWizardUIMap.IsWindowOpen())
                Thread.Sleep(500);
            Thread.Sleep(1000);
            WorkflowWizardUIMap.EnterWorkflowName(workflowName);
            WorkflowWizardUIMap.EnterWorkflowCategory("CodedUITestCategory");
            WorkflowWizardUIMap.DoneButtonClick();
        }

        #endregion Private Test Methods

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
