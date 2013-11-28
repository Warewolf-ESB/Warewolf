using System;
using System.Windows;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clipboard = System.Windows.Clipboard;

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
            ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);
        }

        #endregion

        #region Test Methods

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_ConnectToRemoteServerFromExplorer_RemoteServerConnected()
        {
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
            var selectedSeverName = ExplorerUIMap.GetSelectedSeverName();
            Assert.AreEqual(RemoteServerName, selectedSeverName);

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_CreateRemoteWorkFlow_WorkflowIsCreated()
        {
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
            RibbonUIMap.CreateNewWorkflow();
            var activeTabName = TabManagerUIMap.GetActiveTabName();
            Assert.IsTrue(activeTabName.Contains("Unsaved"));
            Assert.IsTrue(activeTabName.Contains("RemoteConnection"));

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
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", p);
            var activeTabName = TabManagerUIMap.GetActiveTabName();
            Assert.IsTrue(activeTabName.Contains("Find Records - RemoteConnection *"));

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
        public void RemoteServerUITests_DragAndDropWorkflowFromRemoteServerOnALocalHostCreatedWorkflow_WorkFlowIsDroppedAndCanExecute()
        {
            const string remoteWorkflowName = "Recursive File Copy";

            //Ensure that we're in localhost
            ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);

            //Create a workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();
            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();

            //Drag on a remote workflow
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
            ExplorerUIMap.EnterExplorerSearchText(remoteWorkflowName);
            ExplorerUIMap.DragControlToWorkflowDesigner(RemoteServerName, "WORKFLOWS", "UTILITY", remoteWorkflowName, point);

            //Should be able to get clean debug output
            OpenMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

            //Assert that the workflow really is on the design surface and debug output is clean
            Assert.IsFalse(OutputUIMap.IsAnyStepsInError(), "The remote workflow threw errors when executed locally");
            Assert.IsTrue(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, remoteWorkflowName));
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
            PopupDialogUIMap.WaitForDialog();
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

            var remoteTab = TabManagerUIMap.FindTabByName(TextToSearchWith + " - " + RemoteServerName);
            var canidateTab = TabManagerUIMap.GetActiveTab();
            Assert.IsNotNull(remoteTab);
            // verify the active tab is the remote tab
            Assert.AreEqual(remoteTab, canidateTab);
            Assert.IsTrue(OutputUIMap.IsExecutionRemote());

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteDbSource_DbSourceIsEdited()
        {
            const string TextToSearchWith = "DBSource";
            var userName = string.Empty;

            try
            {
                //Edit remote db source
                OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
                Keyboard.SendKeys("{TAB}{TAB}{RIGHT}{TAB}testuser{TAB}test123{TAB}{ENTER}{TAB}{TAB}{ENTER}");
                Playback.Wait(100);
                SaveDialogUIMap.ClickSave();
            }
            finally
            {
                //Change it back
                ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
                userName = DatabaseSourceUIMap.GetUserName();
                Keyboard.SendKeys("{TAB}{TAB}{LEFT}{TAB}{TAB}{ENTER}");
                Playback.Wait(100);
                SaveDialogUIMap.ClickSave();
            }

            Assert.AreEqual("testuser", userName, "Cannot edit remote db source");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebSource_WebSourceIsEdited()
        {
            const string TextToSearchWith = "WebSource";
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            Playback.Wait(3500);
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            Playback.Wait(1000);
            SaveDialogUIMap.ClickSave();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebService_WebServiceIsEdited()
        {
            Assert.Fail("Bad test! Not indicative that things worked!");

            //const string TextToSearchWith = "WebService";
            //OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            //Playback.Wait(3500);
            //SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            //Playback.Wait(1000);
            //SaveDialogUIMap.ClickSave();


        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteDbService_DbServiceIsEdited()
        {
            const string TextToSearchWith = "RemoteDBService";
            string actionName = string.Empty;

            //Edit remote db service
            OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            DatabaseServiceWizardUIMap.ClickScrollActionListUp();
            DatabaseServiceWizardUIMap.ClickFirstAction();
            DatabaseServiceWizardUIMap.ClickTestAction();
            Playback.Wait(100);
            DatabaseServiceWizardUIMap.ClickOK();
            
            //Change it back
            ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            actionName = DatabaseServiceWizardUIMap.GetActionName();
            DatabaseServiceWizardUIMap.ClickSecondAction();
            DatabaseServiceWizardUIMap.ClickTestAction();
            Playback.Wait(100);
            DatabaseServiceWizardUIMap.ClickOK();

            //Assert remote db service changed its action
            Assert.AreEqual("dbo.fn_diagramob", actionName, "Cannot edit remote db service");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteEmailSource_EmailSourceIsEdited()
        {
            const string TextToSearchWith = "EmailSource";
            var timeout = string.Empty;

            //Edit remote email source
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            Keyboard.SendKeys(wizard, "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}1234{TAB}{ENTER}");
            wizard.WaitForControlReady();
            Keyboard.SendKeys("@gmail.com{TAB}dev2developer@yahoo.com{ENTER}");
            Playback.Wait(10000);
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            SaveDialogUIMap.ClickSave();

            //Change it back
            ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            var persistClipboard = Clipboard.GetText();
            Keyboard.SendKeys(wizard, "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{CTRL}c100{TAB}{ENTER}");
            timeout = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);
            wizard.WaitForControlReady();
            Keyboard.SendKeys("@gmail.com{TAB}dev2developer@yahoo.com{ENTER}");
            Playback.Wait(10000);
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            SaveDialogUIMap.ClickSave();

            //Assert remote email source changed its timeout
            Assert.AreEqual("1234", timeout, "Cannot edit remote email source");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginSource_PluginSourceIsEdited()
        {
            const string TextToSearchWith = "PluginSource";
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            Playback.Wait(3500);
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{ENTER}");
            Playback.Wait(1000);
            SaveDialogUIMap.ClickSave();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginService_PluginServiceIsEdited()
        {
            const string TextToSearchWith = "PluginService";
            var actionName = string.Empty;

            //Edit remote plugin service
            OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            PluginServiceWizardUIMap.ClickActionAtIndex(3);
            PluginServiceWizardUIMap.ClickTest();
            Playback.Wait(1000);
            PluginServiceWizardUIMap.ClickOK();

            //Change it back
            ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            actionName = PluginServiceWizardUIMap.GetActionName();
            PluginServiceWizardUIMap.ClickActionAtIndex(4);
            PluginServiceWizardUIMap.ClickTest();
            Playback.Wait(1000);
            PluginServiceWizardUIMap.ClickOK();

            Assert.AreEqual("ToString", actionName, "Cannot change remote plugin service");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_AddRenameAndDeleteARemoteWorkFlow_CompletesSuccessfully()
        {

            var serviceType = "WORKFLOWS";
            var folderName = "Unassigned";

            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);

            //CREATE A WORKFLOW
            RibbonUIMap.CreateNewWorkflow();

            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);

            //SAVE A WORKFLOW
            OpenMenuItem("Save");
            Playback.Wait(2000);
            string InitialName = Guid.NewGuid().ToString();
            EnternameAndSave(InitialName);

            //RENAME A WORKFLOW
            ExplorerUIMap.EnterExplorerSearchText(InitialName);
            Playback.Wait(2000);
            ExplorerUIMap.RightClickRenameProject(RemoteServerName, serviceType, folderName, InitialName);
            string RenameTo = Guid.NewGuid().ToString();
            SendKeys.SendWait(RenameTo + "{ENTER}");

            //DELETE A WORKFLOW
            ExplorerUIMap.EnterExplorerSearchText(RenameTo);
            ExplorerUIMap.RightClickDeleteProject(RemoteServerName, serviceType, folderName, RenameTo);
            Assert.IsFalse(ExplorerUIMap.ServiceExists(RemoteServerName, serviceType, folderName, RenameTo));

        }

        #endregion

        #region Utils

        void EnternameAndSave(string tempName)
        {
            SaveDialogUIMap.ClickAndTypeInNameTextbox(tempName);
            Keyboard.SendKeys("{TAB}{TAB}{ENTER}");
            Playback.Wait(3000);
        }

        void OpenMenuItem(string itemType)
        {
            RibbonUIMap.ClickRibbonMenuItem(itemType);
        }

        void OpenWorkFlow(string serverName, string serviceType, string foldername, string textToSearchWith)
        {
            ExplorerUIMap.ClickServerInServerDDL(serverName);
            ExplorerUIMap.EnterExplorerSearchText(textToSearchWith);
            ExplorerUIMap.DoubleClickOpenProject(serverName, serviceType, foldername, textToSearchWith);
            Playback.Wait(1500);
        }

        #endregion
    }
}