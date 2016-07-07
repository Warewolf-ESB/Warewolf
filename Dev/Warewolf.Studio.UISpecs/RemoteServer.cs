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
            
            //Scenario: Explorer dropdownlist contains "New Remote Server..." option
            uimap.Assert_Connect_Control_Exists_InExplorer();
            uimap.Click_Connect_Control_InExplorer();    
            uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_NewRemoteServer();

            //Scenario: Selecting "New Remote Server..." From the Remote Server Dropdownbox Opens New Server Source Wizard
            //Given "New Remote Server..." exists in the dropdown list
            uimap.Select_NewRemoteServer_From_Explorer_Server_Dropdownlist();
            uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Exists();

            //Scenario: Http is in the address protocol dropdown list
            //uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Exists();
            uimap.Click_Server_Source_Wizard_Address_Protocol_Dropdown();
            uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Contains_Http();

            //Scenario: Selecting http from the address protocol dropdown list sets the selected item to http
            //uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Contains_http();
            uimap.Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            uimap.Assert_Server_Source_Wizard_Address_Protocol_Equals_Http();
            uimap.Assert_Server_Source_Wizard_Address_Textbox_Exists();

            //Scenario: Domain server is in the server source wizard address intellisense
            //uimap.Assert_Server_Source_Wizard_Address_Textbox_Exists();
            uimap.Type_tstci_into_Server_Source_Wizard_Address_Textbox();
            uimap.Assert_Server_Source_Wizard_DropdownList_Contains_TSTCIREMOTE();

            //Scenario: Selecting an item out of the address textbox intellisense in the server source wizard sets the text
            //Given "TST-CI-REMOTE" exists in the dropdown list
            uimap.Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist();
            uimap.Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE();
            uimap.Assert_Server_Source_Wizard_Test_Connection_Button_Exists();

            //Scenario: Clicking test connection button enables save button
            //uimap.Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE();
            //uimap.Assert_Server_Source_Wizard_Test_Connection_Button_Exists();
            uimap.Click_Server_Source_Wizard_Test_Connection_Button();
            uimap.Assert_Save_Ribbon_Button_Enabled();

            //Scenario: Saving a new server source opens save dialog
            //uimap.Assert_Save_Ribbon_Button_Enabled();
            uimap.Click_Save_Ribbon_Button();
            uimap.Assert_SaveDialog_Exists();
            uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();

            //Scenario: Entering a valid server source name into the save dialog does not set the error state of the textbox to true
            //uimap.Assert_New_Server_Source_Save_Dialog_Exists();
            //uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();
            uimap.Enter_Servicename_As_TSTCIREMOTE();

            uimap.Assert_SaveDialog_SaveButton_Enabled();

            //Scenario: Clicking the save button in the save dialog adds it to the explorer remote servers dropdown list
            //uimap.Assert_SaveDialog_SaveButton_Enabled();
            uimap.Click_SaveDialog_YesButton();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollUpInTheExplorerTree();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollToTheBottomOfTheExplorerTree();
            explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\TSTCIREMOTE");

            //Scenario: Server source named TSTCIREMOTE appears in the explorer remote server dropdown list
            //Given "localhost\TSTCIREMOTE" exists in the explorer tree
            uimap.Click_Connect_Control_InExplorer();
            uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_TSTCIREMOTE();
            uimap.Click_Connect_Control_InExplorer();







            //These units must go last:
            //Scenario: Explorer context menu delete exists
            //Given "localhost\TSTCIREMOTE" exists in the explorer tree
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollUpInTheExplorerTree();
            //TODO: Remove this workaround
            explorerTreeItemActionSteps.WhenIScrollToTheBottomOfTheExplorerTree();
            explorerTreeItemActionSteps.WhenIRightClickTheItemInTheExplorerTree("localhost\\TSTCIREMOTE");
            uimap.Assert_ExplorerContextMenu_Delete_Exists();

            //Scenario: Clicking delete in the explorer context menu on TSTCIREMOTE server source shows message box
            //uimap.Assert_ExplorerConextMenu_Delete_Exists();
            uimap.Select_Delete_FromExplorerContextMenu();
            uimap.Assert_MessageBox_Yes_Button_Exists();

            //Scenario: Clicking Yes on the delete prompt dialog removes TSTCIREMOTE from the explorer tree
            //uimap.Assert_MessageBox_Yes_Button_Exists();
            uimap.Click_MessageBox_Yes();
            explorerTreeItemActionSteps.AssertDoesNotExistInExplorerTree("localhost\\TSTCIREMOTE");

            //Scenario: When a server source is deleted from the explorer tree it must be removed from the explorer remote server dropdown list
            //Given "localhost\TSTCIREMOTE" does not exist in the explorer tree
            uimap.Click_Connect_Control_InExplorer();
            uimap.Assert_Explorer_Remote_Server_DropdownList_Does_Not_Contain_TSTCIREMOTE();
            uimap.Click_Connect_Control_InExplorer();

        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
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
