using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class RemoteServers
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void DisconnectedRemoteServerUITest()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Explorer();
            UIMap.Click_Explorer_RemoteServer_Connect_Button();
            Playback.Wait(2000); //This wait replaced a wait for Spinner
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegrationConnected.Exists, "Remote server name does not end in (Connected) in explorer remote server dropdown list after clicking the connect button and waiting for the spinner.");
            UIMap.Click_Explorer_RemoteServer_Connect_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.LocalhostConnectedText.Exists, "Explorer did not change to localhost(Connected) after clicking disconnect button.");
            UIMap.Select_localhost_From_Explorer_Remote_Server_Dropdown_List();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Disconnected_Remote_Server_Seperately_UITest()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Explorer();
            UIMap.Click_Explorer_RemoteServer_Connect_Button();
            Playback.Wait(2000); //This wait replaced a wait for Spinner
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegrationConnected.Exists, "Remote server name does not end in (Connected) in explorer remote server dropdown list after clicking the connect button and waiting for the spinner.");
            UIMap.Click_Explorer_RemoteServer_Connect_Button();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Server_DropDown_Has_Remote_Servers_UITest()
        {
            UIMap.Click_Explorer_Remote_Server_Dropdown_List();
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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
