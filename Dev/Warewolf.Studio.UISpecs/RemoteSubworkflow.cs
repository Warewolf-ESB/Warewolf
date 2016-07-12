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
    public class RemoteSubworkflow
    {
        public RemoteSubworkflow()
        {
        }

        [TestMethod]
        public void BigRemoteSubworkflowUITest()
        {
            Uimap.Assert_NewWorkFlow_RibbonButton_Exists();
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Assert_StartNode_Exists();

            //Action Unit: Explorer dropdownlist contains "New Remote Server..." option
            Uimap.Assert_Connect_Control_Exists_InExplorer();
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
            //Uimap.Assert_SaveDialog_SaveButton_Enabled();
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
            explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\TSTCIREMOTE");

            //Action Step: TSTCIREMOTE server source exists in remote server dropdown list
            //Given: explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\TSTCIREMOTE");
            Uimap.Click_Connect_Control_InExplorer();
            Uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_TSTCIREMOTE();

            //Action Unit: Selecting a remote server in the explorer remote server dropdown list selects that server source in the connect control
            //Given: Uimap.Assert_Explorer_Remote_Server_DropdownList_Contains_TSTCIREMOTE();
            Uimap.Select_TSTCIREMOTE_From_Explorer_Remote_Server_Dropdown_List();
            Uimap.Assert_Explorer_Remote_Server_DropdownList_Has_TSTCIREMOTE_Selected();

            //Action Unit: Clicking the explorer remote server connect button loads the workflow1 remote resource
            //Given: Uimap.Assert_Explorer_Remote_Server_DropdownList_Has_TSTCIREMOTE_Selected();
            Uimap.Click_Explorer_RemoteServer_Connect_Button();
            Playback.Wait(2000);
            explorerTreeItemActionSteps.AssertExistsInExplorerTree("TSTCIREMOTE");

            //Action Unit: filtering and refreshing the explorer tree shows only workflow1 on remote server
            Uimap.Enter_workflow1_Into_Explorer_Filter();
            Uimap.Click_Explorer_Refresh_Button();
            explorerTreeItemActionSteps.AssertExistsInExplorerTree("TSTCIREMOTE\\workflow1");

            //Action Unit: Dragging on a remote workflow onto a local workflow design surface
            //Given: explorerTreeItemActionSteps.AssertExistsInExplorerTree("TSTCIREMOTE\\workflow1");
            explorerTreeItemActionSteps.WhenIDragTheItemFromTheExplorerTreeOntoTheDesignSurface("TSTCIREMOTE\\workflow1");
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
            explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\RemoteServerUITestWorkflow");

            /**TODO: Re-introduce these units after bug is fixed
            //Action Unit: Clicking Debug Button Shows Debug Input Dialog
            //Uimap.Assert_Workflow_Exists_OnDesignSurface();
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Assert_DebugInput_Window_Exists();
            Uimap.Assert_DebugInput_DebugButton_Exists();

            //Action Unit: Clicking Debug Button In Debug Input Dialog Generates Debug Output
            //UIMap.Assert_Debug_Input_Dialog_Exists();
            //Uimap.Assert_DebugInput_DebugButton_Exists();
            Uimap.Click_DebugInput_DebugButton();
            Uimap.Assert_DebugOutput_Exists();
            Uimap.Assert_DebugOutput_SettingsButton_Exists();
            Uimap.Assert_DebugOutput_Contains_Workflow1();
            **/

            //Action Unit: Right clicking an item in the explorer shows 'show dependency' option
            //Given: explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\TSTCIREMOTE");
            explorerTreeItemActionSteps.WhenIRightClickTheItemInTheExplorerTree("localhost\\TSTCIREMOTE");
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
            Uimap.Assert_Settings_SecurityTab_Exists();
            Uimap.Assert_Settings_LoggingTab_Exists();
            Uimap.Assert_Settings_ResourcePermissions_Exists();
            Uimap.Assert_Settings_ServerPermissions_Exists();
            Uimap.Assert_Settings_SecurityTab_Resource_Permissions_Row1_Exists();
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
        }

        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        public void MyTestCleanup()
        {
            try
            {
                //Action Unit: Filtering and refreshing the explorer tree shows only workflow1 on local server
                Uimap.Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
                Uimap.Click_Explorer_Refresh_Button();
                explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\RemoteServerUITestWorkflow");

                //Action Unit: Explorer context menu delete exists
                //Given "localhost\RemoteServerUITestWorkflow" exists in the explorer tree
                explorerTreeItemActionSteps.WhenIRightClickTheItemInTheExplorerTree("localhost\\RemoteServerUITestWorkflow");
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
                explorerTreeItemActionSteps.AssertDoesNotExistInExplorerTree("localhost\\RemoteServerUITestWorkflow");
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
                explorerTreeItemActionSteps.AssertExistsInExplorerTree("localhost\\TSTCIREMOTE");

                //Action Unit: Explorer context menu delete exists
                //Given "localhost\TSTCIREMOTE" exists in the explorer tree
                explorerTreeItemActionSteps.WhenIRightClickTheItemInTheExplorerTree("localhost\\TSTCIREMOTE");
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
                explorerTreeItemActionSteps.AssertDoesNotExistInExplorerTree("localhost\\TSTCIREMOTE");

                //Action Unit: When a server source is deleted from the explorer tree it must be removed from the explorer remote server dropdown list
                //Given "localhost\TSTCIREMOTE" does not exist in the explorer tree
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

        Explorer_Tree_Item_Action_Steps explorerTreeItemActionSteps
        {
            get
            {
                if ((_explorerTreeItemActionSteps == null))
                {
                    _explorerTreeItemActionSteps = new Explorer_Tree_Item_Action_Steps();
                }

                return _explorerTreeItemActionSteps;
            }
        }

        private Explorer_Tree_Item_Action_Steps _explorerTreeItemActionSteps;
    }
}
