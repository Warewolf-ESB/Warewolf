using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class RemoteServers
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void DebugUsingPlayIconRemoteServerUITest()
        {
            UIMap.Filter_Explorer("Hello World");
            Mouse.Hover(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
            Mouse.Hover(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon);
            UIMap.Debug_Using_Play_Icon();
            UIMap.Click_DebugInput_Debug_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.Exists, "Debug Output does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SettingsButton.Exists, "Output SettingsButton does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Exists, "Output SearchTextBox does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Exists, "DebugOutputTree does not exist after clicking Debug button from Debug Dialog");
            UIMap.Click_Close_Workflow_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DisconnectedRemoteServerUITest()
        {
            UIMap.Filter_Explorer("workflow1");
            UIMap.Select_RemoteConnectionIntegration_From_Explorer();
            UIMap.Click_Explorer_RemoteServer_Connect_Button();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegrationConnected.Exists, "Remote server name does not end in (Connected) in explorer remote server dropdown list after clicking the connect button and waiting for the spinner.");
            UIMap.Click_Explorer_RemoteServer_Connect_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegration.Exists, "Remote server name still ends with (Connected) in explorer remote server dropdown list after clicking the disconnect button.");
            UIMap.Select_localhost_From_Explorer_Remote_Server_Dropdown_List();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
        }

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}
