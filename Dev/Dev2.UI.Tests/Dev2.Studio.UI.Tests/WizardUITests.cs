using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.Utils;
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
        public TestContext TestContext { get; set; }

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

            if(uiTestControl != null)
            {
            uiTestControl.WaitForControlEnabled();
            }
            WizardsUIMap.WaitForWizard();
            SendKeys.SendWait("{ESC}");
        }

        /// <summary>
        /// News the database service shortcut key expected new database service wizard opens.
        /// </summary>
        [TestMethod]
        public void NewDatabaseServiceShortcutKeyExpectedDatabaseServiceOpens()
        {
            StudioWindow.WaitForControlReady(1000);
            Keyboard.SendKeys(StudioWindow, "{CTRL}{SHIFT}D");
            WizardsUIMap.WaitForWizard();
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

        #region Web Service And Source Wizards

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("WizardUiTests_WebServiceWizard")]
        public void WizardUiTests_WebServiceWizard_CreateServiceAndSource_ExpectedServiceAndSourceCreated()
        {
            //Initialization
            var sourceNameId = Guid.NewGuid().ToString().Substring(0, 5);
            var sourceName = "codeduitest" + sourceNameId;

            var serviceNameId = Guid.NewGuid().ToString().Substring(0, 5);
            var serviceName = "codeduitest" + serviceNameId;
            const string sourceUrl = "http://RSAKLFSVRTFSBLD/IntegrationTestSite/proxy.ashx";

            //Open wizard
            RibbonUIMap.ClickNewWebService();

            //Click new web source
            WebServiceWizardUIMap.ClickNewWebSource();

            WebServiceWizardUIMap.CreateWebSource(sourceUrl, sourceName);

            WebServiceWizardUIMap.SaveWebService(serviceName);

            //Assert
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists(serviceName, "Unassigned"));

            Assert.IsTrue(ExplorerUIMap.ValidateSourceExists(sourceName, "Unassigned"));

        }

        #endregion

        #region Db Service And Source Wizards

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("WizardUiTests_DbServiceWizard")]
        public void WizardUiTests_DbServiceWizard_CreateNewService_ExpectedServiceCreated()
        {
            //Initialization
            var serverSourceCategoryName = Guid.NewGuid().ToString().Substring(0, 5);
            var sourceNameID = Guid.NewGuid().ToString().Substring(0, 5);
            var serviceNameID = Guid.NewGuid().ToString().Substring(0, 5);
            var cat = "CODEDUITESTS" + serverSourceCategoryName.ToUpper();
            var serviceName = "codeduitest" + serviceNameID;
            var sourceName = "codeduitest" + sourceNameID;
            const string sourcePath = "RSAKLFSVRGENDEV";

            //Open wizard
            RibbonUIMap.ClickNewDbWebService();

            //Click New Db Source button
            DatabaseServiceWizardUIMap.ClickNewDbSource();

            //Create the new Db Source
            DatabaseServiceWizardUIMap.CreateDbSource(sourcePath, sourceName, cat);

            //Create the Db Service
            DatabaseServiceWizardUIMap.CreateDbService(serviceName, cat);

            //Assert
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists(serviceName, cat));
            Assert.IsTrue(ExplorerUIMap.ValidateSourceExists(sourceName, cat));
        }

        #endregion

        #region Email Source Wizard

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("WizardUiTests_EmailSourceWizard")]
        public void WizardUiTests_EmailSourceWizard_CreateNewSource_ExpectedSourceCreated()
        {
            //Initialization
            var startEmailServer = TestUtils.StartEmailServer();

            var sourceName = Guid.NewGuid().ToString().Substring(0, 5);
            var name = "codeduitest" + sourceName;

            //Open wizard
            EmailSourceWizardUIMap.OpenWizard();

            //Create Email Source
            EmailSourceWizardUIMap.CreateEmailSource(name);

            //Assert
            Assert.IsTrue(ExplorerUIMap.ValidateSourceExists(name, "Unassigned"));

            TestUtils.StopEmailServer(startEmailServer);
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

            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, pt);
            WizardsUIMap.WaitForWizard();
            _decisionWizardUiMap.SendTabs(5);
            Playback.Wait(500);
            _decisionWizardUiMap.SelectMenuItem(17); // select between ;)

            _decisionWizardUiMap.SendTabs(11);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(100, 150));

            _decisionWizardUiMap.SendTabs(2);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(400, 150));
            _decisionWizardUiMap.SendTabs(1);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(600, 150));

            _decisionWizardUiMap.SendTabs(6);
            SendKeys.SendWait("{ENTER}");

            //------------Assert Results-------------------------

            const string expected = "If [[VariableName]] Is Between [[VariableName]] and [[VariableName]]";

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

            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));

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

            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            Playback.Wait(5000);
            //------------Execute Test---------------------------
            _decisionWizardUiMap.SendTabs(5);
            Playback.Wait(1000);
            _decisionWizardUiMap.SelectMenuItem(17);
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
            const string expected = "If [[VariableName]] Is Between [[VariableName]] and [[VariableName]]";

            var getDecision = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "FlowDecisionDesigner");
            var getDecisionText = getDecision.GetChildren()[0] as WpfEdit;
            if(getDecisionText != null)
            {
            var displayValue = getDecisionText.Text;

            Assert.AreEqual(expected, displayValue, "Decision intellisense doesnt work when using the keyboard to select intellisense results");
        }
            else
            {
                Assert.Fail();
            }
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
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, workflowPoint1, "For Each");

            // Open the toolbox, and drag the control onto the Workflow
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, requiredPoint);

            // Cancel Decision Wizard
            if(WizardsUIMap.TryWaitForWizard(3000))
            {
                KeyboardCommands.SendTab();
                KeyboardCommands.SendTab();
                KeyboardCommands.SendEnter(100);

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
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, workflowPoint1, "For Each");

            // Open the toolbox, and drag the control onto the Workflow
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Switch, requiredPoint);
            // Cancel Decision Wizard
            if(WizardsUIMap.TryWaitForWizard(3000))
            {
                KeyboardCommands.SendTab();
                KeyboardCommands.SendTab();
                KeyboardCommands.SendEnter(100);

                Assert.Fail("Got dropped ;(");
            }
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("for bug 9717 - copy paste multiple decisions (2013.06.22)")]
        [Owner("Ashley Lewis")]
        public void CopyDecisionsWithContextMenuAndPasteExpectedNoWizardsDisplayed()
        {
            //Initialize
            Clipboard.SetText(" ");
            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            //Drag on two decisions
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            WizardsUIMap.WaitForWizard();

            _decisionWizardUiMap.HitDoneWithKeyboard();
            var newPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            newPoint.Y = newPoint.Y + 200;

            var clickPoint = new Point(newPoint.X, newPoint.Y);

            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, newPoint);
            WizardsUIMap.WaitForWizard(7000);

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
            ExplorerUIMap.ClickNewServerButton(3000);
            UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;
            if(uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the remote warewolf button does not create the new server window");
            }

            KeyboardCommands.SendEsc(100);
        }

        #endregion
    }
}
