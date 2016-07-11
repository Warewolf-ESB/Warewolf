using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace Warewolf.Studio.UISpecs
{
    /// <summary>
    /// Summary description for RemoteServer
    /// </summary>
    [CodedUITest]
    public class RemoteServer
    {
        public RemoteServer()
        {
        }

        [TestMethod]
        public void BigRemoteServerUITest()
        {
            var uimap = new UIMap();
            var explorerTreeItemActionSteps = new Explorer_Tree_Item_Action_Steps();

            uimap.Assert_NewWorkFlow_RibbonButton_Exists();
            uimap.Click_New_Workflow_Ribbon_Button();
            uimap.Assert_StartNode_Exists();

            //Action Unit: Explorer dropdownlist contains "New Remote Server..." option
            uimap.Assert_Connect_Control_Exists_InExplorer();
            uimap.Click_Connect_Control_InExplorer();    
            uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_NewRemoteServer();

            //Action Unit: Selecting "New Remote Server..." From the Remote Server Dropdownbox Opens New Server Source Wizard
            //Given "New Remote Server..." exists in the dropdown list
            uimap.Select_NewRemoteServer_From_Explorer_Server_Dropdownlist();
            uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Exists();

            //Action Unit: Http is in the address protocol dropdown list
            //uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Exists();
            uimap.Click_Server_Source_Wizard_Address_Protocol_Dropdown();
            uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Contains_Http();

            //Action Unit: Selecting http from the address protocol dropdown list sets the selected item to http
            //uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Contains_http();
            uimap.Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            uimap.Assert_Server_Source_Wizard_Address_Protocol_Equals_Http();
            uimap.Assert_Server_Source_Wizard_Address_Textbox_Exists();

            //Action Unit: Domain server is in the server source wizard address intellisense
            //uimap.Assert_Server_Source_Wizard_Address_Textbox_Exists();
            uimap.Type_tstci_into_Server_Source_Wizard_Address_Textbox();
            uimap.Assert_Server_Source_Wizard_DropdownList_Contains_TSTCIREMOTE();

            //Action Unit: Selecting an item out of the address textbox intellisense in the server source wizard sets the text
            //Given "TST-CI-REMOTE" exists in the dropdown list
            uimap.Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist();
            uimap.Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE();
            uimap.Assert_Server_Source_Wizard_Test_Connection_Button_Exists();

            //Action Unit: Clicking test connection button enables save button
            //uimap.Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE();
            //uimap.Assert_Server_Source_Wizard_Test_Connection_Button_Exists();
            uimap.Click_Server_Source_Wizard_Test_Connection_Button();
            Playback.Wait(1000);
            uimap.Assert_Server_Source_Wizard_Test_Passed();
            uimap.Assert_Save_Ribbon_Button_Enabled();

            //Action Unit: Saving a new server source opens save dialog
            //uimap.Assert_Save_Ribbon_Button_Enabled();
            uimap.Click_Save_Ribbon_Button();
            uimap.Assert_SaveDialog_Exists();
            uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();

            //Action Unit: Entering a valid server source name into the save dialog does not set the error state of the textbox to true
            //uimap.Assert_New_Server_Source_Save_Dialog_Exists();
            //uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();
            uimap.Enter_Servicename_As_TSTCIREMOTE();
            uimap.Assert_SaveDialog_SaveButton_Enabled();

            //Action Unit: Clicking the save button in the save dialog adds it to the explorer remote servers dropdown list
            //uimap.Assert_SaveDialog_SaveButton_Enabled();
            uimap.Click_SaveDialog_YesButton();
            //TODO: Remove this workaround
            uimap.Click_Close_Server_Source_Wizard_Tab_Button();
            explorerTreeItemActionSteps.AssertItemExistsAtBottomOfExplorer("localhost\\TSTCIREMOTE");

            //Action Step: TSTCIREMOTE server source exists in remote server dropdown list
            //Given: explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\TSTCIREMOTE");
            uimap.Click_Connect_Control_InExplorer();
            uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_TSTCIREMOTE();

            //Action Unit: Selecting a remote server in the explorer remote server dropdown list selected that server source in the connect control
            //Given: uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_TSTCIREMOTE();
            uimap.Select_TSTCIREMOTE_From_Explorer_Remote_Server_Dropdown_List_2();
            uimap.Assert_Explorer_Remote_Server_DropdownList_Has_TSTCIREMOTE_Selected();

            //Action Unit: Clicking the explorer remote server connect button loads the workflow1 remote resource
            //Given: uimap.Assert_Explorer_Remote_Server_DropdownList_Has_TSTCIREMOTE_Selected();
            uimap.Click_Explorer_RemoteServer_Connect_Button();
            explorerTreeItemActionSteps.AssertItemExistsAtBottomOfExplorer("TSTCIREMOTE\\workflow1");

            //Action Unit: Dragging on a remote workflow onto a local workflow design surface
            //Given: explorerTreeItemActionSteps.AssertExistsInExplorerTree("TSTCIREMOTE\\workflow1");
            explorerTreeItemActionSteps.WhenIDragTheItemFromTheExplorerTreeOntoTheDesignSurface("TSTCIREMOTE\\workflow1");
            uimap.Assert_Workflow_Exists_OnDesignSurface();

            //Action Unit: Clicking the save ribbon button opens save dialog
            uimap.Assert_Save_Ribbon_Button_Exists();
            uimap.Click_Save_Ribbon_Button();
            uimap.Assert_SaveDialog_Exists();
            uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();

            //Action Unit: Entering a valid workflow name into the save dialog does not set the error state of the textbox to true
            //UIMap.Assert_Save_Workflow_Dialog_Exists();
            //Uimap.Assert_Workflow_Name_Textbox_Exists();
            uimap.Enter_Servicename_As_RemoteServerUITestWorkflow();
            uimap.Assert_SaveDialog_SaveButton_Enabled();

            //Action Unit: Clicking the save button in the save dialog creates a new explorer item
            //UIMap.Assert_SaveDialog_SaveButton_Enabled();
            uimap.Click_SaveDialog_YesButton();
            Playback.Wait(1000);
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollDownInTheExplorerTree();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollToTheTopOfTheExplorerTree();
            explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\RemoteServerUITestWorkflow");




            //These units must go last:
            //Action Unit: Explorer context menu delete exists
            //Given "localhost\TSTCIREMOTE" exists in the explorer tree
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollUpInTheExplorerTree();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollToTheBottomOfTheExplorerTree();
            explorerTreeItemActionSteps.WhenIRightClickTheItemInTheExplorerTree("localhost\\TSTCIREMOTE");
            uimap.Assert_ExplorerContextMenu_Delete_Exists();

            //Action Unit: Clicking delete in the explorer context menu on TSTCIREMOTE server source shows message box
            //uimap.Assert_ExplorerConextMenu_Delete_Exists();
            uimap.Select_Delete_FromExplorerContextMenu();
            uimap.Assert_MessageBox_Yes_Button_Exists();

            //Action Unit: Clicking Yes on the delete prompt dialog removes TSTCIREMOTE from the explorer tree
            //uimap.Assert_MessageBox_Yes_Button_Exists();
            uimap.Click_MessageBox_Yes();
            explorerTreeItemActionSteps.AssertDoesNotExistInExplorerTree("localhost\\TSTCIREMOTE");

            //Action Unit: When a server source is deleted from the explorer tree it must be removed from the explorer remote server dropdown list
            //Given "localhost\TSTCIREMOTE" does not exist in the explorer tree
            uimap.Click_Connect_Control_InExplorer();
            uimap.Assert_Explorer_Remote_Server_DropdownList_Does_Not_Contain_TSTCIREMOTE();
            uimap.Click_Connect_Control_InExplorer();
            
            //Action Unit: Explorer context menu delete exists
            //Given "localhost\SomeWorkflow" exists in the explorer tree
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollUpInTheExplorerTree();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollToTheBottomOfTheExplorerTree();
            explorerTreeItemActionSteps.WhenIRightClickTheItemInTheExplorerTree("localhost\\RemoteServerUITestWorkflow");
            uimap.Assert_ExplorerContextMenu_Delete_Exists();

            //Action Unit: Clicking delete in the explorer context menu on SomeWorkflow shows message box
            //UIMap.Assert_ExplorerConextMenu_Delete_Exists();
            uimap.Select_Delete_FromExplorerContextMenu();
            uimap.Assert_MessageBox_Yes_Button_Exists();

            //Action Unit: Clicking Yes on the delete prompt dialog removes SomeWorkflow from the explorer tree
            //UIMap.Assert_MessageBox_Yes_Button_Exists();
            uimap.Click_MessageBox_Yes();
            explorerTreeItemActionSteps.AssertDoesNotExistInExplorerTree("localhost\\RemoteServerUITestWorkflow");
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        //Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.Disabled;
            Playback.PlaybackSettings.ShouldSearchFailFast = false;
            Playback.PlaybackSettings.SearchTimeout = 10000;
            //ActionSteps.StartTest();
        }

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //    Playback.Cleanup();
        //}

        #endregion

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
    }
}
