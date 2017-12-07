﻿using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowServiceTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Deploy.DeployUIMapClasses;
using Warewolf.UI.Tests.Settings.SettingsUIMapClasses;

namespace Warewolf.UI.Tests.ServerSource.ServerSourceUIMapClasses
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
                Click_Server_Source_Wizard_Test_Connection_Button_For_Valid_Server_Source();
                UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Playback.Wait(1000);
                DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                Mouse.Click(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditSourceButton);
                Playback.Wait(1000);
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Selected, "Windows Radio Button not selected.");
                UIMap.Click_Deploy_Ribbon_Button();
                DeployUIMap.Select_ConnectedRemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                DeployUIMap.Click_Deploy_Tab_Source_Server_Edit_Button();
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Selected, "Windows Radio Button not selected.");
            }
            else
            {
                publicRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button_For_Valid_Server_Source();
                UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Playback.Wait(1000);
                DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                Mouse.Click(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditSourceButton);
                Playback.Wait(1000);
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected, "Public Radio Button not selected.");
                UIMap.Click_Deploy_Ribbon_Button();
                DeployUIMap.Select_ConnectedRemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                DeployUIMap.Click_Deploy_Tab_Source_Server_Edit_Button();
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected, "Public Radio Button not selected.");
            }
        }
        
        [When(@"I Change Permissions For Resource ""(.*)"" and Validate")]
        public void WhenIChangeResourcePermissionsandValidate(string resource)
        {
            var publicRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton;
            Assert.IsFalse(publicRadioButton.Selected, "Remote Connection Integration server is not expected to be using public auth.");
            publicRadioButton.Selected = true;
            Click_Server_Source_Wizard_Test_Connection_Button();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            Click_Close_Server_Source_Wizard_Tab_Button();
            DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            DeployUIMap.ValidateICanNotDeploy(resource);
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

        [When("I Select Server Authentication Public")]
        public void Select_Server_Authentication_Public()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected = true;
        }

        [When("I Select Server Authentication Windows")]
        public void Select_Server_Authentication_Windows()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Selected = true;
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

        [When(@"I Click Server Source Wizard Test Connection Button")]
        public void Click_Server_Source_Wizard_Test_Connection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton, new Point(51, 8));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.Spinner);
        }

        [When(@"I Click Server Source Wizard Test Connection Button For Valid Server Source")]
        public void Click_Server_Source_Wizard_Test_Connection_Button_For_Valid_Server_Source()
        {
            Click_Server_Source_Wizard_Test_Connection_Button();
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.Spinner);
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new source.");
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
        
        [When(@"I Click Close Server Source Wizard Tab Button")]
        public void Click_Close_Server_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.CloseButton, new Point(5, 5));
        }

        [Given(@"Server Source Wizard Tab Test Button Is Enabled")]
        [Then(@"Server Source Wizard Tab Test Button Is Enabled")]
        public void TestConnectionButtonIsEnabled()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Enabled, "Test Connection button not enabled");
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
