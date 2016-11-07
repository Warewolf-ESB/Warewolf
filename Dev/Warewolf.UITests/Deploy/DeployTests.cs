using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DeployTests
    {
        [TestMethod]
        public void Deploy_Connect_And_Disconnect_Destination()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration (Connected)", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText, "Deploy tab destination server did not connect after clicking connect button.");
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText, "Deploy tab destination server did not disconnect after clicking disconnect button.");
        }

        [TestMethod]
        public void Deploy_Connect_And_Disconnect_Source()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
            UIMap.Click_Deploy_Tab_Source_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration (Connected)", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText, "Deploy tab destination server did not connect after clicking connect button.");
            UIMap.Click_Deploy_Tab_Source_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText, "Deploy tab destination server did not disconnect after clicking disconnect button.");
        }

        [TestMethod]
        public void Deploy_Hello_World()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            UIMap.Deploy_Service_From_Deploy_View("Hello World");
            UIMap.Assert_Deploy_Was_Successful();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_Deploy_Ribbon_Button();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Deploy_Tab_Button();
        }
        public UIMap UIMap
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
