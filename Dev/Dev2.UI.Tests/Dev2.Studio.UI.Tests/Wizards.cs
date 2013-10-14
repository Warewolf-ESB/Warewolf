using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
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
    public class Wizards : UIMapBase
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
            var window = new UIBusinessDesignStudioWindow();
            //close any open wizards
            var tryFindDialog = window.GetChildren()[0].GetChildren()[0];
            if(tryFindDialog.GetType() == typeof(WpfImage))
            {
                Mouse.Click(tryFindDialog);
                SendKeys.SendWait("{ESCAPE}");
                Assert.Fail("Resource changed dialog hanging after test, might not have rendered properly");
            }
            //close any open tabs
            TabManagerUIMap.CloseAllTabs();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
        }

        #endregion

        #region Service Wizards

        [TestMethod]
        public void ClickNewPluginServiceExpectedPluginServiceOpens()
        {
            RibbonUIMap.ClickRibbonMenu("Plugin Service");
            WizardsUIMap.WaitForWizard(5000, false);
            UITestControl uiTestControl = PluginServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if(uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the new plugin service button does not create the new plugin service window");
            }
            Playback.Wait(5000);
            SendKeys.SendWait("{ESC}");
            PluginServiceWizardUIMap.ClickCancel();
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
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(serviceName);
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SERVICES", "Unassigned", serviceName));
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(sourceName);
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", "Unassigned", sourceName));
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
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(name);

            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", cat, name));
        }

        [TestMethod]
        public void ClickNewDatabaseServiceExpectedDatabaseServiceOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Database Service");
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if(uIItemImage == null)
            {
                Assert.Fail("Error - Clicking the new database service button does not create the new database service window");
            }
            DatabaseServiceWizardUIMap.DatabaseServiceClickCancel();
        }

        [TestMethod]
        public void NewWebServiceShortcutKeyExpectedWebServiceOpens()
        {
            SendKeys.SendWait("{CTRL}{SHIFT}W");

            WizardsUIMap.WaitForWizard(5000);
            WebServiceWizardUIMap.Cancel();
        }

        /// <summary>
        /// News the database service shortcut key expected database service opens.
        /// </summary>
        [TestMethod]
        public void NewDatabaseServiceShortcutKeyExpectedDatabaseServiceOpens()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            SendKeys.SendWait("^+d");
            Playback.Wait(5000);
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if(uIItemImage == null)
            {
                Assert.Fail("Error - Clicking the new database service button does not create the new database service window");
            }
            DatabaseServiceWizardUIMap.DatabaseServiceClickCancel();
        }

        [TestMethod]
        public void ClickNewPluginServiceShortcutKeyExpectedPluginServiceOpens()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            SendKeys.SendWait("^+p");
            Playback.Wait(500);
            UITestControl uiTestControl = PluginServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if(uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the new plugin service button does not create the new plugin service window");
            }
            Playback.Wait(5000);
            PluginServiceWizardUIMap.ClickCancel();
            SendKeys.SendWait("{ESC}");
        }

        #endregion

        #region Source Wizards

        //2013.06.22: Ashley Lewis for bug 9478
        [TestMethod]
        public void EmailSourceWizardCreateNewSourceExpectedSourceCreated()
        {
            //Initialization
            var sourceName = Guid.NewGuid().ToString().Substring(0, 5);
            var name = "codeduitest" + sourceName;

            EmailSourceWizardUIMap.InitializeFullTestSource(name);

            //Assert
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(name);

            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", "Unassigned", name));
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        #region Decision Wizard

        private readonly DecisionWizardUIMap _decisionWizardUiMap = new DecisionWizardUIMap();

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DecisionWizard_Save")]
        public void DecisionWizard_Save_WhenMouseUsedToSelect2ndAnd3rdInputFields_FieldDataSavedCorrectly()
        {
            //------------Setup for test--------------------------
            Clipboard.Clear();
            RibbonUIMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            //------------Execute Test---------------------------

            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("VariableName");
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var decision = ToolboxUIMap.FindControl("Decision");
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            _decisionWizardUiMap.SendTabs(4);
            _decisionWizardUiMap.SelectMenuItem(37); // select between ;)

            _decisionWizardUiMap.SendTabs(11);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(610, 420));

            _decisionWizardUiMap.SendTabs(2);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(940, 420));
            _decisionWizardUiMap.SendTabs(1);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", false, new Point(1110, 420));

            _decisionWizardUiMap.SendTabs(6);
            SendKeys.SendWait("{ENTER}");


            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("^A");
            Thread.Sleep(250);
            SendKeys.SendWait("^C");
            var displayValue = Clipboard.GetData(DataFormats.Text);

            //------------Assert Results-------------------------

            var expected = "If [[VariableName]] Is Between [[VariableName]] and [[VariableName]]";

            Assert.AreEqual(expected, displayValue);

            //Cleanup
            TabManagerUIMap.CloseAllTabs();
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
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("VariableName");
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var decision = ToolboxUIMap.FindControl("Decision");
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            _decisionWizardUiMap.SendTabs(4);
            _decisionWizardUiMap.SelectMenuItem(20);
            //Assert intellisense works
            _decisionWizardUiMap.SendTabs(10);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", true);
            var actual = Clipboard.GetData(DataFormats.Text);
            Assert.AreEqual("[[VariableName]]", actual, "Decision intellisense doesn't work");
            _decisionWizardUiMap.SendTabs(2);
            _decisionWizardUiMap.GetFirstIntellisense("[[V", true);
            actual = Clipboard.GetData(DataFormats.Text);
            Assert.AreEqual("[[VariableName]]", actual, "Decision intellisense doesn't work");
            _decisionWizardUiMap.SendTabs(6);
            SendKeys.SendWait("{ENTER}");

            //Assert can save blank decision
            decision = new WorkflowDesignerUIMap().FindControlByAutomationId(theTab, "FlowDecisionDesigner");
            Point point;
            Assert.IsTrue(decision.TryGetClickablePoint(out point));
            Assert.IsNotNull(point);

            //Cleanup
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        #region Server Wizard
          
        [TestMethod]
        public void ClickNewRemoteWarewolfServerExpectedRemoteWarewolfServerOpens()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClickNewServerButton();
            UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;
            if (uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the remote warewolf button does not create the new server window");
            }
            NewServerUIMap.CloseWindow();
        } 
        
        #endregion
    }
}
