using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.EmailSourceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        [TestCleanup]
        public void TestCleanup()
        {
            Playback.Wait(500);
            //close any open wizards
            var tryFindDialog = StudioWindow.GetChildren()[0].GetChildren()[0];
            if(tryFindDialog.GetType() == typeof(WpfImage))
            {
                Mouse.Click(tryFindDialog);
                SendKeys.SendWait("{ESCAPE}");
                Assert.Fail("Wizard hanging after test, might not have rendered properly");
            }
            //close any open tabs
            TabManagerUIMap.CloseAllTabs();
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
        }

        #endregion

        #region Service Wizards

        [TestMethod]
        // 05/11 - Failure is Intermittent ;)
        public void ClickNewPluginServiceExpectedPluginServiceOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("UI_RibbonHomeTabPluginServiceBtn_AutoID");
            WizardsUIMap.WaitForWizard(5000, false);
            UITestControl uiTestControl = PluginServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if(uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the new plugin service button does not create the new plugin service window");
            }
            Playback.Wait(5000);
            SendKeys.SendWait("{ESC}");
        }

        [TestMethod]
        public void WebServiceWizardCreateServiceAndSourceExpectedServiceCreated()
        {
            //Initialization
            var sourceNameId = Guid.NewGuid().ToString().Substring(0, 5);
            var sourceName = "codeduitest" + sourceNameId;
            var serviceNameId = Guid.NewGuid().ToString().Substring(0, 5);
            var serviceName = "codeduitest" + serviceNameId;

            WebServiceWizardUIMap.InitializeFullTestServiceAndSource(serviceName, sourceName);
            
            //Assert
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.EnterExplorerSearchText(serviceName);
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SERVICES", "Unassigned", serviceName));
            ExplorerUIMap.RightClickDeleteProject("localhost", "SERVICES", "Unassigned", serviceName);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(sourceName);
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", "Unassigned", sourceName));
            ExplorerUIMap.RightClickDeleteProject("localhost", "SOURCES", "Unassigned", sourceName);
        }

        //2013.03.14: Ashley Lewis - Bug 9217
        [TestMethod]
        public void DatabaseServiceWizardCreateNewServiceExpectedServiceCreated()
        {
            //Initialization
            var serverSourceCategoryName = Guid.NewGuid().ToString().Substring(0, 5);
            var serverSourceName = Guid.NewGuid().ToString().Substring(0, 5);
            var cat = "CODEDUITESTS" + serverSourceCategoryName;
            var name = "codeduitest" + serverSourceName;

            DatabaseServiceWizardUIMap.InitializeFullTestServiceAndSource(cat, name);

            //Assert
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(name);

            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", cat, name));
        }

        [TestMethod]
        public void NewDatabaseServiceShortcutKeyExpectedDatabaseServiceOpens()
        {
            StudioWindow.SetFocus();
            SendKeys.SendWait("^+D");
            WizardsUIMap.WaitForWizard();
            DatabaseServiceWizardUIMap.ClickCancel();
        }

        [TestMethod]
        // 05/11 - Failure is Intermittent ;)
        public void NewPluginServiceShortcutKeyExpectedPluginServiceOpens()
        {
            StudioWindow.SetFocus();
            SendKeys.SendWait("^+P");
            WizardsUIMap.WaitForWizard();
            PluginServiceWizardUIMap.ClickCancel();
        }

        [TestMethod]
        // 05/11 - Failure is Correct - Broken Functionality ;)
        public void NewWebServiceShortcutKeyExpectedWebServiceOpens()
        {
            StudioWindow.SetFocus();
            SendKeys.SendWait("^+W");
            WizardsUIMap.WaitForWizard();
            WebServiceWizardUIMap.Cancel();
        }

        #endregion

        #region Source Wizards

        //2013.06.22: Ashley Lewis for bug 9478
        [TestMethod]
        // 05/11 - Failure is Correct - TEST IS FAULTY ;)
        public void EmailSourceWizardCreateNewSourceExpectedSourceCreated()
        {
            //Initialization
            var sourceName = Guid.NewGuid().ToString().Substring(0, 5);
            var name = "codeduitest" + sourceName;

            EmailSourceWizardUIMap.InitializeFullTestSource(name);

            //Assert
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.DoRefresh();
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
            Clipboard.Clear();
            RibbonUIMap.CreateNewWorkflow();

            var theTab = TabManagerUIMap.GetActiveTab();

            //------------Execute Test---------------------------

            DockManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("VariableName");
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            var decision = ToolboxUIMap.FindControl("Decision");
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            WizardsUIMap.WaitForWizard();
            _decisionWizardUiMap.SendTabs(4);
            _decisionWizardUiMap.SelectMenuItem(37); // select between ;)

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
            var getDecisionText = getDecision.GetChildren()[0] as WpfEdit;
            var displayValue = getDecisionText.Text;

            Assert.AreEqual(expected, displayValue, "Decision intellisense doesnt work when using the mouse to select intellisense results");
        }

        //Bug 9339 + Bug 9378
        [TestMethod]
        public void SaveDecisionWithBlankFieldsExpectedDecisionSaved()
        {
            Clipboard.Clear();
            //Initialize
            RibbonUIMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            //Set variable
            DockManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("VariableName");
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            var decision = ToolboxUIMap.FindControl("Decision");
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));

            //Save the decision with blank fields
            WizardsUIMap.WaitForWizard();
            DecisionWizardUIMap.ClickDone();

            //Assert can save blank decision
            decision = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "FlowDecisionDesigner");
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
            DockManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("VariableName");
            var decision = ToolboxUIMap.FindControl("Decision");
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            WizardsUIMap.WaitForWizard();
            //------------Execute Test---------------------------
            _decisionWizardUiMap.SendTabs(4);
            Playback.Wait(100);
            _decisionWizardUiMap.SelectMenuItem(29);
            //Assert intellisense works
            Playback.Wait(100);
            _decisionWizardUiMap.SendTabs(11);
            Playback.Wait(100);

            //First field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(2);
            Playback.Wait(100);

            //Second field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(1);
            Playback.Wait(100);

            //Third field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(6);
            Playback.Wait(100);
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
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);


            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            requiredPoint.Offset(20, 50);

            // Drag a ForEach onto the Workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Open the toolbox, and drag the control onto the Workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", requiredPoint);

            // Cancel Decision Wizard
            if(WizardsUIMap.TryWaitForWizard(5000))
            {
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
            requiredPoint.Offset(20, 40);

            // Drag a ForEach onto the Workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Open the toolbox, and drag the control onto the Workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner("Switch", requiredPoint);
            // Cancel Decision Wizard
            if(WizardsUIMap.TryWaitForWizard(5000))
            {
                Assert.Fail("Got droped ;(");
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
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl startButton = WorkflowDesignerUIMap.FindStartNode(theTab);
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            var decision = ToolboxUIMap.FindControl("Decision");
            //Drag on two decisions
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            WizardsUIMap.WaitForWizard();
            Playback.Wait(2000);
            _decisionWizardUiMap.HitDoneWithKeyboard();
            var newPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            newPoint.Y = newPoint.Y + 200;
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, newPoint);
            WizardsUIMap.WaitForWizard();
            Playback.Wait(2000);
            _decisionWizardUiMap.HitDoneWithKeyboard();
            //Rubberband select them
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
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, startDragPoint);
            var designSurface = WorkflowDesignerUIMap.GetFlowchartDesigner(theTab);
            SendKeys.SendWait("{DOWN}{DOWN}{ENTER}");
            Mouse.Click(designSurface);
            SendKeys.SendWait("^v");
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            Assert.AreEqual("System Menu Bar", uIItemImage.FriendlyName);
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        #region Server Wizard
          
        [TestMethod]
        public void ClickNewRemoteWarewolfServerExpectedRemoteWarewolfServerOpens()
        {
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClickNewServerButton();
            UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;
            if(uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the remote warewolf button does not create the new server window");
            }
            WizardsUIMap.WaitForWizard();
            SendKeys.SendWait("{ESC}");
        } 
        
        #endregion
    }
}
