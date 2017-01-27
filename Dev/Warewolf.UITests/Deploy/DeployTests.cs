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
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.Exists, "Source explorer tree does not exist on deploy.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.Exists, "Source server name in deploy window does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.RefreshButton.Exists, "Refresh button source server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.FilterText.Exists, "Filter source server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.OverrideHyperlink.Exists, "Override count in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.NewResourceHyperlink.Exists, "New Resource count in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditSourceButton.Exists, "Edit source server button does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceConnectButton.Exists, "Connect button in the Source server does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditDestinationButton.Exists, "Edit Destination Server button does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.ConnectDestinationButton.Exists, "Connect Button in Destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Exists, "Source Server connect control does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ShowDependenciesButton.Exists, "Select All Dependencies button Destination Server does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ServicesText.Exists, "Services Label in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ServiceCountText.Exists, "Service Count value in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourcesText.Exists, "Source label in destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceCountText.Exists, "Source Count value in the destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.NewResourcesText.Exists, "New Resource Label in the destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.OverrideText.Exists, "Override label on Destination Server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Exists, "Deploy button in Destination server does not exist in the deploy window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButtonMessageText.Exists, "Success message label does not exist in destination server of the deploy window");
            UIMap.Click_Close_Deploy_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Connect_And_Disconnect_Destination()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration (Connected)", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText, "Deploy tab destination server did not connect after clicking connect button.");
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText, "Deploy tab destination server did not disconnect after clicking disconnect button.");
            UIMap.Click_Close_Deploy_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Connect_And_Disconnect_Source()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
            UIMap.Click_Deploy_Tab_Source_Server_Connect_Button();
            Assert.AreEqual("Remote Connection Integration (Connected)", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText, "Source Combobox text  is: " + UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText);
            UIMap.Click_Deploy_Tab_Source_Refresh_Button();
            UIMap.Click_Deploy_Tab_Source_Server_Connect_Button();
            UIMap.Click_Deploy_Tab_Source_Refresh_Button();
            Assert.AreEqual("Remote Connection Integration", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText + "Deploy tab destination server did not disconnect after clicking disconnect button.");
            UIMap.Click_Close_Deploy_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Hello_World()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            UIMap.Deploy_Service_From_Deploy_View("Hello World");
            UIMap.Click_Close_Deploy_Tab_Button();
            UIMap.ClickDeployVersionConflictsMessageBoxOK();            
            UIMap.ClickDeployConflictsMessageBoxOK();
            UIMap.ClickDeploySuccessfulMessageBoxOK();
            UIMap.Click_Close_Deploy_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void DisconnectRemoteDestinationServerUITest()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ConnectedRemoteConnectionText.Exists, "Remote server name does not end in (Connected) in deploy destination server explorer remote server dropdown list after clicking the connect button and waiting for the spinner.");
            UIMap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.RemoteConnectionIntegrationText.Exists, "Remote server name still ends with (Connected) in deploy destination server explorer remote server dropdown list after clicking the disconnect button.");
            UIMap.Select_localhost_From_Deploy_Tab_Destination_Server_Combobox();
            UIMap.Click_Close_Deploy_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void DisconnectRemoteSourceServerUITest()
        {
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
            UIMap.Click_Deploy_Tab_Source_Server_Connect_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ConnectedRemoteConnectionText.Exists, "Remote server name does not end in (Connected) in deploy source server explorer remote server dropdown list after clicking the connect button and waiting for the spinner.");
            UIMap.Click_Deploy_Tab_Source_Server_Connect_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.Exists, "Remote server name still ends with (Connected) in deploy source server explorer remote server dropdown list after clicking the disconnect button.");
            UIMap.Select_localhost_From_Deploy_Tab_Source_Server_Combobox();
            UIMap.Click_Close_Deploy_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Change_Server_Details_From_DeployDestination_And_Save_Persists_Changes()
        {            
            UIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            UIMap.Click_Edit_Deploy_Destination_Server_Button();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.TabDescription.DisplayText.Contains("*"));
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected);
            UIMap.Select_Server_Authentication_Public();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.TabDescription.DisplayText.Contains("*"));
            UIMap.Click_Server_Source_Wizard_Test_Connection_Button();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_Close_Server_Source_Wizard_Tab_Button();
            UIMap.Click_Edit_Deploy_Destination_Server_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected);
        }    

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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
