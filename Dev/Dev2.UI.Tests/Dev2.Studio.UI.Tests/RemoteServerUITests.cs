using System;
using System.Windows;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps;
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

        private ExplorerUIMap _explorerUi = new ExplorerUIMap();


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
            Mouse.MouseDragSpeed = 10000;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
            _explorerUi.ClickServerInServerDDL(LocalHostServerName);
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
            Playback.Wait(3000);
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

            Assert.Fail("Poor assert conditions. Not properly testing the issue!");

            //const string TextToSearchWith = "Recursive File Copy";
            ////Ensure that we're in localhost
            //ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);
            ////Create a workfliow
            //RibbonUIMap.CreateNewWorkflow();
            //var theTab = TabManagerUIMap.GetActiveTab();
            //ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);

            //var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();

            //ExplorerUIMap.EnterExplorerSearchText(TextToSearchWith);
            //ExplorerUIMap.DragControlToWorkflowDesigner(RemoteServerName, "WORKFLOWS", "UTILITY", TextToSearchWith,
            //                                            point);

            //OpenMenuItem("Debug");
            //PopupDialogUIMap.WaitForDialog();
            //DebugUIMap.ClickExecute();
            //OutputUIMap.WaitForExecution();

            //Assert.IsFalse(OutputUIMap.IsAnyStepsInError(), "The remote workflow threw errors when executed locally");

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")] 
        public void RemoteServerUITests_DragAndDropWorkflowFromALocalServerOnARemoteServerCreatedWorkflow_WorkFlowIsDropped()
        {

            Assert.Fail("Poor assert conditions. Not properly testing the issue!");

            //const string TextToSearchWith = "Utility - Assign";
            //ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
            ////Create a workfliow
            //RibbonUIMap.CreateNewWorkflow();
            //var theTab = TabManagerUIMap.GetActiveTab();
            ////Connect to local server
            //ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);

            //var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();

            //ExplorerUIMap.EnterExplorerSearchText(TextToSearchWith);
            //ExplorerUIMap.DragControlToWorkflowDesigner(LocalHostServerName, "WORKFLOWS", "EXAMPLES",
            //                                            TextToSearchWith, point);

            //OpenMenuItem("Debug");
            //PopupDialogUIMap.WaitForDialog();
            //DebugUIMap.ClickExecute();
            //OutputUIMap.WaitForExecution();

            //Assert.IsFalse(OutputUIMap.IsAnyStepsInError(), "The local workflow threw errors when executed remotely");
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

            Assert.Fail("Poor assert conditions. Not a true reflection of the issue tested for!");

            //const string TextToSearchWith = "Find Records";

            //OpenWorkFlow(LocalHostServerName, "WORKFLOWS", "TESTS", TextToSearchWith);
            //var localHostTab = TabManagerUIMap.FindTabByName(TextToSearchWith);
            //Assert.IsNotNull(localHostTab);

            //OpenWorkFlow(RemoteServerName, "WORKFLOWS", "TESTS", TextToSearchWith);

            //OpenMenuItem("Debug");
            //SendKeys.SendWait("{F5}");
            //Playback.Wait(1000);

            //var remoteTab = TabManagerUIMap.FindTabByName(TextToSearchWith + " - " + RemoteServerName);
            //Assert.IsNotNull(remoteTab);

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteDbSource_DbSourceIsEdited()
        {
            Assert.Fail("Poor assert conditions. Not a true reflection of the issue tested for!");

            //const string TextToSearchWith = "DBSource";
            //OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            //Playback.Wait(2000);
            //DatabaseSourceUIMap.ClickSaveConnection();
            //SaveDialogUIMap.ClickSave();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebSource_WebSourceIsEdited()
        {

            Assert.Fail("Poor assert conditions. Not a true reflection of the issue tested for!");

            //const string TextToSearchWith = "WebSource";
            //OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            //Playback.Wait(2000);
            //SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");
            //SaveDialogUIMap.ClickSave();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebService_WebServiceIsEdited()
        {
            Assert.Fail("Poor assert conditions. Not a true reflection of the issue tested for!");

            //const string TextToSearchWith = "WebService";
            //OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            //Playback.Wait(2000);
            //SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");
            //SaveDialogUIMap.ClickSave();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteDbService_DbServiceIsEdited()
        {
            Assert.Fail("Poor assert conditions. Not properly testing the issue! You need to check window title to ensure RemoteConnection");

            //const string TextToSearchWith = "DBService";
            //OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            //Playback.Wait(2000);
            //DatabaseServiceWizardUIMap.ClickCancel();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteEmailSource_EmailSourceIsEdited()
        {
            Assert.Fail("Poor assert conditions. Not a true reflection of the issue tested for!");

            //const string TextToSearchWith = "EmailSource";
            //OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            //Playback.Wait(2000);
            //EmailSourceWizardUIMap.ClickCancel();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginSource_PluginSourceIsEdited()
        {

            Assert.Fail("Poor assert conditions. Not a true reflection of the issue tested for!");

            //const string TextToSearchWith = "PluginSource";
            //OpenWorkFlow(RemoteServerName, "SOURCES", "REMOTETESTS", TextToSearchWith);
            //Playback.Wait(2000);
            //PluginSourceMap.ClickSave();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginService_PluginServiceIsEdited()
        {

            Assert.Fail("Poor assert conditions. Not a true reflection of the issue tested for!");

            //const string TextToSearchWith = "PluginService";
            //OpenWorkFlow(RemoteServerName, "SERVICES", "REMOTEUITESTS", TextToSearchWith);
            //Playback.Wait(2000);
            //PluginServiceWizardUIMap.ClickTest();
            //PluginServiceWizardUIMap.ClickOK();

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_AddExecuteRenameAndDeleteALocalWorlFlow_ProcessCompletesSuccessfully()
        {

            Assert.Fail("Misleading logic in a single test. Please refactor out!");

            // 13/11 THIS DOES NOT BELONG HERE ;)
            //ProcessAWorkflow(LocalHostServerName, "WORKFLOWS", "Unassigned");

            // THIS IS FINE
            ProcessAWorkflow(RemoteServerName, "WORKFLOWS", "Unassigned");

        }

        void ProcessAWorkflow(string serverName, string serviceType, string folderName)
        {
            //CREATE A WORKFLOW
            RibbonUIMap.CreateNewWorkflow();
            
            ExplorerUIMap.ClickServerInServerDDL(serverName);
            
            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);

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
          
            ExplorerUIMap.EnterExplorerSearchText(InitialName);
            Playback.Wait(2000);
            ExplorerUIMap.RightClickRenameProject(serverName, serviceType, folderName, InitialName);
            string RenameTo = Guid.NewGuid().ToString();
            SendKeys.SendWait(RenameTo + "{ENTER}");

            //DELETE A WORKFLOW
            ExplorerUIMap.EnterExplorerSearchText(RenameTo);
            ExplorerUIMap.RightClickDeleteProject(serverName, serviceType, folderName, RenameTo);
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
            ExplorerUIMap.ClickServerInServerDDL(serverName);
            ExplorerUIMap.EnterExplorerSearchText(textToSearchWith);
            ExplorerUIMap.DoubleClickOpenProject(serverName, serviceType, foldername, textToSearchWith);
        }

        #endregion
    }
}