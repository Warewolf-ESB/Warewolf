using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class RemoteSubworkflow
    {
        const string ServerSourceName = "TSTCIREMOTE";
        const string LocalWorkflowName = "RemoteServerUITestWorkflow";
        const string RemoteSubWorkflowName = "workflow1";
        const string WindowsGroup = "Domain Users";
        private const string ServerAddress = "tst-ci-";

        [TestMethod]
        public void BigRemoteSubworkflowUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Select_NewRemoteServer_From_Explorer_Server_Dropdownlist();
            Uimap.CreateRemoteServerSource(ServerSourceName, ServerAddress);
            Uimap.Enter_Text_Into_Explorer_Filter(ServerSourceName);
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
            Uimap.Select_From_Explorer_Remote_Server_Dropdown_List(Uimap.MainStudioWindow.ComboboxListItemAsTSTCIREMOTE);
            Uimap.Click_Explorer_RemoteServer_Connect_Button();
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
            Uimap.Enter_Text_Into_Explorer_Filter(RemoteSubWorkflowName);
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
            Uimap.TryRefreshExplorerUntilOneItemOnly();
            Uimap.Drag_Explorer_Remote_workflow1_Onto_Workflow_Design_Surface();
            Uimap.Save_With_Ribbon_Button_And_Dialog(LocalWorkflowName);
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_DebugInput_Debug_Button();
            Uimap.Click_Debug_Output_Workflow1_Name();
            Uimap.Enter_Text_Into_Explorer_Filter(LocalWorkflowName);
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.RightClick_Explorer_Localhost_First_Item();
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu();
            Uimap.SetResourcePermissions(LocalWorkflowName, WindowsGroup, true, true);
            Uimap.Click_Deploy_Ribbon_Button();
        }

        #region Additional test attributes
        
        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
            Uimap.WaitForStudioStart();
        }

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
