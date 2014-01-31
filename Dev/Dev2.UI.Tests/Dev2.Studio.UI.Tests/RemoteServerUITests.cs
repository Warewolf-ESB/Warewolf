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
    [Ignore]
    public class RemoteServerUiTests : UIMapBase
    {
        #region Fields

        const string RemoteServerName = "Remote Connection";
        const string LocalHostServerName = "localhost";

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
            Playback.Wait(1500);
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
            Assert.IsTrue(activeTabName.Contains("Remote Connection"));

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
            Assert.IsTrue(activeTabName.Contains("Find Records - Remote Connection *"));

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_ViewRemoteWorkFlowInBrowser_WorkflowIsExecuted()
        {

            const string TextToSearchWith = "Find Records";
            OpenWorkFlow(RemoteServerName, "WORKFLOWS", "TESTS", TextToSearchWith);
            SendKeys.SendWait("{F7}");
            Playback.Wait(5000);
            //assert error dialog not showing
            var child = StudioWindow.GetChildren()[0];
            if(child != null)
            {
                Assert.IsNotInstanceOfType(child.GetChildren()[0], typeof(Window));
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
            StudioWindow.WaitForControlReady();

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
                Keyboard.SendKeys("{TAB}{TAB}{RIGHT}");
                Playback.Wait(100);
                Keyboard.SendKeys("{TAB}testuser{TAB}test123{TAB}");
                Playback.Wait(100);
                Keyboard.SendKeys("{ENTER}{TAB}");
                Playback.Wait(100);
                Keyboard.SendKeys("{TAB}{ENTER}");
                SaveDialogUIMap.ClickSave();
            }
            finally
            {
                //Change it back
                ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
                userName = DatabaseSourceUIMap.GetUserName();
                SendKeys.SendWait("{TAB}{TAB}{LEFT}{TAB}{TAB}{ENTER}");
                Playback.Wait(100);
                SaveDialogUIMap.ClickSave();
            }

            Assert.AreEqual("testuser", userName, "Cannot edit remote db source");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        [Ignore] // FLIPPING FAULTY EXTERNAL RESOURCE
        public void RemoteServerUITests_EditRemoteWebSource_WebSourceIsEdited()
        {
            const string TextToSearchWith = "WebSource";
            var query = string.Empty;

            //Edit remote web source
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            Playback.Wait(7000);
            Keyboard.SendKeys("{TAB}{TAB}{TAB}?CountryName=Canada{TAB}{TAB}{TAB}{ENTER}");
            Playback.Wait(1000);
            SaveDialogUIMap.ClickSave();

            //Change it back
            ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            Playback.Wait(7000);
            var persistClipboard = Clipboard.GetText();
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{CTRL}c?CountryName=South Africa{TAB}{TAB}{TAB}{ENTER}");
            query = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);
            Playback.Wait(1000);
            SaveDialogUIMap.ClickSave();

            Assert.AreEqual("?CountryName=Canada", query, "Cannot change remote web source");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebService_WebServiceIsEdited()
        {
            const string TextToSearchWith = "WebService";
            OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            Playback.Wait(3500);
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            Playback.Wait(1000);
            SaveDialogUIMap.ClickSave();


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
            Playback.Wait(2000);
            DatabaseServiceWizardUIMap.ClickScrollActionListUp();
            DatabaseServiceWizardUIMap.ClickFirstAction();
            DatabaseServiceWizardUIMap.ClickTestAction();
            DatabaseServiceWizardUIMap.ClickOK();
            Playback.Wait(2000);
            //Change it back
            OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            Playback.Wait(2000);
            actionName = DatabaseServiceWizardUIMap.GetActionName();
            DatabaseServiceWizardUIMap.ClickSecondAction();
            DatabaseServiceWizardUIMap.ClickTestAction();
            Playback.Wait(1000);
            DatabaseServiceWizardUIMap.ClickOK();
            Playback.Wait(2000);
            //Assert remote db service changed its action
            Assert.AreEqual("dbo.fn_diagramob", actionName, "Cannot edit remote db service");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        [Ignore] // DO NOT REMOVE UNTIL CONFIGURED TO USE LOCAL SERVER!!!
        public void RemoteServerUITests_EditRemoteEmailSource_EmailSourceIsEdited()
        {
            const string TextToSearchWith = "EmailSource";
            var timeout = string.Empty;

            //Edit remote email source
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}1234{TAB}{ENTER}");
            wizard.WaitForControlReady();
            SendKeys.SendWait("@gmail.com{TAB}dev2developer@yahoo.com");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(12000);
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            SaveDialogUIMap.ClickSave();

            //Change it back
            ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            var persistClipboard = Clipboard.GetText();
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}");
            wizard.WaitForControlReady();
            Keyboard.SendKeys(wizard, "{CTRL}c");
            SendKeys.SendWait("100{TAB}{ENTER}");
            timeout = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);
            wizard.WaitForControlReady();
            SendKeys.SendWait("@gmail.com{TAB}dev2developer@yahoo.com");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(12000);
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
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
            var path = string.Empty;

            //Edit remote plugin source
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            PluginSourceMap.ClickPluginSourceAssemblyPath();
            Keyboard.SendKeys("{LEFT}{LEFT}{LEFT}{LEFT}");
            Keyboard.SendKeys(" ");
            Keyboard.SendKeys("-");
            Keyboard.SendKeys(" ");
            Keyboard.SendKeys("C");
            Keyboard.SendKeys("o");
            Keyboard.SendKeys("p");
            Keyboard.SendKeys("y");
            Playback.Wait(100);
            Keyboard.SendKeys("{TAB}{ENTER}");
            SaveDialogUIMap.ClickSave();

            //Change it back                        
            ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            Playback.Wait(4500);
            path = PluginSourceMap.GetAssemblyPathText();
            Playback.Wait(1000);
            Keyboard.SendKeys("{LEFT}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{LEFT}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{LEFT}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{LEFT}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{BACK}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{BACK}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{BACK}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{BACK}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{BACK}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{BACK}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{BACK}");
            Playback.Wait(1000);
            Keyboard.SendKeys("{TAB}{ENTER}");
            SaveDialogUIMap.ClickSave();

            Assert.AreEqual(@"C:\DevelopmentDropOff\Integration Tests\Pugin1 - Copy.dll", path, "Cannot change remote plugin source");
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
            Playback.Wait(7000);
            PluginServiceWizardUIMap.ClickOK();

            //Change it back
            ExplorerUIMap.DoubleClickOpenProject(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            WizardsUIMap.WaitForWizard();
            actionName = PluginServiceWizardUIMap.GetActionName();
            PluginServiceWizardUIMap.ClickActionAtIndex(4);
            PluginServiceWizardUIMap.ClickTest();
            Playback.Wait(7000);
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
            Playback.Wait(4000);

            //CREATE A WORKFLOW
            RibbonUIMap.CreateNewWorkflow();
            Playback.Wait(4000);

            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);

            //SAVE A WORKFLOW
            OpenMenuItem("Save");
            Playback.Wait(4000);
            string InitialName = Guid.NewGuid().ToString();
            EnternameAndSave(InitialName);

            //RENAME A WORKFLOW
            ExplorerUIMap.EnterExplorerSearchText(InitialName);
            Playback.Wait(3500);
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
            Playback.Wait(4500);
        }

        #endregion
    }
}