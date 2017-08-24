using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Deploy.DeployUIMapClasses;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UI.Tests.Settings.SettingsUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class DeployTests
    {
        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Tab_Default_View()
        {
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.Exists, "Source explorer tree does not exist on deploy.");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.Exists, "Source server name in deploy window does not exist");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.RefreshButton.Exists, "Refresh button source server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.FilterText.Exists, "Filter source server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.OverrideHyperlink.Exists, "Override count in destination server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.NewResourceHyperlink.Exists, "New Resource count in destination server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditSourceButton.Exists, "Edit source server button does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditDestinationButton.Exists, "Edit Destination Server button does not exist");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Exists, "Source Server connect control does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ShowDependenciesButton.Exists, "Select All Dependencies button Destination Server does not exist");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ServicesText.Exists, "Services Label in destination server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ServiceCountText.Exists, "Service Count value in destination server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourcesText.Exists, "Source label in destination server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceCountText.Exists, "Source Count value in the destination server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.NewResourcesText.Exists, "New Resource Label in the destination server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.OverrideText.Exists, "Override label on Destination Server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Exists, "Deploy button in Destination server does not exist in the deploy window");
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButtonMessageText.Exists, "Success message label does not exist in destination server of the deploy window");
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Select_Server_AutoConnects_Destination_Server()
        {
            DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            Assert.AreEqual("Remote Connection Integration (Connected)", DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText, "Deploy tab destination server did not connect after clicking connect button.");
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Select_Server_AutoConnects_Source_Server()
        {
            DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
            Assert.AreEqual("Remote Connection Integration (Connected)", DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText, "Source Combobox text  is: " + DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ConnectedRemoteConnectionText.DisplayText);
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_Hello_World()
        {
            DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            DeployUIMap.Deploy_Service_From_Deploy_View("Hello World");
            DialogsUIMap.ClickDeployVersionConflictsMessageBoxOK();
            DialogsUIMap.ClickDeployConflictsMessageBoxOK();
            DialogsUIMap.ClickDeploySuccessfulMessageBoxOK();
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_DotnetWorkFlowForTesttingSelectAllDependencies_HasSourceSelected()
        {
            const string Source = "DotnetWorkflowForTesting";
            DeployUIMap.Enter_DeployViewOnly_Into_Deploy_Source_Filter(Source);
            DeployUIMap.Select_Deploy_First_Source_Item();
            var displayText = DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.OverrideHyperlink.UIItem1Text.DisplayText;
            Assert.AreEqual("1", displayText);
            DeployUIMap.Click_SelectAllDependencies_Button();
            Playback.Wait(10);
            displayText = DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.OverrideHyperlink.UIItem1Text.DisplayText;
            Assert.AreEqual("2", displayText);
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_ResourcePermissions_TogglesDeployButtonCorrectly()
        {
            const string Source = "ResourceWithViewAndExecutePerm";
            SettingsUIMap.SetupPublicPermissionsForForLocalhost(Source);
            ExplorerUIMap.SetupPublicPermissionsForForRemoteServer(Source);
            ExplorerUIMap.Select_ConnectedRemoteConnectionIntegration_From_Explorer();
            ExplorerUIMap.Click_EditServerButton_From_ExplorerConnectControl();
            var publicRadioButton = ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton;
            if (!publicRadioButton.Selected)
            {
                publicRadioButton.Selected = true;
                ServerSourceUIMap.Click_Server_Source_Wizard_Test_Connection_Button();
                UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
                ServerSourceUIMap.Click_Close_Server_Source_Wizard_Tab_Button();
                DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            }
            else
            {
                ServerSourceUIMap.Click_Close_Server_Source_Wizard_Tab_Button();
                DeployUIMap.Select_ConnectedRemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            }
            DeployUIMap.ValidateICanNotDeploy(Source);
        }

        [TestMethod]
        [TestCategory("Deploy")]
        public void Deploy_EditingServer_KeepsSelectedServer()
        {
            DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
            DeployUIMap.Click_Deploy_Tab_Source_Server_Edit_Button();
            ServerSourceUIMap.Click_Close_Server_Source_Wizard_Tab_Button();
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ConnectedRemoteConnectionText.Exists, "Selected source server in deploy is not Remote Connection Integration (Connected).");
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

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

        DeployUIMap DeployUIMap
        {
            get
            {
                if (_DeployUIMap == null)
                {
                    _DeployUIMap = new DeployUIMap();
                }

                return _DeployUIMap;
            }
        }

        private DeployUIMap _DeployUIMap;

        SettingsUIMap SettingsUIMap
        {
            get
            {
                if (_SettingsUIMap == null)
                {
                    _SettingsUIMap = new SettingsUIMap();
                }

                return _SettingsUIMap;
            }
        }

        private SettingsUIMap _SettingsUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        ServerSourceUIMap ServerSourceUIMap
        {
            get
            {
                if (_ServerSourceUIMap == null)
                {
                    _ServerSourceUIMap = new ServerSourceUIMap();
                }

                return _ServerSourceUIMap;
            }
        }

        private ServerSourceUIMap _ServerSourceUIMap;
        #endregion
    }
}
