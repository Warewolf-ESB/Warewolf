
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Studio.UI.Tests.UIMaps
// ReSharper restore CheckNamespace
{
    [CodedUITest]
    [Ignore]
    public class WizardUITests : UIMapBase
    {

        #region Init/Cleanup
        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            RestartStudioOnFailure();
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

            StudioWindow.SetFocus();
            PluginServiceWizardUIMap.ClickCancel();
        }

        /// <summary>
        /// News the database service shortcut key expected new database service wizard opens.
        /// </summary>
        [TestMethod]
        public void NewDatabaseServiceShortcutKeyExpectedDatabaseServiceOpens()
        {
            StudioWindow.WaitForControlReady(1000);
            Keyboard.SendKeys(StudioWindow, "{CTRL}{SHIFT}D");
            if(!WizardsUIMap.TryWaitForWizard())
            {
                Assert.Fail("New Database service shortcut key doesnt work");
            }

            StudioWindow.SetFocus();
            DatabaseServiceWizardUIMap.ClickCancel();
        }

        [TestMethod]
        public void NewPluginServiceShortcutKeyExpectedPluginServiceOpens()
        {
            StudioWindow.WaitForControlReady(1000);
            Keyboard.SendKeys(StudioWindow, "{CTRL}{SHIFT}P");
            if(!WizardsUIMap.TryWaitForWizard())
            {
                Assert.Fail("New plugin service shortcut key doesnt work");
            }

            StudioWindow.SetFocus();
            PluginServiceWizardUIMap.ClickCancel();
        }

        [TestMethod]
        public void NewWebServiceShortcutKeyExpectedWebServiceOpens()
        {
            StudioWindow.WaitForControlReady(1000);
            Keyboard.SendKeys(StudioWindow, "{CTRL}{SHIFT}W");
            if(!WizardsUIMap.TryWaitForWizard())
            {
                Assert.Fail("New web service shortcut key doesnt work");
            }

            StudioWindow.SetFocus();
            WebServiceWizardUIMap.Cancel();
        }

        #endregion

        #region Web Service And Source Wizards

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("WizardUiTests_WebServiceWizard")]
        [Ignore]
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

            Assert.AreEqual("localhost (http://localhost:3142/dsf)", WizardsUIMap.GetRightTitleText());

            //Click new web source
            WebServiceWizardUIMap.ClickNewWebSource();

            WebServiceWizardUIMap.CreateWebSource(sourceUrl, sourceName);

            WebServiceWizardUIMap.SaveWebService(serviceName);

            // clean up ;)
            Bootstrap.DeleteService(serviceName);
            Bootstrap.DeleteSource(sourceName);

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
            var sourceNameID = Guid.NewGuid().ToString().Substring(0, 5);
            var serviceNameID = Guid.NewGuid().ToString().Substring(0, 5);
            const string cat = "UNASSIGNED";
            var serviceName = "codeduitest" + serviceNameID;
            var sourceName = "codeduitest" + sourceNameID;
            const string sourcePath = "RSAKLFSVRGENDEV";

            //Open wizard
            RibbonUIMap.ClickNewDbWebService();

            Assert.AreEqual("localhost (http://localhost:3142/dsf)", WizardsUIMap.GetRightTitleText());

            //Click New Db Source button
            DatabaseServiceWizardUIMap.ClickNewDbSource();

            //Create the new Db Source
            DatabaseServiceWizardUIMap.CreateDbSource(sourcePath, sourceName);

            //Create the Db Service
            DatabaseServiceWizardUIMap.CreateDbService(serviceName);

            // clean up ;)
            Bootstrap.DeleteService(serviceName);
            Bootstrap.DeleteSource(sourceName);

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

            Assert.AreEqual("New Email Source", WizardsUIMap.GetLeftTitleText());
            Assert.AreEqual("localhost (http://localhost:3142/dsf)", WizardsUIMap.GetRightTitleText());

            //Create Email Source
            EmailSourceWizardUIMap.CreateEmailSource(name);

            // clean up ;)
            Bootstrap.DeleteSource(sourceName);

            //Assert
            Assert.IsTrue(ExplorerUIMap.ValidateSourceExists(name, "Unassigned"), "Email source was not created.");

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
            VariablesUIMap.EnterTextIntoScalarName(0, "VariableName");

            var pt = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);

            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, pt);
            WizardsUIMap.WaitForWizard();
            Assert.AreEqual("Decision Flow", WizardsUIMap.GetLeftTitleText());
            _decisionWizardUiMap.SendTabs(5, 500);
            _decisionWizardUiMap.SelectMenuItem(17, 100); // select between ;)

            _decisionWizardUiMap.SendTabs(11, 500);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(100, 120));

            _decisionWizardUiMap.SendTabs(2, 500);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(400, 120));
            _decisionWizardUiMap.SendTabs(1, 500);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(600, 120));

            _decisionWizardUiMap.SendTabs(6, 500);
            KeyboardCommands.SendEnter();

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
            _decisionWizardUiMap.SendTabs(5, 1000);
            _decisionWizardUiMap.SelectMenuItem(17, 2000);
            _decisionWizardUiMap.SendTabs(11, 1000);

            //First field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(2, 1000);

            //Second field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(1, 1000);

            //Third field
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            _decisionWizardUiMap.SendTabs(6, 1000);

            //Wait for wizard to close
            KeyboardCommands.SendEnter(1500);

            // Assert Decision Title Updates Correctly
            const string Expected = "If [[VariableName]] Is Between [[VariableName]] and [[VariableName]]";

            var getDecision = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "FlowDecisionDesigner");
            var getDecisionText = getDecision.GetChildren()[0] as WpfEdit;
            if(getDecisionText != null)
            {
                var displayValue = getDecisionText.Text;

                Assert.AreEqual(Expected, displayValue, "Decision intellisense doesnt work when using the keyboard to select intellisense results");
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
       
    }
}
