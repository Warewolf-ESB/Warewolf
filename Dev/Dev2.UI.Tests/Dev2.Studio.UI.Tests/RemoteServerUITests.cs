using System.Windows;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Point = System.Drawing.Point;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    ///     These are UI tests based on using a remote server
    /// </summary>
    [CodedUITest]
    public class RemoteServerUiTests : UIMapBase
    {
        #region Fields

        const string RemoteServerName = "RemoteConnection";
        const string LocalHostServerName = "localhost";
        const string ExplorerTab = "Explorer";

        #endregion

        #region Test Methods

        [TestCleanup]
        public void TestCleanup()
        {
            Playback.Wait(500);
            //close any open wizards
            var tryFindDialog = DockManagerUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            Point point;
            if (tryFindDialog.GetType() == typeof (WpfImage))
            {
                if (tryFindDialog.TryGetClickablePoint(out point))
                {
                    Mouse.Click(tryFindDialog);
                    SendKeys.SendWait("{ESCAPE}");
                    Assert.Fail("Dialog hanging after test, might not have rendered properly");
                }
                else
                {
                    SendKeys.SendWait("{ESCAPE}");
                }
            }
            //close any open tabs
            TabManagerUIMap.CloseAllTabs();
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            //reset active server
            ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);
            ExplorerUIMap.ClearExplorerSearchText();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_ConnectToRemoteServerFromExplorer_RemoteServerConnected()
        {
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
            var selectedSeverName = ExplorerUIMap.SelectedSeverName();
            Assert.AreEqual(RemoteServerName, selectedSeverName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_CreateRemoteWorkFlow_WorkflowIsCreated()
        {
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
            RibbonUIMap.CreateNewWorkflow();
            var activeTabName = TabManagerUIMap.GetActiveTabName();
            Assert.IsTrue(activeTabName.Contains("Unsaved"));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWorkFlow_WorkflowIsEdited()
        {
            const string TextToSearchWith = "Find Records";
            OpenWorkFlow(RemoteServerName, "WORKFLOWS", "TESTS", TextToSearchWith);
            var uiControl = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "Assign");
            var p = WorkflowDesignerUIMap.GetPointUnderControl(uiControl);
            ToolboxUIMap.DragControlToWorkflowDesigner("MultiAssign", p);
            var activeTabName = TabManagerUIMap.GetActiveTabName();
            Assert.IsTrue(activeTabName.Contains("Find Records - RemoteConnection"));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_ViewRemoteWorkFlowInBrowser_WorkflowIsExecuted()
        {
            const string TextToSearchWith = "Find Records";
            OpenWorkFlow(RemoteServerName, "WORKFLOWS", "TESTS", TextToSearchWith);
            OpenMenuItem("View in Browser");
            Playback.Wait(5000);
            //assert error dialog not showing
            var child = DockManagerUIMap.UIBusinessDesignStudioWindow.GetChildren()[0];
            if (child != null)
            {
                Assert.IsNotInstanceOfType(child.GetChildren()[0], typeof (Window));
            }
            else
            {
                Assert.Fail("Cannot get studio window after remote workflow show in browser");
            }
            //Try close browser
            ExternalUIMap.CloseAllInstancesOfIE();
        }

        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_DragAndDropWorkflowFromRemoteServerOnALocalHostCreatedWorkflow_WorkFlowIsDropped()
        {
            const string TextToSearchWith = "Simple Remote Workflow";
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            //Ensure that we're in localhost
            ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);
            //Create a workfliow
            RibbonUIMap.CreateNewWorkflow();
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);

            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ExplorerUIMap.ClearExplorerSearchText();

            ExplorerUIMap.EnterExplorerSearchText(TextToSearchWith);
            ExplorerUIMap.DragControlToWorkflowDesigner(RemoteServerName, "WORKFLOWS", "TEST", TextToSearchWith, point);

            OpenMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            SendKeys.SendWait("{F5}");
            Playback.Wait(1000);

            OutputUIMap.WaitForStepCount(34, 5000);
            Playback.Wait(10000);
            Assert.IsFalse(OutputUIMap.IsAnyStepsInError(), "The remote workflow threw errors when executed locally");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")] 
        public void RemoteServerUITests_DragAndDropWorkflowFromALocalServerOnARemoteServerCreatedWorkflow_WorkFlowIsDropped()
        {
            const string TextToSearchWith = "Utility - Assign";
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
            //Create a workfliow
            RibbonUIMap.CreateNewWorkflow();
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            //Connect to local server
            ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);

            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ExplorerUIMap.ClearExplorerSearchText();

            ExplorerUIMap.EnterExplorerSearchText(TextToSearchWith);
            ExplorerUIMap.DragControlToWorkflowDesigner(LocalHostServerName, "WORKFLOWS", "EXAMPLES", TextToSearchWith, point);

            OpenMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

            Assert.IsFalse(OutputUIMap.IsAnyStepsInError(), "The local workflow threw errors when executed remotely");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_OpenWorkflowOnRemoteServerAndOpenWorkflowWithSameNameOnLocalHost_WorkflowIsOpened()
        {
            const string TextToSearchWith = "Find Records";

            OpenWorkFlow(RemoteServerName, "WORKFLOWS", "TESTS", TextToSearchWith);
            var remoteTab = TabManagerUIMap.FindTabByName(TextToSearchWith + " - " + RemoteServerName);
            Assert.IsNotNull(remoteTab);

            OpenWorkFlow(LocalHostServerName, "WORKFLOWS", "TESTS", TextToSearchWith);
            var localHostTab = TabManagerUIMap.FindTabByName(TextToSearchWith);
            Assert.IsNotNull(localHostTab);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_DebugARemoteWorkflowWhenLocalWorkflowWithSameNameIsOpen_WorkflowIsExecuted()
        {
            const string TextToSearchWith = "Find Records";

            OpenWorkFlow(LocalHostServerName, "WORKFLOWS", "TESTS", TextToSearchWith);
            var localHostTab = TabManagerUIMap.FindTabByName(TextToSearchWith);
            Assert.IsNotNull(localHostTab);

            OpenWorkFlow(RemoteServerName, "WORKFLOWS", "TESTS", TextToSearchWith);

            OpenMenuItem("Debug");
            SendKeys.SendWait("{F5}");
            Playback.Wait(1000);

            var remoteTab = TabManagerUIMap.FindTabByName(TextToSearchWith + " - " + RemoteServerName);
            Assert.IsNotNull(remoteTab);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteDbSource_DbSourceIsEdited()
        {
            const string TextToSearchWith = "DBSource";
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            WizardsUIMap.WaitForWizard();
            DatabaseSourceUIMap.ClickSaveConnection();
            SaveDialogUIMap.ClickSave();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebSource_WebSourceIsEdited()
        {
            const string TextToSearchWith = "WebSource";
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            WizardsUIMap.WaitForWizard();
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");
            SaveDialogUIMap.ClickSave();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebService_WebServiceIsEdited()
        {
            const string TextToSearchWith = "WebService";
            OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            WizardsUIMap.WaitForWizard();
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");
            SaveDialogUIMap.ClickSave();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteDbService_DbServiceIsEdited()
        {
            const string TextToSearchWith = "DBService";
            OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            WizardsUIMap.WaitForWizard();
            DatabaseServiceWizardUIMap.ClickCancel();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteEmailSource_EmailSourceIsEdited()
        {
            const string TextToSearchWith = "EmailSource";
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            WizardsUIMap.WaitForWizard();
            EmailSourceWizardUIMap.ClickCancel();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginSource_PluginSourceIsEdited()
        {
            const string TextToSearchWith = "PluginSource";
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            WizardsUIMap.WaitForWizard();
            PluginSourceMap.ClickSave();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginService_PluginServiceIsEdited()
        {
            const string TextToSearchWith = "PluginService";
            OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            WizardsUIMap.WaitForWizard();
            PluginServiceWizardUIMap.ClickTest();
            PluginServiceWizardUIMap.ClickOK();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_AddExecuteRenameAndDeleteALocalWorlFlow_ProcessCompletesSuccessfully()
        {
            ProcessAWorkflow(LocalHostServerName, "WORKFLOWS", "Unassigned");
            ProcessAWorkflow(RemoteServerName, "WORKFLOWS", "Unassigned");
        }

        void ProcessAWorkflow(string serverName, string serviceType, string folderName)
        {
            //CREATE A WORKFLOW
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(serverName);
            RibbonUIMap.CreateNewWorkflow();
            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ToolboxUIMap.DragControlToWorkflowDesigner("MultiAssign", point);

            //SAVE A WORKFLOW
            OpenMenuItem("Save");
            WizardsUIMap.WaitForWizard();
            const string InitialName = "Initial_Name_WF_1";
            EnternameAndSave(InitialName);

            //EXECUTE A WORKFLOW
            OpenMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            SendKeys.SendWait("{F5}");
            TabManagerUIMap.CloseAllTabs();

            //RENAME A WORKFLOW
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(InitialName);
            ExplorerUIMap.RightClickRenameProject(serverName, serviceType, folderName, InitialName);
            const string RenameTo = "RenameTo_Name_WF_1";
            SendKeys.SendWait(RenameTo + "{ENTER}");

            //DELETE A WORKFLOW
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(RenameTo);
            ExplorerUIMap.RightClickDeleteProject(serverName, serviceType, folderName, RenameTo);
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.DoRefresh();
            Assert.IsFalse(ExplorerUIMap.ServiceExists("localhost", serviceType, folderName, RenameTo), "Resources on " + serverName + " cannot be deleted");
        }
        #endregion

        #region Utils

        void EnternameAndSave(string tempName)
        {
            SaveDialogUIMap.ClickAndTypeInNameTextbox(tempName);
            Keyboard.SendKeys("{TAB}{TAB}{ENTER}");
            Playback.Wait(6000);
        }

        void OpenMenuItem(string itemType)
        {
            RibbonUIMap.ClickRibbonMenuItem(itemType);
        }

        void OpenWorkFlow(string serverName, string serviceType, string foldername, string textToSearchWith)
        {
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(serverName);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(textToSearchWith);
            ExplorerUIMap.DoubleClickOpenProject(serverName, serviceType, foldername, textToSearchWith);
        }

        #endregion
    }
}