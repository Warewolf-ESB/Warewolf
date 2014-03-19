using System;
using System.Windows;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clipboard = System.Windows.Clipboard;

// ReSharper disable InconsistentNaming
namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    ///     These are UI tests based on using a remote server
    /// </summary>
    [CodedUITest]
    // ALL TEST HAVE Item with same key bug when reselecting server in connect control
    public class RemoteServerUiTests : UIMapBase
    {
        #region Fields

        const string RemoteServerName = "Remote Connection";
        const string LocalHostServerName = "localhost";

        #endregion

        #region Cleanup
        [TestInitialize]
        public void TestInit()
        {
            Init();
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
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
            var selectedSeverName = ExplorerUIMap.GetSelectedSeverName();
            Assert.AreEqual(RemoteServerName, selectedSeverName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_CreateRemoteWorkFlow_WorkflowIsCreated()
        {
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
            UITestControl tab = ExplorerUIMap.DoubleClickWorkflow("Find Records", "TESTS", RemoteServerName);
            ActivityUiMapBase activityUiMapBase = new DsfActivityUiMap(false);
            activityUiMapBase.TheTab = tab;
            activityUiMapBase.DragToolOntoDesigner(ToolType.Assign);
            var activeTabName = TabManagerUIMap.GetActiveTabName();
            Assert.IsTrue(activeTabName.Contains("Find Records - Remote Connection *"));

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_ViewRemoteWorkFlowInBrowser_WorkflowIsExecuted()
        {

            ExplorerUIMap.DoubleClickWorkflow("Find Records", "TESTS", RemoteServerName);
            KeyboardCommands.SendKey("{F7}");
            PopupDialogUIMap.WaitForDialog();
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

            //Create new workflow and drag a remote workflow onto it
            DsfActivityUiMap activityUiMap = new DsfActivityUiMap();
            activityUiMap.DragWorkflowOntoDesigner(remoteWorkflowName, "UTILITY", RemoteServerName);

            //Should be able to get clean debug output
            RibbonUIMap.DebugShortcutKeyPress();
            OutputUIMap.WaitForExecution();

            //Assert that the workflow really is on the design surface and debug output is clean
            Assert.IsFalse(OutputUIMap.IsAnyStepsInError(), "The remote workflow threw errors when executed locally");
            Assert.IsNotNull(activityUiMap.Activity);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_OpenWorkflowOnRemoteServerAndOpenWorkflowWithSameNameOnLocalHost_WorkflowIsOpened()
        {

            const string TextToSearchWith = "Find Records";

            var remoteTab = ExplorerUIMap.DoubleClickWorkflow(TextToSearchWith, "TESTS", RemoteServerName);
            Assert.IsNotNull(remoteTab);

            var localHostTab = ExplorerUIMap.DoubleClickWorkflow(TextToSearchWith, "TESTS");
            Assert.IsNotNull(localHostTab);

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_DebugARemoteWorkflowWhenLocalWorkflowWithSameNameIsOpen_WorkflowIsExecuted()
        {
            const string TextToSearchWith = "Find Records";

            var localTab = ExplorerUIMap.DoubleClickWorkflow(TextToSearchWith, "TESTS");
            Assert.IsNotNull(localTab);

            var remoteTab = ExplorerUIMap.DoubleClickWorkflow(TextToSearchWith, "TESTS", RemoteServerName);
            Assert.IsNotNull(remoteTab);

            RibbonUIMap.DebugShortcutKeyPress();
            OutputUIMap.WaitForExecution();

            var canidateTab = TabManagerUIMap.GetActiveTab();

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
            string userName;

            try
            {
                //Edit remote db source
                ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);

                var actualLeftTitleText = DatabaseSourceUIMap.GetLeftTitleText();
                var actualRightTitleText = DatabaseSourceUIMap.GetRightTitleText();

                Assert.AreEqual("Edit - DBSource", actualLeftTitleText);
                Assert.AreEqual("Remote Connection (http://rsaklfsvrtfsbld:3142/dsf)", actualRightTitleText);

                DatabaseSourceUIMap.ChangeAuthenticationTypeToUserFromWindows();
                DatabaseSourceUIMap.EnterUsernameAndPassword();
                DatabaseSourceUIMap.TestConnection();
                DatabaseSourceUIMap.ClickSaveDbConnectionFromTestConnection();

                SaveDialogUIMap.ClickSave();
            }
            finally
            {
                //Change it back
                ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);
                userName = DatabaseSourceUIMap.GetUserName();
                DatabaseSourceUIMap.ChangeAuthenticationTypeToWindowsFromUser();
                DatabaseSourceUIMap.ClickSaveDbConnectionFromWindowsRadioButton();
                SaveDialogUIMap.ClickSave();
            }

            Assert.AreEqual("testuser", userName, "Cannot edit remote db source");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebSource_WebSourceIsEdited()
        {
            const string TextToSearchWith = "Dev2GetCountriesWebService";

            //Edit remote web source
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "WEB SRC", RemoteServerName);

            var actualLeftTitleText = WebSourceWizardUIMap.GetLeftTitleText();
            var actualRightTitleText = WebSourceWizardUIMap.GetRightTitleText();

            Assert.AreEqual("Edit - Dev2GetCountriesWebService", actualLeftTitleText);
            Assert.AreEqual("Remote Connection (http://rsaklfsvrtfsbld:3142/dsf)", actualRightTitleText);

            WebSourceWizardUIMap.EnterTextIntoWizardTextBox(3, "?extension=json&prefix=b");
            WebSourceWizardUIMap.PressButtonOnWizard(3);
            SaveDialogUIMap.ClickSave();

            //Change it back
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "WEB SRC", RemoteServerName);
            //Get textbox text
            var persistClipboard = Clipboard.GetText();
            KeyboardCommands.SendTabs(3);
            WebSourceWizardUIMap.PressCtrlC();
            WebSourceWizardUIMap.EnterTextIntoWizardTextBox(0, "?extension=json&prefix=a");
            WebSourceWizardUIMap.PressButtonOnWizard(3);
            string query = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);
            SaveDialogUIMap.ClickSave();

            Assert.AreEqual("?extension=json&prefix=b", query, "Cannot change remote web source");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteWebService_WebServiceIsEdited()
        {
            const string TextToSearchWith = "WebService1234";
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            WebServiceWizardUIMap.ClickSaveButton(10);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteDbService_DbServiceIsEdited()
        {
            const string TextToSearchWith = "RemoteDBService";
            string actionName;

            //Edit remote db service
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            DatabaseServiceWizardUIMap.ClickScrollActionListUp();
            DatabaseServiceWizardUIMap.ClickFirstAction();
            DatabaseServiceWizardUIMap.ClickTestAction();
            DatabaseServiceWizardUIMap.ClickOK();
            //Change it back
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            actionName = DatabaseServiceWizardUIMap.GetActionName();
            DatabaseServiceWizardUIMap.ClickSecondAction();
            DatabaseServiceWizardUIMap.ClickTestAction();
            DatabaseServiceWizardUIMap.ClickOK();
            //Assert remote db service changed its action
            Assert.AreEqual("dbo.fn_diagram", actionName, "Cannot edit remote db service");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteEmailSource_EmailSourceIsEdited()
        {
            var emailServer = TestUtils.StartEmailServer();
            var machineName = Environment.MachineName;
            const string TextToSearchWith = "EmailSource";
            string timeout;

            //Edit remote email source
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);

            var actualLeftTitleText = EmailSourceWizardUIMap.GetLeftTitleText();
            var actualRightTitleText = EmailSourceWizardUIMap.GetRightTitleText();

            Assert.AreEqual("Edit - EmailSource", actualLeftTitleText);
            Assert.AreEqual("Remote Connection (http://rsaklfsvrtfsbld:3142/dsf)", actualRightTitleText);


            //Change Timeout
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(1, machineName);
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(5, "1234");
            //Test Email Source
            EmailSourceWizardUIMap.PressButtonOnWizard(1, 1000);
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(0, "@gmail.com");
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(1, "dev2developer@yahoo.com");
            EmailSourceWizardUIMap.PressButtonOnWizard(1, 1000);
            EmailSourceWizardUIMap.PressButtonOnWizard(8);
            SaveDialogUIMap.ClickSave();

            //Change it back
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);
            //Get the Timeout text
            var persistClipboard = Clipboard.GetText();
            EmailSourceWizardUIMap.SendTabsForWizard(6);
            EmailSourceWizardUIMap.PressCtrlC();
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(0, "100");
            EmailSourceWizardUIMap.PressButtonOnWizard(1);
            timeout = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);

            //Test Email Source
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(0, "@gmail.com");
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(1, "dev2developer@yahoo.com");
            EmailSourceWizardUIMap.PressButtonOnWizard(1, 1000);
            EmailSourceWizardUIMap.PressButtonOnWizard(8);
            SaveDialogUIMap.ClickSave();

            //Assert remote email source changed its timeout
            Assert.AreEqual("1234", timeout, "Cannot edit remote email source");

            TestUtils.StopEmailServer(emailServer);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginSource_PluginSourceIsEdited()
        {
            const string TextToSearchWith = "PluginSource";
            string path;

            //Edit remote plugin source
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);

            var actualLeftTitleText = PluginSourceMap.GetLeftTitleText();
            var actualRightTitleText = PluginSourceMap.GetRightTitleText();

            Assert.AreEqual("Edit - PluginSource", actualLeftTitleText);
            Assert.AreEqual("Remote Connection (http://rsaklfsvrtfsbld:3142/dsf)", actualRightTitleText);


            PluginSourceMap.ClickPluginSourceAssemblyPath();
            PluginSourceMap.EnterTextIntoWizardTextBox(0, ("{LEFT}{LEFT}{LEFT}{LEFT} - Copy"), 100);
            PluginServiceWizardUIMap.PressButtonOnWizard(1);
            SaveDialogUIMap.ClickSave();

            //Change it back                        
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);
            path = PluginSourceMap.GetAssemblyPathText();
            PluginSourceMap.EnterTextIntoWizardTextBox(0, ("{LEFT}{LEFT}{LEFT}{LEFT}{BACK}{BACK}{BACK}{BACK}{BACK}{BACK}{BACK}"), 100);
            PluginServiceWizardUIMap.PressButtonOnWizard(1);
            SaveDialogUIMap.ClickSave();

            Assert.AreEqual(@"C:\DevelopmentDropOff\Integration Tests\Pugin1 - Copy.dll", path, "Cannot change remote plugin source");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginService_PluginServiceIsEdited()
        {
            const string TextToSearchWith = "PluginService";
            string actionName;

            //Edit remote plugin service
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            PluginServiceWizardUIMap.ClickActionAtIndex(3);
            PluginServiceWizardUIMap.ClickTest();
            PluginServiceWizardUIMap.ClickOK();

            //Change it back
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            actionName = PluginServiceWizardUIMap.GetActionName();
            PluginServiceWizardUIMap.ClickActionAtIndex(4);
            PluginServiceWizardUIMap.ClickTest();
            PluginServiceWizardUIMap.ClickOK();

            Assert.AreEqual("ToStrin", actionName, "Cannot change remote plugin service");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_AddRenameAndDeleteARemoteWorkFlow_CompletesSuccessfully()
        {
            const string CategoryName = "Unassigned";

            //CREATE A WORKFLOW
            using(DsfActivityUiMap activityUiMap = new DsfActivityUiMap())
            {
                activityUiMap.DragToolOntoDesigner(ToolType.Assign);

                //SAVE A WORKFLOW
                RibbonUIMap.ClickSave();
                string InitialName = Guid.NewGuid().ToString();
                SaveDialogUIMap.ClickAndTypeInNameTextbox(InitialName);

                //RENAME A WORKFLOW
                string RenameTo = Guid.NewGuid().ToString();
                ExplorerUIMap.RightClickRenameResource(InitialName, CategoryName, ServiceType.Workflows, RenameTo, RemoteServerName);

                //DELETE A WORKFLOW
                ExplorerUIMap.RightClickDeleteResource(RenameTo, CategoryName, ServiceType.Workflows, RemoteServerName);
                Assert.IsFalse(ExplorerUIMap.ValidateServiceExists(RenameTo, CategoryName, RemoteServerName));
            }

        }

        #endregion
    }
}