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

namespace Warewolf.UITests.WebSource.WebSourceUIMapClasses
{
    [Binding]
    public partial class WebSourceUIMap
    {
        public void TryCloseNewWebSourceWizardTab()
        {
            if (UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.CloseButton))
            {
                Click_Close_Web_Source_Wizard_Tab_Button();
                if (UIMap.ControlExistsNow(DialogsUIMap.MessageBoxWindow.NoButton))
                {
                    DialogsUIMap.Click_MessageBox_No();
                }
            }
        }

        [When(@"I Type The Testing Site into Web POST Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_POST_Source_Wizard_Address_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Post";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web post address.");
        }

        [When(@"I Type The Testing Site into Web DELETE Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_DELETE_Source_Wizard_Address_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Delete";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web delete address.");
        }

        [When(@"I Type The Testing Site into Web PUT Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_PUT_Source_Wizard_Address_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Put";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web put address.");
        }
        public void Click_UserButton_On_WebServiceSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserRadioButton);
        }

        public void Click_AnonymousButton_On_WebServiceSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton);
        }

        public void Enter_TextIntoAddress_On_WebServiceSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Text = "http://RSAKLFSVRTFSBLD:9810";
        }

        public void Enter_RunAsUser_On_WebServiceSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserNameTextBox.Text = "IntegrationTester";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "I73573r0";
        }

        public void Enter_DefaultQuery_On_WebServiceSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.DefaultQueryTextBox.Text = "";
        }

        [Given(@"I Click Close Web Source Wizard Tab Button")]
        [When(@"I Click Close Web Source Wizard Tab Button")]
        [Then(@"I Click Close Web Source Wizard Tab Button")]
        public void Click_Close_Web_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.CloseButton, new Point(9, 6));
        }

        [Given(@"I Click New Web Source Test Connection Button")]
        [When(@"I Click New Web Source Test Connection Button")]
        [Then(@"I Click New Web Source Test Connection Button")]
        public void Click_NewWebSource_TestConnectionButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton, new Point(52, 14));
        }

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
    }
}
