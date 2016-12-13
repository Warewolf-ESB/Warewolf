using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.UITests;

namespace Warewolf.UISpecs
{
    [Binding]
    [DeploymentItem("Warewolf.UITests.dll")]
    class SetDefaultPlaybackSettings
    {
        [BeforeScenario]
        public void UseDefaultPlaybackSettings()
        {
            Uimap.SetPlaybackSettings();
        }

        [BeforeScenario("@ExplorerTest")]
        public void TryDisconnectRemoteServer()
        {
            if (Uimap.ControlExistsNow(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegrationConnected))
            {
                Uimap.Click_Explorer_RemoteServer_Connect_Button();
            }
            else
            {
                Uimap.Click_Connect_Control_InExplorer();
                if (Uimap.ControlExistsNow(Uimap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected))
                {
                    Mouse.Click(Uimap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected.Text);
                    Uimap.Click_Explorer_RemoteServer_Connect_Button();
                }
            }
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

        static UIMap _uiMap;
    }
}
