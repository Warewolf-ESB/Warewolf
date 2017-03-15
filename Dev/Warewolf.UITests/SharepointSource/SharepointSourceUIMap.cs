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

namespace Warewolf.UITests.SharepointSource.SharepointSourceUIMapClasses
{
    [Binding]
    public partial class SharepointSourceUIMap
    {
        [When(@"I Enter Sharepoint ServerSource ServerName")]
        public void Enter_Sharepoint_ServerSource_ServerName()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Text = "http://rsaklfsvrsharep/";
        }

        [When(@"I Enter Sharepoint ServerSource User Credentials")]
        public void Enter_Sharepoint_ServerSource_User_Credentials()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserNameTextBox.Text = "IntegrationTester";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox.Text = "I73573r0";
        }

        public void Enter_TextIntoAddress_In_SharepointServiceSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Text = "http://rsaklfsvrsharep";
        }

        [When(@"I Click Sharepoint Server Source TestConnection")]
        public void Click_Sharepoint_Server_Source_TestConnection()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton, new Point(58, 16));
        }

        [Given(@"I Click Close SharepointSource Tab Button")]
        [When(@"I Click Close SharepointSource Tab Button")]
        [Then(@"I Click Close SharepointSource Tab Button")]
        public void Click_Close_SharepointSource_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.CloseButton, new Point(13, 7));
        }

        [Given(@"I Click User Button On Sharepoint Source")]
        [When(@"I Click User Button On Sharepoint Source")]
        [Then(@"I Click User Button On Sharepoint Source")]
        public void Click_UserButton_On_SharepointSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserRadioButton.Selected = true;
        }

        [Given(@"I Click Windows Button On Sharepoint Source")]
        [When(@"I Click Windows Button On Sharepoint Source")]
        [Then(@"I Click Windows Button On Sharepoint Source")]
        public void Click_WindowsButton_On_SharepointSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.WindowsRadioButton.Selected = true;
        }

        [Given(@"I Click Close Sharepoint Server Source Tab")]
        [When(@"I Click Close Sharepoint Server Source Tab")]
        [Then(@"I Click Close Sharepoint Server Source Tab")]
        public void WhenIClickCloseSharepointServerSourceWizardTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.CloseButton);
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
