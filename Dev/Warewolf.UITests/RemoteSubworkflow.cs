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
            Uimap.Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            Uimap.WaitForSpinner(Uimap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.Enter_Servicename_As_TSTCIREMOTE();
            Uimap.Click_SaveDialog_YesButton();
            Uimap.Click_Close_Server_Source_Wizard_Tab_Button();
            Uimap.Enter_TSTCIREMOTE_Into_Explorer_Filter();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.Select_TSTCIREMOTE_From_Explorer_Remote_Server_Dropdown_List();
            Uimap.Click_Explorer_RemoteServer_Connect_Button();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
            Uimap.Enter_Workflow1_Into_Explorer_Filter_Textbox();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
            Uimap.Drag_Explorer_Remote_workflow1_Onto_Workflow_Design_Surface();
            Uimap.Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            Uimap.WaitForSpinner(Uimap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.Enter_Servicename_As_RemoteServerUITestWorkflow();
            Uimap.Click_SaveDialog_YesButton();
            //Uimap.Click_Debug_Ribbon_Button();
            //Uimap.Click_DebugInput_Debug_RemoteServerUITestWorkflow_Button();
            //Uimap.Click_Debug_Output_Workflow1_Cell();
            //Uimap.Click_Debug_Output_Workflow1_Name();
            Uimap.Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.RightClick_Explorer_Localhost_First_Item();
            Uimap.Click_Show_Dependencies_In_Explorer_Context_Menu();
            Uimap.Click_Settings_Ribbon_Button();
            Uimap.Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            Uimap.Enter_RemoteServerUITestWorkflow_Into_Service_Picker_Dialog();
            Uimap.Click_Service_Picker_Dialog_OK();
            Uimap.Click_Settings_Resource_Permissions_Row1_Windows_Group_Button();
            Uimap.Enter_DomainUsers_Into_Windows_Group_Dialog();
            Uimap.Click_Select_Windows_Group_OK_Button();
            Uimap.Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox();
            Uimap.Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox();
            Uimap.Click_Save_Ribbon_Button();
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
            Uimap.TryCloseHangingSaveDialog();
            Uimap.TryCloseHangingWindowsGroupDialog();
            Uimap.TryRemoveRemoteServerUITestWorkflowFromExplorer();
            Uimap.TryDisconnectFromCIREMOTEAndRemoveSourceFromExplorer();
            //TODO: Cleanup and re-impliment this: Uimap.TryCloseAllTabs();
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
