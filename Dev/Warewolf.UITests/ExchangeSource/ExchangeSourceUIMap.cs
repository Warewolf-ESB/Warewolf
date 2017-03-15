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

namespace Warewolf.UITests.ExchangeSource.ExchangeSourceUIMapClasses
{
    [Binding]
    public partial class ExchangeSourceUIMap
    {
        public void Edit_Timeout_On_ExchangeSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Text = "2000";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.Text = "dev2warewolf@gmail.com";
        }

        public void Enter_Text_Into_Exchange_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.AutoDiscoverUrlTxtBox.Text = "https://outlook.office365.com/EWS/Exchange.asmx";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.UserNameTextBox.Text = "Nkosinathi.Sangweni@TheUnlimited.co.za";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.PasswordTextBox.Text = "Password123";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.Text = "dev2warewolf@gmail.com";
        }

        [When(@"I Click ExchangeSource TestConnection Button")]
        public void Click_ExchangeSource_TestConnection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.TestConnectionButton, new Point(58, 16));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.Spinner);
        }

        [Given(@"I Click Close Exchange Source Tab Button")]
        [When(@"I Click Close Exchange Source Tab Button")]
        [Then(@"I Click Close RabExchangebitMQ Source Tab Button")]
        public void Click_ExchangeSource_CloseTabButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.CloseButton);
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
