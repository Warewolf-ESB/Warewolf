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
    /// These are UI tests based on using a remote server
    /// </summary>
    [CodedUITest]
    public class RemoteServerUiTests : UIMapBase
    {
        #region Const

        const string RemoteServerName = "Remote Connection Integration";
        const string LocalHostServerName = "localhost";

        const string remoteConnectionString = "Remote Connection Integration (http://tst-ci-remote:3142/dsf)";

        #endregion

        #region Cleanup

        [TestInitialize]
        public void TestInit()
        {
            Init();
            ExplorerUIMap.ClickServerInServerDDL(RemoteServerName);
            ExplorerUIMap.Server_Click_WarewolfIcon(RemoteServerName);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            //TabManagerUIMap.CloseAllTabs();
            Playback.Wait(1500);
            Halt();
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
            using(ActivityUiMapBase activityUiMapBase = new DsfActivityUiMap(false))
            {
                activityUiMapBase.TheTab = tab;
                activityUiMapBase.DragToolOntoDesigner(ToolType.Assign);
                var activeTabName = TabManagerUIMap.GetActiveTabName();
                StringAssert.Contains(activeTabName, "Find Records - Remote Connection Integration *");
            }
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
            const string remoteWorkflowName = "MyLocalWF";

            //Ensure that we're in localhost
            ExplorerUIMap.ClickServerInServerDDL(LocalHostServerName);

            //Create new workflow and drag a remote workflow onto it
            using(DsfActivityUiMap activityUiMap = new DsfActivityUiMap())
            {
                activityUiMap.DragWorkflowOntoDesigner(remoteWorkflowName, "TestCategory", RemoteServerName);

                //Should be able to get clean debug output
                RibbonUIMap.DebugShortcutKeyPress();
                OutputUIMap.WaitForExecution();

                //Assert that the workflow really is on the design surface and debug output is clean
                Assert.IsFalse(OutputUIMap.IsAnyStepsInError(), "The remote workflow threw errors when executed locally");
                Assert.IsNotNull(activityUiMap.Activity);
            }
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
            // NOTE : Needs to use Dev2TestingDB

            const string TextToSearchWith = "DBSource";
            string userName;

            try
            {
                //Edit remote db source
                ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);

                var actualLeftTitleText = DatabaseSourceUIMap.GetLeftTitleText();
                var actualRightTitleText = DatabaseSourceUIMap.GetRightTitleText();

                Assert.AreEqual("Edit - DBSource", actualLeftTitleText);
                Assert.AreEqual(remoteConnectionString, actualRightTitleText);

                DatabaseSourceUIMap.EnterUsernameAndPassword("testuser2", "pass1234");
                DatabaseSourceUIMap.TestConnection();
                Playback.Wait(5000);
                DatabaseSourceUIMap.ClickSaveDbConnectionFromTestConnection();

                SaveDialogUIMap.ClickSave();
            }
            finally
            {
                //Change it back
                ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);
                userName = DatabaseSourceUIMap.GetUserName();

                DatabaseSourceUIMap.EnterUsernameAndPassword("testuser", "test123");
                DatabaseSourceUIMap.TestConnection();
                Playback.Wait(5000);
                DatabaseSourceUIMap.ClickSaveDbConnectionFromTestConnection();
                SaveDialogUIMap.ClickSave();
            }

            Assert.AreEqual("testuser2", userName, "Cannot edit remote db source");
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
            Assert.AreEqual(remoteConnectionString, actualRightTitleText);

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
            WebServiceWizardUIMap.ClickSaveButton();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteDbService_DbServiceIsEdited()
        {
            // NOTE : Needs to use Dev2TestingDB
            // IF RemoteServerUITests_EditRemoteDbSource_DbSourceIsEdited breaks it will also break this test

            const string TextToSearchWith = "RemoteDBService";

            //Edit remote db service
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            DatabaseServiceWizardUIMap.ClickScrollActionListUp();
            DatabaseServiceWizardUIMap.ClickFourthAction();
            DatabaseServiceWizardUIMap.ClickTestAction();
            KeyboardCommands.SendTabs(5);
            KeyboardCommands.SendEnter();

            //Change it back
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            string actionName = DatabaseServiceWizardUIMap.GetActionName();
            DatabaseServiceWizardUIMap.ClickThirdAction();
            DatabaseServiceWizardUIMap.ClickTestAction();
            KeyboardCommands.SendTabs(5);
            KeyboardCommands.SendEnter();
            //Assert remote db service changed its action
            StringAssert.Contains(actionName, "dbo.FetchHtmlFr");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemoteEmailSource_EmailSourceIsEdited()
        {
            var emailServer = TestUtils.StartEmailServer();
            var machineName = Environment.MachineName;
            const string TextToSearchWith = "EmailSource";

            //Edit remote email source
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);

            var actualLeftTitleText = EmailSourceWizardUIMap.GetLeftTitleText();
            var actualRightTitleText = EmailSourceWizardUIMap.GetRightTitleText();

            Assert.AreEqual("Edit - EmailSource", actualLeftTitleText);
            Assert.AreEqual(remoteConnectionString, actualRightTitleText);


            //Change Timeout
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(1, machineName);
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(5, "1234");
            //Test Email Source
            EmailSourceWizardUIMap.PressButtonOnWizard(1, 1000);
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(0, "@gmail.com");
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(1, "dev2developer@yahoo.com");
            EmailSourceWizardUIMap.PressButtonOnWizard(1, 5000);
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
            string timeout = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);

            //Test Email Source
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(0, "@gmail.com");
            EmailSourceWizardUIMap.EnterTextIntoWizardTextBox(1, "dev2developer@yahoo.com");
            EmailSourceWizardUIMap.PressButtonOnWizard(1, 5000);
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
            // NOTE : 
            // Requires a Plugin Directory on server with :
            // Plugins\PrimativesTestDLL.dll 
            // And Plugins\PrimativesTestDLL - Copy.dll

            const string TextToSearchWith = "PluginSource";

            //Edit remote plugin source
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);

            var actualLeftTitleText = PluginSourceMap.GetLeftTitleText();
            var actualRightTitleText = PluginSourceMap.GetRightTitleText();

            Assert.AreEqual("Edit - PluginSource", actualLeftTitleText);
            Assert.AreEqual(remoteConnectionString, actualRightTitleText);

            KeyboardCommands.SendTabs(2, 250);
            KeyboardCommands.SelectAll();
            KeyboardCommands.SendDel();
            KeyboardCommands.SendKey("Plugins\\PrimativesTestDLL - Copy.dll");
            PluginServiceWizardUIMap.PressButtonOnWizard(1);
            SaveDialogUIMap.ClickSave();

            // Change it back                        
            ExplorerUIMap.DoubleClickSource(TextToSearchWith, "REMOTETESTS", RemoteServerName);
            KeyboardCommands.SendTabs(2, 250);
            KeyboardCommands.SelectAll();

            // get the path to see what it saved as ;)
            var path = KeyboardCommands.SendCopy();

            KeyboardCommands.SendDel();
            KeyboardCommands.SendKey("Plugins\\PrimativesTestDLL.dll");
            PluginServiceWizardUIMap.PressButtonOnWizard(1);
            SaveDialogUIMap.ClickSave();

            Assert.AreEqual(path, @"Plugins\PrimativesTestDLL - Copy.dll", "Cannot change remote plugin source");
        }

        [TestMethod]
        [Ignore]//Ashley: Pending 12095 (interupting run)
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        public void RemoteServerUITests_EditRemotePluginService_PluginServiceIsEdited()
        {
            // NOTE : 
            // Requires PluginSource points the correct location as set below...

            // Requires a Plugin Directory on server with :
            // Plugins\PrimativesTestDLL.dll 
            // And Plugins\PrimativesTestDLL - Copy.dll

            const string TextToSearchWith = "PluginService";

            //Edit remote plugin service
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            PluginServiceWizardUIMap.ClickActionAtIndex(4);
            KeyboardCommands.SendTabs(8, 250);
            KeyboardCommands.SendEnter(500); // test it

            KeyboardCommands.SendTabs(5, 250);
            KeyboardCommands.SendEnter(500); // save it

            //Change it back
            ExplorerUIMap.DoubleClickService(TextToSearchWith, "REMOTEUITESTS", RemoteServerName);
            string actionName = PluginServiceWizardUIMap.GetActionName();
            PluginServiceWizardUIMap.ClickActionAtIndex(13);
            KeyboardCommands.SendTabs(8, 250);
            KeyboardCommands.SendEnter(500); // test it

            KeyboardCommands.SendTabs(5, 250);
            KeyboardCommands.SendEnter(500); // save it

            StringAssert.Contains(actionName, "FetchCharVal");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RemoteServerUITests")]
        [Ignore] // Does not work over RDP / Inconsistent results
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
                ExplorerUIMap.RightClickRenameResource(InitialName, CategoryName, RenameTo, RemoteServerName);

                //DELETE A WORKFLOW
                ExplorerUIMap.RightClickDeleteResource(RenameTo, CategoryName, RemoteServerName);
                Assert.IsFalse(ExplorerUIMap.ValidateServiceExists(RenameTo, CategoryName, RemoteServerName));
            }

        }

        #endregion

        public UIMap UIMap
        {
            get
            {
                if((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}