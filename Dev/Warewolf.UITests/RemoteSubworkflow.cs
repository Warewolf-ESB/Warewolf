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
        public void BigRemoteSubworkflowUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Select_NewRemoteServer_From_Explorer_Server_Dropdownlist();
            Uimap.Click_Server_Source_Wizard_Address_Protocol_Dropdown();
            Uimap.Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            Uimap.Type_tstci_into_Server_Source_Wizard_Address_Textbox();
            Uimap.Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist();
            Uimap.Click_Server_Source_Wizard_Test_Connection_Button();
            Uimap.Click_Save_Ribbon_Button();
            Uimap.Enter_Servicename_As_TSTCIREMOTE();
            Uimap.Click_SaveDialog_YesButton();
            Uimap.Click_Close_Server_Source_Wizard_Tab_Button();
            Uimap.Enter_TSTCIREMOTE_Into_Explorer_Filter();
            Uimap.Select_TSTCIREMOTE_From_Explorer_Remote_Server_Dropdown_List();
            Uimap.Click_Explorer_RemoteServer_Connect_Button();
            Uimap.Drag_Remote_workflow1_Onto_Workflow_Design_Surface();
            Uimap.Click_Save_Ribbon_Button();
            Uimap.Enter_Servicename_As_RemoteServerUITestWorkflow();
            Uimap.Click_SaveDialog_YesButton();
            Uimap.Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
            /**TODO: Re-introduce these actions before WOLF-1923 can be moved to done
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_DebugInput_DebugButtonParams.Workflow1Exists = true;
            Uimap.Click_DebugInput_DebugButton();
	        Uimap.Click_Cell_Highlights_Workflow_OnDesignSurface();
	        Uimap.Click_Debug_Output_Workflow_Name();
            **/
            Uimap.RightClick_Explorer_Localhost_First_Item();
            Uimap.Click_Show_Dependencies_In_Explorer_Context_Menu();
            Uimap.Click_Settings_Ribbon_Button();
            Uimap.Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            Uimap.Enter_RemoteServerUITestWorkflow_Into_Service_Picker_Dialog();
            Uimap.Click_Service_Picker_Dialog_OK();
            Uimap.Click_Settings_Resource_Permissions_Row1_Windows_Group_Button();
            Uimap.Enter_DomainUsers_Into_Windows_Group_Dialog();
            Uimap.Click_Select_Windows_Group_OK_Button();
            Uimap.Click_Settings_Security_Tab_Resource_Permissions_View_Checkbox();
            Uimap.Click_Settings_Security_Tab_ResourcePermissions_Execute_Checkbox();
            Uimap.Click_Save_Ribbon_Button();
            Uimap.Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
            Uimap.Click_Deploy_Ribbon_Button();
        }

        #region Additional test attributes
        
        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }
        
        [TestCleanup]
        public void MyTestCleanup()
        {
            try
            {
                Uimap.Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
                Uimap.RightClick_Explorer_Localhost_First_Item();
                Uimap.Select_Delete_FromExplorerContextMenu();
                Uimap.Click_MessageBox_Yes();
                Uimap.Click_Explorer_Filter_Clear_Button();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove RemoteServerUITestWorkflow. Test may have crashed before RemoteServerUITestWorkflow was created.\n" + e.Message);
            }

            try
            {
                Playback.PlaybackSettings.SearchTimeout = 1000;
                var selectedItemAsTstciremoteConnected = Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsTSTCIREMOTEConnected;
                Playback.PlaybackSettings.SearchTimeout = 5000;
                if (selectedItemAsTstciremoteConnected.Exists)
                {
                    Uimap.Click_Explorer_RemoteServer_Connect_Button();
                }
                else
                {
                    Uimap.Click_Connect_Control_InExplorer();
                    Playback.PlaybackSettings.SearchTimeout = 1000;
                    var comboboxListItemAsTstciremoteConnected = Uimap.MainStudioWindow.ComboboxListItemAsTSTCIREMOTEConnected;
                    Playback.PlaybackSettings.SearchTimeout = 5000;
                    if (comboboxListItemAsTstciremoteConnected.Exists)
                    {
                        Uimap.Select_TSTCIREMOTEConnected_From_Explorer_Remote_Server_Dropdown_List();
                        Uimap.Click_Explorer_RemoteServer_Connect_Button();
                    }
                }
                Uimap.Select_LocalhostConnected_From_Explorer_Remote_Server_Dropdown_List();
                Uimap.Enter_TSTCIREMOTE_Into_Explorer_Filter();
                Playback.PlaybackSettings.SearchTimeout = 1000;
                var wpfTreeItem = Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
                Playback.PlaybackSettings.SearchTimeout = 5000;
                if (wpfTreeItem.Exists)
                {
                    Uimap.RightClick_Explorer_Localhost_First_Item();
                    Uimap.Select_Delete_FromExplorerContextMenu();
                    Uimap.Click_MessageBox_Yes();
                }
                Uimap.Click_Explorer_Filter_Clear_Button();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove remote server TST-CI-REMOTE. Test may have crashed before remote server TST-CI-REMOTE was connected.\n" + e.Message);
                Uimap.Click_Explorer_Filter_Clear_Button();
            }
        }
        
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
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
