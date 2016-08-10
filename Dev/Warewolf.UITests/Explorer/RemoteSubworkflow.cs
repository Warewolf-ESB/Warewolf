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
        const string ServerSourceName = "TSTCIREMOTE";
        const string LocalWorkflowName = "RemoteServerUITestWorkflow";
        const string RemoteSubWorkflowName = "workflow1";
        const string WindowsGroup = "Domain Users";

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
            Uimap.Save_With_Ribbon_Button_And_Dialog(ServerSourceName);
            Uimap.Click_Close_Server_Source_Wizard_Tab_Button();
            Uimap.Enter_Text_Into_Explorer_Filter(ServerSourceName);
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.Select_From_Explorer_Remote_Server_Dropdown_List(Uimap.MainStudioWindow.ComboboxListItemAsTSTCIREMOTE);
            Uimap.Click_Explorer_RemoteServer_Connect_Button();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
            Uimap.Enter_Text_Into_Explorer_Filter(RemoteSubWorkflowName);
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
            Uimap.TryRefreshExplorerUntilOneItemOnly();
            Uimap.Drag_Explorer_Remote_workflow1_Onto_Workflow_Design_Surface();
            Uimap.Save_With_Ribbon_Button_And_Dialog(LocalWorkflowName);
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_DebugInput_Debug_Button();
            Uimap.Click_Debug_Output_Workflow1_Name();
            Uimap.Enter_Text_Into_Explorer_Filter(LocalWorkflowName);
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.RightClick_Explorer_Localhost_First_Item();
            Uimap.Click_Show_Dependencies_In_Explorer_Context_Menu();
            Uimap.Click_Settings_Ribbon_Button();
            Uimap.Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            Uimap.Enter_ServiceName_Into_Service_Picker_Dialog(LocalWorkflowName);
            Uimap.Click_Service_Picker_Dialog_OK();
            Uimap.Click_Settings_Resource_Permissions_Row1_Windows_Group_Button();
            Uimap.Enter_GroupName_Into_Windows_Group_Dialog(WindowsGroup);
            Uimap.Click_Select_Windows_Group_OK_Button();
            Uimap.Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox();
            Uimap.Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox();
            Uimap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Uimap.Click_Deploy_Ribbon_Button();
        }

        #region Additional test attributes
        
        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitForStudioStart();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }
        
        [TestCleanup]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            Uimap.TryCloseHangingSaveDialog();
            Uimap.TryCloseHangingWindowsGroupDialog();
            Uimap.TryCloseHangingDebugInputDialog();
            Uimap.TryRemoveFromExplorer(LocalWorkflowName);
            Uimap.TryDisconnectFromRemoteServerAndRemoveSourceFromExplorer(ServerSourceName);
            Uimap.TryCloseAllTabs();
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
