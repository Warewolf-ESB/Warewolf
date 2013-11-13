using System;
using System.Windows;
using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        private static TabManagerUIMap _tabManager = new TabManagerUIMap();
        private static ExplorerUIMap _explorerUi = new ExplorerUIMap();
        //private static DockManagerUIMap _dockManager = new DockManagerUIMap();

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
        }

        //[ClassCleanup]
        //public static void MyTestCleanup()
        //{
        //    _tabManager.CloseAllTabs();

        //    //DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
        //    _explorerUi.ClickServerInServerDDL(LocalHostServerName);
        //}

        #endregion


        #region Test Methods

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

            const string TextToSearchWith = "Recursive File Copy";
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            //Ensure that we're in localhost
            ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);
            //Create a workfliow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);

            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();

            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(TextToSearchWith);
            ExplorerUIMap.DragControlToWorkflowDesigner(RemoteServerName, "WORKFLOWS", "UTILITY", TextToSearchWith,
                                                        point);

            OpenMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

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
            var theTab = TabManagerUIMap.GetActiveTab();
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            //Connect to local server
            ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);

            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();

            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(TextToSearchWith);
            ExplorerUIMap.DragControlToWorkflowDesigner(LocalHostServerName, "WORKFLOWS", "EXAMPLES",
                                                        TextToSearchWith, point);

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
            Playback.Wait(2000);
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
            Playback.Wait(2000);
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
            Playback.Wait(2000);
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
            Playback.Wait(2000);
            DatabaseServiceWizardUIMap.ClickCancel();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteEmailSource_EmailSourceIsEdited()
        {

            const string TextToSearchWith = "EmailSource";
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            Playback.Wait(2000);
            EmailSourceWizardUIMap.ClickCancel();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginSource_PluginSourceIsEdited()
        {

            const string TextToSearchWith = "PluginSource";
            OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            Playback.Wait(2000);
            PluginSourceMap.ClickSave();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginService_PluginServiceIsEdited()
        {

            const string TextToSearchWith = "PluginService";
            OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            Playback.Wait(2000);
            PluginServiceWizardUIMap.ClickTest();
            PluginServiceWizardUIMap.ClickOK();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_AddExecuteRenameAndDeleteALocalWorlFlow_ProcessCompletesSuccessfully()
        {

            // 13/11 THIS DOES NOT BELONG HERE ;)
            //ProcessAWorkflow(LocalHostServerName, "WORKFLOWS", "Unassigned");

            // THIS IS FINE
            ProcessAWorkflow(RemoteServerName, "WORKFLOWS", "Unassigned");

        }

        void ProcessAWorkflow(string serverName, string serviceType, string folderName)
        {
            //CREATE A WORKFLOW
            RibbonUIMap.CreateNewWorkflow();
            
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(serverName);
            
            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ToolboxUIMap.DragControlToWorkflowDesigner("MultiAssign", point);

            //SAVE A WORKFLOW
            OpenMenuItem("Save");
            Playback.Wait(2000);
            string InitialName = Guid.NewGuid().ToString();
            EnternameAndSave(InitialName);

            //EXECUTE A WORKFLOW
            OpenMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            SendKeys.SendWait("{F5}");

            //RENAME A WORKFLOW
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(InitialName);
            ExplorerUIMap.RightClickRenameProject(serverName, serviceType, folderName, InitialName);
            string RenameTo = Guid.NewGuid().ToString();
            SendKeys.SendWait(RenameTo + "{ENTER}");

            //DELETE A WORKFLOW
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(RenameTo);
            ExplorerUIMap.RightClickDeleteProject(serverName, serviceType, folderName, RenameTo);
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            Assert.IsFalse(ExplorerUIMap.ServiceExists("localhost", serviceType, folderName, RenameTo), "Resources on " + serverName + " cannot be deleted");
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
            DockManagerUIMap.ClickOpenTabPage(ExplorerTab);
            ExplorerUIMap.ClickServerInServerDDL(serverName);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(textToSearchWith);
            ExplorerUIMap.DoubleClickOpenProject(serverName, serviceType, foldername, textToSearchWith);
        }

        #endregion
    }
}