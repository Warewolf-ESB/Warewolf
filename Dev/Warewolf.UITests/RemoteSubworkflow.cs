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


namespace Warewolf.UITests
{
    /// <summary>
    /// Summary description for RemoteServer
    /// </summary>
    [CodedUITest]
    public class RemoteSubworkflow
    {
        public RemoteSubworkflow()
        {
        }

        [TestMethod]
        [Ignore]//TODO: Re-intoduce before WOLF-1925 can be moved to done
        public void BigRemoteSubworkflowUITest()
        {
            Uimap.Assert_NewWorkFlow_RibbonButton_Exists();
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Assert_StartNode_Exists();

            //Action Unit: Explorer exists and connect control dropdownlist contains "New Remote Server..." option
            Uimap.Assert_Explorer_Exists();
            Uimap.Assert_Explorer_Localhost_Icon_Exists();
            Uimap.Assert_Connect_Control_Exists_InExplorer();
            Uimap.Assert_Connect_ConnectControl_Button_Exists_InExplorer();
            Uimap.Assert_Explorer_Edit_Connect_Control_Button_Exists();
            Uimap.Click_Connect_Control_InExplorer();    
            Uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_NewRemoteServer();

            //Action Unit: Selecting "New Remote Server..." From the Remote Server Dropdownbox Opens New Server Source Wizard
            //Given "New Remote Server..." exists in the dropdown list
            Uimap.Select_NewRemoteServer_From_Explorer_Server_Dropdownlist();
            Uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Exists();

            //Action Unit: Http is in the address protocol dropdown list
            //Uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Exists();
            Uimap.Click_Server_Source_Wizard_Address_Protocol_Dropdown();
            Uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Contains_Http();

            //Action Unit: Selecting http from the address protocol dropdown list sets the selected item to http
            //Uimap.Assert_Server_Source_Wizard_Address_Protocol_Dropdown_Contains_http();
            Uimap.Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            Uimap.Assert_Server_Source_Wizard_Address_Protocol_Equals_Http();
            Uimap.Assert_Server_Source_Wizard_Address_Textbox_Exists();

            //Action Unit: Domain server is in the server source wizard address intellisense
            //Uimap.Assert_Server_Source_Wizard_Address_Textbox_Exists();
            Uimap.Type_tstci_into_Server_Source_Wizard_Address_Textbox();
            Uimap.Assert_Server_Source_Wizard_DropdownList_Contains_TSTCIREMOTE();

            //Action Unit: Selecting an item out of the address textbox intellisense in the server source wizard sets the text
            //Given "TST-CI-REMOTE" exists in the dropdown list
            Uimap.Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist();
            Uimap.Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE();
            Uimap.Assert_Server_Source_Wizard_Test_Connection_Button_Exists();

            //Action Unit: Clicking test connection button enables save button
            //Uimap.Assert_Server_Source_Wizard_Address_Textbox_Text_Equals_TSTCIREMOTE();
            //Uimap.Assert_Server_Source_Wizard_Test_Connection_Button_Exists();
            Uimap.Click_Server_Source_Wizard_Test_Connection_Button();
            Playback.Wait(2000);
            Uimap.Assert_Server_Source_Wizard_Test_Passed();
            Uimap.Assert_Save_Ribbon_Button_Enabled();

            //Action Unit: Saving a new server source opens save dialog
            //Uimap.Assert_Save_Ribbon_Button_Enabled();
            Uimap.Click_Save_Ribbon_Button();
            Uimap.Assert_SaveDialog_Exists();
            Uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();

            //Action Unit: Entering a valid server source name into the save dialog does not set the error state of the textbox to true
            //Uimap.Assert_New_Server_Source_Save_Dialog_Exists();
            //Uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();
            Uimap.Enter_Servicename_As_TSTCIREMOTE();
            Uimap.Assert_SaveDialog_SaveButton_Enabled();

            //Action Unit: Clicking the save button in the save dialog dismisses save dialog
            //Given: Uimap.Assert_SaveDialog_SaveButton_Enabled();
            Uimap.Click_SaveDialog_YesButton();
            //TODO: Remove this workaround. Open server source wizard should close when that server source is deleted from the explorer
            Uimap.Click_Close_Server_Source_Wizard_Tab_Button();
            if (Uimap.MessageBoxWindow.NoButton.Exists)
            {
                Uimap.Click_MessageBox_No();
            }
            Uimap.Assert_MessageBox_Does_Not_Exist();

            //Action Unit: filtering and refreshing the explorer tree shows only RemoteServerUITestWorkflow on local server
            Uimap.Enter_TSTCIREMOTE_Into_Explorer_Filter();
            Uimap.Click_Explorer_Refresh_Button();
            Uimap.Assert_Explorer_Localhost_First_Item_Exists();

            //Action Step: TSTCIREMOTE server source exists in remote server dropdown list
            //Given: explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\TSTCIREMOTE");
            Uimap.Click_Connect_Control_InExplorer();
            Uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_TSTCIREMOTE();

            //Action Unit: Selecting a remote server in the explorer remote server dropdown list selects that server source in the connect control
            //Given: Explorer Remote Server DropdownList Contains TSTCIREMOTE
            Uimap.Select_TSTCIREMOTE_From_Explorer_Remote_Server_Dropdown_List();
            Uimap.Assert_Explorer_Remote_Server_DropdownList_Has_TSTCIREMOTE_Selected();

            //Action Unit: Clicking the explorer remote server connect button loads the workflow1 remote resource
            //Given: Explorer Remote Server DropdownList Has TSTCIREMOTE Selected
            Uimap.Click_Explorer_RemoteServer_Connect_Button();
            Playback.Wait(2000);
            Uimap.Assert_Explorer_First_Remote_Server_Exists();

            //Action Unit: filtering and refreshing the explorer tree shows only workflow1 on remote server
            Uimap.Enter_workflow1_Into_Explorer_Filter();
            Uimap.Click_Explorer_Refresh_Button();
            Uimap.Assert_Explorer_First_Remote_Server_First_Item_Exists();

            //Action Unit: Dragging on a remote workflow onto a local workflow design surface
            //Given: Uimap.Assert_Explorer_First_Remote_Server_First_Item_Exists();
            Uimap.Drag_Explorer_First_Remote_Server_First_Item_Onto_Workflow_Design_Surface();
            Uimap.Assert_Workflow_Exists_OnDesignSurface();

            //Action Unit: Clicking the save ribbon button opens save dialog
            Uimap.Assert_Save_Ribbon_Button_Exists();
            Uimap.Click_Save_Ribbon_Button();
            Uimap.Assert_SaveDialog_Exists(); 
            Uimap.Assert_SaveDialog_ServiceName_Textbox_Exists();

            //Action Unit: Entering a valid workflow name into the save dialog does not set the error state of the textbox to true
            //UIMap.Assert_Save_Workflow_Dialog_Exists();
            //Uimap.Assert_Workflow_Name_Textbox_Exists();
            Uimap.Enter_Servicename_As_RemoteServerUITestWorkflow();
            Uimap.Assert_SaveDialog_SaveButton_Enabled();

            //Action Unit: Clicking the save button in the save dialog dismisses save dialog
            //UIMap.Assert_SaveDialog_SaveButton_Enabled();
            Uimap.Click_SaveDialog_YesButton();
            Playback.Wait(2000);
            Uimap.Assert_MessageBox_Does_Not_Exist();

            //Action Unit: filtering and refreshing the explorer tree shows only RemoteServerUITestWorkflow on local server
            Uimap.Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
            Uimap.Click_Explorer_Refresh_Button();
            Uimap.Assert_Explorer_Localhost_First_Item_Exists();

            /**TODO: Re-introduce these units before WOLF-1923 can be moved to done
            //Action Unit: Clicking Debug Button Shows Debug Input Dialog
            //Uimap.Assert_Workflow_Exists_OnDesignSurface();
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Assert_DebugInput_Window_Exists();
            Uimap.Assert_DebugInput_DebugButton_Exists();
	        Uimap.Assert_DebugInput_CancelButton_Exists();
	        Uimap.Assert_DebugInput_RememberCheckbox_Exists();
	        Uimap.Assert_DebugInput_ViewInBrowser_Button_Exists();
	        Uimap.Assert_DebugInput_DebugButton_Exists();
	        Uimap.Assert_DebugInput_InputData_Window_Exists();
	        Uimap.Assert_DebugInput_InputData_Field_Exists();
	        Uimap.Assert_DebugInput_Xml_Tab_Exists();
	        Uimap.Assert_DebugInput_Xml_Window_Exists();
	        Uimap.Assert_DebugInput_Json_Tab_Exists();
	        Uimap.Assert_DebugInput_Json_Window_Exists();

            //Action Unit: Clicking Debug Button In Debug Input Dialog Generates Debug Output
            //UIMap.Assert_Debug_Input_Dialog_Exists();
            //Uimap.Assert_DebugInput_DebugButton_Exists();
            Uimap.Click_DebugInput_DebugButton();
            Uimap.Assert_DebugOutput_Exists();
            Uimap.Assert_DebugOutput_SettingsButton_Exists();
            Uimap.Assert_DebugOutput_Contains_Workflow1();
            Uimap.Assert_DebugOutput_SettingsButton_Exists();
            Uimap.Assert_DebugOutput_Contains_SomeVariable();
	        Uimap.Assert_DebugOutput_ExpandCollapseButton_Exists();
	        Uimap.Assert_DebugOutput_FilterTextbox_Exists();
	        Uimap.Assert_DebugOutput_ResultsTree_Exists();
	        Uimap.Assert_DebugOutput_SettingsButton_Exists();

            //Action Unit: Clicking workflow cell in debug output highlights on design surface
	        Uimap.Click_Cell_Highlights_Workflow_OnDesignSurface();
            Uimap.Assert_Remote_Workflow_Is_Highlighted_On_Design_Surface();

            //Action Unit: Clicking workflow name in the debug output opens workflow
	        Uimap.Click_Debug_Output_Workflow_Name();
            Uimap.Assert_Remote_Workflow_Tab_Is_Open();
            **/

            //Action Unit: Right clicking an item in the explorer shows 'show dependency' option
            //Given: Uimap.Assert_Explorer_Localhost_First_Item_Exists();
            Uimap.RightClick_Explorer_Localhost_First_Item();
            Uimap.Assert_ShowDependencies_Exists_In_Explorer_Context_Menu();

            //Action Unit: Clicking show dependencies explorer context menu button opens the dependencies tab for that workflow
            //Given: Uimap.Assert_ExplorerContextMenu_ShowDependencies_Exists();
            Uimap.Click_Show_Dependencies_In_Explorer_Context_Menu();
            Uimap.Assert_Dependency_Graph_Show_Dependancies_Radio_Button_Is_Selected();
            Uimap.Assert_Dependency_Graph_Nesting_Levels_Textbox_Exists();
            Uimap.Assert_Dependency_Graph_Refresh_Button_Exists();
            Uimap.Assert_RemoteServerUITestWorkflow_Appears_In_Dependency_Diagram();

            //Action Unit: Clicking settings ribbon button shows settings tab
            Uimap.Assert_Settings_Ribbon_Button_Exists();
            Uimap.Click_Settings_Ribbon_Button();
            Uimap.Assert_Settings_LoggingTab_Exists();
            Uimap.Assert_Settings_SecurityTab_Exists();
            Uimap.Assert_Settings_LoggingTab_Exists();
            Uimap.Assert_Settings_ResourcePermissions_Exists();
            Uimap.Assert_Settings_ServerPermissions_Exists();
            Uimap.Assert_Settings_SecurityTab_Exists();
            Uimap.Assert_VariableList_DeleteButton_Exists();
            Uimap.Assert_Settings_SecurityTab_Resource_Permissions_Row1_Exists();

            //Action Unit: Clicking the add resource button shows resource picker
            Mouse.Click(Uimap.FindAddResourceButton(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.SplitPaneContent.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Uimap.Assert_Service_Picker_Dialog_Exists();

            //Action Unit: Selecting RemoteServerUITestWorkflow enabled the OK button
            Uimap.Enter_RemoteServerUITestWorkflow_Into_Service_Picker_Dialog();
            Uimap.Click_Service_Picker_Dialog_First_Service_In_Explorer();
            Uimap.Assert_Service_Picker_OK_Button_Enabled();

            //Action Unit: Clicking the service picker OK button dismissed the dialog
            Uimap.Click_Service_Picker_Dialog_OK();
            Uimap.Assert_Service_Picker_Dialog_Does_Not_Exist();

            //Action Unit: Clicking the add windows group button shows windows group dialog
            Mouse.Click(Uimap.FindAddWindowsGroupButton(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.SplitPaneContent.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Uimap.Assert_Select_Windows_Group_Dialog_Exists();
            Uimap.Assert_Select_Windows_Group_Dialog_Object_Name_Textbox_Exists();

            //Action Unit: Entering 'Domain Users' into object textbox enables OK button
            //Given: Uimap.Assert_Select_Windows_Group_Dialog_Object_Name_Textbox_Exists();
            Uimap.Enter_DomainUsers_Into_Windows_Group_Dialog();
            Uimap.Assert_Select_Windows_Group_Dialog_OK_Button_Enabled();

            //Action Unit: Clicking the OK button on the windows group dialog dismisses the dialog
            //Given: Uimap.Assert_Select_Windows_Group_Dialog_OK_Button_Enabled();
            Uimap.Click_Select_Windows_Group_OK_Button();
            Uimap.Assert_Select_Windows_Group_Dialog_Does_Not_Exist();

            //Action Unit: Checking the View and Execute checkboxes enables the save ribbon button
            Uimap.Click_Settings_Security_Tab_Resource_Permissions_View_Checkbox();
            Uimap.Click_Settings_Security_Tab_ResourcePermissions_Execute_Checkbox();
            Uimap.Assert_Save_Ribbon_Button_Enabled();

            //Action Unit: Saving security settings with restricted permissions displayed the correct icons in the explorer
            Uimap.Assert_Settings_SecurityTab_Resource_Permissions_Row1_View_Checkbox_Is_Checked();
            Uimap.Assert_Settings_SecurityTab_Resource_Permissions_Row1_Execute_Checkbox_Is_Checked();
            Uimap.Click_Save_Ribbon_Button();
            Uimap.Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
            Uimap.Click_Explorer_Refresh_Button();
            //TODO: re-introduce once WOLF-1924 is done.
            //Uimap.Assert_Explorer_Localhost_First_Item_View_Permission_Icon_Exists();
            //Uimap.Assert_Explorer_Localhost_First_Item_View_Permission_Icon_Exists();

            //Action Unit: Clicking the deploy ribbon button shows the deploy screen
            Uimap.Assert_Deploy_Ribbon_Button_Exists();
            Uimap.Click_Deploy_Ribbon_Button();
            Uimap.Assert_Source_Server_Name_Exists();
            Uimap.Assert_Refresh_Button_Source_Server_Exists();
            Uimap.Assert_Filter_Source_Server_Exists();
            Uimap.Assert_Connect_Control_DestinationServer_Exists();
            Uimap.Assert_Override_Count_Exists();
            Uimap.Assert_NewResource_Count_Exists();
            Uimap.Assert_Deploy_Destination_Server_Edit_Exists();
            Uimap.Assert_Connect_Button_Source_Server_Exists();
            Uimap.Assert_Edit_Button_Destination_Server_Exists();
            Uimap.Assert_Connect_button_Destination_Server_Exists();
            Uimap.Assert_Connect_Control_SourceServer_Exists();
            Uimap.Assert_ShowDependencies_Button_DestinationServer_Exists();
            Uimap.Assert_ServiceLabel_DestinationServer_Exists();
            Uimap.Assert_ServicesCount_Label_Exists();
            Uimap.Assert_SourceLabel_DestinationServer_Exists();
            Uimap.Assert_SourceCount_DestinationServer_Exists();
            Uimap.Assert_NewResource_Label_Exists();
            Uimap.Assert_Override_Label_DestinationServer_Exists();
            Uimap.Assert_DeployButton_DestinationServer_Exists();
            Uimap.Assert_SuccessMessage_Label_Exists();
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        //Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            try
            {
                //Action Unit: Filtering and refreshing the explorer tree shows only workflow1 on local server
                Uimap.Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
                Uimap.Click_Explorer_Refresh_Button();
                Uimap.Assert_Explorer_Localhost_First_Item_Exists();

                //Action Unit: Explorer context menu delete exists
                //Given ();localhost\RemoteServerUITestWorkflow(); exists in the explorer tree
                Uimap.RightClick_Explorer_Localhost_First_Item();
                Uimap.Assert_ExplorerContextMenu_Delete_Exists();

                //Action Unit: Clicking delete in the explorer context menu on SomeWorkflow shows message box
                //UIMap.Assert_ExplorerContextMenu_Delete_Exists();
                Uimap.Select_Delete_FromExplorerContextMenu();
                Uimap.Assert_MessageBox_Yes_Button_Exists();

                //Action Unit: Clicking Yes on the delete prompt dialog dismisses the dialog
                //UIMap.Assert_MessageBox_Yes_Button_Exists();
                Uimap.Click_MessageBox_Yes();
                Uimap.Assert_MessageBox_Does_Not_Exist();

                //Action Unit: Clearing and refreshing the explorer filter removes RemoteServerUITestWorkflow from the explorer tree
                Uimap.Click_Explorer_Filter_Clear_Button();
                Uimap.Click_Explorer_Refresh_Button();
                Uimap.Assert_Explorer_Localhost_First_Item_Exists();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove RemoteServerUITestWorkflow. Test may have crashed before RemoteServerUITestWorkflow was created.\n" + e.Message);
            }

            try
            {
                if (Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsTSTCIREMOTEConnected.Exists)
                {
                    Uimap.Click_Explorer_RemoteServer_Connect_Button();
                }
                else
                {
                    Uimap.Click_Connect_Control_InExplorer();
                    if (Uimap.MainStudioWindow.ComboboxListItemAsTSTCIREMOTEConnected.Exists)
                    {
                        Uimap.Select_TSTCIREMOTEConnected_From_Explorer_Remote_Server_Dropdown_List();
                        Uimap.Click_Explorer_RemoteServer_Connect_Button();
                    }
                }

                //Action Unit: Selecting localhost server in the explorer remote server dropdown list selects that server source in the connect control
                Uimap.Select_LocalhostConnected_From_Explorer_Remote_Server_Dropdown_List();
                Uimap.Assert_Explorer_Remote_Server_DropdownList_Has_localhost_Selected();

                //Action Unit: filtering and refreshing the explorer tree shows only TSTCIREMOTE on local server
                Uimap.Enter_TSTCIREMOTE_Into_Explorer_Filter();
                Uimap.Click_Explorer_Refresh_Button();
                Uimap.Assert_Explorer_Localhost_First_Item_Exists();

                //Action Unit: Explorer context menu delete exists
                //Given ();localhost\TSTCIREMOTE(); exists in the explorer tree
                Uimap.RightClick_Explorer_Localhost_First_Item();
                Uimap.Assert_ExplorerContextMenu_Delete_Exists();

                //Action Unit: Clicking delete in the explorer context menu on TSTCIREMOTE server source shows message box
                //Uimap.Assert_ExplorerConextMenu_Delete_Exists();
                Uimap.Select_Delete_FromExplorerContextMenu();
                Uimap.Assert_MessageBox_Yes_Button_Exists();

                //Action Unit: Clicking Yes on the delete prompt dialog dismisses the dialog
                //Uimap.Assert_MessageBox_Yes_Button_Exists();
                Uimap.Click_MessageBox_Yes();
                Uimap.Assert_MessageBox_Does_Not_Exist();

                //Action Unit: Clearing and refreshing the explorer filter removes TSTCIREMOTE from the explorer tree
                Uimap.Click_Explorer_Filter_Clear_Button();
                Uimap.Click_Explorer_Refresh_Button();
                Uimap.Assert_Explorer_Localhost_First_Item_Does_Not_Exist();

                //Action Unit: When a server source is deleted from the explorer tree it must be removed from the explorer remote server dropdown list
                //Given ();localhost\TSTCIREMOTE(); does not exist in the explorer tree
                Uimap.Click_Connect_Control_InExplorer();
                Uimap.Assert_Explorer_Remote_Server_DropdownList_Does_Not_Contain_TSTCIREMOTE();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove remote server TST-CI-REMOTE. Test may have crashed before remote server TST-CI-REMOTE was connected.\n" + e.Message);
                Uimap.Click_Explorer_Filter_Clear_Button();
            }
        }

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

        UIMap Uimap
        {
            get
            {
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;
    }
}
