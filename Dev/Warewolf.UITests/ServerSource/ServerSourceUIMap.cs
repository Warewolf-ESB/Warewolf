using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Deploy.DeployUIMapClasses;
using Warewolf.UITests.Settings.SettingsUIMapClasses;
using Warewolf.UITests.ServerSource.ServerSourceUIMapClasses;

namespace Warewolf.UITests.ServerSource.ServerSourceUIMapClasses
{
    [Binding]
    public partial class ServerSourceUIMap
    {
        private void TryCloseServerSourceWizardTab()
        {
            try
            {
                if (UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.CloseButton))
                {
                    Click_Close_Server_Source_Wizard_Tab_Button();
                }
                if (UIMap.ControlExistsNow(DialogsUIMap.MessageBoxWindow.NoButton))
                {
                    DialogsUIMap.Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryClose method failed to close Server Source tab.\n" + e.Message);
            }
        }

        [When(@"I Select TSTCIREMOTE From Server Source Wizard Dropdownlist")]
        public void Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.TSTCIREMOTE, new Point(70, 19));
            Assert.AreEqual("TST-CI-REMOTE", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.AddressEditBox.Text, "Server source address textbox text does not equal TST-CI-REMOTE");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Exists, "Server source wizard does not contain a test connection button");
        }

        [Given(@"I change Server Authentication From Deploy And Validate Changes From Explorer")]
        [When(@"I change Server Authentication From Deploy And Validate Changes From Explorer")]
        [Then(@"I change Server Authentication From Deploy And Validate Changes From Explorer")]
        public void ChangeServerAuthenticationFromDeployAndValidateChangesFromExplorer()
        {
            var windowsRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton;
            var publicRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton;

            if (publicRadioButton.Selected)
            {
                windowsRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button();
                UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Playback.Wait(1000);
                Click_Close_Server_Source_Wizard_Tab_Button();
                ExplorerUIMap.Select_RemoteConnectionIntegration_From_Explorer();
                ExplorerUIMap.Click_Explorer_RemoteServer_Edit_Button();
                Playback.Wait(1000);
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Selected, "Windows Radio Button not selected.");
                UIMap.Click_Deploy_Ribbon_Button();
                DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                DeployUIMap.Click_Deploy_Tab_Source_Server_Edit_Button();
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Selected, "Windows Radio Button not selected.");
            }
            else
            {
                publicRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button();
                UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Playback.Wait(1000);
                Click_Close_Server_Source_Wizard_Tab_Button();
                ExplorerUIMap.Select_RemoteConnectionIntegration_From_Explorer();
                ExplorerUIMap.Click_Explorer_RemoteServer_Edit_Button();
                Playback.Wait(1000);
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected, "Public Radio Button not selected.");
                UIMap.Click_Deploy_Ribbon_Button();
                DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                DeployUIMap.Click_Deploy_Tab_Source_Server_Edit_Button();
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected, "Public Radio Button not selected.");
            }
        }

        [Given(@"I set AuthenticationType to Public")]
        [When(@"I set AuthenticationType to Public")]
        [Then(@"I set AuthenticationType to Public")]
        public void ChangeServerAuthenticationTypeToPublic()
        {
            ExplorerUIMap.Click_Explorer_RemoteServer_Edit_Button();
            var publicRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton;
            if (!publicRadioButton.Selected)
            {
                publicRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button();
                UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Click_Close_Server_Source_Wizard_Tab_Button();
            }
            else
            {
                Click_Close_Server_Source_Wizard_Tab_Button();
            }
        }

        public void Click_UserButton_On_ServerSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UserRadioButton);
        }

        public void Enter_TextIntoAddress_On_ServerSourceTab(string server)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.AddressEditBox.Text = server;
        }

        public void Enter_RunAsUser_On_ServerSourceTab(string user, string password)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UsernameTextBox.Text = user;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PasswordTextBox.Text = password;
        }

        public void Select_Server_Authentication_Public()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected = true;
        }

        [When(@"I Select http From Server Source Wizard Address Protocol Dropdown")]
        public void Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.ToggleDropdown, new Point(54, 8));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsHttp.Exists, "Http does not exist in server source wizard address protocol dropdown list.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsHttp, new Point(31, 12));
            Assert.AreEqual("http", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.HttpSelectedItemText.DisplayText, "Server source wizard address protocol is not equal to http.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.AddressEditBox.Exists, "Server source wizard address textbox does not exist");
        }

        [When(@"I Click Server Source Wizard Address Protocol Dropdown")]
        public void Click_Server_Source_Wizard_Address_Protocol_Dropdown()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.ToggleDropdown, new Point(54, 8));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsHttp.Exists, "Http does not exist in server source wizard address protocol dropdown list.");
        }

        [When(@"I Click Server Source Wizard Test Connection Button")]
        public void Click_Server_Source_Wizard_Test_Connection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton, new Point(51, 8));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.Spinner);
        }

        [Given(@"I Create Remote Server Source As ""(.*)"" with address ""(.*)""")]
        [When(@"I Create Remote Server Source As ""(.*)"" with address ""(.*)""")]
        [Then(@"I Create Remote Server Source As ""(.*)"" with address ""(.*)""")]
        public void CreateRemoteServerSource(string ServerSourceName, string ServerAddress)
        {
            CreateRemoteServerSource(ServerSourceName, ServerAddress, false);
        }

        public void CreateRemoteServerSource(string ServerSourceName, string ServerAddress, bool PublicAuth = false)
        {
            Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext
                .NewServerSource.AddressComboBox.AddressEditBox.Text = ServerAddress;
            if (ServerAddress == "tst-ci-")
            {
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.TSTCIREMOTE.Exists, "TSTCIREMOTE does not exist in server source wizard drop down list after starting by typing tst-ci-.");
                Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist();
            }
            if (PublicAuth)
            {
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected = true;
            }
            Click_Server_Source_Wizard_Test_Connection_Button();
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.ErrorText.Spinner);
            UIMap.Save_With_Ribbon_Button_And_Dialog(ServerSourceName);
            Click_Close_Server_Source_Wizard_Tab_Button();
        }

        [Given(@"I Click Close Server Source Wizard Tab Button")]
        [When(@"I Click Close Server Source Wizard Tab Button")]
        [Then(@"I Click Close Server Source Wizard Tab Button")]
        public void Click_Close_Server_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.CloseButton, new Point(5, 5));
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

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
    }
}
