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

namespace Warewolf.UITests.EmailSource.EmailSourceUIMapClasses
{
    [Binding]
    public partial class EmailSourceUIMap
    {
        [Given(@"I Click EmailSource TestConnection Button")]
        [When(@"I Click EmailSource TestConnection Button")]
        [Then(@"I Click EmailSource TestConnection Button")]
        public void Click_EmailSource_TestConnection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TestConnectionButton, new Point(58, 16));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.Spinner);
        }

        [Given(@"I Click Close EmailSource Tab")]
        [When(@"I Click Close EmailSource Tab")]
        [Then(@"I Click Close EmailSource Tab")]
        public void Click_Close_EmailSource_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.CloseButton, new Point(13, 10));
        }

        public void Enter_Text_Into_EmailSource_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.HostTextBoxEdit.Text = "localhost";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.UserNameTextBoxEdit.Text = "test";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PasswordTextBoxEdit.Text = "test";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PortTextBoxEdit.Text = "2";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.FromTextBoxEdit.Text = "AThorLocal@norsegods.com";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.ToTextBoxEdit.Text = "dev2warewolf@gmail.com";
        }

        public void Edit_Timeout_On_EmailSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Text = "2000";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.FromTextBoxEdit.Text = "AThorLocal@norsegods.com";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.ToTextBoxEdit.Text = "dev2warewolf@gmail.com";
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
