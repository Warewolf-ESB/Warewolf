using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DeployTests
    {
        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Tab_Default_View()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.Exists, "Source explorer tree does not exist on deploy.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.SourceServerName.Exists, "Source server name in deploy window does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.RefreshButton.Exists, "Refresh button source server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.FilterText.Exists, "Filter source server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.OverrideHyperlink.Exists, "Override count in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.NewResourceHyperlink.Exists, "New Resource count in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.EditSourceButton.Exists, "Edit source server button does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceConnectButton.Exists, "Connect button in the Source server does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.EditDestinationButton.Exists, "Edit Destination Server button does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.ConnectDestinationButton.Exists, "Connect Button in Destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceConnectControl.Exists, "Source Server connect control does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.ShowDependenciesButton.Exists, "Select All Dependencies button Destination Server does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.ServicesText.Exists, "Services Label in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.ServiceCountText.Exists, "Service Count value in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourcesText.Exists, "Source label in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceCountText.Exists, "Source Count value in the destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.NewResourcesText.Exists, "New Resource Label in the destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.OverrideText.Exists, "Override label on Destination Server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DeployButton.Exists, "Deploy button in Destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DeployButtonMessageText.Exists, "Success message label does not exist in destination server of the deploy window");
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Connect_And_Disconnect_Destination()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration (Connected)", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText, "Deploy tab destination server did not connect after clicking connect button.");
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText, "Deploy tab destination server did not disconnect after clicking disconnect button.");
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Connect_And_Disconnect_Source()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
            UIMap.Click_Deploy_Tab_Source_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration (Connected)", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText, "Deploy tab destination server did not connect after clicking connect button.");
            UIMap.Click_Deploy_Tab_Source_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText, "Deploy tab destination server did not disconnect after clicking disconnect button.");
        }

        [TestMethod]
        [TestCategory("Deploy")]
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
            UIMap.CloseHangingDialogs();
            UIMap.Click_Deploy_Ribbon_Button();
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
