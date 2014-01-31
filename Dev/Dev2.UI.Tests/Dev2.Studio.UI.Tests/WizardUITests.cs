using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


namespace Dev2.Studio.UI.Tests.UIMaps
{
    [CodedUITest]
    public class WizardUITests : UIMapBase
    {
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

        #region Service Wizards

        [TestMethod]
        public void ClickNewPluginServiceExpectedPluginServiceOpens()
        {
            RibbonUIMap.ClickNewPlugin();
            UITestControl uiTestControl = PluginServiceWizardUIMap.GetWizardWindow();
            if(uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the new plugin service button does not create the new plugin service window");
            }

            uiTestControl.WaitForControlEnabled();
            WizardsUIMap.WaitForWizard();
            SendKeys.SendWait("{ESC}");
        }

        [TestMethod]
        [Ignore]
        public void WebServiceWizardCreateServiceAndSourceExpectedServiceCreated()
        {
            //Initialization
            var sourceNameId = Guid.NewGuid().ToString().Substring(0, 5);
            var sourceName = "codeduitest" + sourceNameId;

            var serviceNameId = Guid.NewGuid().ToString().Substring(0, 5);
            var serviceName = "codeduitest" + serviceNameId;

            WebServiceWizardUIMap.InitializeFullTestServiceAndSource(serviceName, sourceName);

            //Assert
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.EnterExplorerSearchText(serviceName);
            Playback.Wait(3500);
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SERVICES", "Unassigned", serviceName));

            ExplorerUIMap.EnterExplorerSearchText(sourceName);
            Playback.Wait(3500);
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", "Unassigned", sourceName));

        }

        //2013.03.14: Ashley Lewis - Bug 9217
        [TestMethod]
        [Ignore]
        public void DatabaseServiceWizardCreateNewServiceExpectedServiceCreated()
        {
            //Initialization
            var serverSourceCategoryName = Guid.NewGuid().ToString().Substring(0, 5);
            var serverSourceName = Guid.NewGuid().ToString().Substring(0, 5);
            var cat = "CODEDUITESTS" + serverSourceCategoryName.ToUpper();
            var name = "codeduitest" + serverSourceName;

            DatabaseServiceWizardUIMap.InitializeFullTestServiceAndSource(cat, name);

            //Assert
            ExplorerUIMap.EnterExplorerSearchText(name);

            var result = ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", cat, name);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// News the database service shortcut key expected new database service wizard opens.
        /// </summary>
        [TestMethod]
        public void NewDatabaseServiceShortcutKeyExpectedDatabaseServiceOpens()
        {
            StudioWindow.WaitForControlReady(1000);
            Keyboard.SendKeys(StudioWindow, "{CTRL}{SHIFT}D");
            if(!WizardsUIMap.TryWaitForWizard(10000))
            {
                Assert.Fail("New db service shortcut key doesnt work");
            }
            DatabaseServiceWizardUIMap.ClickCancel();
        }

        [TestMethod]
        public void NewPluginServiceShortcutKeyExpectedPluginServiceOpens()
        {
            StudioWindow.WaitForControlReady(1000);
            Keyboard.SendKeys(StudioWindow, "{CTRL}{SHIFT}P");
            WizardsUIMap.WaitForWizard();
            PluginServiceWizardUIMap.ClickCancel();
        }

        [TestMethod]
        public void NewWebServiceShortcutKeyExpectedWebServiceOpens()
        {
            StudioWindow.WaitForControlReady(1000);
            Keyboard.SendKeys(StudioWindow, "{CTRL}{SHIFT}W");
            if(!WizardsUIMap.TryWaitForWizard(10000))
            {
                Assert.Fail("New web service shortcut key doesnt work");
            }
            WebServiceWizardUIMap.Cancel();
        }

        #endregion

        #region Source Wizards

        //2013.06.22: Ashley Lewis for bug 9478
        [TestMethod]
        [Owner("Travis Frisinger")]
        [Ignore]
        public void EmailSourceWizardCreateNewSourceExpectedSourceCreated()
        {
            //Initialization
            var sourceName = Guid.NewGuid().ToString().Substring(0, 5);
            var name = "codeduitest" + sourceName;

            EmailSourceWizardUIMap.InitializeFullTestSource(name);

            //Assert
            ExplorerUIMap.EnterExplorerSearchText(name);

            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", "Unassigned", name));
        }

        #endregion

        #region Decision Wizard

        private readonly DecisionWizardUIMap _decisionWizardUiMap = new DecisionWizardUIMap();

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DecisionWizard_Save")]
        // 05/11 - Failure is Intermittent ;)
        public void DecisionWizard_Save_WhenMouseUsedToSelect2ndAnd3rdInputFields_FieldDataSavedCorrectly()
        {
            //------------Setup for test--------------------------
            RibbonUIMap.CreateNewWorkflow();

            var theTab = TabManagerUIMap.GetActiveTab();

            //------------Execute Test---------------------------
            VariablesUIMap.ClickScalarVariableName(0);
            SendKeys.SendWait("VariableName");

            var pt = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);

            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", pt);
            WizardsUIMap.WaitForWizard();
            _decisionWizardUiMap.SendTabs(4);
            Playback.Wait(500);
            _decisionWizardUiMap.SelectMenuItem(15); // select between ;)

            _decisionWizardUiMap.SendTabs(11);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(100, 150));

            _decisionWizardUiMap.SendTabs(2);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(400, 150));
            _decisionWizardUiMap.SendTabs(1);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(600, 150));

            _decisionWizardUiMap.SendTabs(6);
            SendKeys.SendWait("{ENTER}");

            //------------Assert Results-------------------------

            var expected = "If [[VariableName]] Is Between [[VariableName]] and [[VariableName]]";

            var getDecision = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "FlowDecisionDesigner");
            getDecision.WaitForControlEnabled();
            var getDecisionText = getDecision.GetChildren()[0] as WpfEdit;
            if(getDecisionText != null)
            {
                var displayValue = getDecisionText.Text;

                Assert.AreEqual(expected, displayValue,
                                "Decision intellisense doesnt work when using the mouse to select intellisense results");
            }
            else
            {
                Assert.Fail("Null decision");
            }
        }

        //Bug 9339 + Bug 9378
        [TestMethod]
        public void SaveDecisionWithBlankFieldsExpectedDecisionSaved()
        {
            //Initialize
            RibbonUIMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            VariablesUIMap.ClickScalarVariableName(0);
            SendKeys.SendWait("VariableName");

            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));

            //Save the decision with blank fields
            WizardsUIMap.WaitForWizard();
            DecisionWizardUIMap.ClickDone();

            //Assert can save blank decision
            var decision = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "FlowDecisionDesigner");
            Point point;
            Assert.IsTrue(decision.TryGetClickablePoint(out point));
            Assert.IsNotNull(point);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("Decision_Intellisense")]
        public void Decision_Intellisense_KeyboardSelect_DecisionTitleUpdatesCorrectly()
        {
            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            VariablesUIMap.ClickScalarVariableName(0);
            SendKeys.SendWait("VariableName");

            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            Playback.Wait(5000);
            //------------Execute Test---------------------------
            _decisionWizardUiMap.SendTabs(4);
            Playback.Wait(1000);
            _decisionWizardUiMap.SelectMenuItem(15);
            //Assert intellisense works
            Playback.Wait(1000);
            _decisionWizardUiMap.SendTabs(11);
            Playback.Wait(1000);

            //First field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(2);
            Playback.Wait(1000);

            //Second field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(1);
            Playback.Wait(1000);

            //Third field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(6);
            Playback.Wait(1000);
            SendKeys.SendWait("{ENTER}");

            // Assert Decision Title Updates Correctly
            var expected = "If [[VariableName]] Is Between [[VariableName]] and [[VariableName]]";

            var getDecision = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "FlowDecisionDesigner");
            var getDecisionText = getDecision.GetChildren()[0] as WpfEdit;
            var displayValue = getDecisionText.Text;

            Assert.AreEqual(expected, displayValue, "Decision intellisense doesnt work when using the keyboard to select intellisense results");
        }

        [TestMethod]
        public void DragADecisionIntoForEachExpectNotAddedToForEach()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);


            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            requiredPoint.Offset(20, 20);

            // Drag a ForEach onto the Workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("ForEach", workflowPoint1, "For Each");

            // Open the toolbox, and drag the control onto the Workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", requiredPoint);

            // Cancel Decision Wizard
            if(WizardsUIMap.TryWaitForWizard(3000))
            {
                SendKeys.SendWait("{TAB}{TAB}{ENTER}");
                Playback.Wait(100);

                Assert.Fail("Got droped ;(");
            }
        }

        [TestMethod]
        public void DragASwitchIntoForEachExpectNotAddedToForEach()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            requiredPoint.Offset(20, 20);

            // Drag a ForEach onto the Workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("ForEach", workflowPoint1, "For Each");

            // Open the toolbox, and drag the control onto the Workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("Switch", requiredPoint);
            // Cancel Decision Wizard
            if(WizardsUIMap.TryWaitForWizard(3000))
            {
                Playback.Wait(200);

                SendKeys.SendWait("{TAB}{TAB}{ENTER}");
                Playback.Wait(100);

                Assert.Fail("Got dropped ;(");
            }
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("for bug 9717 - copy paste multiple decisions (2013.06.22)")]
        [Owner("Ashley")]
        public void CopyDecisionsWithContextMenuAndPasteExpectedNoWizardsDisplayed()
        {
            //Initialize
            Clipboard.SetText(" ");
            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            //Drag on two decisions
            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            WizardsUIMap.WaitForWizard();

            _decisionWizardUiMap.HitDoneWithKeyboard();
            var newPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            newPoint.Y = newPoint.Y + 200;

            var clickPoint = new Point(newPoint.X, newPoint.Y);

            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", newPoint);
            WizardsUIMap.WaitForWizard();
            Playback.Wait(3000);

            _decisionWizardUiMap.HitDoneWithKeyboard();

            //Rubber-band select them
            var startDragPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            startDragPoint.X = startDragPoint.X - 100;
            startDragPoint.Y = startDragPoint.Y - 100;
            Mouse.Move(startDragPoint);
            newPoint.X = newPoint.X + 100;
            newPoint.Y = newPoint.Y + 100;
            Mouse.StartDragging();
            Mouse.StopDragging(newPoint);
            startDragPoint.X = startDragPoint.X + 150;
            startDragPoint.Y = startDragPoint.Y + 150;
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, clickPoint);
            var designSurface = WorkflowDesignerUIMap.GetFlowchartDesigner(theTab);
            SendKeys.SendWait("{DOWN}{DOWN}{ENTER}");
            Mouse.Click(designSurface);
            SendKeys.SendWait("^v");
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];

            // Assert 
            Assert.AreEqual("System Menu Bar", uIItemImage.FriendlyName);
        }

        #endregion

        #region Server Wizard

        [TestMethod]
        public void ClickNewRemoteWarewolfServerExpectedRemoteWarewolfServerOpens()
        {
            ExplorerUIMap.ClickNewServerButton();
            UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;
            if(uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the remote warewolf button does not create the new server window");
            }
            Playback.Wait(2500);
            SendKeys.SendWait("{ESC}");
            Playback.Wait(100);
        }

        #endregion
    }
}
